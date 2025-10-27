using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class VProductCode
{
    public int ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public int? ProductCodeSort { get; set; }
}
