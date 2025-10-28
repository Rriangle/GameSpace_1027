namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class NotificationViewModel
	{
		// ← 新增：用於 Detail/MarkRead 的路由 id
		public int RecipientId { get; set; }

		public int NotificationId { get; set; }

		// 顯示用欄位（沿用你 View 的命名）
		public string? NotificationTitle { get; set; }
		public string? NotificationMessage { get; set; }

		public string? SourceName { get; set; }
		public string? ActionName { get; set; }

		// ← 新增：清單顯示發送者（可能為 null＝系統）
		public string? SenderName { get; set; }

		public DateTime CreatedAt { get; set; }

		// 已讀時間（以時間戳判斷）
		public DateTime? ReadAt { get; set; }
		public bool IsRead => ReadAt != null;
	}
}
