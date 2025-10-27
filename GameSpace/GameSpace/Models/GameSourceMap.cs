using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class GameSourceMap
{
    public int Id { get; set; }

    public int? GameId { get; set; }

    public int? SourceId { get; set; }

    public string? ExternalKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Game? Game { get; set; }

    public virtual MetricSource? Source { get; set; }
}
