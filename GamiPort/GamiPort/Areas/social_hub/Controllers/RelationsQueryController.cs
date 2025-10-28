// =============================================================
// RelationsQueryController（查詢版）— 支援可設定的「好友狀態」
// appsettings.json：
// "Relations": {
//   "AcceptedStatusCodes": [ "FRIEND", "ACCEPTED", "APPROVED", "MUTUAL" ],
//   "AcceptedStatusIds":   [ 2 ]   // 若你資料表使用固定的 status_id（例如 2 代表好友）
// }
// =============================================================
using System.Globalization;
using GamiPort.Infrastructure.Login;
using GamiPort.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[ApiController]
	[Route("social_hub/relations")]
	public sealed class RelationsQueryController : ControllerBase
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ILoginIdentity _login;
		private readonly IConfiguration _cfg;

		public RelationsQueryController(GameSpacedatabaseContext db, ILoginIdentity login, IConfiguration cfg)
		{
			_db = db;
			_login = login;
			_cfg = cfg;
		}

		[AllowAnonymous]
		[HttpGet("friends")]
		public async Task<IActionResult> Friends([FromQuery] bool onlyAccepted = true,
												 [FromQuery] int take = 12,
												 CancellationToken ct = default)
		{
			var me = await _login.GetAsync(ct);
			if (!me.IsAuthenticated || me.UserId is null || me.UserId <= 0)
				return Unauthorized(new { reason = "need_user", message = "請用 ?asUser=10000001 或設定 DevLogin:UserId" });

			var meId = me.UserId.Value;

			// 讀設定：哪些狀態算「好友」
			var acceptedCodes = _cfg.GetSection("Relations:AcceptedStatusCodes").Get<string[]>() ??
								new[] { "FRIEND", "ACCEPTED", "APPROVED", "MUTUAL" };
			var acceptedIds = _cfg.GetSection("Relations:AcceptedStatusIds").Get<int[]>() ?? Array.Empty<int>();

			var q = _db.Relations
				.AsNoTracking()
				.Include(r => r.Status)
				.Where(r => r.UserIdSmall == meId || r.UserIdLarge == meId);

			if (onlyAccepted)
			{
				// 同時支援以 code 或 id 過濾；兩者任一符合即視為好友
				q = q.Where(r =>
					acceptedIds.Contains(r.StatusId) ||
					acceptedCodes.Contains(r.Status.StatusCode));
			}

			var raw = await q
				.OrderByDescending(r => r.CreatedAt)
				.Take(Math.Clamp(take, 1, 100))
				.Select(r => new
				{
					FriendUserId = r.UserIdSmall == meId ? r.UserIdLarge : r.UserIdSmall,
					Nickname = r.FriendNickname,
					StatusId = r.StatusId,
					StatusCode = r.Status.StatusCode,
					StatusName = r.Status.StatusName,
					Since = r.CreatedAt
				})
				.ToListAsync(ct);

			var shaped = raw.Select(x => new
			{
				friendUserId = x.FriendUserId,
				displayName = !string.IsNullOrWhiteSpace(x.Nickname) ? x.Nickname : $"User #{x.FriendUserId}",
				nickname = x.Nickname,
				statusId = x.StatusId,
				statusCode = x.StatusCode,
				statusName = x.StatusName,
				sinceIso = x.Since.ToString("o", CultureInfo.InvariantCulture)
			});

			return Ok(shaped);
		}

		// ---------- 小工具：除錯用，列出我參與的所有關係與狀態 ----------
		[AllowAnonymous]
		[HttpGet("friends/debug")]
		public async Task<IActionResult> FriendsDebug(CancellationToken ct = default)
		{
			var me = await _login.GetAsync(ct);
			if (!me.IsAuthenticated || me.UserId is null || me.UserId <= 0)
				return Unauthorized(new { reason = "need_user" });

			var meId = me.UserId.Value;

			var list = await _db.Relations
				.AsNoTracking()
				.Include(r => r.Status)
				.Where(r => r.UserIdSmall == meId || r.UserIdLarge == meId)
				.OrderBy(r => r.CreatedAt)
				.Select(r => new {
					relationId = r.RelationId,
					userIdSmall = r.UserIdSmall,
					userIdLarge = r.UserIdLarge,
					statusId = r.StatusId,
					statusCode = r.Status.StatusCode,
					statusName = r.Status.StatusName,
					createdAtIso = r.CreatedAt
				})
				.ToListAsync(ct);

			return Ok(list.Select(x => new {
				x.relationId,
				x.userIdSmall,
				x.userIdLarge,
				x.statusId,
				x.statusCode,
				x.statusName,
				createdAtIso = x.createdAtIso.ToString("o", CultureInfo.InvariantCulture)
			}));
		}
	}
}
