// Infrastructure/Login/ClaimFirstLoginIdentity.cs
// 用途：統一取得目前登入使用者的整數 UserId（正式登入優先，不用 dev 回退）。
// 流程：1) Claim("AppUserId") → 2) 解析 IdentityUser.Id = "U:<id>" → 3) UserName 對應 Users 表。

using System.Security.Claims;
using GamiPort.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Infrastructure.Login
{
	public sealed class ClaimFirstLoginIdentity : ILoginIdentity
	{
		private readonly IHttpContextAccessor _http;
		private readonly GameSpacedatabaseContext _db;

		public ClaimFirstLoginIdentity(IHttpContextAccessor http, GameSpacedatabaseContext db)
		{
			_http = http;
			_db = db;
		}

		public async Task<LoginSnapshot> GetAsync(CancellationToken ct = default)
		{
			var ctx = _http.HttpContext;
			var user = ctx?.User;

			if (user?.Identity?.IsAuthenticated == true)
			{
				// 1) 直接讀自訂 Claim（你 LoginController 有寫入 AppUserId；AppClaimsFactory 也會補）
				var raw =
					user.FindFirst("AppUserId")?.Value ??
					user.FindFirst("gp_uid")?.Value ??         // 相容舊稱（若不需要可移除）
					user.FindFirst("gami:user_id")?.Value;

				if (int.TryParse(raw, out var uid) && uid > 0)
					return new LoginSnapshot(true, uid, "Claim",
						user.Identity?.Name, user.FindFirstValue(ClaimTypes.NameIdentifier));

				// 2) 解析 IdentityUser.Id = "U:<id>"
				var nameId = user.FindFirstValue(ClaimTypes.NameIdentifier);
				if (!string.IsNullOrEmpty(nameId) && nameId.StartsWith("U:")
					&& int.TryParse(nameId.AsSpan(2), out var fromId) && fromId > 0)
					return new LoginSnapshot(true, fromId, "IdentityId",
						user.Identity?.Name, nameId);

				// 3) 以 UserName 對應 Users 表（雙保險）
				var name = user.Identity?.Name;
				if (!string.IsNullOrWhiteSpace(name))
				{
					var map = await _db.Users.AsNoTracking()
						.Where(u => u.UserAccount == name || u.UserName == name)
						.Select(u => (int?)u.UserId)
						.FirstOrDefaultAsync(ct);

					if (map is > 0)
						return new LoginSnapshot(true, map, "AccountMap", name, nameId);
				}
			}

			// 未登入或無法解析 → 明確回傳未登入
			return new LoginSnapshot(false, null, "None");
		}
	}
}
