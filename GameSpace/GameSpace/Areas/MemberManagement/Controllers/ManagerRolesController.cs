using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MemberManagement;
using GameSpace.Areas.MemberManagement.Models;
using GameSpace.Areas.MemberManagement.Data;

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	public class ManagerRolesController : Controller
	{
		private readonly IConfiguration _config;

		public ManagerRolesController(IConfiguration config)
		{
			_config = config;
		}

		private MemberManagementDbContext CreateContext()
		{
			var options = new DbContextOptionsBuilder<MemberManagementDbContext>()
				.UseSqlServer(_config.GetConnectionString("GameSpace"))
				.Options;

			return new MemberManagementDbContext(options);
		}

		// GET: /MemberManagement/ManagerRoles
		public async Task<IActionResult> Index()
		{
			using var db = CreateContext();
			var data = await db.ManagerRoles.AsNoTracking().ToListAsync();
			return View(data);
		}

		// GET: /MemberManagement/ManagerRoles/Details?managerId=1&managerRoleId=2
		public async Task<IActionResult> Details(int managerId, int managerRoleId)
		{
			using var db = CreateContext();
			var item = await db.ManagerRoles
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ManagerId == managerId && m.ManagerRoleId == managerRoleId);

			if (item == null) return NotFound();
			return View(item);
		}

		// GET: /MemberManagement/ManagerRoles/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: /MemberManagement/ManagerRoles/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ManagerId,ManagerRoleId,ManagerRoleName")] ManagerRoleEntry model)
		{
			if (!ModelState.IsValid) return View(model);

			using var db = CreateContext();
			db.ManagerRoles.Add(model);
			await db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		// GET: /MemberManagement/ManagerRoles/Edit?managerId=1&managerRoleId=2
		public async Task<IActionResult> Edit(int managerId, int managerRoleId)
		{
			using var db = CreateContext();
			var item = await db.ManagerRoles.FirstOrDefaultAsync(m => m.ManagerId == managerId && m.ManagerRoleId == managerRoleId);
			if (item == null) return NotFound();
			return View(item);
		}

		// POST: /MemberManagement/ManagerRoles/Edit
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int managerId, [Bind("ManagerId,ManagerRoleId")] ManagerRoleEntry model)
		{
			if (managerId != model.ManagerId) return BadRequest();

			using var db = CreateContext();
			db.Entry(model).State = EntityState.Modified;
			await db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		// GET: /MemberManagement/ManagerRoles/Delete?managerId=1&managerRoleId=2
		public async Task<IActionResult> Delete(int managerId, int managerRoleId)
		{
			using var db = CreateContext();
			var item = await db.ManagerRoles
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ManagerId == managerId && m.ManagerRoleId == managerRoleId);

			if (item == null) return NotFound();
			return View(item);
		}

		// POST: /MemberManagement/ManagerRoles/DeleteConfirmed
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int managerId, int managerRoleId)
		{
			using var db = CreateContext();
			var item = await db.ManagerRoles.FindAsync(managerId, managerRoleId);
			if (item != null)
			{
				db.ManagerRoles.Remove(item);
				await db.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}
	}
}
