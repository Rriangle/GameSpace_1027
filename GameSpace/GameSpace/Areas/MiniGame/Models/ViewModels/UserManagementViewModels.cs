using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 用戶詳細資料 ViewModel
    /// </summary>
    public class UserDetailViewModel
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
        /// 用戶帳號
        /// </summary>
        public string UserAccount { get; set; } = string.Empty;

        /// <summary>
        /// 用戶Email
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// 電話號碼
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 年齡
        /// </summary>
        public int? Age => Birthday.HasValue ? (int)((DateTime.Now - Birthday.Value).TotalDays / 365.25) : null;

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Email是否已驗證
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// 帳號鎖定結束時間
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// 帳號是否被鎖定
        /// </summary>
        public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.Now;

        /// <summary>
        /// 註冊時間
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>
        /// 最後登入時間
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 登入失敗次數
        /// </summary>
        public int AccessFailedCount { get; set; }

        /// <summary>
        /// 當前點數
        /// </summary>
        public int CurrentPoints { get; set; }

        /// <summary>
        /// 累計點數收入
        /// </summary>
        public int TotalPointsEarned { get; set; }

        /// <summary>
        /// 累計點數支出
        /// </summary>
        public int TotalPointsSpent { get; set; }

        /// <summary>
        /// 未使用優惠券數
        /// </summary>
        public int UnusedCoupons { get; set; }

        /// <summary>
        /// 未使用電子票券數
        /// </summary>
        public int UnusedEVouchers { get; set; }

        /// <summary>
        /// 寵物資訊
        /// </summary>
        public UserPetInfo? PetInfo { get; set; }

        /// <summary>
        /// 遊戲統計
        /// </summary>
        public UserGameStatistics? GameStats { get; set; }

        /// <summary>
        /// 簽到統計
        /// </summary>
        public UserSignInInfo? SignInInfo { get; set; }

        /// <summary>
        /// 用戶標籤
        /// </summary>
        public List<string> UserTags { get; set; } = new();

        /// <summary>
        /// 備註
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 用戶寵物資訊
    /// </summary>
    public class UserPetInfo
    {
        /// <summary>
        /// 寵物ID
        /// </summary>
        public int PetId { get; set; }

        /// <summary>
        /// 寵物名稱
        /// </summary>
        public string PetName { get; set; } = string.Empty;

        /// <summary>
        /// 寵物等級
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 寵物經驗值
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// 健康度
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// 飽食度
        /// </summary>
        public int Hunger { get; set; }

        /// <summary>
        /// 心情
        /// </summary>
        public int Mood { get; set; }

        /// <summary>
        /// 乾淨度
        /// </summary>
        public int Cleanliness { get; set; }

        /// <summary>
        /// 體力
        /// </summary>
        public int Stamina { get; set; }
    }

    /// <summary>
    /// 用戶簽到資訊
    /// </summary>
    public class UserSignInInfo
    {
        /// <summary>
        /// 總簽到次數
        /// </summary>
        public int TotalSignIns { get; set; }

        /// <summary>
        /// 當前連續簽到天數
        /// </summary>
        public int CurrentConsecutiveDays { get; set; }

        /// <summary>
        /// 最長連續簽到天數
        /// </summary>
        public int MaxConsecutiveDays { get; set; }

        /// <summary>
        /// 最後簽到時間
        /// </summary>
        public DateTime? LastSignInTime { get; set; }

        /// <summary>
        /// 今日是否已簽到
        /// </summary>
        public bool HasSignedInToday { get; set; }

        /// <summary>
        /// 累計簽到獲得點數
        /// </summary>
        public int TotalPointsFromSignIn { get; set; }
    }

    /// <summary>
    /// 用戶遊戲統計
    /// </summary>
    public class UserGameStatistics
    {
        /// <summary>
        /// 總遊戲次數
        /// </summary>
        public int TotalGames { get; set; }

        /// <summary>
        /// 勝利次數
        /// </summary>
        public int WinCount { get; set; }

        /// <summary>
        /// 失敗次數
        /// </summary>
        public int LoseCount { get; set; }

        /// <summary>
        /// 中止次數
        /// </summary>
        public int AbortCount { get; set; }

        /// <summary>
        /// 勝率
        /// </summary>
        public double WinRate => TotalGames > 0 ? (double)WinCount / TotalGames * 100 : 0;

        /// <summary>
        /// 累計獲得點數
        /// </summary>
        public int TotalPointsEarned { get; set; }

        /// <summary>
        /// 累計獲得寵物經驗
        /// </summary>
        public int TotalPetExpEarned { get; set; }

        /// <summary>
        /// 累計獲得優惠券數
        /// </summary>
        public int TotalCouponsEarned { get; set; }

        /// <summary>
        /// 最後遊戲時間
        /// </summary>
        public DateTime? LastGameTime { get; set; }

        /// <summary>
        /// 今日已遊戲次數
        /// </summary>
        public int TodayGameCount { get; set; }

        /// <summary>
        /// 今日剩餘可玩次數
        /// </summary>
        public int RemainingGamesToday { get; set; }

        /// <summary>
        /// 連續遊戲天數
        /// </summary>
        public int ConsecutiveGameDays { get; set; }
    }

    /// <summary>
    /// 用戶查詢模型
    /// </summary>
    public class UserQueryModel
    {
        /// <summary>
        /// 搜尋關鍵字（用戶名稱、帳號、Email）
        /// </summary>
        [StringLength(200)]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 是否已鎖定
        /// </summary>
        public bool? IsLockedOut { get; set; }

        /// <summary>
        /// Email是否已驗證
        /// </summary>
        public bool? EmailConfirmed { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        [StringLength(10)]
        public string? Gender { get; set; }

        /// <summary>
        /// 註冊日期起始
        /// </summary>
        public DateTime? RegisteredFrom { get; set; }

        /// <summary>
        /// 註冊日期結束
        /// </summary>
        public DateTime? RegisteredTo { get; set; }

        /// <summary>
        /// 最後登入日期起始
        /// </summary>
        public DateTime? LastLoginFrom { get; set; }

        /// <summary>
        /// 最後登入日期結束
        /// </summary>
        public DateTime? LastLoginTo { get; set; }

        /// <summary>
        /// 最小點數
        /// </summary>
        public int? MinPoints { get; set; }

        /// <summary>
        /// 最大點數
        /// </summary>
        public int? MaxPoints { get; set; }

        /// <summary>
        /// 排序欄位
        /// </summary>
        [StringLength(50)]
        public string SortBy { get; set; } = "RegisteredAt";

        /// <summary>
        /// 是否降序排列
        /// </summary>
        public bool Descending { get; set; } = true;

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
    /// 用戶統計摘要
    /// </summary>
    public class UserStatisticsSummary
    {
        /// <summary>
        /// 總用戶數
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// 啟用用戶數
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// 停用用戶數
        /// </summary>
        public int InactiveUsers { get; set; }

        /// <summary>
        /// 已鎖定用戶數
        /// </summary>
        public int LockedUsers { get; set; }

        /// <summary>
        /// 今日新增用戶
        /// </summary>
        public int TodayNewUsers { get; set; }

        /// <summary>
        /// 本週新增用戶
        /// </summary>
        public int ThisWeekNewUsers { get; set; }

        /// <summary>
        /// 本月新增用戶
        /// </summary>
        public int ThisMonthNewUsers { get; set; }

        /// <summary>
        /// 今日活躍用戶（今日登入）
        /// </summary>
        public int TodayActiveUsers { get; set; }

        /// <summary>
        /// 本週活躍用戶
        /// </summary>
        public int ThisWeekActiveUsers { get; set; }

        /// <summary>
        /// 本月活躍用戶
        /// </summary>
        public int ThisMonthActiveUsers { get; set; }

        /// <summary>
        /// 平均用戶點數
        /// </summary>
        public double AverageUserPoints { get; set; }

        /// <summary>
        /// 總點數流通量
        /// </summary>
        public long TotalPointsInCirculation { get; set; }

        /// <summary>
        /// 性別分佈
        /// </summary>
        public Dictionary<string, int> GenderDistribution { get; set; } = new();

        /// <summary>
        /// 年齡分佈
        /// </summary>
        public Dictionary<string, int> AgeDistribution { get; set; } = new();

        /// <summary>
        /// 每日註冊趨勢
        /// </summary>
        public Dictionary<string, int> DailyRegistrationTrend { get; set; } = new();

        /// <summary>
        /// 每日登入趨勢
        /// </summary>
        public Dictionary<string, int> DailyLoginTrend { get; set; } = new();
    }

    /// <summary>
    /// 用戶更新請求
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        /// <summary>
        /// 用戶名稱
        /// </summary>
        [StringLength(100, ErrorMessage = "用戶名稱長度不可超過 100 字元")]
        public string? UserName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        [StringLength(200)]
        public string? UserEmail { get; set; }

        /// <summary>
        /// 電話號碼
        /// </summary>
        [Phone(ErrorMessage = "電話號碼格式不正確")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        [StringLength(10)]
        public string? Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [StringLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 用戶鎖定/解鎖請求
    /// </summary>
    public class UserLockRequest
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        /// <summary>
        /// 是否鎖定（true=鎖定, false=解鎖）
        /// </summary>
        [Required(ErrorMessage = "鎖定狀態為必填")]
        public bool IsLock { get; set; }

        /// <summary>
        /// 鎖定天數（僅在鎖定時需要）
        /// </summary>
        [Range(1, 3650, ErrorMessage = "鎖定天數必須在 1-3650 之間")]
        public int? LockDays { get; set; }

        /// <summary>
        /// 原因
        /// </summary>
        [Required(ErrorMessage = "原因為必填")]
        [StringLength(500, ErrorMessage = "原因長度不可超過 500 字元")]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// 調整用戶點數請求
    /// </summary>
    public class AdjustUserPointsRequest
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        /// <summary>
        /// 點數變化量（正數為增加，負數為減少）
        /// </summary>
        [Required(ErrorMessage = "點數變化量為必填")]
        [Range(-1000000, 1000000, ErrorMessage = "點數變化量必須在 -1000000 到 1000000 之間")]
        public int PointsChange { get; set; }

        /// <summary>
        /// 原因描述
        /// </summary>
        [Required(ErrorMessage = "原因描述為必填")]
        [StringLength(500, ErrorMessage = "原因描述長度不可超過 500 字元")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
