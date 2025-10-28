using GamiPort.Areas.Login.Models;
using GamiPort.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamiPort.Services;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	[Authorize]
	public class PasswordController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IPasswordHasher<User> _hasher;
		private readonly ICurrentUserService _current;

		public PasswordController(GameSpacedatabaseContext db, IPasswordHasher<User> hasher, ICurrentUserService current)
		{
			_db = db;
			_hasher = hasher;
			_current = current;
		}

		// 顯示修改頁面
		[HttpGet]
		public async Task<IActionResult> Edit()
		{
			if (!_current.IsAuthenticated)
				return RedirectToAction("Login", "Login");

			var userId = _current.UserId; // ← 直接抓目前登入的 UserId
			var user = await _db.Users.FindAsync(userId);


			if (user == null)
				return NotFound();

			ViewData["UserAccount"] = user.UserAccount;
			return View(new ChangePasswordVM());
		}

		// 處理修改密碼請求
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<IActionResult> Edit(ChangePasswordVM vm)
		{
			if (!_current.IsAuthenticated)
				return RedirectToAction("Login", "Login");

			var userId = _current.UserId;
			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

			if (user == null)
				return NotFound();
			// ★ 無論成功或失敗，都先把帳號放回去給 View 使用
			ViewData["UserAccount"] = user.UserAccount;

			var verify = _hasher.VerifyHashedPassword(user, user.UserPassword, vm.OldPassword);
			if (verify == PasswordVerificationResult.Failed)
			{
				ModelState.AddModelError(nameof(vm.OldPassword), "舊密碼錯誤");
				return View(vm);
			}

			user.UserPassword = _hasher.HashPassword(user, vm.NewPassword);
			user.UserAccessFailedCount = 0;

			await _db.SaveChangesAsync();

			TempData["Success"] = "密碼已成功變更！";
			return RedirectToAction("Edit");
		}
	}
}
