using GameSpace.Areas.MemberManagement.Models;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	public class ManagerRolePermissionController : Controller
	{
		private readonly GameSpacedatabaseContext _context;

		public ManagerRolePermissionController(GameSpacedatabaseContext context)
		{
			_context = context;
		}

		// GET: MemberManagement/ManagerRolePermission
		public async Task<IActionResult> Index(string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			var list = await _context.ManagerRolePermissions
				.Select(u => new ManagerRolePermissionVM
				{
					ManagerRoleId = u.ManagerRoleId,
					RoleName = u.RoleName,
					AdministratorPrivilegesManagement = u.AdministratorPrivilegesManagement ?? false,
					UserStatusManagement = u.UserStatusManagement ?? false,
					ShoppingPermissionManagement = u.ShoppingPermissionManagement ?? false,
					MessagePermissionManagement = u.MessagePermissionManagement ?? false,
					PetRightsManagement = u.PetRightsManagement ?? false,
					CustomerService = u.CustomerService ?? false
				}).ToListAsync();

			return View(list);
		}

		// GET: MemberManagement/ManagerRolePermission/Details/5
		public async Task<IActionResult> Details(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			if (id == null) return NotFound();

			var entity = await _context.ManagerRolePermissions
				.FirstOrDefaultAsync(m => m.ManagerRoleId == id);
			if (entity == null) return NotFound();

			var vm = MapToVM(entity);
			return View(vm);
		}

		//
		// GET: MemberManagement/ManagerRolePermission/Create
		//
		public IActionResult Create(string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			return View(new ManagerRolePermissionVM());
		}

		//
		// POST: MemberManagement/ManagerRolePermission/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ManagerRolePermissionVM vm, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			// 交由程式產號，避免驗證擋住
			ModelState.Remove(nameof(ManagerRolePermissionVM.ManagerRoleId));

			// ★ 先標準化 RoleName
			vm.RoleName = (vm.RoleName ?? string.Empty).Trim();

			// ★ 必填檢查
			if (string.IsNullOrWhiteSpace(vm.RoleName))
			{
				ModelState.AddModelError(nameof(vm.RoleName), "請輸入職位名稱。");
			}

			// ★ 重複檢查（預設 SQL Server 多為不分大小寫的定序，這樣即可；若你的資料庫為區分大小寫，可改用 ToUpper 比對）
			if (await _context.ManagerRolePermissions
							  .AsNoTracking()
							  .AnyAsync(x => x.RoleName == vm.RoleName))
			{
				ModelState.AddModelError(nameof(vm.RoleName), "此職位名稱已存在，請改用其他名稱。");
			}

			if (!ModelState.IsValid) return View(vm);

			await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
			try
			{
				// 1) 先取目前最大號（沒有就 0）
				var nextId = (await _context.ManagerRolePermissions
									  .Select(x => (int?)x.ManagerRoleId)
									  .MaxAsync()) ?? 0;
				nextId++; // 2) +1 起下一號

				// 3) 若有人併發塞入同號，或資料中本來就有該號，往下找直到沒重複
				while (await _context.ManagerRolePermissions
									  .AnyAsync(x => x.ManagerRoleId == nextId))
				{
					nextId++;
				}

				var entity = new ManagerRolePermission
				{
					ManagerRoleId = nextId,
					RoleName = vm.RoleName,
					AdministratorPrivilegesManagement = vm.AdministratorPrivilegesManagement,
					UserStatusManagement = vm.UserStatusManagement,
					ShoppingPermissionManagement = vm.ShoppingPermissionManagement,
					MessagePermissionManagement = vm.MessagePermissionManagement,
					PetRightsManagement = vm.PetRightsManagement,
					CustomerService = vm.CustomerService
				};

				_context.ManagerRolePermissions.Add(entity);
				await _context.SaveChangesAsync();
				await tx.CommitAsync();

				return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
			}
			// ★ 若你在資料庫層加了唯一索引，這裡把違反唯一索引的錯誤轉為友善訊息
			catch (DbUpdateException dbex) when (dbex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
			{
				await tx.RollbackAsync();
				ModelState.AddModelError(nameof(vm.RoleName), "此職位名稱已存在，請改用其他名稱。");
				return View(vm);
			}
			catch (Exception ex)
			{
				await tx.RollbackAsync();
				ModelState.AddModelError(string.Empty, ex.GetBaseException().Message);
				return View(vm);
			}
		}

		// GET: MemberManagement/ManagerRolePermission/Edit/5
		public async Task<IActionResult> Edit(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			if (id == null) return NotFound();

			var entity = await _context.ManagerRolePermissions.FindAsync(id);
			if (entity == null) return NotFound();

			var vm = MapToVM(entity);
			return View(vm);
		}

		// POST: MemberManagement/ManagerRolePermission/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ManagerRolePermissionVM vm, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			if (id != vm.ManagerRoleId) return NotFound();

			if (ModelState.IsValid)
			{
				var entity = await _context.ManagerRolePermissions.FindAsync(id);
				if (entity == null) return NotFound();

				// 更新欄位
				entity.RoleName = vm.RoleName;
				entity.AdministratorPrivilegesManagement = vm.AdministratorPrivilegesManagement;
				entity.UserStatusManagement = vm.UserStatusManagement;
				entity.ShoppingPermissionManagement = vm.ShoppingPermissionManagement;
				entity.MessagePermissionManagement = vm.MessagePermissionManagement;
				entity.PetRightsManagement = vm.PetRightsManagement;
				entity.CustomerService = vm.CustomerService;

				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
			}
			return View(vm);
		}

		// GET: MemberManagement/ManagerRolePermission/Delete/5
		public async Task<IActionResult> Delete(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			if (id == null) return NotFound();

			var entity = await _context.ManagerRolePermissions
				.FirstOrDefaultAsync(m => m.ManagerRoleId == id);
			if (entity == null) return NotFound();

			var vm = MapToVM(entity);
			return View(vm);
		}

		// POST: MemberManagement/ManagerRolePermission/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			var entity = await _context.ManagerRolePermissions.FindAsync(id);
			if (entity != null)
			{
				_context.ManagerRolePermissions.Remove(entity);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
		}

		private bool ManagerRolePermissionExists(int id)
		{
			return _context.ManagerRolePermissions.Any(e => e.ManagerRoleId == id);
		}

		// 🔹 共用轉換方法
		private ManagerRolePermissionVM MapToVM(ManagerRolePermission e) => new()
		{
			ManagerRoleId = e.ManagerRoleId,
			RoleName = e.RoleName,
			AdministratorPrivilegesManagement = e.AdministratorPrivilegesManagement ?? false,
			UserStatusManagement = e.UserStatusManagement ?? false,
			ShoppingPermissionManagement = e.ShoppingPermissionManagement ?? false,
			MessagePermissionManagement = e.MessagePermissionManagement ?? false,
			PetRightsManagement = e.PetRightsManagement ?? false,
			CustomerService = e.CustomerService ?? false
		};

		private ManagerRolePermission MapToEntity(ManagerRolePermissionVM vm) => new()
		{
			// 建議 Create 時不要帶 Id；Edit 才帶
			ManagerRoleId = vm.ManagerRoleId,
			RoleName = vm.RoleName,
			AdministratorPrivilegesManagement = vm.AdministratorPrivilegesManagement,
			UserStatusManagement = vm.UserStatusManagement,
			ShoppingPermissionManagement = vm.ShoppingPermissionManagement,
			MessagePermissionManagement = vm.MessagePermissionManagement,
			PetRightsManagement = vm.PetRightsManagement,
			CustomerService = vm.CustomerService
		};
	}
}
