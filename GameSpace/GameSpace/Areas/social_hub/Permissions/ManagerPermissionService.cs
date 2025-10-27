// ManagerPermissionService.cs
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Models; // GameSpacedatabaseContext, ManagerRolePermission, CsAgent, CsAgentPermission
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Permissions
{
	/// <summary>
	/// 針對「客服工作台」的權限查詢與綜合評估。
	/// 依賴的資料表：
	/// - ManagerData（管理員）
	/// - ManagerRolePermission（角色/權限；與 ManagerData 經由連接表 ManagerRole 為多對多）
	/// - CsAgents（客服代理人；是否啟用）
	/// - CsAgentPermissions（客服細權限）
	/// </summary>
	public sealed class ManagerPermissionService : IManagerPermissionService
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IUserContextReader _ctxReader;

		public ManagerPermissionService(GameSpacedatabaseContext db, IUserContextReader ctxReader)
		{
			_db = db;
			_ctxReader = ctxReader;
		}

		// --------------------------------------------------------------------
		// Gate：CustomerService（是否具備進入客服區的基礎權限）
		// --------------------------------------------------------------------

		/// <summary>
		/// 依 ManagerId 判斷是否通過「客服區入口 Gate」。
		/// 成功時回傳 <see cref="CustomerServiceContext.CanEnter"/> = true，失敗時在 <see cref="CustomerServiceContext.DenyReason"/> 說明原因。
		/// </summary>
		public async Task<CustomerServiceContext> GetCustomerServiceContextAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0)
				return new CustomerServiceContext(null, false, DenyReasons.NotLoggedIn);

			// 先確認管理員是否存在
			var exists = await _db.ManagerData
				.AsNoTracking()
				.AnyAsync(m => m.ManagerId == managerId, ct);

			if (!exists)
				return new CustomerServiceContext(managerId, false, DenyReasons.ManagerNotFound);

			// ✅ Gate 判斷：是否有任一角色其 CustomerService = true
			// 注意：bool? 欄位要使用 == true 避免 NULL 被視為通過
			// 以「角色 → 管理員」方向查詢，對多對多在大型資料下較可利用索引。
			var canEnter = await _db.Set<ManagerRolePermission>()
				.AsNoTracking()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == managerId, ct);

			if (!canEnter)
				return new CustomerServiceContext(managerId, false, DenyReasons.NoPermission);

			return new CustomerServiceContext(managerId, true, null);
		}

		/// <summary>
		/// 以 <see cref="HttpContext"/> 解析當前登入身分，再呼叫 <see cref="GetCustomerServiceContextAsync(int, CancellationToken)"/>。
		/// </summary>
		public async Task<CustomerServiceContext> GetCustomerServiceContextAsync(HttpContext httpContext, CancellationToken ct = default)
		{
			var snap = _ctxReader.Read(httpContext);

			if (!snap.IsAuthenticated)
				return new CustomerServiceContext(null, false, DenyReasons.NotLoggedIn);

			if (!snap.IsManager || snap.ManagerId is null || snap.ManagerId <= 0)
				return new CustomerServiceContext(snap.ManagerId, false, DenyReasons.NotManager);

			return await GetCustomerServiceContextAsync(snap.ManagerId.Value, ct);
		}

		// --------------------------------------------------------------------
		// CS_Agent（是否為啟用中的客服代理人）
		// --------------------------------------------------------------------

		/// <summary>檢查該管理員是否為啟用中的 CS Agent。</summary>
		public async Task<bool> IsActiveCsAgentAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0) return false;

			return await _db.CsAgents
				.AsNoTracking()
				.AnyAsync(a => a.ManagerId == managerId && a.IsActive == true, ct);
		}

		/// <summary>
		/// 內部工具：取得啟用中的 AgentId（不存在或未啟用則回傳 null）。
		/// 以唯一鍵（建議 DB 設 UQ）單筆命中。
		/// </summary>
		private async Task<int?> GetActiveAgentIdAsync(int managerId, CancellationToken ct)
		{
			return await _db.CsAgents
				.AsNoTracking()
				.Where(a => a.ManagerId == managerId && a.IsActive == true)
				.Select(a => (int?)a.AgentId)
				.FirstOrDefaultAsync(ct);
		}

		// --------------------------------------------------------------------
		// 細權限：CS_Agent_Permission
		// --------------------------------------------------------------------

		/// <summary>是否具「可指派」權限。</summary>
		public async Task<bool> HasCsAssignPermissionAsync(int managerId, CancellationToken ct = default)
			=> await HasPermAsync(managerId, p => p.CanAssign == true, ct);

		/// <summary>是否具「可轉單」權限。</summary>
		public async Task<bool> HasCsTransferPermissionAsync(int managerId, CancellationToken ct = default)
			=> await HasPermAsync(managerId, p => p.CanTransfer == true, ct);

		/// <summary>是否具「可接受工單」權限。</summary>
		public async Task<bool> HasCsAcceptPermissionAsync(int managerId, CancellationToken ct = default)
			=> await HasPermAsync(managerId, p => p.CanAccept == true, ct);

		/// <summary>是否具「可編輯全域穢語清單」權限。</summary>
		public async Task<bool> HasCsEditMuteAllPermissionAsync(int managerId, CancellationToken ct = default)
			=> await HasPermAsync(managerId, p => p.CanEditMuteAll == true, ct);

		/// <summary>
		/// 共用：在已啟用的 Agent 上檢查單一細權限。
		/// </summary>
		private async Task<bool> HasPermAsync(
			int managerId,
			System.Linq.Expressions.Expression<Func<CsAgentPermission, bool>> pred,
			CancellationToken ct)
		{
			if (managerId <= 0) return false;

			var agentId = await GetActiveAgentIdAsync(managerId, ct);
			if (agentId is null) return false;

			return await _db.CsAgentPermissions
				.AsNoTracking()
				.Where(p => p.AgentId == agentId.Value)
				.AnyAsync(pred, ct);
		}

		// --------------------------------------------------------------------
		// 綜合評估：可一次同時檢查 Gate 與細權限需求
		// --------------------------------------------------------------------

		/// <summary>
		/// 依需求旗標綜合評估該管理員是否允許執行某操作。
		/// 若任一條件不足，<see cref="AccessDecision.Allowed"/> 為 false，並於 <see cref="AccessDecision.DenyReason"/> 說明原因。
		/// </summary>
		public async Task<AccessDecision> EvaluatePermissionsAsync(
			int managerId,
			bool needCustomerService,
			bool needCsAssign,
			bool needCsTransfer,
			bool needCsAccept,
			bool needCsEditMuteAll,
			CancellationToken ct = default)
		{
			if (managerId <= 0)
				return new AccessDecision(false, DenyReasons.NoManagerContext);

			// 1) 入口 Gate（CustomerService）
			if (needCustomerService)
			{
				var gate = await GetCustomerServiceContextAsync(managerId, ct);
				if (!gate.CanEnter)
					return new AccessDecision(false, DenyReasons.NeedCustomerService);
			}

			// 2) 細權限（任一被要求，皆需要已啟用的 Agent）
			if (needCsAssign || needCsTransfer || needCsAccept || needCsEditMuteAll)
			{
				var agentId = await GetActiveAgentIdAsync(managerId, ct);
				if (agentId is null)
					return new AccessDecision(false, DenyReasons.AgentInactive);

				// 取單筆旗標（全部轉成 non-nullable bool；查不到視為全 false）
				var perm = await _db.CsAgentPermissions
					.AsNoTracking()
					.Where(p => p.AgentId == agentId.Value)
					.Select(p => new
					{
						CanAssign = p.CanAssign == true,
						CanTransfer = p.CanTransfer == true,
						CanAccept = p.CanAccept == true,
						CanEditMuteAll = p.CanEditMuteAll == true
					})
					.FirstOrDefaultAsync(ct);

				var canAssign = perm?.CanAssign ?? false;
				var canTransfer = perm?.CanTransfer ?? false;
				var canAccept = perm?.CanAccept ?? false;
				var canEditAll = perm?.CanEditMuteAll ?? false;

				if (needCsAssign && !canAssign) return new AccessDecision(false, DenyReasons.NeedCsAssign);
				if (needCsTransfer && !canTransfer) return new AccessDecision(false, DenyReasons.NeedCsTransfer);
				if (needCsAccept && !canAccept) return new AccessDecision(false, DenyReasons.NeedCsAccept);
				if (needCsEditMuteAll && !canEditAll) return new AccessDecision(false, DenyReasons.NeedCsEditAll);
			}

			return new AccessDecision(true, null);
		}
	}
}
