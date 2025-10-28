using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SupportTicketMessage
{
    public int MessageId { get; set; }

    public int TicketId { get; set; }

    public int? SenderUserId { get; set; }

    public int? SenderManagerId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public DateTime? ReadByUserAt { get; set; }

    public DateTime? ReadByManagerAt { get; set; }

    public virtual ManagerDatum? SenderManager { get; set; }

    public virtual User? SenderUser { get; set; }

    public virtual SupportTicket Ticket { get; set; } = null!;
}
