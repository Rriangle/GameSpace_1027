﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 簽到規則讀取模型
    /// </summary>
    public class SignInRuleReadModel
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// Name alias for compatibility with views
        /// </summary>
        public string Name
        {
            get => RuleName;
            set => RuleName = value;
        }

        public string Description { get; set; } = string.Empty;
        public int ConsecutiveDays { get; set; }
        public int PointsReward { get; set; }
        public int ExpReward { get; set; }
        public int? CouponTypeId { get; set; }
        public string? CouponTypeName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 簽到記錄讀取模型
    /// </summary>
    public class SignInRecordReadModel
    {
        public int RecordId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime SignInDate { get; set; }
        public int ConsecutiveDays { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public int? CouponGained { get; set; }
        public string? CouponCode { get; set; }
        public string RewardDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// 簽到統計查詢模型
    /// </summary>
    public class SignInStatsQueryModel
    {
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinConsecutiveDays { get; set; }
        public int? MaxConsecutiveDays { get; set; }
        public string? SortBy { get; set; } = "SignInDate";
        public bool Descending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 簽到統計摘要
    /// </summary>
    public class SignInStatsSummary
    {
        public int TotalSignIns { get; set; }
        public int UniqueUsers { get; set; }
        public int TotalPointsDistributed { get; set; }
        public int TotalExpDistributed { get; set; }
        public int TotalCouponsDistributed { get; set; }
        public int AverageConsecutiveDays { get; set; }
        public int MaxConsecutiveDays { get; set; }
        public DateTime? LastSignInDate { get; set; }
        public Dictionary<int, int> ConsecutiveDaysDistribution { get; set; } = new();
        public Dictionary<string, int> DailySignInTrend { get; set; } = new();

        // Additional properties for compatibility
        public int TodaySignInCount { get; set; }
        public int ThisWeekSignInCount { get; set; }
        public int ThisMonthSignInCount { get; set; }
        public int PerfectAttendanceCount { get; set; }
        public int TotalPointsGranted { get => TotalPointsDistributed; set => TotalPointsDistributed = value; }
        public int TotalExpGranted { get => TotalExpDistributed; set => TotalExpDistributed = value; }
        public int TotalCouponsGranted { get => TotalCouponsDistributed; set => TotalCouponsDistributed = value; }
    }

    /// <summary>
    /// 簽到規則模型（用於建立/更新）
    /// </summary>
    public class SignInRulesModel
    {
        public int? RuleId { get; set; }

        [Required(ErrorMessage = "規則名稱為必填")]
        [StringLength(100, ErrorMessage = "規則名稱長度不可超過 100 字元")]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "連續天數為必填")]
        [Range(1, 365, ErrorMessage = "連續天數必須在 1-365 之間")]
        public int ConsecutiveDays { get; set; }

        [Required(ErrorMessage = "點數獎勵為必填")]
        [Range(0, 10000, ErrorMessage = "點數獎勵必須在 0-10000 之間")]
        public int PointsReward { get; set; }

        [Range(0, 10000, ErrorMessage = "經驗值獎勵必須在 0-10000 之間")]
        public int ExpReward { get; set; }

        public int? CouponTypeId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<SignInRuleReadModel> Rules { get; set; } = new();

        public int TotalRules => Rules.Count;
    }

    /// <summary>
    /// 簽到統計索引 ViewModel
    /// </summary>
    public class AdminSignInStatsIndexViewModel
    {
        public SignInStatsSummary Summary { get; set; } = new();
        public List<SignInRecordReadModel> RecentSignIns { get; set; } = new();
        public List<SignInRuleReadModel> ActiveRules { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

    /// <summary>
    /// 簽到規則更新模型
    /// </summary>
    public class SignInRuleUpdateModel
    {
        [Required(ErrorMessage = "規則ID為必填")]
        public int RuleId { get; set; }

        [Required(ErrorMessage = "規則名稱為必填")]
        [StringLength(100, ErrorMessage = "規則名稱長度不可超過 100 字元")]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "連續天數為必填")]
        [Range(1, 365, ErrorMessage = "連續天數必須在 1-365 之間")]
        public int ConsecutiveDays { get; set; }

        [Required(ErrorMessage = "點數獎勵為必填")]
        [Range(0, 10000, ErrorMessage = "點數獎勵必須在 0-10000 之間")]
        public int PointsReward { get; set; }

        [Range(0, 10000, ErrorMessage = "經驗值獎勵必須在 0-10000 之間")]
        public int ExpReward { get; set; }

        public int? CouponTypeId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 簽到規則配置 ViewModel (用於 Rules.cshtml 頁面)
    /// </summary>
    public class SignInRuleConfigViewModel
    {
        public SignInRuleConfig SignInRule { get; set; } = new();
    }

    /// <summary>
    /// 簽到規則配置模型
    /// </summary>
    public class SignInRuleConfig
    {
        [Required(ErrorMessage = "簽到天數為必填")]
        [Range(1, 365, ErrorMessage = "簽到天數必須在 1-365 之間")]
        public int SignInDay { get; set; }

        [Required(ErrorMessage = "每日簽到點數為必填")]
        [Range(0, 10000, ErrorMessage = "每日簽到點數必須在 0-10000 之間")]
        public int DailyPoints { get; set; }

        [Required(ErrorMessage = "週獎勵點數為必填")]
        [Range(0, 10000, ErrorMessage = "週獎勵點數必須在 0-10000 之間")]
        public int WeeklyBonusPoints { get; set; }

        [Required(ErrorMessage = "月獎勵點數為必填")]
        [Range(0, 10000, ErrorMessage = "月獎勵點數必須在 0-10000 之間")]
        public int MonthlyBonusPoints { get; set; }

        [Required(ErrorMessage = "連續簽到天數為必填")]
        [Range(1, 365, ErrorMessage = "連續簽到天數必須在 1-365 之間")]
        public int ConsecutiveDays { get; set; }

        [StringLength(500, ErrorMessage = "規則描述長度不可超過 500 字元")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 簽到規則輸入模型（用於建立/編輯簽到規則）
    /// </summary>
    public class SignInRuleInputModel
    {
        [Required(ErrorMessage = "簽到天數為必填")]
        [Display(Name = "簽到天數")]
        [Range(1, 365, ErrorMessage = "簽到天數必須在 1-365 之間")]
        public int SignInDay { get; set; }

        [Required(ErrorMessage = "點數獎勵為必填")]
        [Display(Name = "點數獎勵")]
        [Range(0, 10000, ErrorMessage = "點數獎勵必須在 0-10000 之間")]
        public int PointsReward { get; set; }

        [Display(Name = "點數")]
        public int Points
        {
            get => PointsReward;
            set => PointsReward = value;
        }

        [Display(Name = "寵物經驗值獎勵")]
        [Range(0, 10000, ErrorMessage = "寵物經驗值獎勵必須在 0-10000 之間")]
        public int? PetExpReward { get; set; }

        [Display(Name = "經驗值")]
        public int? Experience
        {
            get => PetExpReward;
            set => PetExpReward = value;
        }

        [Display(Name = "優惠券類型")]
        public int? CouponTypeId { get; set; }

        [Display(Name = "優惠券代碼")]
        [StringLength(50, ErrorMessage = "優惠券代碼長度不可超過 50 字元")]
        public string? CouponTypeCode { get; set; }

        [Display(Name = "規則描述")]
        [StringLength(500, ErrorMessage = "規則描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 手動簽到輸入模型（用於後台手動簽到操作）
    /// </summary>
    public class ManualSignInInputModel
    {
        [Required(ErrorMessage = "會員ID為必填")]
        [Display(Name = "會員ID")]
        [Range(1, int.MaxValue, ErrorMessage = "請輸入有效的會員ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "簽到日期為必填")]
        [Display(Name = "簽到日期")]
        [DataType(DataType.Date)]
        public DateTime SignInDate { get; set; } = DateTime.Today;

        [Display(Name = "強制連續簽到")]
        public bool ForceConsecutive { get; set; } = false;

        [Required(ErrorMessage = "備註為必填")]
        [Display(Name = "備註")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "備註長度必須在 5-500 字元之間")]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "原因")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "原因長度必須在 5-500 字元之間")]
        public string? Reason
        {
            get => Notes;
            set => Notes = value ?? string.Empty;
        }

        // Additional properties for display purposes
        public string? UserName { get; set; }

        [Display(Name = "會員帳號")]
        public string? UserAccount { get; set; }

        public int? CurrentConsecutiveDays { get; set; }
    }

    /// <summary>
    /// 簽到規則檢視模型（用於顯示簽到規則資料）
    /// </summary>
    public class SignInRuleViewModel
    {
        [Display(Name = "規則ID")]
        public int RuleId { get; set; }

        /// <summary>
        /// Alias for RuleId (for backward compatibility)
        /// </summary>
        public int Id
        {
            get => RuleId;
            set => RuleId = value;
        }

        [Display(Name = "連續天數")]
        public int ConsecutiveDays { get; set; }

        /// <summary>
        /// Alias for ConsecutiveDays (for backward compatibility)
        /// </summary>
        [Display(Name = "簽到天數")]
        public int SignInDay
        {
            get => ConsecutiveDays;
            set => ConsecutiveDays = value;
        }

        [Display(Name = "點數獎勵")]
        [Range(0, int.MaxValue, ErrorMessage = "點數獎勵必須為非負數")]
        public int RewardPoints { get; set; }

        /// <summary>
        /// Alias for RewardPoints (for backward compatibility)
        /// </summary>
        [Display(Name = "點數獎勵")]
        public int PointsReward
        {
            get => RewardPoints;
            set => RewardPoints = value;
        }

        [Display(Name = "寵物經驗值獎勵")]
        [Range(0, int.MaxValue, ErrorMessage = "寵物經驗值獎勵必須為非負數")]
        public int RewardPetExp { get; set; }

        /// <summary>
        /// Alias for RewardPetExp (for backward compatibility)
        /// </summary>
        [Display(Name = "寵物經驗值獎勵")]
        public int? PetExpReward
        {
            get => RewardPetExp;
            set => RewardPetExp = value ?? 0;
        }

        [Display(Name = "優惠券類型ID")]
        public int? CouponTypeId { get; set; }

        [Display(Name = "優惠券類型名稱")]
        [StringLength(100, ErrorMessage = "優惠券類型名稱長度不可超過 100 字元")]
        public string? CouponTypeName { get; set; }

        [Display(Name = "電子票券類型ID")]
        public int? EvoucherTypeId { get; set; }

        [Display(Name = "電子票券類型名稱")]
        [StringLength(100, ErrorMessage = "電子票券類型名稱長度不可超過 100 字元")]
        public string? EvoucherTypeName { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "規則描述")]
        [StringLength(500, ErrorMessage = "規則描述長度不可超過 500 字元")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 簽到統計檢視模型（用於顯示簽到統計資料）
    /// </summary>
    public class SignInStatsViewModel
    {
        [Display(Name = "總簽到次數")]
        public int TotalSignIns { get; set; }

        [Display(Name = "今日簽到次數")]
        public int TodaySignIns { get; set; }

        [Display(Name = "本週簽到次數")]
        public int WeekSignIns { get; set; }

        [Display(Name = "本月簽到次數")]
        public int MonthlySignIns { get; set; }

        [Display(Name = "本月簽到次數（別名）")]
        public int MonthSignIns
        {
            get => MonthlySignIns;
            set => MonthlySignIns = value;
        }

        [Display(Name = "最大連續簽到天數")]
        public int MaxConsecutiveDays { get; set; }

        [Display(Name = "總點數獎勵")]
        public int TotalPointsRewarded { get; set; }

        [Display(Name = "總經驗值獎勵")]
        public int TotalPetExpRewarded { get; set; }

        [Display(Name = "總優惠券獎勵")]
        public int TotalCouponsRewarded { get; set; }

        [Display(Name = "總電子票券獎勵")]
        public int TotalEvouchersRewarded { get; set; }

        [Display(Name = "活躍用戶數")]
        public int ActiveUsers { get; set; }

        [Display(Name = "活躍用戶數（別名）")]
        public int ActiveUsersCount
        {
            get => ActiveUsers;
            set => ActiveUsers = value;
        }

        [Display(Name = "平均連續簽到天數")]
        public decimal AverageConsecutiveDays { get; set; }

        [Display(Name = "統計生成時間")]
        public DateTime? StatsGeneratedAt { get; set; }

        [Display(Name = "簽到排行榜")]
        public List<UserSignInStat> TopUsers { get; set; } = new List<UserSignInStat>();

        /// <summary>
        /// 用戶簽到統計（嵌套類別）
        /// </summary>
        public class UserSignInStat
        {
            [Display(Name = "會員ID")]
            public int UserId { get; set; }

            [Display(Name = "會員名稱")]
            public string UserName { get; set; } = string.Empty;

            [Display(Name = "簽到次數")]
            public int SignInCount { get; set; }

            [Display(Name = "連續簽到天數")]
            public int ConsecutiveDays { get; set; }
        }
    }
}

