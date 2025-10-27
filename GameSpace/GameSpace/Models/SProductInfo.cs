using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SProductInfo
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductType { get; set; } = null!;

    public decimal Price { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishAt { get; set; }

    public DateTime? UnpublishAt { get; set; }

    public bool IsPreorderEnabled { get; set; }

    public DateTime? PreorderStartAt { get; set; }

    public DateTime? PreorderEndAt { get; set; }

    public int? SafetyStock { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public bool IsPhysical { get; set; }

    public virtual SGameProductDetail? SGameProductDetail { get; set; }

    public virtual SProductCode? SProductCode { get; set; }

    public virtual ICollection<SProductImage> SProductImages { get; set; } = new List<SProductImage>();

    public virtual ICollection<SoCartItem> SoCartItems { get; set; } = new List<SoCartItem>();

    public virtual ICollection<SoOrderItem> SoOrderItems { get; set; } = new List<SoOrderItem>();

    public virtual ICollection<SoStockMovement> SoStockMovements { get; set; } = new List<SoStockMovement>();

    public virtual ICollection<SGameGenre> Genres { get; set; } = new List<SGameGenre>();
}
