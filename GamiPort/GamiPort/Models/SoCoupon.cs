using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SoCoupon
{
    public string CouponCode { get; set; } = null!;

    public bool IsActive { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal? Amount { get; set; }

    public decimal? PercentRate { get; set; }

    public decimal? MaxDiscount { get; set; }

    public decimal? MinOrderAmt { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public string? Note { get; set; }
}
