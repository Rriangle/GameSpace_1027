using System;
using System.Collections.Generic;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class TicketDetailVM
	{
		// 基本
		public int TicketId { get; set; }
		public int UserId { get; set; }
		public string Subject { get; set; } = string.Empty;

		public bool IsClosed { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ClosedAt { get; set; }
		public string? CloseNote { get; set; }

		public int? AssignedManagerId { get; set; }
		public DateTime? LastMessageAt { get; set; }

		// 權限 / 動作可見性
		public int MeManagerId { get; set; }
		public bool CanAssignToMe { get; set; }
		public bool CanReassign { get; set; }
		public bool CanClose { get; set; }

		// 訊息
		public IReadOnlyList<SupportMessageVM> Messages { get; set; } = Array.Empty<SupportMessageVM>();
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 30;
		public int TotalMessages { get; set; }
		public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalMessages / PageSize);
	}
}
