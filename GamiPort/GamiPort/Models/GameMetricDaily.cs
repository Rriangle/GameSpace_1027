using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class GameMetricDaily
{
    public int Id { get; set; }

    public int? GameId { get; set; }

    public int? MetricId { get; set; }

    public DateOnly? Date { get; set; }

    public decimal? Value { get; set; }

    public string? AggMethod { get; set; }

    public string? Quality { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Game? Game { get; set; }

    public virtual Metric? Metric { get; set; }
}
