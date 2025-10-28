//using System.Security.Claims;
//using GamiPort.Areas.Login.Services;
//using GamiPort.Models;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace GamiPort.Areas.Login.Controllers
//{
//	[Area("Login")]
//	[Route("[area]/[controller]/[action]")]
//	public class EmailController : Controller
//	{
//		private readonly GameSpacedatabaseContext _db;
//		private readonly IEmailSender _email; // 你自己的寄信服務（前面訊息有 Console 版）

//		private const string ProviderEmail = "Email";
//		private const string NameConfirm = "Confirm";

//		public EmailController(GameSpacedatabaseContext db, IEmailSender email)
//		{
//			_db = db;
//			_email = email;
//		}

//		// GET: /Login/Email/Confirm?uid=10000001&token=xxxx
//		[HttpGet]
//		public async Task<IActionResult> Confirm(int uid, string token)
//		{
//			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == uid);
//			if (user == null) return NotFound();

//			if (user.UserEmailConfirmed)
//				return View("AlreadyConfirmed");

//			// 從 UserTokens 查找有效 Token
//			var now = DateTime.UtcNow;
//			var tokenRow = await _db.UserTokens
//				.Where(t => t.UserId == uid
//							&& t.Provider == ProviderEmail
//							&& t.Name == NameConfirm
//							&& t.Value == token
//							&& t.ExpireAt >= now)
//				.FirstOrDefaultAsync();

//			if (tokenRow == null)
//				return View("ConfirmFailed");

//			// 標記為已驗證，並清光該使用者同類型 Token
//			user.UserEmailConfirmed = true;
//			_db.UserTokens.RemoveRange(
//				_db.UserTokens.Where(t => t.UserId == uid && t.Provider == ProviderEmail && t.Name == NameConfirm)
//			);
//			await _db.SaveChangesAsync();

//			// 若當下已登入 → 刷新 Cookie 的 EmailConfirmed Claim
//			if (User?.Identity?.IsAuthenticated ?? false)
//			{
//				var claims = User.Claims.Where(c => c.Type != "EmailConfirmed").ToList();
//				claims.Add(new Claim("EmailConfirmed", "true"));
//				var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
//			}

//			return View("ConfirmSuccess");
//		}

//		// GET: /Login/Email/Resend  （登入狀態使用）
//		[HttpGet]
//		public async Task<IActionResult> Resend()
//		{
//			if (!(User?.Identity?.IsAuthenticated ?? false))
//				return RedirectToAction("Index", "Login", new { area = "Login" });

//			var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

//			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == uid);
//			var intro = await _db.UserIntroduces.FirstOrDefaultAsync(i => i.UserId == uid);

//			if (user == null || intro == null || string.IsNullOrWhiteSpace(intro.Email))
//				return View("ResendFailed");

//			if (user.UserEmailConfirmed)
//				return View("AlreadyConfirmed");

//			// 刪除舊的 Confirm Token（同一人同類型）
//			var old = _db.UserTokens.Where(t => t.UserId == uid && t.Provider == ProviderEmail && t.Name == NameConfirm);
//			_db.UserTokens.RemoveRange(old);

//			// 建立新的 Token
//			var tokenValue = TokenUtility.NewUrlSafeToken();
//			var tokenRow = new UserToken
//			{
//				UserId = uid,
//				Provider = ProviderEmail,
//				Name = NameConfirm,
//				Value = tokenValue,
//				ExpireAt = DateTime.UtcNow.AddHours(24)
//			};
//			_db.UserTokens.Add(tokenRow);
//			await _db.SaveChangesAsync();

//			// 寄送驗證信
//			var confirmUrl = Url.Action("Confirm", "Email",
//				new { area = "Login", uid = user.UserId, token = tokenValue },
//				Request.Scheme);

//			await _email.SendAsync(intro.Email, "請驗證你的 Email", $"請點擊以下連結完成驗證（24 小時內有效）：\n{confirmUrl}");

//			return View("ResendSuccess");
//		}
//	}

//	// 若你沒有寄信服務，這個介面與 Console 版實作可沿用
//	public interface IEmailSenderInternal
//	{
//		Task SendAsync(string to, string subject, string body);
//	}
//}

using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	[Route("[area]/[controller]/[action]")]
	public class EmailController : Controller
	{
		private readonly GameSpacedatabaseContext _db;

		public EmailController(GameSpacedatabaseContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> Confirm(int uid, string token)
		{
			if (uid <= 0 || string.IsNullOrWhiteSpace(token))
				return View("Invalid");

			var record = await _db.UserTokens
				.FirstOrDefaultAsync(t => t.UserId == uid && t.Provider == "Email" && t.Name == "Confirm");

			if (record == null || record.ExpireAt < DateTime.UtcNow)
				return View("Expired");

			if (record.Value != token)
				return View("Invalid");

			// 驗證成功
			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == uid);
			if (user == null) return View("Invalid");

			user.UserEmailConfirmed = true;
			_db.UserTokens.Remove(record);
			await _db.SaveChangesAsync();

			return View("Success", model: user.UserAccount);
		}

		public IActionResult Success(string id) => View(model: id);
		public IActionResult Invalid() => View();
		public IActionResult Expired() => View();
	}
}
