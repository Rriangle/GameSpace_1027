using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SoShipWeightRule
{
    public int RuleId { get; set; }

    public int ShipMethodId { get; set; }

    public decimal MinWeight { get; set; }

    public decimal AddFee { get; set; }

    public string? Note { get; set; }
}
