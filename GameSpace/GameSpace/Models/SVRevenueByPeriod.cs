using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SVRevenueByPeriod
{
    public byte PeriodType { get; set; }

    public DateOnly RankingDate { get; set; }

    public decimal? RevenueAmount { get; set; }

    public int? RevenueVolume { get; set; }
}
