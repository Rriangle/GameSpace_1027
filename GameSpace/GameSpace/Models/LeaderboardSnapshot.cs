using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class LeaderboardSnapshot
{
    public int SnapshotId { get; set; }

    public string? Period { get; set; }

    public DateTime? Ts { get; set; }

    public int? Rank { get; set; }

    public int? GameId { get; set; }

    public decimal? IndexValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Game? Game { get; set; }
}
