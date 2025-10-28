using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Metric
{
    public int MetricId { get; set; }

    public int? SourceId { get; set; }

    public string? Code { get; set; }

    public string? Unit { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GameMetricDaily> GameMetricDailies { get; set; } = new List<GameMetricDaily>();

    public virtual MetricSource? Source { get; set; }
}
