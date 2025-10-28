using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.SignalR;
using GamiPort.Areas.social_hub.SignalR.Contracts;
// ✅ 改：改用我們自己的統一介面來「吃登入」；不再依賴 ILoginIdentity
using GamiPort.Infrastructure.Security; // IAppCurrentUser
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;     // ✅ 新增：用於記錄連線例外

namespace GamiPort.Areas.social_hub.Hubs
{
	/// <summary>
	/// 一對一聊天的 SignalR Hub（即時傳送、已讀回報、未讀彙總）。
	/// 【重點】改為注入 IAppCurrentUser 來讀取目前登入者：
	///   - 快速路徑：直接讀取 Claims("AppUserId") -> _me.UserId（無 DB）
	///   - 備援路徑：_me.GetUserIdAsync()（解析 NameIdentifier 或內部呼叫 ILoginIdentity 做 DB 對應）
	/// 另：在 OnConnected/OnDisconnected 加上 try/catch 與記錄，避免生命週期未攔截例外導致連線被中止。
	/// </summary>
	[Authorize] // 仍要求已登入才能連線 Hub
	public sealed class ChatHub : Hub
	{
		private readonly IAppCurrentUser _me; // ✅ 統一入口，集中「吃登入」邏輯
		private readonly IChatService _svc;
		private readonly IChatNotifier _notify;
		private readonly ILogger<ChatHub> _logger; // ✅ 新增：記錄生命週期/呼叫時可能的例外

		public ChatHub(IAppCurrentUser me, IChatService svc, IChatNotifier notify, ILogger<ChatHub> logger)
		{
			_me = me;
			_svc = svc;
			_notify = notify;
			_logger = logger;
		}

		/// <summary>
		/// 取得目前使用者的整數 UserId。
		/// 先走「快取/快速路徑」_me.UserId（取自 Claims: AppUserId），
		/// 若為 0 再走備援 _me.GetUserIdAsync()（支援解析 NameIdentifier 或 DB 對應）。
		/// </summary>
		private async Task<int> GetMeAsync()
		{
			// 快速路徑：_me.UserId 已經把 "AppUserId" Claim 轉成 int（沒有就回 0）
			var id = _me.UserId;
			if (id > 0) return id;

			// 備援路徑：必要時才呼叫（可能解析 NameIdentifier 或透過 ILoginIdentity 做一次 DB 對應）
			return await _me.GetUserIdAsync();
		}

		/// <summary>
		/// 連線建立時：把目前使用者加入「個人群組」，用於點對點推播。
		/// ⚠️ 用 try/catch 包住，避免生命週期未攔截例外直接讓伺服器關閉 WebSocket。
		/// </summary>
		public override async Task OnConnectedAsync()
		{
			try
			{
				var me = await GetMeAsync();
				if (me > 0)
				{
					await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.User(me));
					_logger.LogInformation("ChatHub connected. uid={Uid}, conn={ConnId}", me, Context.ConnectionId);
				}
				else
				{
					// 未登入或 Cookie 舊（沒有 AppUserId），不要丟例外，以免中止連線；記錄即可。
					_logger.LogWarning("ChatHub connected without valid user id. conn={ConnId}", Context.ConnectionId);
				}
			}
			catch (Exception ex)
			{
				// ★ 關鍵：不把例外往外拋，避免「Server returned an error on close」
				_logger.LogError(ex, "OnConnectedAsync failed. conn={ConnId}", Context.ConnectionId);
			}
			finally
			{
				await base.OnConnectedAsync();
			}
		}

		/// <summary>
		/// 連線中斷時：從個人群組移除。
		/// 同樣包 try/catch，確保這裡的例外不會再影響整體連線狀態回報。
		/// </summary>
		public override async Task OnDisconnectedAsync(Exception? ex)
		{
			try
			{
				var me = await GetMeAsync();
				if (me > 0)
					await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.User(me));

				if (ex != null)
					_logger.LogWarning(ex, "ChatHub disconnected with error. conn={ConnId}", Context.ConnectionId);
				else
					_logger.LogInformation("ChatHub disconnected normally. conn={ConnId}", Context.ConnectionId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "OnDisconnectedAsync failed. conn={ConnId}", Context.ConnectionId);
			}
			finally
			{
				await base.OnDisconnectedAsync(ex);
			}
		}

		/// <summary>
		/// 傳送訊息給某位好友（對方）。
		/// 成功後：推播雙方、並更新雙方未讀數。
		/// </summary>
		public async Task<DirectMessagePayload> SendMessageTo(int otherId, string text)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == me) throw new HubException(ErrorCodes.NoPeer);

			// 寫入訊息（服務層負責落 DB 與產生 payload）
			var payload = await _svc.SendDirectAsync(me, otherId, text);

			// 推播雙方：新訊息
			await _notify.BroadcastReceiveDirectAsync(me, otherId, payload);

			// 未讀更新（雙向）
			var (totalMe, peerMe) = await _svc.ComputeUnreadAsync(me, otherId);
			await _notify.BroadcastUnreadAsync(me, new UnreadUpdatePayload { PeerId = otherId, Unread = peerMe, Total = totalMe });

			var (totalOt, peerOt) = await _svc.ComputeUnreadAsync(otherId, me);
			await _notify.BroadcastUnreadAsync(otherId, new UnreadUpdatePayload { PeerId = me, Unread = peerOt, Total = totalOt });

			return payload;
		}

		/// <summary>
		/// 通知「我」已讀到某時間點；回傳已讀回執並推播雙方，同步更新未讀。
		/// </summary>
		public async Task<ReadReceiptPayload> NotifyRead(int otherId, string upToIso)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == me) throw new HubException(ErrorCodes.NoPeer);

			// upToIso 防守性解析（失敗就以現在時間當作已讀界線）
			if (!DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out var upToUtc))
				upToUtc = DateTime.UtcNow;

			// 寫入已讀、推播回執
			var receipt = await _svc.MarkReadAsync(me, otherId, upToUtc);
			await _notify.BroadcastReadReceiptAsync(me, otherId, receipt);

			// 未讀更新（雙向）
			var (totalMe, peerMe) = await _svc.ComputeUnreadAsync(me, otherId);
			await _notify.BroadcastUnreadAsync(me, new UnreadUpdatePayload { PeerId = otherId, Unread = peerMe, Total = totalMe });

			var (totalOt, peerOt) = await _svc.ComputeUnreadAsync(otherId, me);
			await _notify.BroadcastUnreadAsync(otherId, new UnreadUpdatePayload { PeerId = me, Unread = peerOt, Total = totalOt });

			return receipt;
		}

		/// <summary>
		/// 前端連線完成後，主動請求目前「全站未讀總數」。
		/// </summary>
		public async Task RefreshUnread()
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);

			var total = await _svc.ComputeTotalUnreadAsync(me);
			await _notify.BroadcastUnreadAsync(me, new UnreadUpdatePayload { PeerId = 0, Unread = 0, Total = total });
		}

		/// <summary>
		/// 一次回傳多位好友的未讀統計（以及全站總未讀），供好友清單顯示。
		/// </summary>
		public async Task<object> GetUnreadForPeers(int[] peerIds)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);

			var result = new List<object>();
			var uniq = (peerIds ?? Array.Empty<int>()).Where(x => x > 0 && x != me).Distinct();

			foreach (var pid in uniq)
			{
				var (_, peer) = await _svc.ComputeUnreadAsync(me, pid);
				result.Add(new { peerId = pid, unread = peer });
			}

			var total = await _svc.ComputeTotalUnreadAsync(me);
			return new { total, peers = result };
		}
	}
}
