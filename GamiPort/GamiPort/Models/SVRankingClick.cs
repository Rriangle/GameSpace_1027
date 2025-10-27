using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SVRankingClick
{
    public DateOnly RankingDate { get; set; }

    public byte PeriodType { get; set; }

    public int ProductId { get; set; }

    public int RankingPosition { get; set; }

    public decimal? ClickScore { get; set; }

    public string? MetricNote { get; set; }
}
