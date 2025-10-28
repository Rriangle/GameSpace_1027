using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MemberManagement.Models;

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	[Route("MemberManagement/[controller]")] // /MemberManagement/ManagerData
	public class ManagerDataController : Controller
	{
		private readonly GameSpacedatabaseContext _context;

		public ManagerDataController(GameSpacedatabaseContext context)
		{
			_context = context;
		}

		// GET /MemberManagement/ManagerData
		[HttpGet("")]
		public async Task<IActionResult> Index(int? managerId, string? managerName,
											   string? sortBy, string? sortDir,
											   string sidebar = "admin")
		{
			sortBy = (sortBy ?? "id").ToLower();
			sortDir = (sortDir ?? "asc").ToLower();

			ViewBag.Sidebar = sidebar ?? "admin";
			ViewBag.ManagerId = managerId;
			ViewBag.ManagerName = managerName;
			ViewBag.SortBy = sortBy;
			ViewBag.SortDir = sortDir;

			var q = _context.ManagerData.AsNoTracking().AsQueryable();

			if (managerId.HasValue)
				q = q.Where(m => m.ManagerId == managerId.Value);

			if (!string.IsNullOrWhiteSpace(managerName))
			{
				var key = managerName.Trim();
				q = q.Where(m => EF.Functions.Like(m.ManagerName!, $"%{key}%"));
			}

			var asc = sortDir != "desc";
			switch (sortBy)
			{
				case "date":
					q = asc
						? q.OrderBy(m => m.AdministratorRegistrationDate).ThenBy(m => m.ManagerId)
						: q.OrderByDescending(m => m.AdministratorRegistrationDate).ThenByDescending(m => m.ManagerId);
					break;
				case "id":
				default:
					q = asc ? q.OrderBy(m => m.ManagerId) : q.OrderByDescending(m => m.ManagerId);
					break;
			}

			var vms = await q
				.Select(m => new ManagerDatumVM
				{
					ManagerId = m.ManagerId,
					ManagerName = m.ManagerName,
					ManagerAccount = m.ManagerAccount,
					// 為避免洩漏，列表不帶出密碼
					ManagerPassword = null,
					AdministratorRegistrationDate = m.AdministratorRegistrationDate,
					ManagerEmail = m.ManagerEmail,
					ManagerEmailConfirmed = m.ManagerEmailConfirmed,
					ManagerAccessFailedCount = m.ManagerAccessFailedCount,
					ManagerLockoutEnabled = m.ManagerLockoutEnabled,
					ManagerLockoutEnd = m.ManagerLockoutEnd
				})
				.ToListAsync();

			return View(vms);
		}

		// GET /MemberManagement/ManagerData/Details/5
		[HttpGet("Details/{id:int}")]
		public async Task<IActionResult> Details(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			if (id == null) return NotFound();

			var e = await _context.ManagerData.FirstOrDefaultAsync(x => x.ManagerId == id);
			if (e == null) return NotFound();

			return View(ToVM(e));
		}

		// GET /MemberManagement/ManagerData/Create
		[HttpGet("Create")]
		public async Task<IActionResult> Create(string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
			return View(new ManagerDatumVM());
		}

		// POST /MemberManagement/ManagerData/Create
		[HttpPost("Create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("ManagerName,ManagerAccount,ManagerPassword,ManagerEmail")] ManagerDatumVM vm,
			string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			if (!ModelState.IsValid)
			{
				ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
				return View(vm);
			}

			var name = vm.ManagerName?.Trim() ?? string.Empty;
			var account = vm.ManagerAccount?.Trim() ?? string.Empty;
			var password = vm.ManagerPassword?.Trim() ?? string.Empty;
			var email = vm.ManagerEmail?.Trim() ?? string.Empty;

			// 基本長度檢查（與 VM 對齊，雙保險）
			if (account.Length < 8)
				ModelState.AddModelError(nameof(vm.ManagerAccount), "帳號至少需要 8 碼。");
			if (password.Length < 8)
				ModelState.AddModelError(nameof(vm.ManagerPassword), "密碼至少需要 8 碼。");

			if (!ModelState.IsValid)
			{
				ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
				return View(vm);
			}

			// 重複檢查（姓名/帳號/信箱）
			var duplicates = await _context.ManagerData
				.Where(m => m.ManagerName == name || m.ManagerAccount == account || m.ManagerPassword ==password || m.ManagerEmail == email)
				.Select(m => new { m.ManagerName, m.ManagerAccount, m.ManagerEmail })
				.ToListAsync();

			if (duplicates.Any(d => string.Equals(d.ManagerName, name, StringComparison.OrdinalIgnoreCase)))
				ModelState.AddModelError(nameof(vm.ManagerName), "此管理者姓名已存在，請更換。");
			if (duplicates.Any(d => string.Equals(d.ManagerAccount, account, StringComparison.OrdinalIgnoreCase)))
				ModelState.AddModelError(nameof(vm.ManagerAccount), "此帳號已存在，請更換。");
			if (duplicates.Any(d => string.Equals(d.ManagerEmail, email, StringComparison.OrdinalIgnoreCase)))
				ModelState.AddModelError(nameof(vm.ManagerEmail), "此信箱已存在，請更換。");

			if (!ModelState.IsValid)
			{
				ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
				return View(vm);
			}

			// 產生唯一的 ManagerId，若主鍵衝突則重試（避免並發）
			for (int attempt = 0; attempt < 3; attempt++)
			{
				var newId = await GenerateNextManagerIdAsync();

				var e = new ManagerDatum
				{
					ManagerId = newId,
					ManagerName = name,
					ManagerAccount = account,
					ManagerPassword = password, // 用修剪後的值
					AdministratorRegistrationDate = DateTime.Now,
					ManagerEmail = email,
					ManagerEmailConfirmed = false,
					ManagerAccessFailedCount = 0,
					ManagerLockoutEnabled = false,
					ManagerLockoutEnd = null
				};

				_context.ManagerData.Add(e);

				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
				}
				catch (DbUpdateException ex)
				{
					var msg = ex.InnerException?.Message ?? ex.Message;
					var duplicateKey =
						msg.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) ||
						msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
						msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase);

					_context.Entry(e).State = EntityState.Detached;

					if (duplicateKey)
					{
						if (msg.Contains("ManagerAccount", StringComparison.OrdinalIgnoreCase))
							ModelState.AddModelError(nameof(vm.ManagerAccount), "此帳號已存在，請更換。");
						else if (msg.Contains("ManagerName", StringComparison.OrdinalIgnoreCase))
							ModelState.AddModelError(nameof(vm.ManagerName), "此管理者姓名已存在，請更換。");
						else if (msg.Contains("ManagerEmail", StringComparison.OrdinalIgnoreCase))
							ModelState.AddModelError(nameof(vm.ManagerEmail), "此信箱已存在，請更換。");
						else
							ModelState.AddModelError(string.Empty, "資料重複，請檢查輸入內容。");

						ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
						return View(vm);
					}

					ModelState.AddModelError(string.Empty, msg);
					ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
					return View(vm);
				}
			}

			ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
			ModelState.AddModelError(string.Empty, "建立失敗，請稍後再試。");
			return View(vm);
		}

		// 產號
		private async Task<int> GenerateNextManagerIdAsync()
		{
			const int start = 30000001;
			var maxId = await _context.ManagerData.MaxAsync(m => (int?)m.ManagerId) ?? 0;
			var candidate = Math.Max(start, maxId + 1); // <-- NOTE: C# uses Math.Max, not Math.max; fix below
			while (await _context.ManagerData.AnyAsync(m => m.ManagerId == candidate))
				candidate++;
			return candidate;
		}

		// GET /MemberManagement/ManagerData/Edit/5
		[HttpGet("Edit/{id:int}")]
		public async Task<IActionResult> Edit(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			if (id == null) return NotFound();
			var e = await _context.ManagerData.FindAsync(id);
			if (e == null) return NotFound();
			return View(ToVM(e));
		}

		// POST /MemberManagement/ManagerData/Edit/5
		[HttpPost("Edit/{id:int}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ManagerDatumVM vm, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			if (id != vm.ManagerId) return NotFound();

			if (!ModelState.IsValid) return View(vm);

			var name = vm.ManagerName?.Trim() ?? string.Empty;
			var account = vm.ManagerAccount?.Trim() ?? string.Empty;
			var password = vm.ManagerPassword?.Trim() ?? string.Empty;
			var email = vm.ManagerEmail?.Trim() ?? string.Empty;

			if (account.Length < 8)
				ModelState.AddModelError(nameof(vm.ManagerAccount), "帳號至少需要 8 碼。");
			if (password.Length < 8)
				ModelState.AddModelError(nameof(vm.ManagerPassword), "密碼至少需要 8 碼。");

			// 重複檢查（排除自己）
			var dupEdit = await _context.ManagerData
				.Where(m => m.ManagerId != id &&
							(m.ManagerName == name || m.ManagerAccount == account || m.ManagerPassword == email || m.ManagerEmail == email))
				.Select(m => new { m.ManagerName, m.ManagerAccount, m.ManagerEmail })
				.ToListAsync();

			if (dupEdit.Any(d => string.Equals(d.ManagerName, name, StringComparison.OrdinalIgnoreCase)))
				ModelState.AddModelError(nameof(vm.ManagerName), "此管理者姓名已存在，請更換。");
			if (dupEdit.Any(d => string.Equals(d.ManagerAccount, account, StringComparison.OrdinalIgnoreCase)))
				ModelState.AddModelError(nameof(vm.ManagerAccount), "此帳號已存在，請更換。");

			if (dupEdit.Any(d => string.Equals(d.ManagerEmail, email, StringComparison.OrdinalIgnoreCase)))
				ModelState.AddModelError(nameof(vm.ManagerEmail), "此信箱已存在，請更換。");

			if (!ModelState.IsValid) return View(vm);

			var e = await _context.ManagerData.FindAsync(id);
			if (e == null) return NotFound();

			e.ManagerName = name;
			e.ManagerAccount = account;
			e.ManagerPassword = password;
			e.ManagerEmail = email;
			e.AdministratorRegistrationDate = vm.AdministratorRegistrationDate;
			e.ManagerEmailConfirmed = vm.ManagerEmailConfirmed;
			e.ManagerAccessFailedCount = vm.ManagerAccessFailedCount;
			e.ManagerLockoutEnabled = vm.ManagerLockoutEnabled;
			e.ManagerLockoutEnd = vm.ManagerLockoutEnd;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException ex)
			{
				ModelState.AddModelError(string.Empty, ex.InnerException?.Message ?? ex.Message);
				return View(vm);
			}

			return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
		}

		// GET /MemberManagement/ManagerData/Delete/5
		[HttpGet("Delete/{id:int}")]
		public async Task<IActionResult> Delete(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			if (id == null) return NotFound();

			var e = await _context.ManagerData.FirstOrDefaultAsync(x => x.ManagerId == id);
			if (e == null) return NotFound();

			return View(ToVM(e));
		}

		// POST /MemberManagement/ManagerData/Delete/5
		[HttpPost("Delete/{ManagerId:int}")]
		[ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int ManagerId, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			var entity = await _context.ManagerData.FindAsync(ManagerId);
			if (entity == null) return NotFound();

			_context.ManagerData.Remove(entity);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
		}

		private static ManagerDatumVM ToVM(ManagerDatum m) => new ManagerDatumVM
		{
			ManagerId = m.ManagerId,
			ManagerName = m.ManagerName,
			ManagerAccount = m.ManagerAccount,
			ManagerPassword = m.ManagerPassword, // 視需要在 View 隱藏
			AdministratorRegistrationDate = m.AdministratorRegistrationDate,
			ManagerEmail = m.ManagerEmail,
			ManagerEmailConfirmed = m.ManagerEmailConfirmed,
			ManagerAccessFailedCount = m.ManagerAccessFailedCount,
			ManagerLockoutEnabled = m.ManagerLockoutEnabled,
			ManagerLockoutEnd = m.ManagerLockoutEnd
		};

		private static ManagerDatum ToEntity(ManagerDatumVM vm) => new ManagerDatum
		{
			ManagerId = vm.ManagerId,
			ManagerName = vm.ManagerName,
			ManagerAccount = vm.ManagerAccount,
			ManagerPassword = vm.ManagerPassword,
			AdministratorRegistrationDate = vm.AdministratorRegistrationDate,
			ManagerEmail = vm.ManagerEmail,
			ManagerEmailConfirmed = vm.ManagerEmailConfirmed,
			ManagerAccessFailedCount = vm.ManagerAccessFailedCount,
			ManagerLockoutEnabled = vm.ManagerLockoutEnabled,
			ManagerLockoutEnd = vm.ManagerLockoutEnd
		};
	}
}
