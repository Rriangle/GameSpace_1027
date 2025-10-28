using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SSupplierStatus
{
    public byte StatusCode { get; set; }

    public string StatusName { get; set; } = null!;
}
