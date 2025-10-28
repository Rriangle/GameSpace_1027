using GamiPort.Areas.social_hub.SignalR.Contracts;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>DM 業務邏輯（EF 讀寫集中在這）</summary>
	public interface IChatService
	{
		Task<DirectMessagePayload> SendDirectAsync(int senderId, int recipientId, string content, CancellationToken ct = default);
		Task<ReadReceiptPayload> MarkReadAsync(int readerId, int otherId, DateTime upToUtc, CancellationToken ct = default);
		Task<IReadOnlyList<DirectMessagePayload>> GetHistoryAsync(int userId, int otherId, DateTime? afterUtc, int pageSize, CancellationToken ct = default);
		Task<(int total, int peer)> ComputeUnreadAsync(int userId, int peerId, CancellationToken ct = default);

		/// <summary>僅回傳「全站未讀總數」</summary>
		Task<int> ComputeTotalUnreadAsync(int userId, CancellationToken ct = default);
	}
}
