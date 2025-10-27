namespace GamiPort.Areas.social_hub.SignalR
{
	/// <summary>訊息策略（可由 appsettings 覆寫）</summary>
	public sealed class MessagePolicyOptions
	{
		public int MaxContentLength { get; set; } = 255;
	}
}
