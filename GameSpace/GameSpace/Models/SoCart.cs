using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SoCart
{
    public Guid CartId { get; set; }

    public int? UserId { get; set; }

    public Guid? AnonymousToken { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal SubtotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal? GrandTotal { get; set; }

    public string? CouponCode { get; set; }

    public string? CouponSnapshot { get; set; }

    public bool IsLocked { get; set; }

    public DateTime? LockedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<SoCartItem> SoCartItems { get; set; } = new List<SoCartItem>();
}
