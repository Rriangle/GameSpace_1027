using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SOfficialStoreRanking
{
    public int RankingId { get; set; }

    public byte PeriodType { get; set; }

    public DateOnly RankingDate { get; set; }

    public int ProductId { get; set; }

    public string RankingMetric { get; set; } = null!;

    public int RankingPosition { get; set; }

    public decimal? MetricValueNum { get; set; }

    public decimal? TradingAmount { get; set; }

    public int? TradingVolume { get; set; }

    public DateTime RankingUpdatedAt { get; set; }

    public string? MetricNote { get; set; }
}
