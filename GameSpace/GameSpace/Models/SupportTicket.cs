using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public int UserId { get; set; }

    public int? AssignedManagerId { get; set; }

    public string Subject { get; set; } = null!;

    public bool IsClosed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? ClosedByManagerId { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public string? CloseNote { get; set; }

    public virtual ManagerDatum? AssignedManager { get; set; }

    public virtual ManagerDatum? ClosedByManager { get; set; }

    public virtual ICollection<SupportTicketAssignment> SupportTicketAssignments { get; set; } = new List<SupportTicketAssignment>();

    public virtual ICollection<SupportTicketMessage> SupportTicketMessages { get; set; } = new List<SupportTicketMessage>();

    public virtual User User { get; set; } = null!;
}
