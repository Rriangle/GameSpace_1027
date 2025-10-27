using System.ComponentModel.DataAnnotations;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理員儀表板視圖模型
    /// </summary>
    public class AdminDashboardViewModel
    {
        // 基本統計
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalMiniGames { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TotalPoints { get; set; }

        // 今日統計
        public int TodaySignIns { get; set; }
        public int TodayGames { get; set; }
        public int TodayGamePlays { get; set; }

        // 本月統計
        public int ThisMonthSignIns { get; set; }
        public int ThisMonthGames { get; set; }

        // 活躍用戶統計
        public int ActiveUsersLast7Days { get; set; }

        // 寵物統計
        public int PetsWithMaxLevel { get; set; }
        public int PetsWithMaxHappiness { get; set; }

        // 錢包統計
        public int TotalPointsInCirculation { get; set; }
        public int AveragePointsPerUser { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsSpent { get; set; }

        // 優惠券統計
        public int UsedCoupons { get; set; }
        public int AvailableCoupons { get; set; }

        // 電子券統計
        public int UsedEVouchers { get; set; }
        public int AvailableEVouchers { get; set; }

        // 最近活動 - 通用列表（AdminDashboardController 使用）
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new List<RecentActivityViewModel>();
        public List<RecentActivityViewModel> RecentGames { get; set; } = new List<RecentActivityViewModel>();
        public List<RecentActivityViewModel> RecentSignIns { get; set; } = new List<RecentActivityViewModel>();

        // 圖表數據
        public List<ChartData> SignInChartData { get; set; } = new List<ChartData>();
        public List<ChartData> GameChartData { get; set; } = new List<ChartData>();
        public List<ChartData> PointsChartData { get; set; } = new List<ChartData>();

        // 用戶成長數據（用於圖表）
        public List<UserGrowthChartData> UserGrowthData { get; set; } = new List<UserGrowthChartData>();

        // 遊戲遊玩數據（用於圖表）
        public List<GamePlayChartData> GamePlayData { get; set; } = new List<GamePlayChartData>();

        // AdminController 專用屬性（使用不同的屬性名稱避免衝突）
        public List<RecentSignInModel> RecentSignInList { get; set; } = new List<RecentSignInModel>();
        public List<RecentGameRecordModel> RecentGameRecords { get; set; } = new List<RecentGameRecordModel>();
        public List<RecentWalletTransactionModel> RecentWalletTransactions { get; set; } = new List<RecentWalletTransactionModel>();
        public SystemStatsModel SystemStats { get; set; } = new SystemStatsModel();
    }

    /// <summary>
    /// 最近活動視圖模型
    /// </summary>
    public class RecentActivityViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime ActivityTime { get; set; }
        public string Description { get; set; } = string.Empty;

        // AdminDashboardController 使用的屬性
        public string Activity { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public int PointsEarned { get; set; }
    }

    /// <summary>
    /// 用戶統計視圖模型
    /// </summary>
    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveUsersToday { get; set; }
        public int ActiveUsersThisWeek { get; set; }
        public int ConfirmedUsers { get; set; }
        public int UnconfirmedUsers { get; set; }
        public int LockedUsers { get; set; }
    }

    /// <summary>
    /// 遊戲統計視圖模型
    /// </summary>
    public class GameStatisticsViewModel
    {
        public int TotalGames { get; set; }
        public int GamesToday { get; set; }
        public int GamesThisWeek { get; set; }
        public int GamesThisMonth { get; set; }
        public List<GameTypeStatisticsViewModel> GameTypeStatistics { get; set; } = new List<GameTypeStatisticsViewModel>();
        public List<TopPlayerViewModel> TopPlayers { get; set; } = new List<TopPlayerViewModel>();
    }

    /// <summary>
    /// 遊戲類型統計視圖模型
    /// </summary>
    public class GameTypeStatisticsViewModel
    {
        public string GameType { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageScore { get; set; }
        public int MaxScore { get; set; }
    }

    /// <summary>
    /// 頂級玩家視圖模型
    /// </summary>
    public class TopPlayerViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
        public int TotalScore { get; set; }
        public double AverageScore { get; set; }
        public int MaxScore { get; set; }
    }

    /// <summary>
    /// 錢包統計視圖模型
    /// </summary>
    public class WalletStatisticsViewModel
    {
        public int TotalWallets { get; set; }
        public int TotalPoints { get; set; }
        public double AveragePoints { get; set; }
        public int TodayTransactions { get; set; }
        public int TodayPointsAdded { get; set; }
        public int TodayPointsDeducted { get; set; }
        public int ThisWeekTransactions { get; set; }
        public int ThisWeekPointsAdded { get; set; }
        public int ThisWeekPointsDeducted { get; set; }
        public int ThisMonthTransactions { get; set; }
        public int ThisMonthPointsAdded { get; set; }
        public int ThisMonthPointsDeducted { get; set; }
        public List<TransactionTypeStatisticsViewModel> TransactionTypeStatistics { get; set; } = new List<TransactionTypeStatisticsViewModel>();
    }

    /// <summary>
    /// 交易類型統計視圖模型
    /// </summary>
    public class TransactionTypeStatisticsViewModel
    {
        public string ChangeType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalPoints { get; set; }
        public double AveragePoints { get; set; }
    }

    /// <summary>
    /// 系統健康狀態視圖模型
    /// </summary>
    public class SystemHealthViewModel
    {
        public bool DatabaseConnectionHealthy { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int TotalGames { get; set; }
        public int GamesToday { get; set; }
        public int ErrorCount { get; set; }
        public string SystemStatus { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
    }

    /// <summary>
    /// 查詢模型基類
    /// </summary>
    public class BaseQueryModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// 用戶查詢模型
    /// </summary>
    public class UserQueryModel : BaseQueryModel
    {
        public string Status { get; set; } = string.Empty;
        public bool? EmailConfirmed { get; set; }
        public bool? IsLocked { get; set; }
    }

    /// <summary>
    /// 管理員查詢模型
    /// </summary>
    public class ManagerQueryModel : BaseQueryModel
    {
        public string Status { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? IsLocked { get; set; }
    }

    /// <summary>
    /// 簽到查詢模型
    /// </summary>
    public class SignInQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public DateTime? SignDate { get; set; }
        public int? MinConsecutiveDays { get; set; }
        public int? MaxConsecutiveDays { get; set; }
    }

    /// <summary>
    /// 寵物查詢模型
    /// </summary>
    public class PetQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public string SkinColor { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用戶權限查詢模型
    /// </summary>
    public class UserRightQueryModel
    {
        public int? UserId { get; set; }
        public string? RightName { get; set; }
        public string? RightType { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? ExpiresFrom { get; set; }
        public DateTime? ExpiresTo { get; set; }
        public string SortBy { get; set; } = string.Empty;
        public bool SortDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 管理員寵物索引 ViewModel
    /// </summary>
    public class AdminPetIndexViewModel
    {
        public PagedResult<GameSpace.Models.Pet> Pets { get; set; } = new();
    }

    /// <summary>
    /// 管理員小遊戲索引 ViewModel
    /// </summary>
    public class AdminMiniGameIndexViewModel
    {
        public PagedResult<GameSpace.Models.MiniGame> MiniGames { get; set; } = new();
    }

    /// <summary>
    /// 管理員簽到索引 ViewModel
    /// </summary>
    public class AdminSignInIndexViewModel
    {
        public PagedResult<GameSpace.Areas.MiniGame.Models.UserSignInStats> SignIns { get; set; } = new();
    }

    /// <summary>
    /// 管理員用戶索引 ViewModel
    /// </summary>
    public class AdminUserIndexViewModel
    {
        public PagedResult<GameSpace.Models.User> Users { get; set; } = new();
    }

    /// <summary>
    /// 用戶成長圖表數據
    /// </summary>
    public class UserGrowthChartData
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// 遊戲遊玩圖表數據
    /// </summary>
    public class GamePlayChartData
    {
        public string GameType { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}

