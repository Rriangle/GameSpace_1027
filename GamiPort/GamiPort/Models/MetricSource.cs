using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class MetricSource
{
    public int SourceId { get; set; }

    public string? Name { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GameSourceMap> GameSourceMaps { get; set; } = new List<GameSourceMap>();

    public virtual ICollection<Metric> Metrics { get; set; } = new List<Metric>();
}
