using System.Text.Json.Serialization;

namespace GamiPort.Areas.social_hub.SignalR.Contracts
{
	public sealed class UnreadUpdatePayload
	{
		[JsonPropertyName("peerId")] public int PeerId { get; init; }
		[JsonPropertyName("unread")] public int Unread { get; init; }
		[JsonPropertyName("total")] public int Total { get; init; }
	}
}
