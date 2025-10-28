using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SoRemoteZipcode
{
    public string Zipcode { get; set; } = null!;

    public string? Note { get; set; }
}
