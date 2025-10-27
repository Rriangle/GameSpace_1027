using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SoOrderItem
{
    public int ItemId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int LineNo { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal? Subtotal { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string? DeleteReason { get; set; }

    public virtual SoOrderInfo Order { get; set; } = null!;

    public virtual SProductInfo Product { get; set; } = null!;
}
