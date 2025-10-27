using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
// ✅ 改：改用我們自己的統一介面來「吃登入」；不再依賴 ILoginIdentity
using GamiPort.Infrastructure.Security; // IAppCurrentUser

namespace GamiPort.Areas.social_hub.Controllers
{
	/// <summary>
	/// 一對一聊天：歷史查詢 / 好友最新預覽 + 未讀
	/// （即時傳送、已讀回報放在 SignalR Hub）
	/// 【重點】本版改為注入 IAppCurrentUser 來讀取目前登入者：
	///   - 快速路徑：直接讀取 Claims("AppUserId") -> _me.UserId（無 DB）
	///   - 備援路徑：_me.GetUserIdAsync()（解析 NameIdentifier 或內部呼叫 ILoginIdentity 做 DB 對應）
	/// </summary>
	[Area("social_hub")]
	[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
	public sealed class ChatController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IAppCurrentUser _me; // ✅ 改：統一入口，集中「吃登入」邏輯

		public ChatController(GameSpacedatabaseContext db, IAppCurrentUser me)
		{
			_db = db;
			_me = me;
		}

		// ==========================================================
		// 共用：目前使用者（只吃正式登入 Cookie）
		// 讀取順序：
		//   1) _me.UserId：直接讀 Claims("AppUserId")（最快、無 DB）
		//   2) _me.GetUserIdAsync()：備援解析 NameIdentifier 或呼叫 ILoginIdentity 做一次 DB 對應
		// ==========================================================
		private async Task<int> GetMeAsync()
		{
			// 快速路徑：_me.UserId 會把 "AppUserId" Claim 轉成 int（沒有就 0）
			var uid = _me.UserId;
			if (uid > 0) return uid;

			// 備援路徑：必要時才呼叫（可能解析 NameIdentifier 或透過 ILoginIdentity 做一次 DB 對應）
			return await _me.GetUserIdAsync();
		}

		/// <summary>確認 Users 中是否存在（你的主鍵屬性是 UserId）</summary>
		private Task<bool> ExistsAsync(int userId) =>
			_db.Users.AsNoTracking().AnyAsync(u => u.UserId == userId);

		/// <summary>統一把時間輸出成 UTC ISO8601（防守性再 ToUniversalTime 一次）</summary>
		private static string UtcIso(DateTime dt)
		{
			// 若 DB 取回來是 Unspecified（datetime2 常見），把它「指定為 UTC」
			if (dt.Kind == DateTimeKind.Unspecified)
				dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

			// Local 就轉成 UTC；UTC 直接用
			if (dt.Kind == DateTimeKind.Local)
				dt = dt.ToUniversalTime();

			return dt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
		}

		// ==========================================================
		// 1) 歷史（分頁）
		//    GET /social_hub/Chat/History?otherId=1002&take=20
		//    方向：
		//      - beforeIso：抓更舊（往上捲），結果會整理為「舊→新」
		//      - direction=latest：進對話第一次就抓最新 N 筆（結果「舊→新」）
		//      - afterIso：抓更新（往下補），結果「舊→新」
		// ==========================================================
		[HttpGet]
		public async Task<IActionResult> History(
			int otherId,
			string? afterIso,
			string? beforeIso,
			int take = 20,
			string? direction = null)
		{
			var me = await GetMeAsync();
			if (me <= 0) return Unauthorized();
			if (otherId <= 0 || otherId == me) return BadRequest();

			// 僅允許查詢「存在的我 / 對方」
			if (!await ExistsAsync(me) || !await ExistsAsync(otherId)) return NotFound();

			// 解析時間
			DateTime? after = null, before = null;
			if (!string.IsNullOrWhiteSpace(afterIso) &&
				DateTime.TryParse(afterIso, null, DateTimeStyles.RoundtripKind, out var a)) after = a;
			if (!string.IsNullOrWhiteSpace(beforeIso) &&
				DateTime.TryParse(beforeIso, null, DateTimeStyles.RoundtripKind, out var b)) before = b;

			take = Math.Clamp(take, 1, 100);

			// 找會話（僅 me/other 之間）
			var p1 = Math.Min(me, otherId);
			var p2 = Math.Max(me, otherId);

			var conv = await _db.DmConversations
				.AsNoTracking()
				.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2);

			// 若尚未對話，直接回空陣列（符合前端期待）
			if (conv == null) return Json(Array.Empty<object>());

			var iAmP1 = (me == conv.Party1Id);

			// 共同投影器（避免重複寫）
			IEnumerable<object> Project(IEnumerable<DmMessage> src) =>
				src.Select(m => new
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Content = m.MessageText,
					SentAtIso = UtcIso(m.EditedAt),
					IsMine = (m.SenderIsParty1 == iAmP1),
					IsRead = m.IsRead
				});

			// ---------- A) beforeIso：載入更舊 ----------
			if (before.HasValue)
			{
				var older = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == conv.ConversationId && m.EditedAt < before.Value)
					.OrderByDescending(m => m.EditedAt) // 先抓新→舊
					.Take(take)
					.ToListAsync();

				older.Reverse(); // 轉成舊→新
				return Json(Project(older));
			}

			// ---------- B) latest：進對話第一次載入 ----------
			if (string.Equals(direction, "latest", StringComparison.OrdinalIgnoreCase))
			{
				var latest = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == conv.ConversationId)
					.OrderByDescending(m => m.EditedAt)
					.Take(take)
					.ToListAsync();

				latest.Reverse(); // 舊→新
				return Json(Project(latest));
			}

			// ---------- C) afterIso：載入更新 ----------
			if (after.HasValue)
			{
				var newer = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == conv.ConversationId && m.EditedAt > after.Value)
					.OrderBy(m => m.EditedAt) // 原本就舊→新
					.Take(take)
					.ToListAsync();

				return Json(Project(newer));
			}

			// ---------- D) 預設 = latest ----------
			var def = await _db.DmMessages.AsNoTracking()
				.Where(m => m.ConversationId == conv.ConversationId)
				.OrderByDescending(m => m.EditedAt)
				.Take(take)
				.ToListAsync();

			def.Reverse();
			return Json(Project(def));
		}

		// ==========================================================
		// 2) 好友們的「最新」 + 未讀
		//    用途：好友清單一次拿到多人的預覽/未讀（可選用）
		//    GET /social_hub/Chat/PeersLatest?peerIds=1001,1002
		//    備註：若 peerIds 為空，回覆所有「我參與的會話」的對象；
		//          沒有會話的好友自然就不會出現在這個清單（FriendDock 仍可個別 call History 取不到即顯示「尚無訊息」）
		// ==========================================================
		public sealed record PeerLatestDto(int PeerId, string? LastContent, string? LastIso, int Unread);

		[HttpGet]
		public async Task<IActionResult> PeersLatest(string? peerIds = null)
		{
			var me = await GetMeAsync();
			if (me <= 0) return Unauthorized();

			List<int>? filter = null;
			if (!string.IsNullOrWhiteSpace(peerIds))
			{
				filter = peerIds
					.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
					.Select(s => int.TryParse(s, out var v) ? v : 0)
					.Where(v => v > 0 && v != me)
					.Distinct()
					.ToList();
			}

			// 先抓我參與的所有會話（或僅限於指定 peerIds）
			var convs = await _db.DmConversations.AsNoTracking()
				.Where(c => !c.IsManagerDm && (c.Party1Id == me || c.Party2Id == me))
				.Select(c => new
				{
					c.ConversationId,
					PeerId = (c.Party1Id == me ? c.Party2Id : c.Party1Id),
					IAmP1 = (c.Party1Id == me)
				})
				.ToListAsync();

			if (filter is { Count: > 0 })
				convs = convs.Where(c => filter.Contains(c.PeerId)).ToList();

			var list = new List<PeerLatestDto>(capacity: convs.Count);

			// 逐一計算（通常朋友數量不大；要極致效能可以用 GroupBy / CROSS APPLY 寫成單查詢）
			foreach (var c in convs)
			{
				// 最新一則
				var last = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == c.ConversationId)
					.OrderByDescending(m => m.EditedAt)
					.Select(m => new { m.MessageText, m.EditedAt })
					.FirstOrDefaultAsync();

				// 未讀數：對「我」而言，對方發的且未讀
				var unread = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == c.ConversationId
							 && !m.IsRead
							 && m.SenderIsParty1 != c.IAmP1)
					.CountAsync();

				list.Add(new PeerLatestDto(
					PeerId: c.PeerId,
					LastContent: last?.MessageText ?? "",
					LastIso: last is null ? null : UtcIso(last.EditedAt),
					Unread: unread
				));
			}

			// 依最新時間排序（null 放最後）
			list = list
				.OrderByDescending(x =>
				{
					if (string.IsNullOrEmpty(x.LastIso)) return DateTime.MinValue;
					return DateTime.Parse(x.LastIso, null, DateTimeStyles.RoundtripKind);
				})
				.ToList();

			return Json(list);
		}
	}
}
