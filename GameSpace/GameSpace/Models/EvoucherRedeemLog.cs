using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class EvoucherRedeemLog
{
    public int RedeemId { get; set; }

    public int EvoucherId { get; set; }

    public int? TokenId { get; set; }

    public int UserId { get; set; }

    public DateTime ScannedAt { get; set; }

    public string Status { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual Evoucher Evoucher { get; set; } = null!;

    public virtual EvoucherToken? Token { get; set; }

    public virtual User User { get; set; } = null!;
}
