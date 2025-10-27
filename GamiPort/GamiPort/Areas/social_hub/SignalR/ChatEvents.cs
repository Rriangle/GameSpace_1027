namespace GamiPort.Areas.social_hub.SignalR
{
	/// <summary>前後端共用事件名，避免魔術字串</summary>
	public static class ChatEvents
	{
		public const string ReceiveDirect = "ReceiveDirect";
		public const string ReadReceipt = "ReadReceipt";
		public const string UnreadUpdate = "UnreadUpdate";
	}
}
