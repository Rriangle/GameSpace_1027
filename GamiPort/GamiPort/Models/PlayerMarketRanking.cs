using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class PlayerMarketRanking
{
    public int PRankingId { get; set; }

    public string? PPeriodType { get; set; }

    public DateOnly? PRankingDate { get; set; }

    public int? PProductId { get; set; }

    public string? PRankingMetric { get; set; }

    public int? PRankingPosition { get; set; }

    public decimal? PTradingAmount { get; set; }

    public int? PTradingVolume { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual PlayerMarketProductInfo? PProduct { get; set; }
}
