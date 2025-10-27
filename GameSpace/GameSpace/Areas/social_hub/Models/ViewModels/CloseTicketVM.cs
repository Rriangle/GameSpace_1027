using System;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class CloseTicketVM
	{
		public int TicketId { get; set; }
		public int UserId { get; set; }
		public string Subject { get; set; } = string.Empty;
		public int? AssignedManagerId { get; set; }
		public bool IsClosed { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastMessageAt { get; set; }

		// 表單欄位
		public string? CloseNote { get; set; }
	}
}
