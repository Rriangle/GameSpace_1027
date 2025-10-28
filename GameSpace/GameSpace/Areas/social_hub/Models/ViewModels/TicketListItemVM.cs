using System;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	/// <summary>
	/// 客服工單清單用的最小資料集合
	/// </summary>
	public class TicketListItemVM
	{
		public int TicketId { get; set; }

		public int UserId { get; set; }

		public string Subject { get; set; } = string.Empty;

		public int? AssignedManagerId { get; set; }

		public bool IsClosed { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime? LastMessageAt { get; set; }

		/// <summary>
		/// 給「我（客服）」的未讀數（只算使用者→客服且尚未讀）
		/// </summary>
		public int UnreadForMe { get; set; }

		/// <summary>
		/// 用於排序或顯示的最後活躍時間：LastMessageAt ?? CreatedAt
		/// </summary>
		public DateTime LastActivityAt => LastMessageAt ?? CreatedAt;
	}
}
