using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class VwSoOrderAddressesFull
{
    public int OrderId { get; set; }

    public string Recipient { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string FullAddress { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
