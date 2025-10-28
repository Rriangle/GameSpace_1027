using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SoShipMethod
{
    public int ShipMethodId { get; set; }

    public string MethodName { get; set; } = null!;

    public decimal BaseFee { get; set; }

    public decimal FreeThreshold { get; set; }

    public bool ForStorePickup { get; set; }

    public bool AllowRemoteSurcharge { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<SoShipment> SoShipments { get; set; } = new List<SoShipment>();
}
