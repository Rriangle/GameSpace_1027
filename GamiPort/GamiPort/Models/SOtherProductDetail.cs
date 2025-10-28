using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SOtherProductDetail
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    public int SupplierId { get; set; }

    public int? MerchTypeId { get; set; }

    public string? DigitalCode { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string? Weight { get; set; }

    public string? Dimensions { get; set; }

    public string? Material { get; set; }

    public bool IsDeleted { get; set; }
}
