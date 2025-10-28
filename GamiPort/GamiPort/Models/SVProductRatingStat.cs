using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SVProductRatingStat
{
    public int ProductId { get; set; }

    public int? RatingCount { get; set; }

    public decimal? RatingAvg { get; set; }
}
