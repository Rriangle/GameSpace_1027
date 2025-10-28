using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models; // GameSpacedatabaseContext, ManagerDatum, ManagerRolePermission
using GameSpace.Areas.MemberManagement.Models; // ManagerWithRoleVM, ManagerRoleAssignVM

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	public class ManagerWithRolesController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		public ManagerWithRolesController(GameSpacedatabaseContext db) => _db = db;

		// 只接 ManagerRole 的兩個欄位（不拿名稱）
		private sealed class ManagerRoleRow
		{
			public int ManagerId { get; set; }      // Manager_Id
			public int ManagerRoleId { get; set; }  // ManagerRole_Id
		}

		// ========== Index ==========
		// 支援搜尋：managerId、managerName（模糊）、roleName（模糊）
		// 支援排序：sortBy = id | role，sortDir = asc | desc
		public async Task<IActionResult> Index(
			int? managerId,
			string? managerName,
			string? roleName,
			string? sortBy,
			string? sortDir,
			string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			sortBy = (sortBy ?? "id").ToLower();
			sortDir = (sortDir ?? "asc").ToLower();

			// 暴露給 View 保留查詢/排序狀態
			ViewBag.ManagerId = managerId;
			ViewBag.ManagerName = managerName;
			ViewBag.RoleName = roleName;
			ViewBag.SortBy = sortBy;
			ViewBag.SortDir = sortDir;

			// 1) 先取 Manager 基本資料 +（套用 ManagerId / ManagerName 篩選）
			var nameKey = (managerName ?? string.Empty).Trim();
			var mgrQuery = _db.ManagerData.AsNoTracking().AsQueryable();

			if (managerId.HasValue)
				mgrQuery = mgrQuery.Where(m => m.ManagerId == managerId.Value);

			if (!string.IsNullOrWhiteSpace(nameKey))
				mgrQuery = mgrQuery.Where(m => EF.Functions.Like(m.ManagerName!, $"%{nameKey}%"));

			var managers = await mgrQuery
				.Select(m => new { m.ManagerId, m.ManagerName })
				.ToListAsync();

			// 若沒有符合的管理者，直接回傳空集合
			if (managers.Count == 0)
				return View(Enumerable.Empty<ManagerWithRoleVM>());

			// 2) 取角色字典（Id -> Name），並依 roleName 產出過濾用的 RoleId 集合
			var roleDict = await _db.ManagerRolePermissions
				.AsNoTracking()
				.Select(r => new { r.ManagerRoleId, r.RoleName })
				.ToDictionaryAsync(x => x.ManagerRoleId, x => x.RoleName);

			HashSet<int>? roleIdFilter = null;
			bool filterNoRole = false;
			var roleKey = (roleName ?? string.Empty).Trim();
			if (!string.IsNullOrWhiteSpace(roleKey))
			{
				// 角色名稱模糊比對（大小寫不敏感）
				roleIdFilter = roleDict
					.Where(kv => kv.Value != null && kv.Value.ToLower().Contains(roleKey.ToLower()))
					.Select(kv => kv.Key)
					.ToHashSet();

				// 若使用者明確搜尋 "(無角色)"，同時啟用無角色過濾
				if ("(無角色)".Contains(roleKey))
					filterNoRole = true;

				// 若 keyword 完全沒有符合任何角色名稱，且沒要求無角色，提早回空
				if (roleIdFilter.Count == 0 && !filterNoRole)
					return View(Enumerable.Empty<ManagerWithRoleVM>());
			}

			// 3) 取 ManagerRole 連結（僅抓本次 managers 相關的）
			var managerIdSet = managers.Select(m => m.ManagerId).ToHashSet();
			var allLinks = await _db.Database.SqlQuery<ManagerRoleRow>($@"
                SELECT mr.Manager_Id     AS ManagerId,
                       mr.ManagerRole_Id AS ManagerRoleId
                FROM dbo.ManagerRole AS mr
            ").ToListAsync();

			var links = allLinks.Where(l => managerIdSet.Contains(l.ManagerId)).ToList();

			// 4) 組合結果（Left Join）：沒有角色者產一筆（RoleId=0, RoleName="(無角色)"）
			var combined = managers
				.GroupJoin(
					links,
					m => m.ManagerId,
					l => l.ManagerId,
					(m, ls) => ls.Any()
						? ls.Select(x => new ManagerWithRoleVM
						{
							ManagerId = m.ManagerId,
							ManagerName = m.ManagerName,
							ManagerRoleId = x.ManagerRoleId,
							RoleName = roleDict.TryGetValue(x.ManagerRoleId, out var rn) ? rn : "(未知角色)"
						})
						: new[]
						{
							new ManagerWithRoleVM
							{
								ManagerId = m.ManagerId,
								ManagerName = m.ManagerName,
								ManagerRoleId = 0,
								RoleName = "(無角色)"
							}
						})
				.SelectMany(x => x);

			// 5) 角色名稱過濾（若有指定 roleName）
			if (!string.IsNullOrWhiteSpace(roleKey))
			{
				combined = combined.Where(x =>
					(x.ManagerRoleId != 0 && roleIdFilter!.Contains(x.ManagerRoleId)) ||
					(filterNoRole && x.ManagerRoleId == 0));
			}

			// 6) 排序
			bool asc = sortDir != "desc";
			combined = sortBy switch
			{
				"role" => asc
					? combined.OrderBy(x => x.RoleName).ThenBy(x => x.ManagerId)
					: combined.OrderByDescending(x => x.RoleName).ThenByDescending(x => x.ManagerId),
				_ => asc
					? combined.OrderBy(x => x.ManagerId).ThenBy(x => x.RoleName)
					: combined.OrderByDescending(x => x.ManagerId).ThenByDescending(x => x.RoleName)
			};

			var result = combined.ToList();
			return View(result);
		}

		// ========== Details ==========
		public async Task<IActionResult> Details(int managerId, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			var manager = await _db.ManagerData
				.AsNoTracking()
				.Where(m => m.ManagerId == managerId)
				.Select(m => new { m.ManagerId, m.ManagerName })
				.FirstOrDefaultAsync();

			if (manager == null) return NotFound();

			var roleDict = await _db.ManagerRolePermissions
				.AsNoTracking()
				.Select(r => new { r.ManagerRoleId, r.RoleName })
				.ToDictionaryAsync(x => x.ManagerRoleId, x => x.RoleName);

			var links = await _db.Database.SqlQuery<ManagerRoleRow>($@"
                SELECT mr.Manager_Id     AS ManagerId,
                       mr.ManagerRole_Id AS ManagerRoleId
                FROM dbo.ManagerRole AS mr
                WHERE mr.Manager_Id = {managerId}
            ").ToListAsync();

			List<ManagerWithRoleVM> model;
			if (links.Count == 0)
			{
				model = new List<ManagerWithRoleVM>
				{
					new ManagerWithRoleVM
					{
						ManagerId = manager.ManagerId,
						ManagerName = manager.ManagerName,
						ManagerRoleId = 0,
						RoleName = "(無角色)"
					}
				};
			}
			else
			{
				model = links
					.Select(x => new ManagerWithRoleVM
					{
						ManagerId = manager.ManagerId,
						ManagerName = manager.ManagerName,
						ManagerRoleId = x.ManagerRoleId,
						RoleName = roleDict.TryGetValue(x.ManagerRoleId, out var name) ? name : "(未知角色)"
					})
					.OrderBy(x => x.ManagerRoleId)
					.ToList();
			}

			return View(model);
		}

		// ========== Edit (GET) ==========
		// 顯示「僅修改 ManagerRoleId」的頁面；其餘欄位唯讀
		public async Task<IActionResult> Edit(int managerId, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			var manager = await _db.ManagerData
				.AsNoTracking()
				.Where(m => m.ManagerId == managerId)
				.Select(m => new { m.ManagerId, m.ManagerName })
				.FirstOrDefaultAsync();
			if (manager == null) return NotFound();

			// 抓目前角色（若多筆取第一筆；沒有則 null/0）
			int? currentRoleId = await _db.Database
				.SqlQuery<int?>($@"
                    SELECT TOP(1) mr.ManagerRole_Id AS [Value]
                    FROM dbo.ManagerRole AS mr
                    WHERE mr.Manager_Id = {managerId}
                    ORDER BY mr.ManagerRole_Id")
				.FirstOrDefaultAsync();

			var roleItems = await _db.ManagerRolePermissions
				.AsNoTracking()
				.Select(r => new SelectListItem
				{
					Value = r.ManagerRoleId.ToString(),
					Text = r.RoleName
				})
				.ToListAsync();

			var vm = new ManagerRoleAssignVM
			{
				ManagerId = manager.ManagerId,
				ManagerName = manager.ManagerName,
				SelectedRoleId = currentRoleId ?? 0,
				Roles = roleItems
			};

			return View(vm);
		}

		// ========== Edit (POST) ==========
		// 刪舊插新：SelectedRoleId == 0 表示清除角色
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ManagerRoleAssignVM vm, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			if (!ModelState.IsValid)
			{
				vm.Roles = await _db.ManagerRolePermissions
					.AsNoTracking()
					.Select(r => new SelectListItem { Value = r.ManagerRoleId.ToString(), Text = r.RoleName })
					.ToListAsync();
				return View(vm);
			}

			bool managerExists = await _db.ManagerData.AnyAsync(m => m.ManagerId == vm.ManagerId);
			if (!managerExists) return NotFound();

			if (vm.SelectedRoleId != 0)
			{
				bool roleExists = await _db.ManagerRolePermissions
					.AnyAsync(r => r.ManagerRoleId == vm.SelectedRoleId);
				if (!roleExists)
				{
					ModelState.AddModelError(nameof(vm.SelectedRoleId), "選擇的角色不存在。");
					vm.Roles = await _db.ManagerRolePermissions
						.AsNoTracking()
						.Select(r => new SelectListItem { Value = r.ManagerRoleId.ToString(), Text = r.RoleName })
						.ToListAsync();
					return View(vm);
				}
			}

			using var tx = await _db.Database.BeginTransactionAsync();
			try
			{
				// 刪除既有關聯（若允許多角色情境，這步會全部清掉）
				await _db.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM dbo.ManagerRole
                    WHERE Manager_Id = {vm.ManagerId}");

				// 插入新關聯（若選 0 表示清除角色，就不插）
				if (vm.SelectedRoleId != 0)
				{
					await _db.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO dbo.ManagerRole (Manager_Id, ManagerRole_Id)
                        VALUES ({vm.ManagerId}, {vm.SelectedRoleId})");
				}

				await tx.CommitAsync();
			}
			catch
			{
				await tx.RollbackAsync();
				throw;
			}

			// 回到該管理者的詳細，立即看到結果（保留 sidebar 與 area）
			return RedirectToAction(nameof(Details), new { area = "MemberManagement", managerId = vm.ManagerId, sidebar = ViewBag.Sidebar });
		}
	}
}
