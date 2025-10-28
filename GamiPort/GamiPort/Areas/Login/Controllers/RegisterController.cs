using GamiPort.Areas.Login.Models;
// ★ Token 與寄信
using GamiPort.Areas.Login.Services;                 // TokenUtility.NewUrlSafeToken()
using GamiPort.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	[Route("[area]/[controller]/[action]")]
	public class RegisterController : Controller
	{
		private const string Step1TempKey = "Register_Step1";

		private readonly GameSpacedatabaseContext _db;
		private readonly IPasswordHasher<User> _hasher;
		private readonly IEmailSender _email;

		public RegisterController(GameSpacedatabaseContext db, IPasswordHasher<User> hasher, IEmailSender emailSender)
		{
			_db = db;
			_hasher = hasher;
			_email = emailSender;
		}

		// -------- 第一步：帳號資訊 (GET) --------
		[HttpGet]
		public IActionResult Step1()
		{
			RegisterStep1VM vm = new();

			// 若剛剛從 Step2/交易錯誤被導回，回填資料並顯示錯誤
			if (TempData.TryGetValue(Step1TempKey, out var json) && json is string s && !string.IsNullOrWhiteSpace(s))
			{
				try { vm = JsonSerializer.Deserialize<RegisterStep1VM>(s) ?? new(); }
				catch { vm = new(); }
				TempData[Step1TempKey] = s; // 放回去，避免下一次取不到
			}
			if (TempData.TryGetValue("Step1Error", out var err) && err is string msg && !string.IsNullOrWhiteSpace(msg))
			{
				ViewBag.Step1Error = msg; // 你可在 View 用 alert 顯示
			}

			return View(vm);
		}

		// -------- 第一步：帳號資訊 (POST) --------
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<IActionResult> Step1(RegisterStep1VM vm)
		{
			if (!ModelState.IsValid) return View(vm);

			// 正規化
			vm.UserName = vm.UserName?.Trim() ?? string.Empty;
			vm.UserAccount = vm.UserAccount?.Trim() ?? string.Empty;
			vm.UserPassword = vm.UserPassword?.Trim() ?? string.Empty;
			vm.ConfirmPassword = vm.ConfirmPassword?.Trim() ?? string.Empty;

			// 密碼規則
			if (vm.UserPassword.Length < 8)
			{
				ModelState.AddModelError(nameof(vm.UserPassword), "密碼至少 8 碼");
				return View(vm);
			}
			if (!string.Equals(vm.UserPassword, vm.ConfirmPassword))
			{
				ModelState.AddModelError(nameof(vm.ConfirmPassword), "兩次輸入的密碼不一致");
				return View(vm);
			}

			// 唯一性（Step1 當下就擋）
			if (await _db.Users.AnyAsync(u => EF.Functions.Collate(u.UserName, "Latin1_General_CS_AS") == vm.UserName.Trim()))
			{
				ModelState.AddModelError(nameof(vm.UserName), "此使用者名稱已被使用");
				return View(vm);
			}



			if (await _db.Users.AnyAsync(u => EF.Functions.Collate(u.UserAccount, "Latin1_General_CS_AS") == vm.UserAccount.Trim()))
			{
				ModelState.AddModelError(nameof(vm.UserAccount), "此帳號已被使用");
				return View(vm);
			}

			// 進入 Step2
			TempData[Step1TempKey] = JsonSerializer.Serialize(vm);
			return RedirectToAction(nameof(Step2));
		}

		// -------- 第二步：個資 (GET) --------
		[HttpGet]
		public IActionResult Step2()
		{
			if (!TempData.TryGetValue(Step1TempKey, out var json))
				return RedirectToAction(nameof(Step1));

			// 放回去，讓 POST 還取得到
			TempData[Step1TempKey] = json;
			if (TempData.TryGetValue("Step2Error", out var err) && err is string e && !string.IsNullOrWhiteSpace(e))
				ViewBag.Step2Error = err;
			var vm = new UserIntroduceVM { DateOfBirth = DateOnly.FromDateTime(DateTime.Today) };
			return View(vm);
		}

		// -------- 第二步：個資 (POST) --------
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<IActionResult> Step2(
			[Bind("UserNickName,Gender,IdNumber,Cellphone,Email,Address,DateOfBirth,UserIntroduce1")]
			UserIntroduceVM vm,
			IFormFile? UserPictureFile)
		{
			if (!TempData.TryGetValue(Step1TempKey, out var json))
				return RedirectToAction(nameof(Step1));

			// 還原 Step1
			var step1 = JsonSerializer.Deserialize<RegisterStep1VM>(json!.ToString()!)!;
			TempData[Step1TempKey] = JsonSerializer.Serialize(step1); // 若驗證失敗可再回填

			// 上傳圖檔
			if (UserPictureFile is not null && UserPictureFile.Length > 0)
				vm.UserPicture = await ReadFileBytesAsync(UserPictureFile);

			// 若你的 ModelState 曾因陰影屬性多出鍵值，可移除
			ModelState.Remove("User");
			ModelState.Remove("User.UserId");
			ModelState.Remove("UserId");

			if (!ModelState.IsValid) return View(vm);

			// Step2 欄位唯一性（Email/手機）
			if(await _db.UserIntroduces.AnyAsync(x => x.UserNickName == vm.UserNickName.Trim()))
			{ ModelState.AddModelError(nameof(vm.UserNickName), "此 暱稱 已被使用"); return View(vm); }

			if (await _db.UserIntroduces.AnyAsync(x => x.IdNumber == vm.IdNumber.Trim()))
			{ ModelState.AddModelError(nameof(vm.IdNumber), "此 身分證字號 已被使用"); return View(vm); }

			if (await _db.UserIntroduces.AnyAsync(x => x.Email == vm.Email.Trim()))
			{ ModelState.AddModelError(nameof(vm.Email), "此 Email 已被使用"); return View(vm); }

			if (await _db.UserIntroduces.AnyAsync(x => x.Cellphone == vm.Cellphone.Trim()))
			{ ModelState.AddModelError(nameof(vm.Cellphone), "此 手機號碼 已被使用"); return View(vm); }

			try
			{
				// ⚡ 原子寫入：同一交易、一次 SaveChanges（含：User / UserIntroduce / UserToken）
				var result = await CreateUserAtomicAsync(step1, vm);

				// 交易成功 → 組驗證連結並寄信
				var confirmUrl = Url.Action(
					"Confirm", "Email",
					new { area = "Login", uid = result.UserId, token = result.EmailConfirmToken },
					Request.Scheme
				);

				await _email.SendAsync(vm.Email.Trim(), "請驗證你的 Email",
					$"請點擊以下連結完成驗證（24 小時內有效）：\n{confirmUrl}");

				return RedirectToAction(nameof(Success), new { id = result.UserId });
			}
			// 最後保險：任何唯一性衝突（帳號/名稱…）都回 Step1；交易已回滾，不會殘留半套資料
			catch (DbUpdateException ex) when (IsUniqueViolation(ex))
			{
				TempData["Step2Error"] = "個資或帳號資訊發生唯一性衝突。詳細：" + (ex.InnerException?.Message ?? ex.Message);
				TempData[Step1TempKey] = JsonSerializer.Serialize(step1);
				return RedirectToAction(nameof(Step2));
			}
			catch (DbUpdateException ex)
			{
				TempData["Step2Error"] = ex.InnerException?.Message ?? ex.Message;
				TempData[Step1TempKey] = JsonSerializer.Serialize(step1);
				return RedirectToAction(nameof(Step2));
			}
		}

		// -------- 完成頁 --------
		[HttpGet]
		public IActionResult Success(int id) => View(model: id);

		// -------- 測試寄信功能 --------
		[HttpGet]
		public async Task<IActionResult> TestEmail()
		{
			try
			{
				// 看一下現在注入進來的是哪個實作（SmtpEmailSender / NullEmailSender）
				var impl = _email.GetType().FullName ?? "(unknown)";

				// 模擬驗證信連結（這裡只是範例）
				var confirmUrl = Url.Action(
					"Confirm", "Email",
					new { area = "Login", uid = 10000001, token = "sampleToken" },
					Request.Scheme
				);

				// 寄送測試信
				await _email.SendAsync(
					"aa0953693201@gmail.com", // ← 你的收件信箱
					"請驗證你的 Email",
					$"請點擊以下連結完成驗證（24 小時內有效）：\n{confirmUrl}"
				);

				return Content($"✅ 測試信件已寄出（使用：{impl}）。請檢查收件匣/垃圾信/寄件備份。");
			}
			catch (Exception ex)
			{
				// 若寄信失敗，把錯誤顯示出來
				return Content("❌ 寄信失敗：\n" + ex.ToString());
			}
		}


		// ===== Helper：單一交易 + 一次 SaveChanges，兩表同時成功/失敗 =====
		private async Task<(int UserId, string EmailConfirmToken)> CreateUserAtomicAsync(RegisterStep1VM step1, UserIntroduceVM step2)
		{
			await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
			try
			{
				// User（UserId 由 DB 產生）
				var user = new User
				{
					UserName = step1.UserName.Trim(),
					UserAccount = step1.UserAccount.Trim(),
					UserPassword = "placeholder",
					UserEmailConfirmed = false,
					UserPhoneNumberConfirmed = false,
					UserTwoFactorEnabled = false,
					UserAccessFailedCount = 0,
					UserLockoutEnabled = false,
					CreateAccount = DateTime.UtcNow
				};
				user.UserPassword = _hasher.HashPassword(user, step1.UserPassword);

				// UserIntroduce（關聯帶 FK）
				var introduce = new UserIntroduce
				{
					UserNickName = step2.UserNickName.Trim(),
					Gender = step2.Gender,
					IdNumber = step2.IdNumber.Trim(),
					Cellphone = step2.Cellphone.Trim(),
					Email = step2.Email.Trim(),
					Address = step2.Address.Trim(),
					DateOfBirth = step2.DateOfBirth,
					CreateAccount = DateTime.UtcNow,
					UserIntroduce1 = step2.UserIntroduce1,
					UserPicture = step2.UserPicture,
					User = user
				};

				_db.AddRange(user, introduce);

				// ★ 建立 Email 驗證 Token（同交易）
				// 先清理（理論上新使用者不會有，但保險）
				var provider = "Email";
				var name = "Confirm";
				// 此時 user.UserId 還未有值，先 SaveChanges 再插 token？→ 交由 EF 追蹤關聯也可以。
				// 我們先 SaveChanges 再插 token，確保有 UserId。
				await _db.SaveChangesAsync();

				// 刪除同使用者同類型舊 token（保險）
				var olds = _db.UserTokens.Where(t => t.UserId == user.UserId && t.Provider == provider && t.Name == name);
				_db.UserTokens.RemoveRange(olds);

				var emailToken = TokenUtility.NewUrlSafeToken();
				var tokenRow = new UserToken
				{
					UserId = user.UserId,
					Provider = provider,
					Name = name,
					Value = emailToken,
					ExpireAt = DateTime.UtcNow.AddHours(24)
				};
				_db.UserTokens.Add(tokenRow);

				await _db.SaveChangesAsync();
				await tx.CommitAsync();

				return (user.UserId, emailToken);
			}
			catch
			{
				await tx.RollbackAsync();
				throw;
			}
		}

		private static bool IsUniqueViolation(DbUpdateException ex)
		{
			if (ex.InnerException is SqlException sql)
				return sql.Number == 2627 || sql.Number == 2601; // UNIQUE/PK/索引衝突
			return false;
		}

		private static async Task<byte[]> ReadFileBytesAsync(IFormFile file)
		{
			using var ms = new MemoryStream();
			await file.CopyToAsync(ms);
			return ms.ToArray();
		}


	}
}
