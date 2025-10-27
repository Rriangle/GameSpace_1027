using GamiPort.Models;
using GamiPort.Areas.Login.Models;
using GamiPort.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	[Authorize]
	public class IntroduceController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ICurrentUserService _current;

		public IntroduceController(GameSpacedatabaseContext db, ICurrentUserService current)
		{
			_db = db;
			_current = current;
		}

		/// <summary>
		/// 我的個人資料（唯讀檢視）
		/// /Login/Introduce/Details
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Details()
		{
			if (!_current.IsAuthenticated)
				return RedirectToAction("Login", "Login", new { area = "Login" });

			var uid = _current.UserId!.Value;

			var entity = await _db.UserIntroduces
				.AsNoTracking()
				.Include(x => x.User)
				.FirstOrDefaultAsync(x => x.UserId == uid);

			if (entity == null)
			{
				TempData["Info"] = "尚未建立個人資料，請先完成編輯。";
				return RedirectToAction(nameof(Edit));
			}

			var vm = ToVM(entity);
			return View(vm);
		}

		/// <summary>
		/// 編輯個人資料（載入）
		/// /Login/Introduce/Edit
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Edit()
		{
			if (!_current.IsAuthenticated)
				return RedirectToAction("Login", "Login", new { area = "Login" });

			var uid = _current.UserId!.Value;

			var entity = await _db.UserIntroduces
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.UserId == uid);

			// 若無記錄，建立一筆初始資料（避免第一次進來無可編輯資料）
			if (entity == null)
			{
				entity = new UserIntroduce
				{
					UserId = uid,
					UserNickName = _current.NickName ?? $"User_{uid}",
					Gender = "M",
					IdNumber = string.Empty,
					Cellphone = string.Empty,
					Email = _current.Email ?? string.Empty,
					Address = string.Empty,
					DateOfBirth = DateOnly.FromDateTime(DateTime.Today),
					CreateAccount = DateTime.UtcNow
				};
				_db.UserIntroduces.Add(entity);
				await _db.SaveChangesAsync();
			}

			var vm = ToVM(entity);
			return View(vm);
		}

		/// <summary>
		/// 編輯個人資料（送出）
		/// 僅允許修改：UserNickName、Gender、Cellphone、UserPicture、Address、UserIntroduce1
		/// 禁止修改：UserId、IdNumber、Email、DateOfBirth、CreateAccount
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(UserIntroduceEditInput input, IFormFile? userPictureFile)
		{
			if (!_current.IsAuthenticated)
				return RedirectToAction("Login", "Login", new { area = "Login" });

			var uid = _current.UserId!.Value;

			var entity = await _db.UserIntroduces.FirstOrDefaultAsync(x => x.UserId == uid);
			if (entity == null)
			{
				ModelState.AddModelError(string.Empty, "找不到個人資料紀錄。");
				return View(await BuildVMForError(uid));
			}

			// 伺服器端保險驗證（Gender 僅允許 M/F）
			if (input.Gender != "M" && input.Gender != "F")
			{
				ModelState.AddModelError(nameof(input.Gender), "性別必須為 M 或 F。");
			}

			// 唯一性：UserNickName（排除自己）
			if (!string.IsNullOrWhiteSpace(input.UserNickName))
			{
				var nickExists = await _db.UserIntroduces
					.AnyAsync(x => x.UserNickName == input.UserNickName && x.UserId != uid);
				if (nickExists)
					ModelState.AddModelError(nameof(input.UserNickName), "此暱稱已被使用。");
			}

			// 唯一性：Cellphone（排除自己）
			if (!string.IsNullOrWhiteSpace(input.Cellphone))
			{
				var phoneExists = await _db.UserIntroduces
					.AnyAsync(x => x.Cellphone == input.Cellphone && x.UserId != uid);
				if (phoneExists)
					ModelState.AddModelError(nameof(input.Cellphone), "此手機號碼已被使用。");
			}

			// 處理頭像（選填）
			byte[]? newPicture = null;
			if (userPictureFile != null && userPictureFile.Length > 0)
			{
				// 可視需求加上大小/副檔名限制（例如 2MB 以內）
				using var ms = new MemoryStream();
				await userPictureFile.CopyToAsync(ms);
				newPicture = ms.ToArray();
			}

			if (!ModelState.IsValid)
			{
				// 回填不可編輯欄位以便顯示
				var vmForReturn = ToVM(entity);
				vmForReturn.UserNickName = input.UserNickName;
				vmForReturn.Gender = input.Gender;
				vmForReturn.Cellphone = input.Cellphone;
				vmForReturn.Address = input.Address;
				vmForReturn.UserIntroduce1 = input.UserIntroduce1;
				return View(vmForReturn);
			}

			// 僅覆寫允許修改的欄位
			entity.UserNickName = input.UserNickName.Trim();
			entity.Gender = input.Gender;
			entity.Cellphone = input.Cellphone.Trim();
			entity.Address = input.Address.Trim();
			entity.UserIntroduce1 = input.UserIntroduce1;

			if (newPicture != null)
				entity.UserPicture = newPicture;

			// 不可修改的欄位：UserId、IdNumber、Email、DateOfBirth、CreateAccount —— 一律不觸碰

			await _db.SaveChangesAsync();

			TempData["Success"] = "個人資料已更新！";
			return RedirectToAction(nameof(Edit));
		}

		// ---------- Private Helpers ----------

		private static UserIntroduceVM ToVM(UserIntroduce e) => new UserIntroduceVM
		{
			UserId = e.UserId,
			UserNickName = e.UserNickName,
			Gender = e.Gender,
			IdNumber = e.IdNumber,
			Cellphone = e.Cellphone,
			Email = e.Email,
			Address = e.Address,
			DateOfBirth = e.DateOfBirth,
			CreateAccount = e.CreateAccount,
			UserPicture = e.UserPicture,
			UserIntroduce1 = e.UserIntroduce1,
			User = e.User
		};

		private async Task<UserIntroduceVM> BuildVMForError(int uid)
		{
			var fallback = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == uid);
			return new UserIntroduceVM
			{
				UserId = uid,
				UserNickName = _current.NickName ?? $"User_{uid}",
				Gender = "M",
				Cellphone = "",
				Email = _current.Email ?? (fallback?.UserName ?? ""),
				Address = "",
				DateOfBirth = DateOnly.FromDateTime(DateTime.Today),
				CreateAccount = DateTime.UtcNow
			};
		}
	}
}