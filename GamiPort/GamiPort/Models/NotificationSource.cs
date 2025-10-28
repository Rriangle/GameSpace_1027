using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class NotificationSource
{
    public int SourceId { get; set; }

    public string SourceName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
