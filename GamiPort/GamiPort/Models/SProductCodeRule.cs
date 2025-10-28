using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SProductCodeRule
{
    public int RuleId { get; set; }

    public string ProductType { get; set; } = null!;

    public string Prefix { get; set; } = null!;

    public byte PadLength { get; set; }
}
