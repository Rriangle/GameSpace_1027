using System.Globalization;
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.SignalR;
using GamiPort.Areas.social_hub.SignalR.Contracts;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>DM 服務（EF Core 操作）</summary>
	public sealed class ChatService : IChatService
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly MessagePolicyOptions _policy;

		public ChatService(GameSpacedatabaseContext db, IOptions<MessagePolicyOptions> policy)
		{ _db = db; _policy = policy.Value ?? new MessagePolicyOptions(); }

		public async Task<DirectMessagePayload> SendDirectAsync(int senderId, int recipientId, string content, CancellationToken ct = default)
		{
			if (senderId <= 0) throw new InvalidOperationException(ErrorCodes.NotLoggedIn);
			if (recipientId <= 0 || recipientId == senderId) throw new InvalidOperationException(ErrorCodes.NoPeer);

			await EnsureUserExistsAsync(senderId, ct);
			await EnsureUserExistsAsync(recipientId, ct);

			if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException(ErrorCodes.BadText);
			content = content.Trim();
			if (content.Length > _policy.MaxContentLength) content = content[.._policy.MaxContentLength];

			var conv = await GetOrCreateConversationAsync(senderId, recipientId, ct);
			var iAmP1 = senderId == conv.Party1Id;
			var now = DateTime.UtcNow;

			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = content,
				IsRead = false,
				EditedAt = now
			};
			_db.DmMessages.Add(msg);
			await _db.SaveChangesAsync(ct);

			return new DirectMessagePayload
			{
				MessageId = msg.MessageId,
				SenderId = senderId,
				ReceiverId = recipientId,
				Content = content,
				SentAtIso = now.ToString("o", CultureInfo.InvariantCulture) // UTC ISO
			};
		}

		public async Task<ReadReceiptPayload> MarkReadAsync(int readerId, int otherId, DateTime upToUtc, CancellationToken ct = default)
		{
			if (readerId <= 0) throw new InvalidOperationException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == readerId) throw new InvalidOperationException(ErrorCodes.NoPeer);

			await EnsureUserExistsAsync(readerId, ct);
			await EnsureUserExistsAsync(otherId, ct);

			var conv = await GetOrCreateConversationAsync(readerId, otherId, ct);
			var iAmP1 = readerId == conv.Party1Id;

			await _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId && !m.IsRead && m.SenderIsParty1 != iAmP1 && m.EditedAt <= upToUtc)
				.ExecuteUpdateAsync(s => s
					.SetProperty(m => m.IsRead, true)
					.SetProperty(m => m.ReadAt, DateTime.UtcNow), ct);

			return new ReadReceiptPayload
			{
				FromUserId = readerId,
				UpToIso = upToUtc.ToString("o", CultureInfo.InvariantCulture) // UTC ISO
			};
		}

		public async Task<IReadOnlyList<DirectMessagePayload>> GetHistoryAsync(int userId, int otherId, DateTime? afterUtc, int pageSize, CancellationToken ct = default)
		{
			if (userId <= 0) throw new InvalidOperationException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == userId) throw new InvalidOperationException(ErrorCodes.NoPeer);

			await EnsureUserExistsAsync(userId, ct);
			await EnsureUserExistsAsync(otherId, ct);

			var p1 = Math.Min(userId, otherId);
			var p2 = Math.Max(userId, otherId);

			var conv = await _db.DmConversations.AsNoTracking()
				.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2, ct);

			if (conv == null) return Array.Empty<DirectMessagePayload>();

			IQueryable<DmMessage> q = _db.DmMessages.AsNoTracking().Where(m => m.ConversationId == conv.ConversationId);

			if (afterUtc.HasValue) q = q.Where(m => m.EditedAt > afterUtc.Value).OrderBy(m => m.EditedAt);
			else q = q.OrderByDescending(m => m.EditedAt).Take(pageSize).OrderBy(m => m.EditedAt);

			// ★ 關鍵：取出基礎欄位後在記憶體端指定為 UTC，再輸出 ISO
			var raw = await q.Select(m => new
			{
				m.MessageId,
				m.SenderIsParty1,
				m.MessageText,
				m.EditedAt
			}).ToListAsync(ct);

			return raw.Select(m =>
			{
				// 某些舊資料 Kind 可能是 Unspecified；指定為 UTC 以利前端正確轉台灣時區
				var utc = DateTime.SpecifyKind(m.EditedAt, DateTimeKind.Utc);
				return new DirectMessagePayload
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Content = m.MessageText,
					SentAtIso = utc.ToString("o", CultureInfo.InvariantCulture)
				};
			}).ToList();
		}

		public async Task<(int total, int peer)> ComputeUnreadAsync(int userId, int peerId, CancellationToken ct = default)
		{
			var conv = await GetOrCreateConversationAsync(userId, peerId, ct);
			var iAmP1 = userId == conv.Party1Id;

			var peerUnread = await _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId && !m.IsRead && m.SenderIsParty1 != iAmP1)
				.CountAsync(ct);

			var totalUnread = await (
				from m in _db.DmMessages
				join c in _db.DmConversations on m.ConversationId equals c.ConversationId
				where !c.IsManagerDm && !m.IsRead &&
					  ((c.Party1Id == userId && m.SenderIsParty1 == false) || (c.Party2Id == userId && m.SenderIsParty1 == true))
				select m
			).CountAsync(ct);

			return (totalUnread, peerUnread);
		}

		public async Task<int> ComputeTotalUnreadAsync(int userId, CancellationToken ct = default)
		{
			return await (
				from m in _db.DmMessages
				join c in _db.DmConversations on m.ConversationId equals c.ConversationId
				where !c.IsManagerDm && !m.IsRead &&
					  ((c.Party1Id == userId && m.SenderIsParty1 == false) || (c.Party2Id == userId && m.SenderIsParty1 == true))
				select m
			).CountAsync(ct);
		}

		// ---------- EF helpers ----------
		private async Task EnsureUserExistsAsync(int userId, CancellationToken ct)
		{
			var exists = await _db.Users.AnyAsync(u => u.UserId == userId, ct);
			if (!exists) throw new InvalidOperationException(ErrorCodes.UserNotFound);
		}
		private async Task<DmConversation> GetOrCreateConversationAsync(int a, int b, CancellationToken ct)
		{
			var p1 = Math.Min(a, b);
			var p2 = Math.Max(a, b);
			var conv = await _db.DmConversations.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2, ct);
			if (conv != null) return conv;

			conv = new DmConversation { IsManagerDm = false, Party1Id = p1, Party2Id = p2, CreatedAt = DateTime.UtcNow };
			_db.DmConversations.Add(conv);
			await _db.SaveChangesAsync(ct);
			return conv;
		}
	}
}
