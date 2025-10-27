﻿using System;
using System.ComponentModel.DataAnnotations;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    // 管理員相關 ViewModels
    public class AdminManagerCreateViewModel
    {
        [Required(ErrorMessage = "管理者名稱為必填")]
        [StringLength(50, ErrorMessage = "管理者名稱長度不能超過50個字符")]
        public string Manager_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "管理者帳號為必填")]
        [StringLength(50, ErrorMessage = "管理者帳號長度不能超過50個字符")]
        public string Manager_Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6-100個字符之間")]
        public string Manager_Password { get; set; } = string.Empty;

        /// <summary>
        /// Password alias for compatibility
        /// </summary>
        public string Password
        {
            get => Manager_Password;
            set => Manager_Password = value;
        }

        [Required(ErrorMessage = "確認密碼為必填")]
        [Compare("Manager_Password", ErrorMessage = "確認密碼與密碼不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件為必填")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [StringLength(100, ErrorMessage = "電子郵件長度不能超過100個字符")]
        public string Manager_Email { get; set; } = string.Empty;

        /// <summary>
        /// Role IDs for role assignment
        /// </summary>
        public List<int> RoleIds { get; set; } = new List<int>();

        /// <summary>
        /// Whether the manager is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    public class AdminManagerEditViewModel
    {
        public int Manager_Id { get; set; }

        [Required(ErrorMessage = "管理者名稱為必填")]
        [StringLength(50, ErrorMessage = "管理者名稱長度不能超過50個字符")]
        public string Manager_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "管理者帳號為必填")]
        [StringLength(50, ErrorMessage = "管理者帳號長度不能超過50個字符")]
        public string Manager_Account { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6-100個字符之間")]
        public string? Manager_Password { get; set; }

        [Required(ErrorMessage = "電子郵件為必填")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [StringLength(100, ErrorMessage = "電子郵件長度不能超過100個字符")]
        public string Manager_Email { get; set; } = string.Empty;

        /// <summary>
        /// New password for password change (optional)
        /// </summary>
        [StringLength(100, MinimumLength = 6, ErrorMessage = "新密碼長度必須在6-100個字符之間")]
        public string? NewPassword { get; set; }

        /// <summary>
        /// Confirm new password
        /// </summary>
        [Compare("NewPassword", ErrorMessage = "確認新密碼與新密碼不一致")]
        public string? ConfirmNewPassword { get; set; }

        /// <summary>
        /// Role IDs for role assignment
        /// </summary>
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class AdminManagerRoleCreateViewModel
    {
        [Required(ErrorMessage = "角色名稱為必填")]
        [StringLength(50, ErrorMessage = "角色名稱長度不能超過50個字符")]
        public string role_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "角色描述為必填")]
        [StringLength(200, ErrorMessage = "角色描述長度不能超過200個字符")]
        public string role_description { get; set; } = string.Empty;

        /// <summary>
        /// Permission IDs to assign to this role
        /// </summary>
        public List<int> PermissionIds { get; set; } = new List<int>();

        /// <summary>
        /// Whether the role is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Legacy properties for backward compatibility
        [Required]
        public int Manager_Id { get; set; }

        [Required]
        public int ManagerRole_Id { get; set; }
    }

    public class AdminManagerIndexViewModel
    {
        public IReadOnlyList<ManagerDatum> Managers { get; set; } = Array.Empty<ManagerDatum>();
        public IReadOnlyList<ManagerRolePermission> Roles { get; set; } = Array.Empty<ManagerRolePermission>();
        public string SearchTerm { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? SelectedRoleId { get; set; }
        public string SortBy { get; set; } = "name";
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / Math.Max(PageSize, 1));
        public int ActiveManagers { get; set; }
        public int TotalRoles { get; set; }
        public int TodayLogins { get; set; }
    }

    // 用戶相關 ViewModels
    public class AdminUserCreateViewModel
    {
        [Required(ErrorMessage = "用戶名稱為必填")]
        [StringLength(50, ErrorMessage = "用戶名稱長度不可超過 50 字元")]
        public string User_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "用戶帳號為必填")]
        [StringLength(50, ErrorMessage = "用戶帳號長度不可超過 50 字元")]
        public string User_account { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在 6-100 字元之間")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "確認密碼為必填")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密碼與確認密碼不符")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件為必填")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [StringLength(100, ErrorMessage = "電子郵件長度不可超過 100 字元")]
        public string User_email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "電話號碼格式不正確")]
        [StringLength(20, ErrorMessage = "電話號碼長度不可超過 20 字元")]
        public string? User_phone { get; set; }

        [DataType(DataType.Date)]
        public DateTime? User_birthday { get; set; }

        [StringLength(10, ErrorMessage = "性別長度不可超過 10 字元")]
        public string? User_gender { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "地址長度不可超過 500 字元")]
        public string? User_address { get; set; }
    }

    public class AdminUserEditViewModel
    {
        public int UserId { get; set; }

        // Alias for compatibility
        public int User_Id { get => UserId; set => UserId = value; }

        [Required(ErrorMessage = "用戶名稱為必填")]
        [StringLength(50, ErrorMessage = "用戶名稱長度不可超過 50 字元")]
        public string User_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "用戶帳號為必填")]
        [StringLength(50, ErrorMessage = "用戶帳號長度不可超過 50 字元")]
        public string User_account { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件為必填")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [StringLength(100, ErrorMessage = "電子郵件長度不可超過 100 字元")]
        public string User_email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "電話號碼格式不正確")]
        [StringLength(20, ErrorMessage = "電話號碼長度不可超過 20 字元")]
        public string? User_phone { get; set; }

        [DataType(DataType.Date)]
        public DateTime? User_birthday { get; set; }

        [StringLength(10, ErrorMessage = "性別長度不可超過 10 字元")]
        public string? User_gender { get; set; }

        public bool IsActive { get; set; }

        [StringLength(500, ErrorMessage = "地址長度不可超過 500 字元")]
        public string? User_address { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "新密碼長度必須在 6-100 字元之間")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認新密碼不符")]
        public string? ConfirmNewPassword { get; set; }
    }

    // 錢包相關 ViewModels
    public class UserWalletViewModel
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // Point, Coupon, EVoucher
        public int Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class AdminWalletIndexViewModel
    {
        public List<UserWalletViewModel> UserWallets { get; set; } = new();
        public PagedResult<UserCouponModel> Coupons { get; set; } = new();
        public CouponQueryModel CouponQuery { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public string SearchTerm { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public string SortBy { get; set; } = "date";
        public int TotalTransactions { get; set; }
        public int TotalEarned { get; set; }
        public int TotalSpent { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / Math.Max(PageSize, 1));
    }

    public class AdminWalletTransactionViewModel
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }

        /// <summary>
        /// 交易金額（別名屬性，為了向後相容）
        /// </summary>
        public decimal TransactionAmount
        {
            get => Amount;
            set => Amount = value;
        }

        /// <summary>
        /// 交易類型（別名屬性，為了向後相容）
        /// </summary>
        public string TransactionType
        {
            get => ChangeType;
            set => ChangeType = value;
        }
    }

    // EVoucher Index page model
    public class AdminEVoucherIndexViewModel
    {
        public PagedResult<GameSpace.Models.Evoucher> Evouchers { get; set; } = new();
        
        /// <summary>
        /// Total count for pagination display
        /// </summary>
        public int TotalCount => Evouchers?.TotalCount ?? 0;
    }

    public class AdjustEVouchersModel
    {
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "電子券類型ID為必填")]
        public int EvoucherTypeId { get; set; }

        /// <summary>
        /// Alias for view compatibility
        /// </summary>
        public int EVoucherTypeId
        {
            get => EvoucherTypeId;
            set => EvoucherTypeId = value;
        }

        [Required(ErrorMessage = "數量為必填")]
        [Range(1, 10, ErrorMessage = "數量必須在1-10之間")]
        public int Quantity { get; set; }

        [StringLength(200, ErrorMessage = "原因長度不可超過200字元")]
        public string? Reason { get; set; }

        [Range(0, 999999.99, ErrorMessage = "自訂金額必須在0-999999.99之間")]
        public decimal? CustomValue { get; set; }

        [StringLength(500, ErrorMessage = "描述長度不可超過500字元")]
        public string? Description { get; set; }

        /// <summary>
        /// Expiry date for the e-vouchers
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
    }

    public class GrantCouponsModel
    {
        [Required(ErrorMessage = "使用者 ID 為必填")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "優惠券類型 ID 為必填")]
        public int CouponTypeId { get; set; }

        [Required(ErrorMessage = "數量為必填")]
        [Range(1, 1000, ErrorMessage = "數量必須介於 1 到 1000 之間")]
        public int Quantity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        [Required(ErrorMessage = "原因為必填")]
        [StringLength(200, ErrorMessage = "原因不得超過 200 個字元")]
        public string Reason { get; set; } = string.Empty;

        [Range(0, 999999.99, ErrorMessage = "自訂金額必須介於 0 到 999999.99 之間")]
        public decimal? CustomValue { get; set; }

        [StringLength(500, ErrorMessage = "描述不得超過 500 個字元")]
        public string Description { get; set; } = string.Empty;

        // 保留舊屬性名稱以支持向後相容
        [Obsolete("請使用 UserId 屬性")]
        public int UserID
        {
            get => UserId;
            set => UserId = value;
        }

        [Obsolete("請使用 CouponTypeId 屬性")]
        public int CouponTypeID
        {
            get => CouponTypeId;
            set => CouponTypeId = value;
        }
    }

    public class GrantEVouchersModel
    {
        public int UserId { get; set; }
        public int EvoucherTypeId { get; set; }
        public int Quantity { get; set; }
    }

    public class WalletHistoryModel
    {
        public List<WalletHistory> Histories { get; set; } = new();
        public List<WalletHistory> Transactions { get; set; } = new(); // Alias for Histories
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    // 優惠券相關 ViewModels
    public class AdminCouponCreateViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CouponTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CouponName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999.99)]
        public decimal DiscountValue { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal MinOrderAmount { get; set; }

        [Range(0, 999999.99)]
        public decimal? MaxDiscountAmount { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? UsageLimit { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        // 保留舊屬性名稱以支持向後相容
        [Obsolete("請使用 UserId 屬性")]
        public int UserID
        {
            get => UserId;
            set => UserId = value;
        }

        [Obsolete("請使用 CouponTypeId 屬性")]
        public int CouponTypeID
        {
            get => CouponTypeId;
            set => CouponTypeId = value;
        }
    }

    // 電子券相關 ViewModels
    public class AdminEVoucherCreateViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int EvoucherTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string EVoucherCode { get; set; } = string.Empty;

        // Additional properties for Edit scenarios
        public int UserID { get => UserId; set => UserId = value; }
        public int EVoucherTypeID { get => EvoucherTypeId; set => EvoucherTypeId = value; }
        public string EvoucherCode { get => EVoucherCode; set => EVoucherCode = value; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public bool IsUsed { get; set; }
    }

    public class AdminEVoucherTypeCreateViewModel
    {
        [Required(ErrorMessage = "禮券類型名稱為必填")]
        [StringLength(100, ErrorMessage = "禮券類型名稱長度不能超過100個字符")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "禮券描述為必填")]
        [StringLength(500, ErrorMessage = "禮券描述長度不能超過500個字符")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "面額為必填")]
        [Range(0.01, double.MaxValue, ErrorMessage = "面額必須大於0.01")]
        public decimal ValueAmount { get; set; }

        [Required(ErrorMessage = "有效期限為必填")]
        [Range(1, 365, ErrorMessage = "有效期限必須在1-365天之間")]
        public int ValidDays { get; set; } = 30;

        [StringLength(500, ErrorMessage = "禮券圖片網址長度不能超過500個字符")]
        [Url(ErrorMessage = "禮券圖片網址格式不正確")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Whether the evoucher type is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Legacy properties for backward compatibility
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        public DateTime ValidTo { get; set; } = DateTime.Now.AddDays(30);

        [Required]
        public int PointsCost { get; set; }

        [Required]
        public int TotalAvailable { get; set; }
    }

    public class EVoucherCreateVM
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int EvoucherTypeId { get; set; }
    }

    public class EVoucherEditVM
    {
        public int EvoucherId { get; set; }
        public int UserId { get; set; }
        public int EvoucherTypeId { get; set; }
    }

    // 簽到相關 ViewModels
    public class AdminSignInCreateViewModel
    {
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "簽到活動名稱為必填")]
        [StringLength(100, ErrorMessage = "簽到活動名稱長度不能超過100個字符")]
        public string SignInName { get; set; } = string.Empty;

        [Required(ErrorMessage = "簽到活動描述為必填")]
        [StringLength(500, ErrorMessage = "簽到活動描述長度不能超過500個字符")]
        public string SignInDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "獎勵點數為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "獎勵點數必須大於0")]
        public int RewardPoints { get; set; } = 100;

        [Required(ErrorMessage = "開始日期為必填")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "結束日期為必填")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);

        /// <summary>
        /// Whether the sign-in activity is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Legacy properties for backward compatibility
        public DateTime SignTime { get; set; } = DateTime.Now;

        public int PointsGained { get; set; } = 0;

        public int ExpGained { get; set; } = 0;

        [StringLength(50)]
        public string? CouponGained { get; set; }
    }

    public class AdminSignInRulesViewModel
    {
        public SignInRuleReadModel? SignInRule { get; set; }
        public List<SignInRecordReadModel> RecentRecords { get; set; } = new();
    }

    public class SignInRecordModel
    {
        public List<UserSignInStats> Records { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;

        // Properties for individual record display (used when model represents a single record)
        /// <summary>
        /// 簽到記錄ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 簽到日期
        /// </summary>
        public DateTime SignInDate { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 獲得點數
        /// </summary>
        public int PointsEarned { get; set; }

        /// <summary>
        /// 連續天數
        /// </summary>
        public int ConsecutiveDays { get; set; }

        /// <summary>
        /// 獎勵類型
        /// </summary>
        public string? BonusType { get; set; }

        /// <summary>
        /// IP位址
        /// </summary>
        public string? IPAddress { get; set; }
    }

    // 寵物相關 ViewModels
    public class AdminPetCreateViewModel
    {
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "寵物名稱為必填")]
        [StringLength(50, ErrorMessage = "寵物名稱長度不能超過50個字符")]
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "寵物描述為必填")]
        [StringLength(500, ErrorMessage = "寵物描述長度不能超過500個字符")]
        public string PetDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "寵物類型為必填")]
        [StringLength(20, ErrorMessage = "寵物類型長度不能超過20個字符")]
        public string PetType { get; set; } = string.Empty;

        [Required(ErrorMessage = "稀有度為必填")]
        [StringLength(20, ErrorMessage = "稀有度長度不能超過20個字符")]
        public string Rarity { get; set; } = string.Empty;

        [Required(ErrorMessage = "掉落機率為必填")]
        [Range(0.01, 100, ErrorMessage = "掉落機率必須在0.01-100之間")]
        public decimal DropRate { get; set; } = 1.0m;

        [StringLength(500, ErrorMessage = "寵物圖片網址長度不能超過500個字符")]
        [Url(ErrorMessage = "寵物圖片網址格式不正確")]
        public string? PetImageUrl { get; set; }

        /// <summary>
        /// Whether the pet is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Legacy properties for backward compatibility
        [Required]
        [StringLength(10)]
        public string SkinColor { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string BackgroundColor { get; set; } = string.Empty;
    }

    public class PetColorChangeQueryModel
    {
        public List<PetSkinColorChangeLog> ColorChanges { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// 頁碼（別名屬性，為了向後相容）
        /// </summary>
        public int PageNumber
        {
            get => CurrentPage;
            set => CurrentPage = value;
        }

        /// <summary>
        /// 變更類型 (Skin/Background)
        /// </summary>
        public string? ChangeType { get; set; }

        public int? UserId { get; set; }
        public int? PetId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 變更記錄項目列表（用於顯示查詢結果）
        /// </summary>
        [Display(Name = "變更記錄")]
        public List<PetColorChangeItem> Items { get; set; } = new();

        /// <summary>
        /// 總頁數
        /// </summary>
        [Display(Name = "總頁數")]
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 寵物顏色變更記錄項目（統一的皮膚/背景變更記錄）
    /// </summary>
    public class PetColorChangeItem
    {
        /// <summary>
        /// 記錄ID
        /// </summary>
        [Display(Name = "記錄ID")]
        public int Id { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Display(Name = "用戶ID")]
        public int UserId { get; set; }

        /// <summary>
        /// 寵物ID
        /// </summary>
        [Display(Name = "寵物ID")]
        public int PetId { get; set; }

        /// <summary>
        /// 寵物名稱
        /// </summary>
        [Display(Name = "寵物名稱")]
        public string PetName { get; set; } = string.Empty;

        /// <summary>
        /// 變更類型 (Skin/Background)
        /// </summary>
        [Display(Name = "變更類型")]
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// 變更前的值
        /// </summary>
        [Display(Name = "變更前")]
        public string OldValue { get; set; } = string.Empty;

        /// <summary>
        /// 變更後的值
        /// </summary>
        [Display(Name = "變更後")]
        public string NewValue { get; set; } = string.Empty;

        /// <summary>
        /// 消耗點數
        /// </summary>
        [Display(Name = "消耗點數")]
        public int PointsCost { get; set; }

        /// <summary>
        /// 變更時間
        /// </summary>
        [Display(Name = "變更時間")]
        public DateTime ChangeTime { get; set; }
    }

    public class PetIndividualSettingsModel
    {
        public Pet? Pet { get; set; }
        public List<PetSkinColorChangeLog> SkinChanges { get; set; } = new();
        public List<PetBackgroundColorChangeLog> BackgroundChanges { get; set; } = new();

        /// <summary>
        /// 寵物列表（用於查詢結果顯示）
        /// </summary>
        [Display(Name = "寵物列表")]
        public List<Pet> Pets { get; set; } = new();
    }

    public class PetListQueryModel
    {
        public List<Pet> Pets { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// 分頁資訊
        /// </summary>
        public PagedResult<Pet>? Pagination { get; set; }
    }

    public class AdminPetRulesViewModel
    {
        public PetRuleReadModel? PetRule { get; set; }
        public List<PetSummary> PetSummaries { get; set; } = new();
    }

    // 小遊戲相關 ViewModels
    public class AdminMiniGameCreateViewModel
    {
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "遊戲名稱為必填")]
        [StringLength(100, ErrorMessage = "遊戲名稱長度不能超過100個字符")]
        public string GameName { get; set; } = string.Empty;

        [Required(ErrorMessage = "遊戲描述為必填")]
        [StringLength(500, ErrorMessage = "遊戲描述長度不能超過500個字符")]
        public string GameDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "遊戲類型為必填")]
        [StringLength(50, ErrorMessage = "遊戲類型長度不能超過50個字符")]
        public string GameType { get; set; } = string.Empty;

        [Required(ErrorMessage = "消耗點數為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "消耗點數必須大於等於0")]
        public int CostPoints { get; set; } = 0;

        [Required(ErrorMessage = "獎勵點數為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "獎勵點數必須大於等於0")]
        public int RewardPoints { get; set; } = 100;

        [Required(ErrorMessage = "最大遊玩次數為必填")]
        [Range(1, 1000, ErrorMessage = "最大遊玩次數必須在1-1000之間")]
        public int MaxPlayCount { get; set; } = 3;

        [StringLength(500, ErrorMessage = "遊戲圖片網址長度不能超過500個字符")]
        [Url(ErrorMessage = "遊戲圖片網址格式不正確")]
        public string? GameImageUrl { get; set; }

        /// <summary>
        /// Whether the game is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Legacy properties for backward compatibility
        [Required]
        public int PetID { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        [StringLength(20)]
        public string? Result { get; set; }

        public int PointsEarned { get; set; } = 0;
        public int ExpEarned { get; set; } = 0;
        public int CouponEarned { get; set; } = 0;

        [StringLength(100)]
        public string? SessionID { get; set; }
    }

    public class GameRecordModel
    {
        public List<GameSpace.Models.MiniGame> GameRecords { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;

        // Properties for individual record display (used when model represents a single record)
        /// <summary>
        /// 遊戲記錄ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 遊戲名稱
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// 遊戲結果 (Win/Lose/Draw)
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// 分數
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 獲得點數
        /// </summary>
        public int PointsEarned { get; set; }

        /// <summary>
        /// 遊戲時長（秒）
        /// </summary>
        public int Duration { get; set; }
    }

    // 系統統計模型
    public class SystemStatsModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TodaySignIns { get; set; }
        public int ThisMonthSignIns { get; set; }
    }

    // 圖表數據模型
    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    // 最近錢包交易模型
    public class RecentWalletTransactionModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public int PointsChanged { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
    }

    // 最近簽到模型
    public class RecentSignInModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string? CouponGained { get; set; }
    }

    // 最近遊戲記錄模型
    public class RecentGameRecordModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string? Result { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? GameName { get; set; }
    }

    // 系統狀態模型
    public class SystemStatusModel
    {
        public bool DatabaseConnection { get; set; }
        public string MemoryUsage { get; set; } = string.Empty;
        public DateTime? LastBackup { get; set; }
        public int ErrorCount { get; set; }
        public string CpuUsage { get; set; } = string.Empty;
        public string DiskSpace { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; } = DateTime.Now;
    }

}
