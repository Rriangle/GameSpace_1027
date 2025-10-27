using GamiPort.Infrastructure.Login; // ILoginIdentity
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Infrastructure.Security
{
	/// <summary>
	/// IAppCurrentUser 的預設實作（只改你這邊的程式碼）。
	/// 讀取順序：AppUserId -> NameIdentifier(int) -> NameIdentifier("U:<id>") -> ILoginIdentity（DB備援）
	/// </summary>
	public sealed class AppCurrentUser : IAppCurrentUser
	{
		private const string CacheKeyUserId = "app.uid";

		private readonly IHttpContextAccessor _http;
		private readonly ILoginIdentity _loginIdentity;

		public AppCurrentUser(IHttpContextAccessor http, ILoginIdentity loginIdentity)
		{
			_http = http;
			_loginIdentity = loginIdentity;
		}

		public ClaimsPrincipal Principal => _http.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

		public bool IsAuthenticated => Principal.Identity?.IsAuthenticated ?? false;

		/// <summary>
		/// 僅從 Claims 快速推得整數 UserId；不做 DB（拿不到回 0）。
		/// </summary>
		public int UserId
		{
			get
			{
				// 先看是否已有每請求快取
				var http = _http.HttpContext;
				if (http?.Items.TryGetValue(CacheKeyUserId, out var cached) == true
					&& cached is int cachedId && cachedId > 0)
				{
					return cachedId;
				}

				// 1) AppUserId（方案A：登入時就補這顆）
				if (TryGetIntClaim("AppUserId", out var id))
					return id;

				// 2) NameIdentifier 直接是整數
				if (TryGetIntClaim(ClaimTypes.NameIdentifier, out id))
					return id;

				// 3) NameIdentifier 若是 "U:123" 這種格式，解析
				if (TryParseUidFromNameIdentifier(out id))
					return id;

				return 0;
			}
		}

		public string? NickName
		{
			get
			{
				var nick = Principal.FindFirst("UserNickName")?.Value;
				if (!string.IsNullOrWhiteSpace(nick)) return nick;
				return Principal.Identity?.Name;
			}
		}

		public string? Email => Principal.FindFirst(ClaimTypes.Email)?.Value;

		public async ValueTask<int> GetUserIdAsync(CancellationToken ct = default)
		{
			var http = _http.HttpContext;

			// 每請求快取
			if (http?.Items.TryGetValue(CacheKeyUserId, out var cached) == true
				&& cached is int cachedId && cachedId > 0)
			{
				return cachedId;
			}

			// 先用 Claims 直接推（不打 DB）
			var id = UserId;
			if (id > 0)
			{
				if (http != null) http.Items[CacheKeyUserId] = id;
				return id;
			}

			// 最後才以 ILoginIdentity 備援（內部可能會去 DB 映射）
			var login = await _loginIdentity.GetAsync(ct);
			if (login.IsAuthenticated && login.UserId is > 0)
			{
				if (http != null) http.Items[CacheKeyUserId] = login.UserId.Value;
				return login.UserId.Value;
			}

			return 0;
		}

		// ---------- helpers ----------

		private bool TryGetIntClaim(string claimType, out int id)
		{
			id = 0;
			var val = Principal.FindFirst(claimType)?.Value;
			return int.TryParse(val, out id) && id > 0;
		}

		private bool TryParseUidFromNameIdentifier(out int id)
		{
			id = 0;
			var val = Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrWhiteSpace(val)) return false;

			// 支援 "U:123" 格式
			if (val.StartsWith("U:", StringComparison.OrdinalIgnoreCase))
			{
				var part = val.Substring(2).Trim();
				return int.TryParse(part, out id) && id > 0;
			}
			return false;
		}
	}
}
