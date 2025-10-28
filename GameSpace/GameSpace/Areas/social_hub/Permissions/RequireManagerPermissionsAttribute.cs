using GameSpace.Areas.social_hub.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameSpace.Areas.social_hub.Permissions
{
	/// <summary>以屬性宣告的方式進行權限檢查（只依賴 IManagerPermissionService + IUserContextReader）。</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireManagerPermissionsAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
	{
		/// <summary>要求具備客服工作台門檻（ManagerRolePermission.CustomerService）</summary>
		public bool CustomerService { get; set; } = false;

		/// <summary>要求具備「派單/幫轉單/結單」權（CS_Agent_Permission.can_assign）</summary>
		public bool CsAssign { get; set; } = false;

		/// <summary>要求具備「轉單」權（CS_Agent_Permission.can_transfer）</summary>
		public bool CsTransfer { get; set; } = false;

		/// <summary>要求具備「接單」權（CS_Agent_Permission.can_accept）</summary>
		public bool CsAccept { get; set; } = false;

		/// <summary>要求具備「穢語可編修全部」權（CS_Agent_Permission.can_edit_mute_all）</summary>
		public bool CsEditMuteAll { get; set; } = false;

		/// <summary>確保在 SocialHubAuth（-2000）之後</summary>
		public int Order => -1000;

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var http = context.HttpContext;
			var ct = http.RequestAborted;

			var reader = http.RequestServices.GetRequiredService<IUserContextReader>();
			var permSvc = http.RequestServices.GetRequiredService<IManagerPermissionService>();

			var snap = reader.Read(http);
			if (!snap.IsAuthenticated || !snap.IsManager || snap.ManagerId is null || snap.ManagerId <= 0)
			{
				http.Response.Headers[AuthConstants.HeaderDenyReason] = DenyReasons.NoManagerContext;
				if (!(http.User?.Identity?.IsAuthenticated ?? false))
					context.Result = new ChallengeResult(AuthConstants.AdminCookieScheme);
				else
					context.Result = new ForbidResult(AuthConstants.AdminCookieScheme);
				return;
			}

			var needGate = CustomerService || CsAssign || CsTransfer || CsAccept || CsEditMuteAll;

			var decision = await permSvc.EvaluatePermissionsAsync(
				snap.ManagerId.Value,
				needCustomerService: needGate,
				needCsAssign: CsAssign,
				needCsTransfer: CsTransfer,
				needCsAccept: CsAccept,
				needCsEditMuteAll: CsEditMuteAll,
				ct);

			if (!decision.Allowed)
			{
				http.Response.Headers[AuthConstants.HeaderDenyReason] = decision.DenyReason ?? string.Empty;
				context.Result = new ForbidResult(AuthConstants.AdminCookieScheme);
				return;
			}

			await next();
		}
	}
}
