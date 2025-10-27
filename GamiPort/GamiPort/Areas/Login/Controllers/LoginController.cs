using GamiPort.Models;
using GamiPort.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // 只用 IPasswordHasher<User> 做雜湊/驗證
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	[Route("[area]/[controller]/[action]")]
	public class LoginController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IPasswordHasher<User> _hasher;

		public LoginController(GameSpacedatabaseContext db, IPasswordHasher<User> hasher)
		{
			_db = db;
			_hasher = hasher;
		}

		// GET: /Login/Login
		[HttpGet("Login")]
		[AllowAnonymous]
		public IActionResult Login(string? returnUrl = null)
			=> View(new LoginVM { ReturnUrl = returnUrl });

		// POST: /Login/Login
		[HttpPost("Login")]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> Login(LoginVM vm)
		{
			// 1) 前端基本驗證
			if (!ModelState.IsValid)
				return View(vm);

			var inputAccount = (vm.UserAccount ?? string.Empty).Trim();

			// 2) 以「大小寫敏感」的方式查帳號
			//    使用 Latin1_General_CS_AS 讓 'UserA' 與 'usera' 視為不同帳號
			var user = await _db.Users
				.Where(u => EF.Functions.Collate(u.UserAccount, "Latin1_General_CS_AS") == inputAccount)
				.SingleOrDefaultAsync();

			if (user == null)
			{
				// 不洩漏是哪一項錯誤（帳號或密碼），提升安全性
				ModelState.AddModelError(nameof(vm.UserAccount), "帳號或密碼錯誤。");
				return View(vm);
			}

			// 3) 密碼驗證：優先嘗試 Identity 格式；若舊資料是明文，允許一次性升級為雜湊
			PasswordVerificationResult verify;
			try
			{
				verify = _hasher.VerifyHashedPassword(user, user.UserPassword, vm.UserPassword);
			}
			catch (FormatException)
			{
				// ⚠️ 舊資料可能是明文；給一次相容：
				if (user.UserPassword == vm.UserPassword)
				{
					verify = PasswordVerificationResult.Success;

					// 立刻升級為雜湊存回資料庫（避免下次再走明文比對）
					user.UserPassword = _hasher.HashPassword(user, vm.UserPassword);
					await _db.SaveChangesAsync();
				}
				else
				{
					verify = PasswordVerificationResult.Failed;
				}
			}

			if (verify == PasswordVerificationResult.Failed)
			{
				ModelState.AddModelError(nameof(vm.UserAccount), "帳號或密碼錯誤。");
				return View(vm);
			}

			// 4) 信箱未驗證不得登入（依你的規則）
			if (!user.UserEmailConfirmed)
			{
				ModelState.AddModelError(string.Empty, "請先完成 Email 驗證後再登入。");
				return View(vm);
			}

			// 5) 取擴充檔（暱稱/信箱）以提供較佳顯示體驗
			var intro = await _db.UserIntroduces
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.UserId == user.UserId);

			var nick = intro?.UserNickName ?? user.UserName; // 暱稱優先，沒有就退回使用者名稱
			var email = intro?.Email ?? string.Empty;

			// 6) 建立 Claims（★重點：補 "AppUserId" 讓 IAppCurrentUser 能直接從 Claims 取得整數 UserId）
			var claims = new List<Claim>
			{
				// 標準識別：NameIdentifier 用「整數字串」存 UserId（部分相容舊程式會直接讀此欄位）
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),

				// 直接把暱稱放到 Name，前端顯示更簡單（例如頁首顯示當前使用者）
				new Claim(ClaimTypes.Name, nick ?? string.Empty),

				// Email（若 intro 沒有，可以是空字串）
				new Claim(ClaimTypes.Email, email),

				// 自訂 Claims（給 UI/服務端使用）
				new Claim("UserNickName", nick ?? string.Empty),
				new Claim("EmailConfirmed", user.UserEmailConfirmed ? "true" : "false"),

				// 你的 IAppCurrentUser 會先讀這顆，再退回 NameIdentifier，再退回 ILoginIdentity（備援）
				new Claim("AppUserId", user.UserId.ToString())
			};

			// 7) 簽發 Cookie（不走 Identity 的 SignInManager；純 Cookie 驗證）
			var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				principal,
				new AuthenticationProperties
				{
					// 記住我：持久化 Cookie；否則為 Session Cookie（瀏覽器關閉即失效）
					IsPersistent = vm.RememberMe,
					// 有勾記住我才指定 ExpiresUtc；否則沿用 Cookie 中的 SlidingExpiration 規則
					ExpiresUtc = vm.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
				});

			// 8) 安全導回（只允許本機相對路徑）
			if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
				return LocalRedirect(vm.ReturnUrl);

			// 9) 預設導回首頁
			return RedirectToAction("Index", "Home", new { area = "" });
		}

		// POST: /Login/Login/Logout
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("/Login/Login/Logout")]
		public async Task<IActionResult> Logout()
		{
			// 登出：清除目前 Cookie
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			// 登出後導回前台首頁（可依需求改為登入頁）
			return RedirectToAction("Index", "Home", new { area = "" });
		}

		[HttpGet]
		public IActionResult Denied() => View();
	}
}
