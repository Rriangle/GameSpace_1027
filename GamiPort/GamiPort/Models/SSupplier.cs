using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SSupplier
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public byte StatusCode { get; set; }

    public int IsActive { get; set; }

    public string? Note { get; set; }

    public DateTime? ContractEndAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public virtual ICollection<SGameProductDetail> SGameProductDetails { get; set; } = new List<SGameProductDetail>();
}
