using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SoShipPieceRule
{
    public int RuleId { get; set; }

    public int ShipMethodId { get; set; }

    public int MinQty { get; set; }

    public decimal AddFee { get; set; }

    public string? Note { get; set; }
}
