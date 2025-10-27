using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SignInRule
{
    public int Id { get; set; }

    public int SignInDay { get; set; }

    public int Points { get; set; }

    public int Experience { get; set; }

    public bool HasCoupon { get; set; }

    public string? CouponTypeCode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual CouponType? CouponTypeCodeNavigation { get; set; }
}
