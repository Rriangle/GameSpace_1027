using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Relation
{
    public int RelationId { get; set; }

    public int UserIdSmall { get; set; }

    public int UserIdLarge { get; set; }

    public int StatusId { get; set; }

    public int? RequestedBy { get; set; }

    public string? FriendNickname { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? RequestedByNavigation { get; set; }

    public virtual RelationStatus Status { get; set; } = null!;

    public virtual User UserIdLargeNavigation { get; set; } = null!;

    public virtual User UserIdSmallNavigation { get; set; } = null!;
}
