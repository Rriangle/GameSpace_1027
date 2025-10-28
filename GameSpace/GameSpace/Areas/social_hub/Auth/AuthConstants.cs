using Microsoft.AspNetCore.Http;

namespace GameSpace.Areas.social_hub.Auth
{
	internal static class AuthConstants
	{
		// Items keys（每次請求的暫存）
		public const string ItemsGsId = "gs_id";
		public const string ItemsGsKind = "gs_kind"; // bool

		// Cookie keys（跨請求）
		public const string CookieGsId = "gs_id";
		public const string CookieGsKind = "gs_kind"; // "true"/"false"

		// Header keys
		public const string HeaderDenyReason = "X-Deny-Reason";

		// 認證方案（你的後台 Cookie）
		public const string AdminCookieScheme = "AdminCookie";

		// 預設 Cookie 選項
		public static CookieOptions DefaultCookieOptions(HttpContext http) => new CookieOptions
		{
			HttpOnly = false,             // 前端可讀
			IsEssential = true,
			Path = "/",                   // 全站可見
			SameSite = SameSiteMode.Lax,  // 若跨站需求改用 None+Secure
			Secure = http.Request.IsHttps,
			Expires = DateTimeOffset.UtcNow.AddHours(4)
		};
	}
}
