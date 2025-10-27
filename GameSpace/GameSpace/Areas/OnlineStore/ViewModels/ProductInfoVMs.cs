using System.ComponentModel.DataAnnotations;


    namespace GameSpace.Areas.OnlineStore.ViewModels
    {
        public class ProductIndexRowVM
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public string ProductType { get; set; } = "";
            public decimal Price { get; set; }
            public string CurrencyCode { get; set; } = "TWD";
            public int ShipmentQuantity { get; set; }
            public bool IsActive { get; set; }
            public DateTime ProductCreatedAt { get; set; }
            public string ProductCode { get; set; } = "";
            public string CoverUrl { get; set; } = "";
        }

        public class ProductCardVM
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public string ProductType { get; set; } = "";
            public decimal Price { get; set; }
            public string CurrencyCode { get; set; } = "TWD";
            public bool IsActive { get; set; }
            public string ProductCode { get; set; } = "";
            public string? CoverUrl { get; set; }
            // Game
            public string? PlatformName { get; set; }
            public string? GameType { get; set; }
            // Other
            public string? MerchTypeName { get; set; }
            public string? Size { get; set; }
            public string? Color { get; set; }
            // Common
            public string? SupplierName { get; set; }
        }

    public class ProductInfoFormVM
    {
        public int ProductId { get; set; }

        // ===== 主表 S_ProductInfo =====
        [Required]
        public string? ProductName { get; set; }

        // "game" / "notgame"
        [Required]
        public string? ProductType { get; set; }

        // S_ProductInfo.Price 是 decimal (非 nullable)
        public decimal Price { get; set; }

        // S_ProductInfo.CurrencyCode 允許 null；預設給 TWD 方便表單
        public string? CurrencyCode { get; set; } = "TWD";

        // ===== 共通（兩種明細都會用到）=====
        // 兩個明細表的 supplier_id 都是 NOT NULL，所以在 Controller 驗證 HasValue
        public int? SupplierId { get; set; }

        // ===== 遊戲明細 S_GameProductDetails =====
        // SQL 是 NULLABLE → 這裡保留為 int?
        public int? PlatformId { get; set; }
        public string? DownloadLink { get; set; }
        public string? GameProductDescription { get; set; }

        // ===== 周邊明細 S_OtherProductDetails =====
        public int? MerchTypeId { get; set; }   // SQL 可為 NULL
        public string? DigitalCode { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Material { get; set; }
        public string? OtherProductDescription { get; set; } 

        // ===== 你之前 VM 裡出現但目前 DB 沒有的欄位（先移除）=====
        // public int ShipmentQuantity { get; set; }
        // public bool IsActive { get; set; } = true;
        // public string? PlatformName { get; set; }
        // public string? GameType { get; set; }
        // public int? StockQuantity { get; set; }

    }


}


