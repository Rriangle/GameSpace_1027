using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GameSpace.Models; // ManagerDatum, GameSpacedatabaseContext
using GameSpace.Models.ViewModels; // LoginInput, VerifyEmailInput, LoginSuccessVM
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;   // 雜湊驗證用（可保留）
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameSpace.Controllers
{
	public class LoginController : Controller
	{
		private readonly GameSpacedatabaseContext _db;

		// ★ 與 Program.cs 完全一致：後台獨立 Cookie 方案
		private const string AdminCookieScheme = "AdminCookie";

		// 鎖定/驗證相關參數
		private const int MaxFailedAccessCount = 5;
		private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
		private const string OtpSessionUserIdKey = "ManagerOtpUserId";
		private const string OtpSessionCodeKey = "ManagerOtpCode";
		private const string OtpSessionExpireKey = "ManagerOtpExpire";

		public LoginController(GameSpacedatabaseContext db) => _db = db;

		// GET: /Login
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Index() => View("Login", new LoginInputVM());

		// POST: /Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(LoginInputVM input)
		{
			if (!ModelState.IsValid) return View("Login", input);

			var now = DateTime.UtcNow;
			var account = input.ManagerAccount?.Trim();
			var pwd = input.ManagerPassword?.Trim();

			var manager = await _db.ManagerData
				.Include(m => m.ManagerRoles) // 讀角色與權限
				.FirstOrDefaultAsync(m => m.ManagerAccount == account);

			if (manager == null)
			{
				ModelState.AddModelError(string.Empty, "帳號或密碼錯誤。");
				return View("Login", input);
			}

			// 鎖定檢查
			if (manager.ManagerLockoutEnabled && manager.ManagerLockoutEnd.HasValue && manager.ManagerLockoutEnd.Value > now)
			{
				var mins = (int)Math.Ceiling((manager.ManagerLockoutEnd.Value - now).TotalMinutes);
				ModelState.AddModelError(string.Empty, $"帳號已被鎖定，請於 {mins} 分鐘後再試。");
				return View("Login", input);
			}

			// 密碼驗證（若欄位是 Identity 雜湊則驗證雜湊，否則純文字）
			if (!VerifyPassword(manager, pwd, manager.ManagerPassword))
			{
				manager.ManagerAccessFailedCount += 1;
				if (manager.ManagerAccessFailedCount >= MaxFailedAccessCount)
				{
					manager.ManagerLockoutEnabled = true;
					manager.ManagerLockoutEnd = now.Add(LockoutDuration);
				}
				await _db.SaveChangesAsync();
				ModelState.AddModelError(string.Empty, "帳號或密碼錯誤。");
				return View("Login", input);
			}

			// 密碼正確 → 歸零錯誤次數、解除鎖定
			manager.ManagerAccessFailedCount = 0;
			manager.ManagerLockoutEnabled = false;
			manager.ManagerLockoutEnd = null;
			await _db.SaveChangesAsync();

			// 尚未完成 Email 驗證 → 走一次性驗證碼
			if (!manager.ManagerEmailConfirmed)
			{
				var (code, expireAt) = GenerateOtp();
				HttpContext.Session.SetString(OtpSessionUserIdKey, manager.ManagerId.ToString());
				HttpContext.Session.SetString(OtpSessionCodeKey, code);
				HttpContext.Session.SetString(OtpSessionExpireKey, expireAt.ToUniversalTime().Ticks.ToString());

				TempData["DevOtp"] = code; // TODO: 實作寄信
				return RedirectToAction(nameof(VerifyEmail));
			}

			// 已驗證 Email → 直接簽入（含角色與權限 Claims）
			await SignInAsync(manager);
			return RedirectToAction(nameof(Success));
		}

		// GET: /Login/VerifyEmail
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> VerifyEmail()
		{
			var pending = await GetPendingOtpUserAsync();
			if (pending == null) return RedirectToAction(nameof(Index));

			var maskedEmail = MaskEmail(pending.ManagerEmail);
			return View(new VerifyEmailInputVM { MaskedEmail = maskedEmail });
		}

		// POST: /Login/VerifyEmail
		[HttpPost]
		[AllowAnonymous]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> VerifyEmail(VerifyEmailInputVM input)
		{
			var pending = await GetPendingOtpUserAsync();
			if (pending == null)
			{
				ModelState.AddModelError(string.Empty, "驗證流程已失效，請重新登入。");
				return View(input);
			}
			if (!ModelState.IsValid) return View(input);

			var (otpCode, expireAtUtc) = ReadOtpFromSession();
			if (otpCode == null || expireAtUtc == null)
			{
				ModelState.AddModelError(string.Empty, "驗證碼已失效，請重新登入。");
				return View(input);
			}
			if (DateTime.UtcNow > expireAtUtc.Value)
			{
				ModelState.AddModelError(string.Empty, "驗證碼已過期，請重新登入取得新的驗證碼。");
				return View(input);
			}
			if (!string.Equals(otpCode, input.Code?.Trim()))
			{
				ModelState.AddModelError(nameof(input.Code), "驗證碼不正確。");
				return View(input);
			}

			pending.ManagerEmailConfirmed = true;
			await _db.SaveChangesAsync();

			// 清 Session
			HttpContext.Session.Remove(OtpSessionUserIdKey);
			HttpContext.Session.Remove(OtpSessionCodeKey);
			HttpContext.Session.Remove(OtpSessionExpireKey);

			await SignInAsync(pending);
			return RedirectToAction(nameof(Success));
		}

		// POST: /Login/Logout
		[HttpPost] // ★★ 移除路由樣板，避免與慣例路由重複造成 AmbiguousMatch
		[IgnoreAntiforgeryToken]
		[AllowAnonymous] // 或移除任何 Authorize
		public async Task<IActionResult> Logout()
		{
			// 1) 後台獨立 Cookie（你登入用的就是這個）
			await HttpContext.SignOutAsync("AdminCookie");
			//// 若你也有前台的 cookie，可一起登出（沒有就註解）
			//await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			// 2) 如有使用 ASP.NET Identity，再逐一登出（存在才登出，避免例外）
			var schemeProvider = HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
			var schemes = await schemeProvider.GetAllSchemesAsync();
			bool Has(string name) => schemes.Any(s => string.Equals(s.Name, name, StringComparison.Ordinal));

			if (Has(IdentityConstants.ApplicationScheme))
				await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
			if (Has(IdentityConstants.ExternalScheme))
				await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
			if (Has(IdentityConstants.TwoFactorUserIdScheme))
				await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
			if (Has(IdentityConstants.TwoFactorRememberMeScheme))
				await HttpContext.SignOutAsync(IdentityConstants.TwoFactorRememberMeScheme);

			//(可選)清 Session
			HttpContext.Session?.Clear();

			return RedirectToAction("Index", "Home", new { area = "" });
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Denied() => View();

		// ========= 顯示登入成功頁 =========
		[Authorize(AuthenticationSchemes = AdminCookieScheme)]
		[HttpGet]
		public IActionResult Success()
		{
			// 直接用 Claims
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
			var name = User.Identity?.Name ?? "";
			var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

			return View("Success", new LoginSuccessVM
			{
				ManagerId = int.TryParse(id, out var x) ? x : 0,
				ManagerName = name,
				Positions = roles
			});
		}

		// ========= 我是誰 / 刷新 Claims =========

		[Authorize(AuthenticationSchemes = AdminCookieScheme)]
		[HttpGet]
		public IActionResult Me()
		{
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
			var name = User.Identity?.Name ?? "";
			var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Distinct().ToList();
			var perms = User.Claims.Where(c => c.Type.StartsWith("perm:", StringComparison.OrdinalIgnoreCase))
								   .ToDictionary(c => c.Type, c => c.Value, StringComparer.OrdinalIgnoreCase);

			return Json(new { authenticated = true, managerId = id, managerName = name, roles, permissions = perms });
		}

		[Authorize(AuthenticationSchemes = AdminCookieScheme)]
		[HttpGet]
		public IActionResult WhoAmI() => Success();

		[Authorize(AuthenticationSchemes = AdminCookieScheme)]
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> RefreshClaims()
		{
			var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(idStr, out var managerId)) return RedirectToAction(nameof(Index));

			var manager = await _db.ManagerData.Include(m => m.ManagerRoles).FirstOrDefaultAsync(m => m.ManagerId == managerId);
			if (manager == null) return RedirectToAction(nameof(Index));

			await SignInAsync(manager); // 重新簽入，刷新 Cookie Claims
			TempData["Msg"] = "已重新整理授權資訊。";
			return RedirectToAction(nameof(WhoAmI));
		}

		// ========= Helper =========

		private async Task SignInAsync(ManagerDatum manager)
		{
			// 確保已載入角色
			if (manager.ManagerRoles == null || manager.ManagerRoles.Count == 0)
				await _db.Entry(manager).Collection(m => m.ManagerRoles).LoadAsync();

			// 角色清單：去空白、去重
			var roleNames = manager.ManagerRoles?
				.Select(r => (r.RoleName ?? string.Empty).Trim())
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList() ?? new List<string>();

			if (roleNames.Count == 0) roleNames.Add("Manager");

			// 基本 Claims
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, manager.ManagerId.ToString()),
				new Claim(ClaimTypes.Name, manager.ManagerName ?? manager.ManagerAccount ?? $"Manager#{manager.ManagerId}"),
				new Claim(ClaimTypes.Email, manager.ManagerEmail ?? string.Empty),
				new Claim("IsManager", "true"),
				new Claim("mgr:id", manager.ManagerId.ToString())
			};

			// 角色 Claims（同時寫入 ClaimTypes.Role 與 "role"）
			foreach (var rn in roleNames)
			{
				claims.Add(new Claim(ClaimTypes.Role, rn));
				claims.Add(new Claim("role", rn)); // 兼容某些只讀 "role" 的元件/UI
			}

			// 布林權限 -> perm:* Claims（任一角色為 true 即給）
			bool canAdmin = manager.ManagerRoles.Any(r => (r.AdministratorPrivilegesManagement ?? false));
			bool canUserStat = manager.ManagerRoles.Any(r => (r.UserStatusManagement ?? false));
			bool canShopping = manager.ManagerRoles.Any(r => (r.ShoppingPermissionManagement ?? false));
			bool canMessage = manager.ManagerRoles.Any(r => (r.MessagePermissionManagement ?? false));
			bool canPet = manager.ManagerRoles.Any(r => (r.PetRightsManagement ?? false));
			bool canCS = manager.ManagerRoles.Any(r => (r.CustomerService ?? false));

			void AddPerm(string key, bool val) { if (val) claims.Add(new Claim("perm:" + key, "true")); }
			AddPerm("Admin", canAdmin);
			AddPerm("UserStat", canUserStat);
			AddPerm("Shopping", canShopping);
			AddPerm("Message", canMessage);
			AddPerm("Pet", canPet);
			AddPerm("CS", canCS);

			// ★ 明確指定 RoleClaimType & NameClaimType
			var identity = new ClaimsIdentity(
				claims,
				AdminCookieScheme,
				ClaimTypes.Name,   // NameClaimType
				ClaimTypes.Role    // RoleClaimType
			);
			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync(
				AdminCookieScheme,
				principal,
				new AuthenticationProperties
				{
					IsPersistent = true,
					ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4)
				});
		}

		private static bool VerifyPassword(ManagerDatum mgr, string? input, string? stored)
		{
			if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(stored)) return false;

			// Identity v3 風格雜湊（常見前綴 "AQAAAA"）
			if (stored.StartsWith("AQAAAA", StringComparison.Ordinal))
			{
				var hasher = new PasswordHasher<ManagerDatum>();
				return hasher.VerifyHashedPassword(mgr, stored, input) != PasswordVerificationResult.Failed;
			}
			// 過渡期：純文字比較
			return string.Equals(stored, input, StringComparison.Ordinal);
		}

		private (string code, DateTime expireAtUtc) GenerateOtp()
		{
			var rnd = new Random();
			return (rnd.Next(0, 999999).ToString("D6"), DateTime.UtcNow.AddMinutes(10));
		}

		private (string? code, DateTime? expireAtUtc) ReadOtpFromSession()
		{
			var code = HttpContext.Session.GetString(OtpSessionCodeKey);
			var ticks = HttpContext.Session.GetString(OtpSessionExpireKey);
			if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(ticks)) return (null, null);
			return long.TryParse(ticks, out var t) ? (code, new DateTime(t, DateTimeKind.Utc)) : (null, null);
		}

		private async Task<ManagerDatum?> GetPendingOtpUserAsync()
		{
			var userIdStr = HttpContext.Session.GetString(OtpSessionUserIdKey);
			return int.TryParse(userIdStr, out var id)
				? await _db.ManagerData.FirstOrDefaultAsync(m => m.ManagerId == id)
				: null;
		}

		private static string MaskEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) return "（無法顯示）";
			var parts = email.Split('@');
			var name = parts[0];
			var domain = parts[1];
			var maskLen = Math.Max(1, name.Length - 2);
			var head = name.Substring(0, Math.Min(1, name.Length));
			var tail = name.Length > 1 ? name[^1..] : "";
			return $"{head}{new string('•', maskLen)}{tail}@{domain}";
		}
	}
}
