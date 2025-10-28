using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class NotificationAction
{
    public int ActionId { get; set; }

    public string ActionName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
