using System.Collections.Generic;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class ReassignTicketVM
	{
		public int TicketId { get; set; }
		public int UserId { get; set; }
		public string Subject { get; set; } = string.Empty;

		public int? CurrentAssignedManagerId { get; set; }

		// 表單欄位
		public int? ToManagerId { get; set; }
		public string? Note { get; set; }

		// 提示用清單（具客服權限的 ManagerId）
		public IReadOnlyList<int> CandidateManagerIds { get; set; } = new List<int>();
	}
}
