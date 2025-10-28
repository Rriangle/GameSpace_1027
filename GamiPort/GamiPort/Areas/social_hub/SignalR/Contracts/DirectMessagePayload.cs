using System.Text.Json.Serialization;

namespace GamiPort.Areas.social_hub.SignalR.Contracts
{
	/// <summary>DM 訊息 Payload（輸出 camelCase）</summary>
	public sealed class DirectMessagePayload
	{
		[JsonPropertyName("messageId")] public int MessageId { get; init; }
		[JsonPropertyName("senderId")] public int SenderId { get; init; }
		[JsonPropertyName("receiverId")] public int ReceiverId { get; init; }
		[JsonPropertyName("content")] public string Content { get; init; } = "";
		[JsonPropertyName("sentAtIso")] public string SentAtIso { get; init; } = "";
	}
}
