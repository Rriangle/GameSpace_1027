using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PopularityIndexDaily
{
    public int Id { get; set; }

    public int? GameId { get; set; }

    public DateOnly? Date { get; set; }

    public decimal? IndexValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Game? Game { get; set; }
}
