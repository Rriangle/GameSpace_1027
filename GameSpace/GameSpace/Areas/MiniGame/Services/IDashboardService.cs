using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IDashboardService
    {
        // 總覽統計
        Task<DashboardOverview> GetDashboardOverviewAsync();
        Task<UserStatistics> GetUserStatisticsAsync();
        Task<GameStatistics> GetGameStatisticsAsync();
        Task<RevenueStatistics> GetRevenueStatisticsAsync();

        // 趨勢數據
        Task<Dictionary<string, int>> GetUserGrowthTrendAsync(int days = 30);
        Task<Dictionary<string, int>> GetGamePlayTrendAsync(int days = 30);
        Task<Dictionary<string, decimal>> GetRevenueTrendAsync(int days = 30);

        // 排行榜
        Task<IEnumerable<TopUser>> GetTopUsersAsync(int count = 10);
        Task<IEnumerable<TopGame>> GetTopGamesAsync(int count = 10);

        // 最近活動
        Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(int count = 20);

        // 警報
        Task<IEnumerable<SystemAlert>> GetActiveAlertsAsync();
    }

    public class DashboardOverview
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalGames { get; set; }
        public int TodayGames { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int OnlineUsers { get; set; }
    }

    public class UserStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double AverageSessionTime { get; set; }
    }

    public class GameStatistics
    {
        public int TotalGamesPlayed { get; set; }
        public int GamesToday { get; set; }
        public int GamesThisWeek { get; set; }
        public int GamesThisMonth { get; set; }
        public double AverageGameDuration { get; set; }
        public string MostPopularGame { get; set; } = string.Empty;
    }

    public class RevenueStatistics
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal AverageRevenuePerUser { get; set; }
    }

    public class TopUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Points { get; set; }
        public int Level { get; set; }
        public int GamesPlayed { get; set; }
    }

    public class TopGame
    {
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public int PlayCount { get; set; }
        public double AverageScore { get; set; }
    }

    public class RecentActivity
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class SystemAlert
    {
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

