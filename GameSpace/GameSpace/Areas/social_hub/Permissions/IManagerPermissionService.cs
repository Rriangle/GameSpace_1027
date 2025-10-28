// IManagerPermissionService.cs
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace GameSpace.Areas.social_hub.Permissions
{
	/// <summary>客服權限服務（僅以 CustomerService Gate + CS_Agent / CS_Agent_Permission 為準）</summary>
	public interface IManagerPermissionService
	{
		// ---- Gate：ManagerRolePermission.CustomerService ----
		Task<CustomerServiceContext> GetCustomerServiceContextAsync(int managerId, CancellationToken ct = default);
		Task<CustomerServiceContext> GetCustomerServiceContextAsync(HttpContext httpContext, CancellationToken ct = default); // 相容：從 HttpContext 取身分

		// ---- CS_Agent（需啟用）----
		Task<bool> IsActiveCsAgentAsync(int managerId, CancellationToken ct = default);

		// ---- 細權限（CS_Agent_Permission）----
		Task<bool> HasCsAssignPermissionAsync(int managerId, CancellationToken ct = default);
		Task<bool> HasCsTransferPermissionAsync(int managerId, CancellationToken ct = default);
		Task<bool> HasCsAcceptPermissionAsync(int managerId, CancellationToken ct = default);
		Task<bool> HasCsEditMuteAllPermissionAsync(int managerId, CancellationToken ct = default);

		// ---- 綜合評估（一次判斷多旗標）----
		Task<AccessDecision> EvaluatePermissionsAsync(
			int managerId,
			bool needCustomerService,
			bool needCsAssign,
			bool needCsTransfer,
			bool needCsAccept,
			bool needCsEditMuteAll,
			CancellationToken ct = default);
	}
}
