// Areas/social_hub/Models/ViewModels/AssignmentHistoryPageVM.cs
using System;
using System.Collections.Generic;
using GameSpace.Models;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class AssignmentHistoryPageVM
	{
		public int TicketId { get; set; }
		public int UserId { get; set; }
		public string Subject { get; set; } = string.Empty;

		public IReadOnlyList<SupportTicketAssignment> Items { get; set; }
			= Array.Empty<SupportTicketAssignment>();

		// 分頁
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public int TotalCount { get; set; }
		public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

		public bool HasPrev => Page > 1;
		public bool HasNext => Page < TotalPages;
	}
}
