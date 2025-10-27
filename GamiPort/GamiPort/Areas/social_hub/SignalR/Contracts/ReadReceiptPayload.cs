using System.Text.Json.Serialization;

namespace GamiPort.Areas.social_hub.SignalR.Contracts
{
	public sealed class ReadReceiptPayload
	{
		[JsonPropertyName("fromUserId")] public int FromUserId { get; init; }
		[JsonPropertyName("upToIso")] public string UpToIso { get; init; } = "";
	}
}
