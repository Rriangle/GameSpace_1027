using System;
using System.Collections.Generic;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class ChatHomeVM
	{
		public int MeId { get; set; }
		public List<ConversationListItemVM> Conversations { get; set; } = new();
		public List<ContactItemVM> Contacts { get; set; } = new();
	}

	public class ConversationListItemVM
	{
		public int ConversationId { get; set; }
		public int OtherId { get; set; }
		public DateTime? LastMessageAt { get; set; }
		public int UnreadCount { get; set; }
		public string? LastPreview { get; set; }
	}

	public class ContactItemVM
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string? Nick { get; set; }
		public int Unread { get; set; }
		public DateTime? LastAt { get; set; }
	}

	public class SimpleChatMessageVM
	{
		public int MessageId { get; set; }
		public int SenderId { get; set; }
		public int ReceiverId { get; set; }
		public string Text { get; set; } = "";
		public DateTime At { get; set; }
		public bool IsMine { get; set; }
		public bool IsRead { get; set; }
		public DateTime? ReadAt { get; set; }
	}

	// ★ 新增：供 ChatController.With(...) 使用
	public class ChatThreadVM
	{
		public int OtherId { get; set; }
		public List<SimpleChatMessageVM> Messages { get; set; } = new();
	}
}
