using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SoShipment
{
    public long ShipmentId { get; set; }

    public long ShipmentSeq { get; set; }

    public string? ShipmentCode { get; set; }

    public int OrderId { get; set; }

    public int? ShipMethodId { get; set; }

    public string Carrier { get; set; } = null!;

    public string? TrackingNo { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual SoOrderInfo Order { get; set; } = null!;

    public virtual SoShipMethod? ShipMethod { get; set; }
}
