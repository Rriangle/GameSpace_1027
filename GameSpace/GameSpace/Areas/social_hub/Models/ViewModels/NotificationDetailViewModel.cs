using System;
using System.Collections.Generic;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	/// <summary>
	/// 明細頁專用：與列表的 NotificationViewModel 不同，提供更多語意欄位
	/// </summary>
	public class NotificationDetailViewModel
	{
		public int RecipientId { get; set; }
		public int NotificationId { get; set; }

		// 標題/內容保持完整名稱（方便與資料表對應）
		public string? NotificationTitle { get; set; }
		public string? NotificationMessage { get; set; }

		// 來源/動作/群組
		public string? SourceName { get; set; }
		public string? ActionName { get; set; }
		public string? GroupName { get; set; }

		// 發送者
		public string? SenderName { get; set; }
		/// <summary>user / manager / system</summary>
		public string SenderType { get; set; } = "system";

		// 狀態/時間
		public DateTime CreatedAt { get; set; }
		public bool IsRead { get; set; }
		/// <summary>若資料表尚未有 ReadAt，這裡會在「本次檢視時自動已讀」的情境下，填現在時間（僅顯示用）</summary>
		public DateTime? ReadAt { get; set; }

		// 額外語意（細節頁才需要）
		/// <summary>例如「來源/動作」組合的完整顯示：Order → Shipped</summary>
		public string? FullSourcePath { get; set; }
		/// <summary>預留：附加連結、行為按鈕等</summary>
		public Dictionary<string, string>? Links { get; set; }
	}
}
