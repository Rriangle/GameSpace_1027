using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Areas.social_hub.Auth
{
	/// <summary>登入快照（僅來源整併；不包含任何授權判斷）</summary>
	public sealed record AuthSnapshot(
		bool IsAuthenticated,
		bool IsManager,
		int? ManagerId,
		int? UserId
	);

	public interface IUserContextReader
	{
		AuthSnapshot Read(HttpContext http);
	}

	/// <summary>將 Items / Cookies / Claims 的身分資訊整併成統一快照。</summary>
	public sealed class UserContextReader : IUserContextReader
	{
		public AuthSnapshot Read(HttpContext http)
		{
			// 1) Items（由 SocialHubAuth 寫入）
			if (http.Items.TryGetValue(AuthConstants.ItemsGsKind, out var kObj) && kObj is bool isMgrItems &&
				http.Items.TryGetValue(AuthConstants.ItemsGsId, out var idObj) && idObj is int idItems && idItems > 0)
			{
				return isMgrItems
					? new AuthSnapshot(true, true, idItems, null)
					: new AuthSnapshot(true, false, null, idItems);
			}

			// 2) Cookies（相容：bool "true"/"false" 或字串 "manager"/"user"）
			var kindRaw = http.Request.Cookies.TryGetValue(AuthConstants.CookieGsKind, out var ck) ? ck?.Trim().ToLowerInvariant() : null;
			var isMgrCookie = kindRaw switch
			{
				null => (bool?)null,
				"true" => true,
				"false" => false,
				"manager" => true,
				"user" => false,
				_ => bool.TryParse(kindRaw, out var b) ? b : (bool?)null
			};

			var idRaw = http.Request.Cookies.TryGetValue(AuthConstants.CookieGsId, out var cid) ? cid : null;
			int.TryParse(idRaw, out var idCookie);

			if (isMgrCookie is not null && idCookie > 0)
			{
				return isMgrCookie.Value
					? new AuthSnapshot(true, true, idCookie, null)
					: new AuthSnapshot(true, false, null, idCookie);
			}

			// 3) Claims 後援
			var p = http.User;
			var isAuth = p?.Identity?.IsAuthenticated ?? false;

			// 3-1) 自訂管理員 Claim
			var mgrClaim = p?.FindFirst("mgr:id")?.Value;
			if (int.TryParse(mgrClaim, out var mid) && mid > 0)
				return new AuthSnapshot(isAuth, true, mid, null);

			// 3-2) IsManager=true + NameIdentifier（作為 ManagerId）
			if (p?.HasClaim(c => c.Type == "IsManager" && c.Value == "true") == true)
			{
				var idStr = p.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (int.TryParse(idStr, out var mid2) && mid2 > 0)
					return new AuthSnapshot(isAuth, true, mid2, null);
			}

			// 3-3) 一般使用者（常見: sub 或 NameIdentifier）
			var uidStr = p?.FindFirst("sub")?.Value ?? p?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (int.TryParse(uidStr, out var uid) && uid > 0)
				return new AuthSnapshot(isAuth, false, null, uid);

			return new AuthSnapshot(isAuth, false, null, null);
		}
	}
}
