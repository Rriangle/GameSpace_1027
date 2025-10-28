using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SPeriodType
{
    public byte PeriodCode { get; set; }

    public string PeriodName { get; set; } = null!;
}
