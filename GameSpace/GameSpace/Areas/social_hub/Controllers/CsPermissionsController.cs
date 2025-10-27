using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Areas.social_hub.Permissions; // IManagerPermissionService
using GameSpace.Infrastructure.Login;          // ILoginIdentity
using GameSpace.Models;                       // DbContext 與實體
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GameSpace.Areas.social_hub.Models.ViewModels.CsPermEditVM;

namespace GameSpace.Areas.social_hub.Controllers
{
	/// <summary>
	/// 客服人員（CS_Agent）與客服權限（CS_Agent_Permission）的管理介面
	/// 規則：
	/// 1) 只有具最高權限（CanAssign=true）的主管能管理所有人與新增/調整
	/// 2) 一般客服只能檢視自己的權限（唯讀）
	/// 3) 任何頁面皆需通過「舊客服 Gate」（CustomerService=1）才可進入
	/// </summary>
	[Area("social_hub")]
	[SocialHubAuth]
	public sealed class CsPermissionsController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ILoginIdentity _login;
		private readonly IManagerPermissionService _perm;

		public CsPermissionsController(
			GameSpacedatabaseContext db,
			ILoginIdentity login,
			IManagerPermissionService perm)
		{
			_db = db;
			_login = login;
			_perm = perm;
		}

		// ===== 我的身分快照 =====
		private sealed class MyCtx
		{
			public bool IsManager { get; init; }
			public int ManagerId { get; init; }
			public int? AgentId { get; init; }
			public bool IsSupervisor { get; init; } // 主管：CanAssign==true
			public bool CanEnter { get; init; }     // 舊客服 Gate（CustomerService=1）
		}

		/// <summary>
		/// 從統一登入取目前管理員 + 檢查客服 Gate + 取 AgentId 與是否主管
		/// </summary>
		private async Task<MyCtx> GetMyCtxAsync()
		{
			var me = await _login.GetAsync();
			var isMgr = me.IsAuthenticated &&
						string.Equals(me.Kind, "manager", StringComparison.OrdinalIgnoreCase) &&
						me.ManagerId.HasValue;

			if (!isMgr)
				return new MyCtx { IsManager = false, ManagerId = 0, CanEnter = false };

			var mid = me.ManagerId!.Value;

			// 舊客服 Gate（很重要：控制是否能進此模組）
			var gate = await _perm.GetCustomerServiceContextAsync(mid);

			// 找到自己的 AgentId
			int? myAid = await _db.CsAgents
				.AsNoTracking()
				.Where(a => a.ManagerId == mid)
				.Select(a => (int?)a.AgentId)
				.FirstOrDefaultAsync();

			// 是否主管（CanAssign）
			bool isSupervisor = false;
			if (myAid is int aid)
			{
				isSupervisor = await _db.CsAgentPermissions
					.AsNoTracking()
					.Where(p => p.AgentId == aid)
					.Select(p => p.CanAssign)
					.FirstOrDefaultAsync();
			}

			return new MyCtx
			{
				IsManager = true,
				ManagerId = mid,
				AgentId = myAid,
				IsSupervisor = isSupervisor,
				CanEnter = gate.CanEnter
			};
		}

		private static string MgrName(ManagerDatum m)
			=> string.IsNullOrWhiteSpace(m.ManagerName)
			   ? (m.ManagerAccount ?? $"管理員#{m.ManagerId}")
			   : m.ManagerName;

		// ===== 共用：產生「可加入」下拉清單 =====
		/// <summary>
		/// 取得「可加入客服專用權限」之管理員清單
		/// 條件：
		///   (A) 已具『客服入口權限』（舊 Gate）且
		///   (B) 尚未擁有專用權限（CS_Agent_Permission）：
		///       - 不存在 CS_Agent，或
		///       - 存在 CS_Agent 但缺 CS_Agent_Permission
		/// </summary>
		private async Task<List<SelectListItem>> BuildEligibleManagerOptionsAsync()
		{
			var managers = await _db.ManagerData
				.AsNoTracking()
				.Select(m => new { m.ManagerId, m.ManagerName })
				.ToListAsync();

			var agentsByMid = await _db.CsAgents
				.AsNoTracking()
				.Select(a => new { a.ManagerId, a.AgentId })
				.ToDictionaryAsync(x => x.ManagerId, x => x.AgentId);

			// EF 有版本差，改成 ToListAsync() 再 ToHashSet()
			var permAgentIds = (await _db.CsAgentPermissions
				.AsNoTracking()
				.Select(p => p.AgentId)
				.ToListAsync()).ToHashSet();

			var options = new List<SelectListItem>();

			foreach (var m in managers)
			{
				var hasAgent = agentsByMid.TryGetValue(m.ManagerId, out var aid);
				var hasPerm = hasAgent && permAgentIds.Contains(aid);
				if (hasPerm) continue; // 已有專用權限 → 不列入

				// 舊 Gate：CustomerService=1
				var gate = await _perm.GetCustomerServiceContextAsync(m.ManagerId);
				if (!gate.CanEnter) continue;

				var suffix = hasAgent ? "（缺專用權限）" : "（未在客服清單）";
				var name = string.IsNullOrWhiteSpace(m.ManagerName) ? $"管理員#{m.ManagerId}" : m.ManagerName;

				options.Add(new SelectListItem
				{
					Value = m.ManagerId.ToString(),
					Text = $"{name}{suffix}"
				});
			}

			return options.OrderBy(x => x.Text).ToList();
		}

		// ===== 清單（Index） =====
		/// <summary>
		/// 客服清單（含 Agent 基本資料與四個權限旗標）
		/// 只有通過舊 Gate 的管理員能進入；主管可見全表，非主管也可看清單（編輯會限縮在自己）
		/// </summary>
		public async Task<IActionResult> Index(int page = 1, string? q = null)
		{
			var mine = await GetMyCtxAsync();
			if (!mine.IsManager || !mine.CanEnter) return Forbid();

			const int PageSize = 10;

			// 基本查詢（Agent + Manager + Perm）
			var baseQuery =
				from a in _db.CsAgents.AsNoTracking()
				join m in _db.ManagerData.AsNoTracking() on a.ManagerId equals m.ManagerId
				join p in _db.CsAgentPermissions.AsNoTracking() on a.AgentId equals p.AgentId into gp
				from p in gp.DefaultIfEmpty()
				select new { a, m, p };

			// 搜尋
			var qTrim = (q ?? string.Empty).Trim();
			if (!string.IsNullOrWhiteSpace(qTrim))
			{
				var lower = qTrim.ToLowerInvariant();
				if (lower.StartsWith("id:") && int.TryParse(lower[3..], out var idv))
					baseQuery = baseQuery.Where(x => x.a.AgentId == idv);
				else if (lower.StartsWith("mid:") && int.TryParse(lower[4..], out var midv))
					baseQuery = baseQuery.Where(x => x.a.ManagerId == midv);
				else if (lower is "on" or "true" or "啟用")
					baseQuery = baseQuery.Where(x => x.a.IsActive == true);
				else if (lower is "off" or "false" or "停用")
					baseQuery = baseQuery.Where(x => x.a.IsActive == false);
				else
					baseQuery = baseQuery.Where(x =>
						(x.m.ManagerName != null && x.m.ManagerName.Contains(qTrim)) ||
						x.a.ManagerId.ToString() == qTrim);
			}

			// 分頁
			var total = await baseQuery.CountAsync();
			var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
			page = Math.Clamp(page, 1, totalPages);

			var rows = await baseQuery
				.OrderByDescending(x => x.a.AgentId)
				.Skip((page - 1) * PageSize)
				.Take(PageSize)
				.ToListAsync();

			var list = rows.Select(x => new CsPermListItemVM
			{
				AgentId = x.a.AgentId,
				ManagerId = x.a.ManagerId,
				ManagerName = MgrName(x.m),
				IsActive = x.a.IsActive,
				MaxConcurrent = x.a.MaxConcurrent,
				CreatedAt = x.a.CreatedAt,
				CanAssign = x.p?.CanAssign ?? false,
				CanTransfer = x.p?.CanTransfer ?? false,
				CanAccept = x.p?.CanAccept ?? false,
				CanEditMuteAll = x.p?.CanEditMuteAll ?? false
			}).ToList();

			ViewBag.Page = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.TotalItems = total;
			ViewBag.PageSize = PageSize;

			ViewBag.MeIsSupervisor = mine.IsSupervisor;
			ViewBag.MeAgentId = mine.AgentId;

			return View(list);
		}

		// ===== 加入頁（GET/POST） =====
		/// <summary>
		/// 加入客服專用權限（頁面）— 僅主管可進入
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Add()
		{
			var mine = await GetMyCtxAsync();
			if (!mine.IsManager || !mine.CanEnter || !mine.IsSupervisor) return Forbid();

			var vm = new CsAgentAddVM { EligibleManagers = await BuildEligibleManagerOptionsAsync() };
			return View(vm);
		}

		/// <summary>
		/// 提交加入（建立或補建 CS_Agent 與 CS_Agent_Permission）— 僅主管
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add(CsAgentAddVM vm)
		{
			var mine = await GetMyCtxAsync();
			if (!mine.IsManager || !mine.CanEnter || !mine.IsSupervisor) return Forbid();

			if (!ModelState.IsValid)
			{
				vm.EligibleManagers = await BuildEligibleManagerOptionsAsync();
				return View(vm);
			}

			var managerId = vm.ManagerId!.Value;

			// (1) 管理員存在？
			var mgr = await _db.ManagerData.FirstOrDefaultAsync(m => m.ManagerId == managerId);
			if (mgr == null)
			{
				ModelState.AddModelError(nameof(CsAgentAddVM.ManagerId), $"找不到管理員 #{managerId}。");
				vm.EligibleManagers = await BuildEligibleManagerOptionsAsync();
				return View(vm);
			}

			// (2) 舊客服 Gate
			var gate = await _perm.GetCustomerServiceContextAsync(managerId);
			if (!gate.CanEnter)
			{
				ModelState.AddModelError(nameof(CsAgentAddVM.ManagerId),
					"此管理員尚未具『客服入口權限（CustomerService）』，無法加入客服專用權限。");
				vm.EligibleManagers = await BuildEligibleManagerOptionsAsync();
				return View(vm);
			}

			// (3) 取得/建立 CS_Agent
			var agent = await _db.CsAgents.FirstOrDefaultAsync(a => a.ManagerId == managerId);
			if (agent == null)
			{
				agent = new CsAgent
				{
					ManagerId = managerId,
					IsActive = vm.InitIsActive,
					MaxConcurrent = vm.InitMaxConcurrent,
					CreatedAt = DateTime.UtcNow,
					CreatedByManager = mine.ManagerId
				};
				_db.CsAgents.Add(agent);
				await _db.SaveChangesAsync(); // 取得 AgentId
			}

			// (4) 建立或補建 CS_Agent_Permission
			var perm = await _db.CsAgentPermissions.FirstOrDefaultAsync(p => p.AgentId == agent.AgentId);
			if (perm == null)
			{
				perm = new CsAgentPermission
				{
					AgentId = agent.AgentId,
					CanAssign = vm.InitCanAssign,
					CanTransfer = vm.InitCanTransfer,
					CanAccept = vm.InitCanAccept,
					CanEditMuteAll = vm.InitCanEditMuteAll
				};
				_db.CsAgentPermissions.Add(perm);
				await _db.SaveChangesAsync();

				TempData["Toast.Success"] = $"已加入「{MgrName(mgr)}」並建立客服專用權限。";
				TempData["Toast.AutoHideMs"] = 4000;
			}
			else
			{
				TempData["Toast.Info"] = $"「{MgrName(mgr)}」已具客服專用權限。";
				TempData["Toast.AutoHideMs"] = 4000;
			}

			return RedirectToAction(nameof(Index));
		}

		// ===== 權限編輯（主管可改；一般人只能看自己的） =====
		[HttpGet]
		public async Task<IActionResult> EditPerm(int id)
		{
			var mine = await GetMyCtxAsync();
			if (!mine.IsManager || !mine.CanEnter) return Forbid();

			var row = await _db.CsAgents
				.Include(a => a.Manager)
				.Include(a => a.CsAgentPermission)
				.FirstOrDefaultAsync(a => a.AgentId == id);
			if (row == null) return NotFound();

			var isSelf = mine.AgentId == row.AgentId;
			if (!mine.IsSupervisor && !isSelf) return Forbid(); // 非主管只能看自己

			var vm = new CsPermEditVM
			{
				AgentId = row.AgentId,
				ManagerId = row.ManagerId,
				ManagerName = MgrName(row.Manager),
				CanAssign = row.CsAgentPermission?.CanAssign ?? false,
				CanTransfer = row.CsAgentPermission?.CanTransfer ?? false,
				CanAccept = row.CsAgentPermission?.CanAccept ?? false,
				CanEditMuteAll = row.CsAgentPermission?.CanEditMuteAll ?? false,
				IsReadOnly = !mine.IsSupervisor,
				EditableCanAssign = mine.IsSupervisor
			};
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPerm(CsPermEditVM vm)
		{
			var mine = await GetMyCtxAsync();
			if (!mine.IsManager || !mine.CanEnter || !mine.IsSupervisor) return Forbid();

			var agent = await _db.CsAgents
				.Include(a => a.CsAgentPermission)
				.FirstOrDefaultAsync(a => a.AgentId == vm.AgentId);
			if (agent == null) return NotFound();

			var p = agent.CsAgentPermission ?? new CsAgentPermission { AgentId = agent.AgentId };
			if (agent.CsAgentPermission == null) _db.CsAgentPermissions.Add(p);

			p.CanAssign = vm.CanAssign;
			p.CanTransfer = vm.CanTransfer;
			p.CanAccept = vm.CanAccept;
			p.CanEditMuteAll = vm.CanEditMuteAll;

			await _db.SaveChangesAsync();

			TempData["Toast.Success"] = "已更新客服權限。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}

		// ===== Agent 基本設定（主管可改） =====
		[HttpGet]
		public async Task<IActionResult> EditAgent(int id)
		{
			var mine = await GetMyCtxAsync();
			if (!mine.IsManager || !mine.CanEnter || !mine.IsSupervisor) return Forbid();

			var a = await _db.CsAgents
				.Include(x => x.Manager)
				.FirstOrDefaultAsync(x => x.AgentId == id);
			if (a == null) return NotFound();

			var vm = new CsAgentEditVM
			{
				AgentId = a.AgentId,
				ManagerId = a.ManagerId,
				ManagerName = MgrName(a.Manager),
				IsActive = a.IsActive,
				MaxConcurrent = a.MaxConcurrent,
				CreatedAt = a.CreatedAt,
				UpdatedAt = a.UpdatedAt,
				IsReadOnly = false,
				AllowToggleActive = true,
				AllowEditMaxConcurrent = true
			};
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditAgent(CsAgentEditVM vm)
		{
			var mine = await GetMyCtxAsync();
			if (!mine.IsManager || !mine.CanEnter || !mine.IsSupervisor) return Forbid();

			if (!ModelState.IsValid) return View(vm);

			var a = await _db.CsAgents.FirstOrDefaultAsync(x => x.AgentId == vm.AgentId);
			if (a == null) return NotFound();

			a.IsActive = vm.IsActive;
			a.MaxConcurrent = vm.MaxConcurrent;
			a.UpdatedAt = DateTime.UtcNow;
			a.UpdatedByManager = mine.ManagerId;

			await _db.SaveChangesAsync();

			TempData["Toast.Success"] = "已更新客服人員設定。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}
	}
}
