using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class NotificationRecipient
{
    public int RecipientId { get; set; }

    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public int? ManagerId { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual ManagerDatum? Manager { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual User? User { get; set; }
}
