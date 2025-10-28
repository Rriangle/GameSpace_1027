using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SoOrderInfo
{
    public int OrderId { get; set; }

    public string OrderCode { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public decimal OrderTotal { get; set; }

    public DateTime? PaymentAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal? GrandTotal { get; set; }

    public int? PayMethodId { get; set; }

    public virtual SoPayMethod? PayMethod { get; set; }

    public virtual SoOrderAddress? SoOrderAddress { get; set; }

    public virtual ICollection<SoOrderItem> SoOrderItems { get; set; } = new List<SoOrderItem>();

    public virtual ICollection<SoOrderStatusHistory> SoOrderStatusHistories { get; set; } = new List<SoOrderStatusHistory>();

    public virtual ICollection<SoPaymentTransaction> SoPaymentTransactions { get; set; } = new List<SoPaymentTransaction>();

    public virtual ICollection<SoShipment> SoShipments { get; set; } = new List<SoShipment>();

    public virtual ICollection<SoStockMovement> SoStockMovements { get; set; } = new List<SoStockMovement>();

    public virtual User User { get; set; } = null!;
}
