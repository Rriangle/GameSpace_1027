using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SGameProductDetail
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    public int SupplierId { get; set; }

    public int? PlatformId { get; set; }

    public string? DownloadLink { get; set; }

    public bool IsDeleted { get; set; }

    public virtual SPlatform? Platform { get; set; }

    public virtual SProductInfo Product { get; set; } = null!;

    public virtual SSupplier Supplier { get; set; } = null!;
}
