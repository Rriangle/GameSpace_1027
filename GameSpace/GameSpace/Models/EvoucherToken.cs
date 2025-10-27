using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class EvoucherToken
{
    public int TokenId { get; set; }

    public int EvoucherId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual Evoucher Evoucher { get; set; } = null!;

    public virtual ICollection<EvoucherRedeemLog> EvoucherRedeemLogs { get; set; } = new List<EvoucherRedeemLog>();
}
