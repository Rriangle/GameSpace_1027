// CustomerServiceTypes.cs
namespace GameSpace.Areas.social_hub.Permissions
{
	/// <summary>客服區進入情境（Gate 結果）</summary>
	public sealed record CustomerServiceContext(
		int? ManagerId,
		bool CanEnter,
		string? DenyReason // "not_logged_in" | "not_manager" | "manager_not_found" | "no_permission"
	);

	/// <summary>統一的授權決策結果</summary>
	public sealed record AccessDecision(bool Allowed, string? DenyReason = null);

	/// <summary>拒絕原因常數（前後端/X-Deny-Reason 對齊）</summary>
	public static class DenyReasons
	{
		public const string NoManagerContext = "no_manager_context";
		public const string NeedCustomerService = "need_customer_service";
		public const string AgentInactive = "agent_inactive";
		public const string NeedCsAssign = "need_cs_assign";
		public const string NeedCsTransfer = "need_cs_transfer";
		public const string NeedCsAccept = "need_cs_accept";
		public const string NeedCsEditAll = "need_cs_edit_all";

		// 相容舊 GetCustomerServiceContextAsync(HttpContext)
		public const string NotLoggedIn = "not_logged_in";
		public const string NotManager = "not_manager";
		public const string ManagerNotFound = "manager_not_found";
		public const string NoPermission = "no_permission";
	}
}
