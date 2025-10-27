using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SoStockMovement
{
    public long MovementId { get; set; }

    public int ProductId { get; set; }

    public int? OrderId { get; set; }

    public int ChangeQty { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? Note { get; set; }

    public virtual SoOrderInfo? Order { get; set; }

    public virtual SProductInfo Product { get; set; } = null!;
}
