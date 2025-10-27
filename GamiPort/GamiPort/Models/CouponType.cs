using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class CouponType
{
    public int CouponTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal? DiscountValue { get; set; }

    public decimal? MinSpend { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public int PointsCost { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    public virtual ICollection<SignInRule> SignInRules { get; set; } = new List<SignInRule>();
}
