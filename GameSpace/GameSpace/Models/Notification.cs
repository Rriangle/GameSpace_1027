using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int SourceId { get; set; }

    public int ActionId { get; set; }

    public int? GroupId { get; set; }

    public int? SenderUserId { get; set; }

    public int? SenderManagerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual NotificationAction Action { get; set; } = null!;

    public virtual Group? Group { get; set; }

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public virtual ManagerDatum? SenderManager { get; set; }

    public virtual User? SenderUser { get; set; }

    public virtual NotificationSource Source { get; set; } = null!;
}
