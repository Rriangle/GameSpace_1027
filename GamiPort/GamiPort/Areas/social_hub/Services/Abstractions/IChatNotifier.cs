using GamiPort.Areas.social_hub.SignalR.Contracts;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>前端廣播介面（現在用 SignalR）</summary>
	public interface IChatNotifier
	{
		Task BroadcastReceiveDirectAsync(int userAId, int userBId, DirectMessagePayload payload, CancellationToken ct = default);
		Task BroadcastReadReceiptAsync(int userAId, int userBId, ReadReceiptPayload payload, CancellationToken ct = default);
		Task BroadcastUnreadAsync(int userId, UnreadUpdatePayload payload, CancellationToken ct = default);
	}
}
