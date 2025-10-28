using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 錢包視圖模型集合
    /// </summary>
    public class WalletViewModels
    {
        /// <summary>
        /// 錢包統計信息
        /// </summary>
        public class WalletStatisticsReadModel
        {
            public int TotalUsers { get; set; }
            public int UsersWithPoints { get; set; }
            public long TotalPoints { get; set; }
            public long TotalCoupons { get; set; }
            public long TotalEVouchers { get; set; }
        }

        /// <summary>
        /// 錢包交易記錄
        /// </summary>
        public class WalletTransaction
        {
            public int TransactionId { get; set; }
            public int UserId { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public int Amount { get; set; }
            public string Description { get; set; } = string.Empty;
            public DateTime TransactionTime { get; set; }
        }
    }

    /// <summary>
    /// 錢包歷史記錄篩選視圖模型
    /// </summary>
    public class WalletHistoryFilterViewModel
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 交易類型 (Point/Coupon/EVoucher)
        /// </summary>
        [StringLength(20)]
        public string? TransactionType { get; set; }

        /// <summary>
        /// 開始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 最小金額
        /// </summary>
        public int? MinAmount { get; set; }

        /// <summary>
        /// 最大金額
        /// </summary>
        public int? MaxAmount { get; set; }

        /// <summary>
        /// 描述關鍵字
        /// </summary>
        [StringLength(200)]
        public string? Description { get; set; }

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

        /// <summary>
        /// 排序欄位
        /// </summary>
        [StringLength(50)]
        public string? SortBy { get; set; } = "TransactionTime";

        /// <summary>
        /// 排序方向 (asc/desc)
        /// </summary>
        [StringLength(4)]
        public string? SortOrder { get; set; } = "desc";
    }
}

