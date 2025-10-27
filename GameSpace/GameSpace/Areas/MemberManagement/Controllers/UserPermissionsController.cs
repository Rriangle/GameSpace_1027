using System.Linq;
using System.Threading.Tasks;
using GameSpace.Areas.MemberManagement.Models;
using GameSpace.Models; // 你的 DbContext 與實體命名空間
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	public class UserPermissionsController : Controller
	{
		// 若你用的是 MemberManagementDbContext，請把型別替換掉
		private readonly GameSpacedatabaseContext _context;

		public UserPermissionsController(GameSpacedatabaseContext context)
		{
			_context = context;
		}

		// GET: MemberManagement/UserPermissions
		// 新增搜尋參數：id、nickname（皆可省略）
		public async Task<IActionResult> Index(int? id, string? nickname)
		{
			// 保留目前搜尋值，回填到畫面
			ViewBag.Id = id;
			ViewBag.Nickname = nickname;

			// 先做 Left Join，再依參數過濾
			var baseQuery =
				from ui in _context.UserIntroduces.AsNoTracking()
				join ur in _context.UserRights.AsNoTracking()
					on ui.UserId equals ur.UserId into gj
				from ur in gj.DefaultIfEmpty()
				select new { ui, ur };

			if (id.HasValue)
			{
				baseQuery = baseQuery.Where(x => x.ui.UserId == id.Value);
			}

			if (!string.IsNullOrWhiteSpace(nickname))
			{
				var key = nickname.Trim();
				// 使用 LIKE 做模糊搜尋（通常 SQL Server 預設是大小寫不敏感的定序）
				baseQuery = baseQuery.Where(x => EF.Functions.Like(x.ui.UserNickName, $"%{key}%"));
			}

			var list = await baseQuery
				.Select(x => new UserPermissionVM
				{
					UserId = x.ui.UserId,
					UserNickName = x.ui.UserNickName,
					UserStatus = (x.ur != null && x.ur.UserStatus.HasValue) ? x.ur.UserStatus.Value : false,
					ShoppingPermission = (x.ur != null && x.ur.ShoppingPermission.HasValue) ? x.ur.ShoppingPermission.Value : false,
					MessagePermission = (x.ur != null && x.ur.MessagePermission.HasValue) ? x.ur.MessagePermission.Value : false,
					
				})
				.ToListAsync();

			return View(list);
		}
		public async Task<IActionResult> Edit(int id)
		{
			var vm = await (
				from ui in _context.UserIntroduces.AsNoTracking()
				join ur in _context.UserRights.AsNoTracking()
					on ui.UserId equals ur.UserId into gj
				from ur in gj.DefaultIfEmpty()
				where ui.UserId == id
				select new UserPermissionVM
				{
					UserId = ui.UserId,
					UserNickName = ui.UserNickName,
					UserStatus = (ur != null && ur.UserStatus.HasValue) ? ur.UserStatus.Value : false,
					ShoppingPermission = (ur != null && ur.ShoppingPermission.HasValue) ? ur.ShoppingPermission.Value : false,
					MessagePermission = (ur != null && ur.MessagePermission.HasValue) ? ur.MessagePermission.Value : false,
					
				}
			).FirstOrDefaultAsync();

			if (vm == null) return NotFound();
			return View(vm);
		}

		// POST: MemberManagement/UserPermissions/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, UserPermissionVM vm)
		{
			if (id != vm.UserId) return BadRequest();

			if (!ModelState.IsValid)
			{
				// UserId/UserNickName 只讀，不會被更新；權限四欄由表單回傳
				return View(vm);
			}

			// 讀取或建立對應的 UserRight
			var right = await _context.UserRights.FirstOrDefaultAsync(r => r.UserId == id);
			if (right == null)
			{
				right = new UserRight { UserId = id };
				_context.UserRights.Add(right);
			}

			right.UserStatus = vm.UserStatus;
			right.ShoppingPermission = vm.ShoppingPermission;
			right.MessagePermission = vm.MessagePermission;
			

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		}
}
