using System;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class SupportMessageVM
	{
		public int MessageId { get; set; }
		public int TicketId { get; set; }

		public int? SenderUserId { get; set; }
		public int? SenderManagerId { get; set; } // 我方或其他客服

		public string SenderType =>
			SenderUserId != null ? "user" :
			SenderManagerId != null ? "manager" : "system";

		public string MessageText { get; set; } = string.Empty;
		public DateTime SentAt { get; set; }

		public DateTime? ReadByUserAt { get; set; }
		public DateTime? ReadByManagerAt { get; set; }

		/// <summary>是否為我（目前登入的客服）所送出</summary>
		public bool IsMine { get; set; }
	}
}
