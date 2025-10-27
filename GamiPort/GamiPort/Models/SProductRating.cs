using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SProductRating
{
    public long RatingId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public byte Rating { get; set; }

    public string? ReviewText { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public int? ApprovedBy { get; set; }
}
