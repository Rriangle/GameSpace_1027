using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SupportTicketAssignment
{
    public int AssignmentId { get; set; }

    public int TicketId { get; set; }

    public int? FromManagerId { get; set; }

    public int ToManagerId { get; set; }

    public int? AssignedByManagerId { get; set; }

    public DateTime AssignedAt { get; set; }

    public string? Note { get; set; }

    public virtual ManagerDatum? AssignedByManager { get; set; }

    public virtual ManagerDatum? FromManager { get; set; }

    public virtual SupportTicket Ticket { get; set; } = null!;

    public virtual ManagerDatum ToManager { get; set; } = null!;
}
