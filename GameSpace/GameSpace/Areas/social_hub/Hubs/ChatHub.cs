using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GameSpace.Infrastructure.Login;

// ⬇️ 用別名避免命名衝突（與 Program.cs 一致）
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;

namespace GameSpace.Areas.social_hub.Hubs
{
	/// <summary>
	/// 1 對 1 使用者私訊 Hub（不含管理員對管理員）。
	/// 前端事件：
	///   - ReceiveDirect({ messageId, senderId, receiverId, content, sentAtIso })
	///   - ReadReceipt({ fromUserId, upToIso })
	///   - UnreadUpdate({ total, peerId, unread })
	///   - Error("NOT_LOGGED_IN" | "NO_PEER" | "BAD_TEXT")
	/// </summary>
	public class ChatHub : Hub
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ILogger<ChatHub> _logger;
		private readonly IMuteFilterAlias _mute; // 顯示端遮罩（只影響推送內容）
		private readonly ILoginIdentity _login;

		public ChatHub(GameSpacedatabaseContext db, ILogger<ChatHub> logger, IMuteFilterAlias mute, ILoginIdentity login)
		{
			_db = db;
			_logger = logger;
			_mute = mute;
			_login = login;
		}

		public override async Task OnConnectedAsync()
		{
			var (uid, _) = await GetIdentityAsync();
			if (uid > 0)
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(uid));
				_logger.LogInformation("User {Uid} connected to ChatHub.", uid);
			}
			else
			{
				await Clients.Caller.SendAsync("Error", "NOT_LOGGED_IN");
			}
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var (uid, _) = await GetIdentityAsync();
			if (uid > 0)
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(uid));
			await base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// 送訊息 →（必要時）建會話 → 存 DB(原文) → 推給雙方(遮罩後) → 推對方未讀
		/// ※ 只處理「一般使用者 DM」，不處理管理員對管理員。
		/// </summary>
		public async Task<object> SendMessageTo(int otherId, string text)
		{
			var (meId, _) = await GetIdentityAsync();
			if (meId <= 0) throw new HubException("NOT_LOGGED_IN");
			if (otherId <= 0 || otherId == meId) throw new HubException("NO_PEER");

			if (string.IsNullOrWhiteSpace(text)) throw new HubException("BAD_TEXT");
			text = text.Trim();
			if (text.Length > 255) text = text[..255];

			// ✅ 只有送訊息才建會話（一般 DM）
			var conv = await GetOrCreateConversationAsync(meId, otherId);
			var iAmP1 = (meId == conv.Party1Id);

			var now = DateTime.UtcNow;

			// ★ 更新會話排序時間（避免列表排序不對或 LastMessageAt 為 NULL）
			conv.LastMessageAt = now;

			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = text,  // DB 永遠存原文
				IsRead = false,
				EditedAt = now
			};

			_db.DmMessages.Add(msg);
			await _db.SaveChangesAsync(); // ← 此時 msg.MessageId 已有值

			// ✅ 廣播前遮罩（只影響顯示）
			var display = _mute.Apply(text);

			var payload = new
			{
				messageId = msg.MessageId,       // ★ 帶真實 ID，前端才能去重
				senderId = meId,
				receiverId = otherId,
				content = display,
				sentAtIso = msg.EditedAt.ToString("o")
			};

			await Clients.Group(UserGroup(meId)).SendAsync("ReceiveDirect", payload);
			await Clients.Group(UserGroup(otherId)).SendAsync("ReceiveDirect", payload);

			// 對方未讀更新
			await BroadcastUnreadAsync(otherId, meId);

			// Hub ack（前端可用但非必要）
			return new { ok = true, messageId = msg.MessageId, atIso = msg.EditedAt.ToString("o") };
		}

		/// <summary>
		/// 只做讀回執推播；實際清未讀由 HTTP/With 處理
		/// </summary>
		public async Task NotifyRead(int otherId, string? upToIso)
		{
			var (meId, _) = await GetIdentityAsync();
			if (meId <= 0 || otherId <= 0) return;

			if (!string.IsNullOrWhiteSpace(upToIso) &&
				DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out _))
			{
				await Clients.Group(UserGroup(otherId)).SendAsync("ReadReceipt", new { fromUserId = meId, upToIso });
			}
			else
			{
				await Clients.Group(UserGroup(otherId)).SendAsync("ReadReceipt", new { fromUserId = meId, upToIso = (string?)null });
			}
		}

		public async Task RegisterUser(string displayName)
		{
			var (meId, _) = await GetIdentityAsync();
			if (meId > 0 && !string.IsNullOrWhiteSpace(displayName))
				_logger.LogInformation("User {Uid} name: {Name}", meId, displayName.Trim());
		}

		// ===== helpers =====
		private async Task<(int meId, bool isManager)> GetIdentityAsync()
		{
			var r = await _login.GetAsync();
			return (r.EffectiveId, r.Kind == "manager");
		}

		private static string UserGroup(int userId) => $"U_{userId}";

		/// <summary>
		/// 取或建「一般 DM」會話（不含管理員 DM）
		/// - 建立時同步初始化 LastMessageAt，避免 NOT NULL 或排序問題
		/// - 併發建置時：若 Save 失敗再查一次，仍找不到就拋錯
		/// </summary>
		private async Task<DmConversation> GetOrCreateConversationAsync(int a, int b)
		{
			var conv = await _db.DmConversations
				.FirstOrDefaultAsync(c => !c.IsManagerDm &&
					((c.Party1Id == a && c.Party2Id == b) || (c.Party1Id == b && c.Party2Id == a)));

			if (conv != null) return conv;

			// 正規化（小的作為 p1）
			var p1 = Math.Min(a, b);
			var p2 = Math.Max(a, b);
			var now = DateTime.UtcNow;

			conv = new DmConversation
			{
				IsManagerDm = false,
				Party1Id = p1,
				Party2Id = p2,
				CreatedAt = now,
				LastMessageAt = now   // ★ 關鍵：初始化，避免 NULL / 排序錯
			};

			_db.DmConversations.Add(conv);
			try
			{
				await _db.SaveChangesAsync();
				return conv;
			}
			catch (DbUpdateException ex)
			{
				_logger.LogWarning(ex, "Create conversation race: {P1}-{P2}", p1, p2);
				var again = await _db.DmConversations
					.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2);
				if (again != null) return again;
				throw; // 真的建不起來 → 讓外層看到真正錯誤
			}
		}

		private async Task<(int total, int peerUnread)> ComputeUnreadAsync(int userId, int peerId)
		{
			var total = await _db.DmMessages
				.Where(m => !m.IsRead &&
					!m.Conversation.IsManagerDm &&
					(
						(m.Conversation.Party1Id == userId && !m.SenderIsParty1) ||
						(m.Conversation.Party2Id == userId && m.SenderIsParty1)
					))
				.CountAsync();

			var conv = await _db.DmConversations
				.FirstOrDefaultAsync(c => !c.IsManagerDm &&
					((c.Party1Id == userId && c.Party2Id == peerId) ||
					 (c.Party1Id == peerId && c.Party2Id == userId)));

			var peerUnread = 0;
			if (conv != null)
			{
				var iAmP1 = (conv.Party1Id == userId);
				peerUnread = await _db.DmMessages
					.Where(m => m.ConversationId == conv.ConversationId && !m.IsRead && m.SenderIsParty1 != iAmP1)
					.CountAsync();
			}
			return (total, peerUnread);
		}

		private async Task BroadcastUnreadAsync(int userId, int peerId)
		{
			var (total, peerUnread) = await ComputeUnreadAsync(userId, peerId);
			await Clients.Group(UserGroup(userId))
				.SendAsync("UnreadUpdate", new { total, peerId, unread = peerUnread });
		}
	}
}
