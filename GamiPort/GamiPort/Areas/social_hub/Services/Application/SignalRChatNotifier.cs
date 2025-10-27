using GamiPort.Areas.social_hub.Hubs;
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.SignalR;
using GamiPort.Areas.social_hub.SignalR.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>SignalR 實作的廣播介面</summary>
	public sealed class SignalRChatNotifier : IChatNotifier
	{
		private readonly IHubContext<ChatHub> _hub;
		public SignalRChatNotifier(IHubContext<ChatHub> hub) => _hub = hub;

		public async Task BroadcastReceiveDirectAsync(int userAId, int userBId, DirectMessagePayload payload, CancellationToken ct = default)
		{
			await _hub.Clients.Group(GroupNames.User(userAId)).SendAsync(ChatEvents.ReceiveDirect, payload, ct);
			await _hub.Clients.Group(GroupNames.User(userBId)).SendAsync(ChatEvents.ReceiveDirect, payload, ct);
		}

		public async Task BroadcastReadReceiptAsync(int userAId, int userBId, ReadReceiptPayload payload, CancellationToken ct = default)
		{
			await _hub.Clients.Group(GroupNames.User(userAId)).SendAsync(ChatEvents.ReadReceipt, payload, ct);
			await _hub.Clients.Group(GroupNames.User(userBId)).SendAsync(ChatEvents.ReadReceipt, payload, ct);
		}

		public async Task BroadcastUnreadAsync(int userId, UnreadUpdatePayload payload, CancellationToken ct = default)
		{
			await _hub.Clients.Group(GroupNames.User(userId)).SendAsync(ChatEvents.UnreadUpdate, payload, ct);
		}
	}
}
