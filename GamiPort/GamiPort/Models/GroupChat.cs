using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class GroupChat
{
    public int MessageId { get; set; }

    public int GroupId { get; set; }

    public int SenderUserId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User SenderUser { get; set; } = null!;
}
