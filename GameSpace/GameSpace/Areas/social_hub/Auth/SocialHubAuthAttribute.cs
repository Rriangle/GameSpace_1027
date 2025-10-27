using GameSpace.Infrastructure.Login; // ILoginIdentity
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameSpace.Areas.social_hub.Auth
{
	/// <summary>
	/// 只負責「身分 → 相容變數」：將 ILoginIdentity 轉為 Items 與 Cookies。
	/// 不做 Authenticate、不導向、不回 401/403。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class SocialHubAuthAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
	{
		/// <summary>執行順序：越小越先跑（建議在權限檢查前）。</summary>
		public int Order => -2000;

		public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
		{
			var http = ctx.HttpContext;

			// 1) 從統一登入介面取身分
			var login = http.RequestServices.GetRequiredService<ILoginIdentity>();
			var me = await login.GetAsync(http.RequestAborted);

			// 2) 寫入 Items 與相容 Cookies（只寫非敏感兩個）
			if (me.IsAuthenticated)
			{
				var isManager = me.IsManager;
				var id = isManager ? (me.ManagerId ?? 0) : (me.UserId ?? 0);

				http.Items[AuthConstants.ItemsGsKind] = isManager; // bool
				http.Items[AuthConstants.ItemsGsId] = id;          // int

				// 寫 Cookie（若不同才更新）
				var opts = AuthConstants.DefaultCookieOptions(http);
				var needWriteId = !http.Request.Cookies.TryGetValue(AuthConstants.CookieGsId, out var sId) || sId != id.ToString();
				var needWriteKind = !http.Request.Cookies.TryGetValue(AuthConstants.CookieGsKind, out var sKind) || sKind != (isManager ? "true" : "false");

				if (needWriteId) http.Response.Cookies.Append(AuthConstants.CookieGsId, id.ToString(), opts);
				if (needWriteKind) http.Response.Cookies.Append(AuthConstants.CookieGsKind, isManager ? "true" : "false", opts);
			}

			await next();
		}
	}
}
