namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class TicketInfoVM
	{
		public int TicketId { get; set; }
		public long UserId { get; set; }
		public string Subject { get; set; } = "";
		public int? AssignedManagerId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastMessageAt { get; set; }
		public bool IsClosed { get; set; }
		public DateTime? ClosedAt { get; set; }
		public string? CloseNote { get; set; }
	}
}
