using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SoPaymentTransaction
{
    public long PaymentId { get; set; }

    public string PaymentCode { get; set; } = null!;

    public int OrderId { get; set; }

    public string TxnType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Provider { get; set; } = null!;

    public string? ProviderTxn { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public string? Note { get; set; }

    public string? Meta { get; set; }

    public virtual SoOrderInfo Order { get; set; } = null!;
}
