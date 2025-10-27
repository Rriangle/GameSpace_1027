using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SVRankingSale
{
    public DateOnly RankingDate { get; set; }

    public byte PeriodType { get; set; }

    public int ProductId { get; set; }

    public int RankingPosition { get; set; }

    public int? PurchaseVolume { get; set; }

    public decimal? RevenueAmount { get; set; }

    public string? MetricNote { get; set; }
}
