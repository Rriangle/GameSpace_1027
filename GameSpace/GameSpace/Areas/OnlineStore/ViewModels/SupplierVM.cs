using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
    // Index 查詢參數 + 分頁回填
    public class SupplierIndexQueryVM
    {
        public string? Q { get; set; }
        public string Status { get; set; } = "all"; // all|active|inactive
        public string? From { get; set; }
        public string? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int Total { get; set; }
    }

    // Index 顯示的每列
    public class SupplierIndexRowVM
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
        public bool? IsActive { get; set; }
        public int GameProductCount { get; set; }
        public int OtherProductCount { get; set; }
        public int TotalProducts => GameProductCount + OtherProductCount;
    }

    // 儀表板
    public class SupplierTopVM
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
        public int TotalProducts { get; set; }
    }
    public class SupplierDashboardVM
    {
        public List<SupplierTopVM> TopSuppliers { get; set; } = new();
        public int GameSupplierCount { get; set; }
        public int OtherSupplierCount { get; set; }
        public int InactiveSupplierCount { get; set; }
    }

    // Index 頁整體 VM
    public class SupplierIndexPageVM
    {
        public SupplierIndexQueryVM Query { get; set; } = new();
        public List<SupplierIndexRowVM> Rows { get; set; } = new();
        public SupplierDashboardVM Dashboard { get; set; } = new();
    }

    // 編輯/新增
    public class SupplierVM
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "供應商名稱為必填")]
        [MaxLength(200)]
        public string SupplierName { get; set; } = string.Empty;

        public bool? IsActive { get; set; } // 若 DB 無此欄位，控制器會忽略
    }

    // 詳細
    public class SupplierDetailVM
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
        public bool? IsActive { get; set; }
        public int GameProductCount { get; set; }
        public int OtherProductCount { get; set; }
    }

    // 供應商所屬商品列
    public class SupplierProductVM
    {
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }

        public string Category { get; set; } = ""; // 遊戲商品 / 周邊商品
    }
}