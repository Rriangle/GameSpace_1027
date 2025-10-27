using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.social_hub.Hubs;
using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Data;
using GameSpace.Infrastructure.Login;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
// 顯示端遮罩服務（只影響送到前端的內容）
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[SocialHubAuth]
	public class ChatController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IHubContext<ChatHub> _hub;
		private readonly IMuteFilterAlias _mute;
		private readonly ILoginIdentity _login;

		public ChatController(
			GameSpacedatabaseContext db,
			IHubContext<ChatHub> hub,
			IMuteFilterAlias mute,
			ILoginIdentity login)
		{
			_db = db;
			_hub = hub;
			_mute = mute;
			_login = login;
		}

		private async Task<(int meId, bool isManager)> GetIdentityAsync()
		{
			var r = await _login.GetAsync();
			return (r.EffectiveId, r.Kind == "manager");
		}

		private static bool AmIParty1(int meId, DmConversation c) => meId == c.Party1Id;
		private static string UserGroup(int userId) => $"U_{userId}";

		// ========= Index：會話清單 =========
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var (meId, isManager) = await GetIdentityAsync();
			if (meId <= 0) return RedirectToAction(nameof(NotLoggedIn));

			var convs = await _db.DmConversations
				.AsNoTracking()
				.Where(c =>
					(c.Party1Id == meId || c.Party2Id == meId) &&
					(isManager || !c.IsManagerDm))
				.Select(c => new
				{
					c.ConversationId,
					c.IsManagerDm,
					OtherId = (c.Party1Id == meId ? c.Party2Id : c.Party1Id),
					c.LastMessageAt,
					LastText = _db.DmMessages
						.Where(m => m.ConversationId == c.ConversationId)
						.OrderByDescending(m => m.EditedAt)
						.Select(m => m.MessageText)
						.FirstOrDefault()
				})
				.OrderByDescending(x => x.LastMessageAt)
				.ToListAsync();

			var convIds = convs.Select(x => x.ConversationId).ToList();

			var unreadList = await (
				from m in _db.DmMessages
				join c in _db.DmConversations on m.ConversationId equals c.ConversationId
				where convIds.Contains(m.ConversationId)
					&& !m.IsRead
					&& (isManager || !c.IsManagerDm)
					&& (
						(c.Party1Id == meId && !m.SenderIsParty1) ||
						(c.Party2Id == meId && m.SenderIsParty1)
					)
				group m by m.ConversationId into g
				select new { ConversationId = g.Key, Count = g.Count() }
			).ToListAsync();
			var unreadDict = unreadList.ToDictionary(x => x.ConversationId, x => x.Count);

			var conversations = convs.Select(x => new ConversationListItemVM
			{
				ConversationId = x.ConversationId,
				OtherId = x.OtherId,
				LastMessageAt = x.LastMessageAt,
				LastPreview = _mute.Apply(x.LastText ?? string.Empty),
				UnreadCount = unreadDict.TryGetValue(x.ConversationId, out var v) ? v : 0
			}).ToList();

			var totalUnread = await CountTotalUnreadAsync(meId, isManager);
			ViewBag.TotalUnread = totalUnread;

			return View(new ChatHomeVM
			{
				MeId = meId,
				Conversations = conversations,
				Contacts = new List<ContactItemVM>()
			});
		}

		// ========= With：只顯示既有會話；找不到就顯示空（不建立） =========
		[HttpGet]
		public async Task<IActionResult> With(int otherId)
		{
			var (meId, _) = await GetIdentityAsync();
			if (meId <= 0) return RedirectToAction(nameof(NotLoggedIn));
			if (otherId <= 0 || otherId == meId) return BadRequest("bad peer");

			var conv = await FindConversationAsync(meId, otherId);
			ViewBag.MeId = meId; // 前端 meta 使用

			if (conv == null)
			{
				return View(new ChatThreadVM
				{
					OtherId = otherId,
					Messages = new List<SimpleChatMessageVM>()
				});
			}

			// 清未讀（只清「對方傳來的未讀」）
			var iAmP1 = AmIParty1(meId, conv);
			var unread = await _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId && !m.IsRead && m.SenderIsParty1 != iAmP1)
				.ToListAsync();

			if (unread.Count > 0)
			{
				foreach (var m in unread) m.IsRead = true;
				await _db.SaveChangesAsync();

				var upToIso = unread.Max(m => m.EditedAt).ToUniversalTime().ToString("o");
				await _hub.Clients.Group(UserGroup(otherId))
					.SendAsync("ReadReceipt", new { fromUserId = meId, upToIso });
			}

			// 歷史訊息（最近 50 → 正序顯示），★ At 明確標成 UTC
			var history = await _db.DmMessages
				.AsNoTracking()
				.Where(m => m.ConversationId == conv.ConversationId)
				.OrderByDescending(m => m.EditedAt).Take(50)
				.OrderBy(m => m.EditedAt)
				.Select(m => new SimpleChatMessageVM
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Text = m.MessageText ?? string.Empty,
					At = DateTime.SpecifyKind(m.EditedAt, DateTimeKind.Utc), // ★ 這行是關鍵
					IsMine = (m.SenderIsParty1 == iAmP1),
					IsRead = m.IsRead,
					ReadAt = null
				})
				.ToListAsync();

			history.ForEach(m => m.Text = _mute.Apply(m.Text));

			return View(new ChatThreadVM
			{
				OtherId = otherId,
				Messages = history
			});
		}

		// ========= History：最近 50 筆；★ At 明確標成 UTC =========
		[HttpGet]
		public async Task<IActionResult> History(int otherId, string? beforeIso = null, int take = 50)
		{
			var (meId, _) = await GetIdentityAsync();
			if (meId <= 0) return Unauthorized();
			if (otherId <= 0 || otherId == meId) return BadRequest("bad peer");

			var conv = await FindConversationAsync(meId, otherId);
			if (conv == null) return Ok(Array.Empty<SimpleChatMessageVM>());

			var iAmP1 = AmIParty1(meId, conv);

			DateTime? before = null;
			if (!string.IsNullOrWhiteSpace(beforeIso) &&
				DateTime.TryParse(beforeIso, null, DateTimeStyles.RoundtripKind, out var parsed))
			{
				before = parsed;
			}

			var query = _db.DmMessages.AsNoTracking()
				.Where(m => m.ConversationId == conv.ConversationId);
			if (before.HasValue) query = query.Where(m => m.EditedAt < before.Value);

			var list = await query
				.OrderByDescending(m => m.EditedAt)
				.Take(Math.Clamp(take, 10, 200))
				.OrderBy(m => m.EditedAt)
				.Select(m => new SimpleChatMessageVM
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Text = m.MessageText ?? string.Empty,
					At = DateTime.SpecifyKind(m.EditedAt, DateTimeKind.Utc), // ★ 關鍵
					IsMine = (m.SenderIsParty1 == iAmP1),
					IsRead = m.IsRead,
					ReadAt = null
				})
				.ToListAsync();

			list.ForEach(it => it.Text = _mute.Apply(it.Text));
			return Ok(list);
		}

		// ========= Send（HTTP 後備）— 只有這裡會建立對話；回傳 UTC ISO =========
		[HttpPost]
		public async Task<IActionResult> Send(int otherId, string text)
		{
			var (meId, isManager) = await GetIdentityAsync();
			if (meId <= 0) return Unauthorized();
			if (otherId <= 0 || otherId == meId) return BadRequest("bad peer");
			if (string.IsNullOrWhiteSpace(text)) return BadRequest("empty");

			text = text.Trim();
			if (text.Length > 255) text = text[..255];

			var conv = await FindConversationAsync(meId, otherId)
				?? await GetOrCreateConversationAsync(meId, otherId, preferManagerDm: isManager);

			var iAmP1 = AmIParty1(meId, conv);

			var now = DateTime.UtcNow;
			conv.LastMessageAt = now;

			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = text,   // DB 永遠存原文
				IsRead = false,
				EditedAt = now
			};

			_db.DmMessages.Add(msg);
			await _db.SaveChangesAsync(); // 這時 msg.MessageId 有值

			// 廣播用遮罩後文字
			var display = _mute.Apply(text);
			var payload = new
			{
				messageId = msg.MessageId,
				senderId = meId,
				receiverId = otherId,
				content = display,
				sentAtIso = msg.EditedAt.ToString("o") // UTC ISO
			};

			await _hub.Clients.Group(UserGroup(meId)).SendAsync("ReceiveDirect", payload);
			await _hub.Clients.Group(UserGroup(otherId)).SendAsync("ReceiveDirect", payload);

			await BroadcastUnreadAsync(otherId, meId);

			// ★ 回傳同時帶 at 與 atIso（都為 UTC ISO），前端怎麼取都 OK
			var atIso = msg.EditedAt.ToUniversalTime().ToString("o");
			return Ok(new { ok = true, messageId = msg.MessageId, at = atIso, atIso, text = display });
		}

		// ========= 前景時手動讀回執（只推播，不動資料） =========
		[HttpPost]
		public async Task<IActionResult> MarkRead(int otherId, string? upToIso)
		{
			var (meId, _) = await GetIdentityAsync();
			if (meId <= 0) return Unauthorized();

			if (!string.IsNullOrWhiteSpace(upToIso) &&
				DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out _))
			{
				await _hub.Clients.Group(UserGroup(otherId))
					.SendAsync("ReadReceipt", new { fromUserId = meId, upToIso });
			}
			else
			{
				await _hub.Clients.Group(UserGroup(otherId))
					.SendAsync("ReadReceipt", new { fromUserId = meId, upToIso = (string?)null });
			}

			var (total, peerUnread) = await ComputeUnreadAsync(meId, otherId);
			return Ok(new { total, peerId = otherId, unread = peerUnread });
		}

		// ========= 未讀摘要 =========
		[HttpGet]
		public async Task<IActionResult> UnreadSummary(int? peerId)
		{
			var (meId, _) = await GetIdentityAsync();
			if (meId <= 0) return Unauthorized();

			var (total, peerUnread) = await ComputeUnreadAsync(meId, peerId ?? 0);
			return Ok(new { total, peerId = peerId ?? 0, unread = peerUnread });
		}

		// ========= 聯絡人（使用者：好友；管理員：所有管理員） =========
		[HttpGet]
		public async Task<IActionResult> Contacts()
		{
			var (meId, isManager) = await GetIdentityAsync();
			if (meId <= 0) return Unauthorized();

			if (isManager)
			{
				var managers = await _db.ManagerData
					.AsNoTracking()
					.Where(m => m.ManagerId != meId)
					.Select(m => new { m.ManagerId, m.ManagerName })
					.ToListAsync();

				var peerIds = managers.Select(x => x.ManagerId).ToList();

				var convs = await _db.DmConversations
					.AsNoTracking()
					.Where(c => c.IsManagerDm &&
						((c.Party1Id == meId && peerIds.Contains(c.Party2Id)) ||
						 (c.Party2Id == meId && peerIds.Contains(c.Party1Id))))
					.Select(c => new { c.ConversationId, OtherId = (c.Party1Id == meId ? c.Party2Id : c.Party1Id), c.LastMessageAt })
					.ToListAsync();

				var convIds = convs.Select(c => c.ConversationId).ToList();

				var unreadDict = convIds.Count == 0
					? new Dictionary<int, int>()
					: (await (
						from m in _db.DmMessages
						join c in _db.DmConversations on m.ConversationId equals c.ConversationId
						where convIds.Contains(m.ConversationId)
							&& !m.IsRead
							&& c.IsManagerDm
							&& (
								(c.Party1Id == meId && !m.SenderIsParty1) ||
								(c.Party2Id == meId && m.SenderIsParty1)
							)
						group m by m.ConversationId into g
						select new { ConversationId = g.Key, Count = g.Count() }
					).ToListAsync()).ToDictionary(x => x.ConversationId, x => x.Count);

				var list = managers
					.Select(m =>
					{
						var conv = convs.FirstOrDefault(c => c.OtherId == m.ManagerId);
						var lastAt = conv?.LastMessageAt;
						var unread = (conv != null && unreadDict.TryGetValue(conv.ConversationId, out var v)) ? v : 0;
						return new ContactItemVM
						{
							Id = m.ManagerId,
							Name = string.IsNullOrWhiteSpace(m.ManagerName) ? $"Manager {m.ManagerId}" : m.ManagerName,
							Nick = null,
							Unread = unread,
							LastAt = lastAt
						};
					})
					.OrderByDescending(x => x.LastAt)
					.ToList();

				return Json(list);
			}
			else
			{
				var acceptedId = await _db.RelationStatuses
					.AsNoTracking()
					.Where(s => s.StatusCode == "ACCEPTED")
					.Select(s => (int?)s.StatusId)
					.FirstOrDefaultAsync();

				if (acceptedId is null)
					return Json(new List<ContactItemVM>());

				var friendPairs = await _db.Relations
					.AsNoTracking()
					.Where(r => r.StatusId == acceptedId &&
								(r.UserIdSmall == meId || r.UserIdLarge == meId))
					.Select(r => new
					{
						FriendId = (r.UserIdSmall == meId ? r.UserIdLarge : r.UserIdSmall),
						r.FriendNickname
					})
					.ToListAsync();

				if (friendPairs.Count == 0)
					return Json(new List<ContactItemVM>());

				var friendIds = friendPairs.Select(x => x.FriendId).Distinct().ToList();

				var friendNames = await _db.Users
					.AsNoTracking()
					.Where(u => friendIds.Contains(u.UserId))
					.Select(u => new { u.UserId, u.UserName })
					.ToListAsync();
				var nameDict = friendNames.ToDictionary(x => x.UserId, x => x.UserName);

				var convs = await _db.DmConversations
					.AsNoTracking()
					.Where(c => !c.IsManagerDm &&
						((c.Party1Id == meId && friendIds.Contains(c.Party2Id)) ||
						 (c.Party2Id == meId && friendIds.Contains(c.Party1Id))))
					.Select(c => new { c.ConversationId, OtherId = (c.Party1Id == meId ? c.Party2Id : c.Party1Id), c.LastMessageAt })
					.ToListAsync();

				var convIds = convs.Select(c => c.ConversationId).ToList();

				var unreadDict = convIds.Count == 0
					? new Dictionary<int, int>()
					: (await (
						from m in _db.DmMessages
						join c in _db.DmConversations on m.ConversationId equals c.ConversationId
						where convIds.Contains(m.ConversationId)
							&& !m.IsRead
							&& !c.IsManagerDm
							&& (
								(c.Party1Id == meId && !m.SenderIsParty1) ||
								(c.Party2Id == meId && m.SenderIsParty1)
							)
						group m by m.ConversationId into g
						select new { ConversationId = g.Key, Count = g.Count() }
					).ToListAsync()).ToDictionary(x => x.ConversationId, x => x.Count);

				var nickDict = friendPairs.GroupBy(x => x.FriendId)
					.ToDictionary(g => g.Key, g => g.Select(z => z.FriendNickname).FirstOrDefault(fn => !string.IsNullOrWhiteSpace(fn)));

				var list = friendIds
					.Select(fid =>
					{
						var conv = convs.FirstOrDefault(c => c.OtherId == fid);
						var lastAt = conv?.LastMessageAt;
						var unread = (conv != null && unreadDict.TryGetValue(conv.ConversationId, out var v)) ? v : 0;
						return new ContactItemVM
						{
							Id = fid,
							Name = nameDict.TryGetValue(fid, out var nm) && !string.IsNullOrWhiteSpace(nm) ? nm : $"User {fid}",
							Nick = nickDict.TryGetValue(fid, out var nk) ? nk : null,
							Unread = unread,
							LastAt = lastAt
						};
					})
					.OrderByDescending(x => x.LastAt)
					.ToList();

				return Json(list);
			}
		}

		[HttpGet]
		public IActionResult NotLoggedIn() => View();

		// ====================== 私有：共用查詢/計算 ======================

		private async Task<DmConversation?> FindConversationAsync(int a, int b)
		{
			return await _db.DmConversations
				.FirstOrDefaultAsync(c =>
					((c.Party1Id == a && c.Party2Id == b) || (c.Party1Id == b && c.Party2Id == a)) &&
					c.IsManagerDm)
				?? await _db.DmConversations
				.FirstOrDefaultAsync(c =>
					!c.IsManagerDm &&
					((c.Party1Id == a && c.Party2Id == b) || (c.Party1Id == b && c.Party2Id == a)));
		}

		// ChatController.cs  ★ 取代既有的 ShouldBeManagerDmAsync
		private async Task<bool> ShouldBeManagerDmAsync(int a, int b, bool meIsManager)
		{
			if (!meIsManager) return false;

			// ★ FIX：直接用 ManagerData 驗證雙方都是管理員（而不是靠是否已有管理員 DM 紀錄）
			var ids = new[] { a, b };
			var count = await _db.ManagerData
				.AsNoTracking()
				.CountAsync(m => ids.Contains(m.ManagerId));

			return count == 2;
		}


		// ChatController.cs  ★ 覆寫 GetOrCreateConversationAsync 內決策 IsManagerDm 的那行
		private async Task<DmConversation> GetOrCreateConversationAsync(int a, int b, bool preferManagerDm)
		{
			var found = await FindConversationAsync(a, b);
			if (found != null) return found;

			// ★ FIX：preferManagerDm（我方是管理員）且「雙方皆存在於 ManagerData」=> 才建立管理員 DM
			var isMgrDm = preferManagerDm && await ShouldBeManagerDmAsync(a, b, true);

			var p1 = Math.Min(a, b);
			var p2 = Math.Max(a, b);
			var now = DateTime.UtcNow;

			var conv = new DmConversation
			{
				IsManagerDm = isMgrDm,
				Party1Id = p1,
				Party2Id = p2,
				CreatedAt = now,
				LastMessageAt = now
			};

			_db.DmConversations.Add(conv);
			try
			{
				await _db.SaveChangesAsync();
				return conv;
			}
			catch (DbUpdateException)
			{
				// 競態下重查一次
				var again = await FindConversationAsync(a, b);
				if (again != null) return again;
				throw;
			}
		}


		private async Task<(int total, int peerUnread)> ComputeUnreadAsync(int userId, int peerId)
		{
			var targetIsManager = await _db.DmConversations
				.AsNoTracking()
				.AnyAsync(c => c.IsManagerDm && (c.Party1Id == userId || c.Party2Id == userId));

			var total = await _db.DmMessages
				.Where(m => !m.IsRead &&
					(targetIsManager || !m.Conversation.IsManagerDm) &&
					(
						(m.Conversation.Party1Id == userId && !m.SenderIsParty1) ||
						(m.Conversation.Party2Id == userId && m.SenderIsParty1)
					))
				.CountAsync();

			var conv = await FindConversationAsync(userId, peerId);
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
			await _hub.Clients.Group(UserGroup(userId))
				.SendAsync("UnreadUpdate", new { total, peerId, unread = peerUnread });
		}

		private async Task<int> CountTotalUnreadAsync(int meId, bool isManager)
		{
			return await _db.DmMessages
				.Where(m => !m.IsRead &&
					(isManager || !m.Conversation.IsManagerDm) &&
					(
						(m.Conversation.Party1Id == meId && !m.SenderIsParty1) ||
						(m.Conversation.Party2Id == meId && m.SenderIsParty1)
					))
				.CountAsync();
		}
	}
}
