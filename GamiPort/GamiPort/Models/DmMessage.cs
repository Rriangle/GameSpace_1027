using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class DmMessage
{
    public int MessageId { get; set; }

    public int ConversationId { get; set; }

    public bool SenderIsParty1 { get; set; }

    public string MessageText { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime EditedAt { get; set; }

    public virtual DmConversation Conversation { get; set; } = null!;
}
