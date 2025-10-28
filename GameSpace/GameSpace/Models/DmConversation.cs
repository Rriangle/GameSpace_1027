using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class DmConversation
{
    public int ConversationId { get; set; }

    public bool IsManagerDm { get; set; }

    public int Party1Id { get; set; }

    public int Party2Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public virtual ICollection<DmMessage> DmMessages { get; set; } = new List<DmMessage>();
}
