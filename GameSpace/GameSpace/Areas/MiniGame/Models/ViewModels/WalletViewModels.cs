﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 錢包查詢模型
    /// </summary>
    public class WalletQueryModel
    {
        [Range(0, int.MaxValue, ErrorMessage = "會員 ID 不可為負數")]
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? ChangeType { get; set; } // "Point", "Coupon", "EVoucher"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "最少點數不可為負數")]
        public int? MinAmount { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "最多點數不可為負數")]
        public int? MaxAmount { get; set; }
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "ChangeTime";
        public bool Descending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 錢包統計模型
    /// </summary>
    public class WalletStatisticsModel
    {
        public int TotalTransactions { get; set; }
        public int TotalPointsDistributed { get; set; }
        public int TotalPointsSpent { get; set; }
        public int CurrentTotalPoints { get; set; }
        public int TotalCouponsDistributed { get; set; }
        public int TotalEVouchersDistributed { get; set; }
        public Dictionary<string, int> TransactionTypeDistribution { get; set; } = new();
        public Dictionary<string, int> DailyTransactionTrend { get; set; } = new();
    }

    /// <summary>
    /// 用戶錢包摘要
    /// </summary>
    public class UserWalletSummaryModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsSpent { get; set; }
        public int TotalCoupons { get; set; }
        public int UnusedCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int UnusedEVouchers { get; set; }
        public DateTime? LastTransaction { get; set; }
    }

    /// <summary>
    /// 錢包摘要模型
    /// </summary>
    public class WalletSummary
    {
        public int TotalPoints { get; set; }
        public int PointsEarned { get; set; }
        public int PointsSpent { get; set; }
        public int AvailableCoupons { get; set; }
        public int UsedCoupons { get; set; }
        public int AvailableEVouchers { get; set; }
        public int UsedEVouchers { get; set; }
        public DateTime? LastTransactionTime { get; set; }
        public int TransactionCount { get; set; }

        // Additional properties for compatibility
        public int TotalUsers { get; set; }
    }

    /// <summary>
    /// 錢包交易模型
    /// </summary>
    public class WalletTransaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // Point, Coupon, EVoucher
        public int PointsChanged { get; set; }
        public int PointsAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
        public string? RelatedCouponCode { get; set; }
        public string? RelatedEVoucherCode { get; set; }
    }

    /// <summary>
    /// 發放點數輸入模型
    /// </summary>
    public class IssuePointsInputModel
    {
        [Required(ErrorMessage = "會員 ID 為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "請輸入有效的會員 ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "點數為必填")]
        [Range(-1000000, 1000000, ErrorMessage = "點數範圍必須在 -1,000,000 到 1,000,000 之間")]
        public int Points { get; set; }

        [Required(ErrorMessage = "發放原因為必填")]
        [StringLength(200, ErrorMessage = "發放原因最多 200 字元")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "備註最多 500 字元")]
        public string? Notes { get; set; }

        public string? UserName { get; set; }
        public int? CurrentPoints { get; set; }
        public string TransactionType { get; set; } = "後台調整";
    }

    /// <summary>
    /// 發放優惠券輸入模型
    /// </summary>
    public class IssueCouponInputModel
    {
        [Required(ErrorMessage = "會員 ID 為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "請輸入有效的會員 ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "優惠券類型 ID 為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的優惠券類型")]
        public int CouponTypeId { get; set; }

        [Required(ErrorMessage = "發放數量為必填")]
        [Range(1, 100, ErrorMessage = "發放數量範圍必須在 1 到 100 之間")]
        public int Quantity { get; set; } = 1;

        [Range(1, 365, ErrorMessage = "有效期範圍必須在 1 到 365 天之間")]
        public int? ValidityDays { get; set; } = 30;

        public DateTime? CustomExpiryDate { get; set; }

        [StringLength(500, ErrorMessage = "備註最多 500 字元")]
        public string? Notes { get; set; }

        public string? UserName { get; set; }
        public List<CouponTypeOption>? AvailableCouponTypes { get; set; }
    }

    /// <summary>
    /// 優惠券類型選項
    /// </summary>
    public class CouponTypeOption
    {
        public int CouponTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty; // "Percentage" or "Amount"
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
    }

    /// <summary>
    /// 調整電子禮券輸入模型
    /// </summary>
    public class AdjustEVoucherInputModel
    {
        [Required(ErrorMessage = "電子禮券 ID 為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "請輸入有效的電子禮券 ID")]
        public int EVoucherId { get; set; }

        [Required(ErrorMessage = "操作類型為必填")]
        public string Action { get; set; } = string.Empty; // "Revoke", "MarkUsed", "Restore"

        [Required(ErrorMessage = "備註為必填")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "備註長度必須在 5 到 500 字元之間")]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// 查詢錢包交易歷史模型
    /// </summary>
    public class QueryHistoryModel
    {
        [Display(Name = "會員 ID")]
        public int? UserId { get; set; }

        [Display(Name = "交易類型")]
        public string? TransactionType { get; set; } // Points, Coupon, EVoucher

        [Display(Name = "操作類型")]
        public string? ActionType { get; set; } // Earn, Spend, Use, Expire

        [Display(Name = "開始日期")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "結束日期")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "當前頁")]
        public int Page { get; set; } = 1;

        [Display(Name = "每頁筆數")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 交易記錄結果列表
        /// </summary>
        public List<WalletTransactionItem> Transactions { get; set; } = new();

        /// <summary>
        /// 錢包交易項目
        /// </summary>
        public class WalletTransactionItem
        {
            [Display(Name = "記錄 ID")]
            public int LogId { get; set; }

            [Display(Name = "會員 ID")]
            public int UserId { get; set; }

            [Display(Name = "交易類型")]
            public string TransactionType { get; set; } = string.Empty;

            [Display(Name = "操作類型")]
            public string ActionType { get; set; } = string.Empty;

            [Display(Name = "點數變更")]
            public int PointsChanged { get; set; }

            [Display(Name = "描述")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "交易時間")]
            public DateTime TransactionDate { get; set; }
        }
    }

    /// <summary>
    /// 調整會員點數模型
    /// </summary>
    public class AdjustPointsModel
    {
        [Required(ErrorMessage = "會員 ID 為必填")]
        [Display(Name = "會員 ID")]
        public int UserId { get; set; }

        [Display(Name = "會員名稱")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "目前點數")]
        public int CurrentPoints { get; set; }

        [Required(ErrorMessage = "調整類型為必填")]
        [Display(Name = "調整類型")]
        public string AdjustmentType { get; set; } = string.Empty; // "Add" or "Deduct"

        [Required(ErrorMessage = "調整金額為必填")]
        [Range(1, 100000, ErrorMessage = "調整金額範圍必須在 1 到 100,000 之間")]
        [Display(Name = "調整金額")]
        public int AdjustmentAmount { get; set; }

        [Display(Name = "調整金額（別名）")]
        public int Amount
        {
            get => AdjustmentAmount;
            set => AdjustmentAmount = value;
        }

        [Required(ErrorMessage = "調整原因為必填")]
        [StringLength(500, ErrorMessage = "調整原因最多 500 字元")]
        [Display(Name = "調整原因")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述最多 500 字元")]
        [Display(Name = "描述")]
        public string? Description
        {
            get => Reason;
            set => Reason = value ?? string.Empty;
        }

        [Display(Name = "操作者 ID")]
        public int OperatorId { get; set; }
    }
}

