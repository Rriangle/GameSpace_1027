using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SProductCode
{
    public string ProductCode { get; set; } = null!;

    public int ProductId { get; set; }

    public virtual SProductInfo Product { get; set; } = null!;
}
