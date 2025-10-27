using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PostMetricSnapshot
{
    public int PostId { get; set; }

    public int? GameId { get; set; }

    public DateOnly? Date { get; set; }

    public decimal? IndexValue { get; set; }

    public string? DetailsJson { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Game? Game { get; set; }

    public virtual Post Post { get; set; } = null!;
}
