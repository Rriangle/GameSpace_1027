namespace GamiPort.Areas.social_hub.SignalR
{
	/// <summary>SignalR 群組命名集中管理</summary>
	public static class GroupNames
	{
		/// <summary>單人 DM 群組：u:{userId}</summary>
		public static string User(int userId) => $"u:{userId}";
	}
}
