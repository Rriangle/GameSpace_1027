using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class BannedWord
{
    public int WordId { get; set; }

    public string? Word { get; set; }

    public DateTime? CreatedAt { get; set; }
}
