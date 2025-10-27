using GamiPort.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamiPort.Areas.Member.Controllers
{
	[Area("Member")]
	[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
	public class MemberController : Controller
	{
		private readonly GameSpacedatabaseContext _db;

		public MemberController(GameSpacedatabaseContext db)
		{
			_db = db;
		}

		public async Task<IActionResult> Profile()
		{
			// 這行：抓目前登入者的 UserId（登入時已寫入 Cookie 的 Claim）
			int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			// 查使用者基本資料
			var user = await _db.Users
				.Include(u => u.UserIntroduce)
				.FirstOrDefaultAsync(u => u.UserId == userId);

			if (user == null)
				return NotFound();

			return View(user);
		}
	}
}
