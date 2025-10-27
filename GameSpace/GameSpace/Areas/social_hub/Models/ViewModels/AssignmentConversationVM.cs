// Areas/social_hub/Models/ViewModels/AssignmentConversationVM.cs
using System;
using System.Collections.Generic;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class AssignmentConversationVM
	{
		public int TicketId { get; set; }
		public int AssignmentId { get; set; }   // 若你的主鍵名稱不同，請同步調整 Controller 與 Views
		public int UserId { get; set; }
		public string Subject { get; set; } = string.Empty;
		public int? AssignedByManagerId { get; set; } // 指派此任務的客服
		public int? FromManagerId { get; set; }
		public int ToManagerId { get; set; }
		public DateTime AssignedAt { get; set; }
		public DateTime? NextAssignedAt { get; set; } // 下一次指派時間（用來界定結束）
		public string? Note { get; set; }               // ★ 新增：此次轉單備註
		public IReadOnlyList<SupportMessageVM> Messages { get; set; } = Array.Empty<SupportMessageVM>();

		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 30;
		public int TotalMessages { get; set; }
		public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalMessages / PageSize);
	}
}
