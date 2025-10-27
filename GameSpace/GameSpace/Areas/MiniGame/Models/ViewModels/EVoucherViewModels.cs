using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 電子票券讀取模型
    /// </summary>
    public class EVoucherReadModel
    {
        /// <summary>
        /// 票券ID
        /// </summary>
        public int EVoucherId { get; set; }

        /// <summary>
        /// 票券代碼
        /// </summary>
        public string EVoucherCode { get; set; } = string.Empty;

        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 用戶Email
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// 票券類型ID
        /// </summary>
        public int EVoucherTypeId { get; set; }

        /// <summary>
        /// 票券類型名稱
        /// </summary>
        public string EVoucherTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 票券類型代碼
        /// </summary>
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 票券價值
        /// </summary>
        public decimal VoucherValue { get; set; }

        /// <summary>
        /// 商家名稱
        /// </summary>
        public string? MerchantName { get; set; }

        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// 取得時間
        /// </summary>
        public DateTime AcquiredTime { get; set; }

        /// <summary>
        /// 使用時間
        /// </summary>
        public DateTime? UsedTime { get; set; }

        /// <summary>
        /// 有效期限起始
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// 有效期限結束
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// 是否已過期
        /// </summary>
        public bool IsExpired => DateTime.Now > ValidTo;

        /// <summary>
        /// 是否有效（未使用且未過期）
        /// </summary>
        public bool IsValid => !IsUsed && !IsExpired;

        /// <summary>
        /// 狀態文字
        /// </summary>
        public string Status
        {
            get
            {
                if (IsUsed) return "已使用";
                if (IsExpired) return "已過期";
                return "未使用";
            }
        }

        /// <summary>
        /// 使用地點/訂單ID
        /// </summary>
        public string? UsedLocation { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 電子票券查詢模型
    /// </summary>
    public class EVoucherQueryModel
    {
        // Result properties (for displaying search results)
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string EVoucherCode { get; set; } = string.Empty;
        public string EVoucherTypeName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? UsedDate { get; set; }
        public bool IsUsed { get; set; }

        /// <summary>
        /// 搜尋詞（用於查詢）
        /// </summary>
        [StringLength(100)]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "會員 ID 不可為負數")]
        public int? UserId { get; set; }

        /// <summary>
        /// 票券類型ID
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "電子禮券類型ID 不可為負數")]
        public int? EVoucherTypeId { get; set; }

        /// <summary>
        /// 票券類型代碼
        /// </summary>
        [StringLength(20)]
        public string? TypeCode { get; set; }

        /// <summary>
        /// 票券代碼（模糊搜尋）
        /// </summary>
        [StringLength(50)]
        public string? VoucherCode { get; set; }

        /// <summary>
        /// 取得時間起始
        /// </summary>
        public DateTime? AcquiredFrom { get; set; }

        /// <summary>
        /// 取得時間結束
        /// </summary>
        public DateTime? AcquiredTo { get; set; }

        /// <summary>
        /// 使用時間起始
        /// </summary>
        public DateTime? UsedFrom { get; set; }

        /// <summary>
        /// 使用時間結束
        /// </summary>
        public DateTime? UsedTo { get; set; }

        /// <summary>
        /// 是否僅顯示有效票券
        /// </summary>
        public bool? OnlyValid { get; set; }

        /// <summary>
        /// 商家名稱
        /// </summary>
        [StringLength(100)]
        public string? MerchantName { get; set; }

        /// <summary>
        /// 排序欄位
        /// </summary>
        [StringLength(50)]
        public string SortBy { get; set; } = "AcquiredTime";

        /// <summary>
        /// 狀態篩選 (unused/used/expired)
        /// </summary>
        [StringLength(20)]
        public string? Status { get; set; }

        /// <summary>
        /// 是否降序排列
        /// </summary>
        public bool SortDescending { get; set; } = true;

        /// <summary>
        /// Descending 別名屬性 (向後相容)
        /// </summary>
        public bool Descending
        {
            get => SortDescending;
            set => SortDescending = value;
        }

        /// <summary>
        /// 頁碼
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 電子票券統計模型
    /// </summary>
    public class EVoucherStatisticsModel
    {
        /// <summary>
        /// 票券總數
        /// </summary>
        public int TotalVouchers { get; set; }

        /// <summary>
        /// 已使用票券數
        /// </summary>
        public int UsedVouchers { get; set; }

        /// <summary>
        /// 未使用票券數
        /// </summary>
        public int UnusedVouchers { get; set; }

        /// <summary>
        /// 已過期票券數
        /// </summary>
        public int ExpiredVouchers { get; set; }

        /// <summary>
        /// 有效票券數
        /// </summary>
        public int ValidVouchers { get; set; }

        /// <summary>
        /// 票券類型總數
        /// </summary>
        public int TotalVoucherTypes { get; set; }

        /// <summary>
        /// 票券總價值
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// 已使用票券總價值
        /// </summary>
        public decimal UsedValue { get; set; }

        /// <summary>
        /// 未使用票券總價值
        /// </summary>
        public decimal UnusedValue { get; set; }

        /// <summary>
        /// 票券類型分佈統計
        /// </summary>
        public Dictionary<string, int> VoucherTypeDistribution { get; set; } = new();

        /// <summary>
        /// 每日使用趨勢
        /// </summary>
        public Dictionary<string, int> DailyUsageTrend { get; set; } = new();

        /// <summary>
        /// 商家分佈統計
        /// </summary>
        public Dictionary<string, int> MerchantDistribution { get; set; } = new();

        /// <summary>
        /// 使用率
        /// </summary>
        public double UsageRate => TotalVouchers > 0 ? (double)UsedVouchers / TotalVouchers * 100 : 0;
    }

    /// <summary>
    /// 電子票券類型 ViewModel
    /// </summary>
    public class EVoucherTypeViewModel
    {
        /// <summary>
        /// 票券類型ID
        /// </summary>
        public int EVoucherTypeId { get; set; }

        /// <summary>
        /// 類型名稱
        /// </summary>
        [Required(ErrorMessage = "類型名稱為必填")]
        [StringLength(100, ErrorMessage = "類型名稱長度不可超過 100 字元")]
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 類型代碼
        /// </summary>
        [Required(ErrorMessage = "類型代碼為必填")]
        [StringLength(20, ErrorMessage = "類型代碼長度不可超過 20 字元")]
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 票券價值
        /// </summary>
        [Required(ErrorMessage = "票券價值為必填")]
        [Range(0.01, 1000000, ErrorMessage = "票券價值必須在 0.01-1000000 之間")]
        public decimal VoucherValue { get; set; }

        /// <summary>
        /// 商家名稱
        /// </summary>
        [StringLength(100, ErrorMessage = "商家名稱長度不可超過 100 字元")]
        public string? MerchantName { get; set; }

        /// <summary>
        /// 商家聯絡方式
        /// </summary>
        [StringLength(200)]
        public string? MerchantContact { get; set; }

        /// <summary>
        /// 兌換所需點數
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "兌換所需點數必須大於等於 0")]
        public int PointsCost { get; set; }

        /// <summary>
        /// 有效天數
        /// </summary>
        [Range(1, 3650, ErrorMessage = "有效天數必須在 1-3650 之間")]
        public int ValidDays { get; set; } = 90;

        /// <summary>
        /// 使用說明
        /// </summary>
        [StringLength(1000)]
        public string? UsageInstructions { get; set; }

        /// <summary>
        /// 使用條款
        /// </summary>
        [StringLength(2000)]
        public string? TermsAndConditions { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 庫存數量（-1 表示無限制）
        /// </summary>
        public int StockQuantity { get; set; } = -1;

        /// <summary>
        /// 已發放數量
        /// </summary>
        public int IssuedQuantity { get; set; } = 0;

        /// <summary>
        /// 圖示URL
        /// </summary>
        [StringLength(500)]
        [Url(ErrorMessage = "圖示URL格式不正確")]
        public string? IconUrl { get; set; }

        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 用戶電子票券摘要
    /// </summary>
    public class UserEVoucherSummary
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 票券總數
        /// </summary>
        public int TotalVouchers { get; set; }

        /// <summary>
        /// 未使用票券數
        /// </summary>
        public int UnusedVouchers { get; set; }

        /// <summary>
        /// 已使用票券數
        /// </summary>
        public int UsedVouchers { get; set; }

        /// <summary>
        /// 已過期票券數
        /// </summary>
        public int ExpiredVouchers { get; set; }

        /// <summary>
        /// 有效票券總價值
        /// </summary>
        public decimal ValidVouchersValue { get; set; }

        /// <summary>
        /// 最後取得時間
        /// </summary>
        public DateTime? LastAcquiredTime { get; set; }

        /// <summary>
        /// 最後使用時間
        /// </summary>
        public DateTime? LastUsedTime { get; set; }
    }

    /// <summary>
    /// 電子票券發放請求
    /// </summary>
    public class IssueEVoucherRequest
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        /// <summary>
        /// 票券類型ID
        /// </summary>
        [Required(ErrorMessage = "票券類型ID為必填")]
        public int EVoucherTypeId { get; set; }

        /// <summary>
        /// 發放數量
        /// </summary>
        [Required(ErrorMessage = "發放數量為必填")]
        [Range(1, 100, ErrorMessage = "發放數量必須在 1-100 之間")]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// 發放原因
        /// </summary>
        [StringLength(500)]
        public string? Reason { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 電子票券編輯模型
    /// </summary>
    public class EVoucherEditModel
    {
        /// <summary>
        /// 票券ID
        /// </summary>
        [Required(ErrorMessage = "票券ID為必填")]
        [Display(Name = "票券ID")]
        public int Id { get; set; }

        /// <summary>
        /// 票券代碼
        /// </summary>
        [Required(ErrorMessage = "票券代碼為必填")]
        [StringLength(50, ErrorMessage = "票券代碼長度不可超過 50 字元")]
        [Display(Name = "票券代碼")]
        public string EVoucherCode { get; set; } = string.Empty;

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填")]
        [Display(Name = "用戶ID")]
        public int UserId { get; set; }

        /// <summary>
        /// 票券類型ID（主屬性，使用小寫 Id 以符合資料庫欄位命名）
        /// </summary>
        [Required(ErrorMessage = "票券類型ID為必填")]
        [Display(Name = "票券類型")]
        public int EvoucherTypeId { get; set; }

        /// <summary>
        /// 票券類型ID（別名屬性，用於向後相容）
        /// </summary>
        public int EVoucherTypeID
        {
            get => EvoucherTypeId;
            set => EvoucherTypeId = value;
        }

        /// <summary>
        /// 票券價值
        /// </summary>
        [Range(0.01, 1000000, ErrorMessage = "票券價值必須在 0.01-1000000 之間")]
        [Display(Name = "票券價值")]
        public decimal? Value { get; set; }

        /// <summary>
        /// 有效期限
        /// </summary>
        [Display(Name = "有效期限")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// 是否已使用
        /// </summary>
        [Display(Name = "使用狀態")]
        public bool IsUsed { get; set; }

        /// <summary>
        /// 使用日期
        /// </summary>
        [Display(Name = "使用日期")]
        public DateTime? UsedDate { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(1000, ErrorMessage = "描述長度不可超過 1000 字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime? UpdatedAt { get; set; }
    }
}
