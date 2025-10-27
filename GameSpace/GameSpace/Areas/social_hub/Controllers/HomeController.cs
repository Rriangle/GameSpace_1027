// Areas/social_hub/Controllers/HomeController.cs
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Infrastructure.Login;        // ILoginIdentity
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;             // ⬅ CookieOptions
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[SocialHubAuth]
	public class HomeController : Controller
	{
		private readonly ILoginIdentity _login;
		private const string AuthScheme = "AdminCookie"; // 與 Program.cs 一致

		public HomeController(ILoginIdentity login) => _login = login;

		[HttpGet, AllowAnonymous]
		public IActionResult Index() => RedirectToAction(nameof(WhoAmI));

		[HttpGet, AllowAnonymous]
		public IActionResult Login(string? returnUrl = null) => RedirectToAction(nameof(WhoAmI));

		// ========== 共用：把 id/kind 種成相容 cookies（給舊前端/外部模組讀） ==========
		private void SetCompatCookies(string kind, int id, bool keep)
		{
			// 舊前端是用 gs_id / gs_kind；存活時間與 AdminCookie 同步
			var expires = keep ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(4);

			// 若前端 JS 會讀取，就不要設 HttpOnly；若不需要可改成 true。
			var opts = new CookieOptions
			{
				HttpOnly = false,
				IsEssential = true,
				SameSite = SameSiteMode.Lax,          // 你目前同站開發足夠；跨站可改 None + Secure
				Secure = Request.IsHttps,             // 本地 http:false；上線 https:true
				Expires = expires
			};

			Response.Cookies.Append("gs_id", id.ToString(), opts);
			Response.Cookies.Append("gs_kind", kind.ToLowerInvariant(), opts);
		}

		private void ClearCompatCookies()
		{
			Response.Cookies.Delete("gs_id");
			Response.Cookies.Delete("gs_kind");
		}

		// ========== 共用：簽入（user / manager） ==========
		private async Task SignInAsAsync(string kind, int id, bool persistent = false)
		{
			// 先登出舊的
			try { await HttpContext.SignOutAsync(AuthScheme); } catch { /* ignore */ }
			try { await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); } catch { /* ignore */ }

			var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, id.ToString()) };
			if (string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase))
				claims.Add(new Claim("IsManager", "true"));          // SocialHubAuth 用這個判斷 manager
			claims.Add(new Claim($"sh:{kind}:id", id.ToString()));   // 方便除錯

			var identity = new ClaimsIdentity(claims, AuthScheme);
			var principal = new ClaimsPrincipal(identity);
			var props = new AuthenticationProperties
			{
				IsPersistent = persistent,
				ExpiresUtc = persistent ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(4)
			};

			await HttpContext.SignInAsync(AuthScheme, principal, props);

			// ★ 簽完 AdminCookie，順手種舊制 cookies（給外部/舊前端使用）
			SetCompatCookies(kind, id, keep: persistent);
		}

		// ========== 快速登入（唯一入口） ==========
		// 例：/social_hub/Home/QuickLogin?kind=user&id=10000012
		//     /social_hub/Home/QuickLogin?kind=manager&id=30000001
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> QuickLogin(string kind, int id, bool keep = false)
		{
			if (!string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase))
				return BadRequest("kind 只能是 user 或 manager");
			if (id <= 0) return BadRequest("id 必須為正整數。");

			await SignInAsAsync(kind.ToLowerInvariant(), id, persistent: keep);
			TempData["msg"] = $"Login OK：{kind} #{id}";
			return RedirectToAction(nameof(WhoAmI));
		}

		// ========== 登出 ==========
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> Logout()
		{
			try { await HttpContext.SignOutAsync(AuthScheme); } catch { /* ignore */ }
			try { await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); } catch { /* ignore */ }
			ClearCompatCookies(); // ★ 一併清掉相容 cookies
			TempData["msg"] = "已登出。";
			return RedirectToAction(nameof(WhoAmI));
		}

		// ========== 我是誰（顯示 + 補強相容狀態） ==========
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> WhoAmI()
		{
			var me = await _login.GetAsync();

			// 回填 Items（這是「當次請求可用」；舊程式若用 Items 讀，這次請求就有值）
			if (me.IsAuthenticated)
			{
				if (HttpContext.Items["gs_id"] is null) HttpContext.Items["gs_id"] = me.EffectiveId;
				if (HttpContext.Items["gs_kind"] is null) HttpContext.Items["gs_kind"] = me.Kind ?? "";
			}

			// ★ 若 cookies 尚未寫入或與身分不一致 → 在 WhoAmI 頁也補一次
			if (me.IsAuthenticated)
			{
				var cid = Request.Cookies["gs_id"];
				var ckind = Request.Cookies["gs_kind"];
				var needWrite =
					!int.TryParse(cid, out var rid) || rid != me.EffectiveId ||
					string.IsNullOrWhiteSpace(ckind) || !string.Equals(ckind, me.Kind, StringComparison.OrdinalIgnoreCase);

				if (needWrite)
				{
					// keep=false：這裡用短時效；若想長期記住請用 QuickLogin?keep=true
					SetCompatCookies(me.Kind ?? "user", me.EffectiveId, keep: false);
				}
			}

			ViewData["msg"] = TempData["msg"] as string ?? "";
			ViewData["kind"] = me.Kind ?? "(未設定)";
			ViewData["id"] = me.IsAuthenticated ? me.EffectiveId.ToString() : "（未登入）";
			return View(); // 對應 WhoAmI.cshtml
		}
	}
}
