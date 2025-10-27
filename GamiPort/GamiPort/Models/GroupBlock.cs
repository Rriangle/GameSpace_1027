using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class GroupBlock
{
    public int BlockId { get; set; }

    public int GroupId { get; set; }

    public int UserId { get; set; }

    public int? BlockedByUserId { get; set; }

    public string? Reason { get; set; }

    public DateTime BlockedAt { get; set; }

    public DateTime? UnblockedAt { get; set; }

    public virtual User? BlockedByUser { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
