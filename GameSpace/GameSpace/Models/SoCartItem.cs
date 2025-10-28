using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SoCartItem
{
    public long CartItemId { get; set; }

    public Guid CartId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int? PlatformId { get; set; }

    public string? VariantSku { get; set; }

    public string? OptionSnapshot { get; set; }

    public string? ImageThumb { get; set; }

    public decimal UnitPrice { get; set; }

    public int Qty { get; set; }

    public decimal? LineSubtotal { get; set; }

    public bool IsSelected { get; set; }

    public string ItemStatus { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual SoCart Cart { get; set; } = null!;

    public virtual SProductInfo Product { get; set; } = null!;
}
