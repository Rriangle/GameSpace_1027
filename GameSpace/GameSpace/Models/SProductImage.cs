using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SProductImage
{
    public int ProductimgId { get; set; }

    public int ProductId { get; set; }

    public string ProductimgUrl { get; set; } = null!;

    public DateTime ProductimgUpdatedAt { get; set; }

    public bool IsPrimary { get; set; }

    public int SortOrder { get; set; }

    public virtual SProductInfo Product { get; set; } = null!;
}
