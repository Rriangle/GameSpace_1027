using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ShipMethod
{
    public int ShipMethodId { get; set; }

    public string MethodName { get; set; } = null!;

    public decimal BaseFee { get; set; }

    public decimal FreeThreshold { get; set; }

    public bool ForStorePickup { get; set; }

    public bool AllowRemoteSurcharge { get; set; }
}
