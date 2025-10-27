using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface ISignInService
    {
        // 簽到基本操作
        Task<bool> SignInAsync(int userId);
        Task<bool> CanSignInTodayAsync(int userId);
        Task<DateTime?> GetLastSignInDateAsync(int userId);
        Task<int> GetConsecutiveDaysAsync(int userId);

        // 簽到記錄查詢
        Task<IEnumerable<UserSignInStats>> GetSignInHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<UserSignInStats>> GetAllSignInsAsync(int pageNumber = 1, int pageSize = 50);
        Task<IEnumerable<UserSignInStats>> GetSignInsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<UserSignInStats?> GetSignInDetailAsync(int logId);

        // 簽到獎勵
        Task<SignInReward> CalculateSignInRewardAsync(int userId);
        Task<bool> GrantSignInRewardAsync(int userId, SignInReward reward);
        Task<IEnumerable<SignInRewardRule>> GetSignInRewardRulesAsync();

        // 簽到統計
        Task<SignInStatistics> GetUserSignInStatisticsAsync(int userId);
        Task<SignInStatistics> GetGlobalSignInStatisticsAsync();
        Task<Dictionary<string, int>> GetSignInTrendDataAsync(int days = 30);
        Task<IEnumerable<UserSignInRanking>> GetSignInLeaderboardAsync(int count = 10);

        // 簽到規則管理
        Task<IEnumerable<SignInRule>> GetAllSignInRulesAsync();
        Task<SignInRule?> GetSignInRuleByIdAsync(int ruleId);
        Task<bool> CreateSignInRuleAsync(SignInRule rule);
        Task<bool> UpdateSignInRuleAsync(SignInRule rule);
        Task<bool> DeleteSignInRuleAsync(int ruleId);
        Task<bool> ToggleSignInRuleStatusAsync(int ruleId);
    }

    // 輔助類別
    public class SignInReward
    {
        public int Points { get; set; }
        public int Experience { get; set; }
        public string? CouponCode { get; set; }
        public int ConsecutiveDayBonus { get; set; }
    }

    public class SignInStatistics
    {
        public int TotalSignIns { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalExpEarned { get; set; }
        public int TotalCouponsEarned { get; set; }
        public DateTime? LastSignInDate { get; set; }
    }

    public class UserSignInRanking
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int SignInCount { get; set; }
        public int ConsecutiveDays { get; set; }
        public int TotalPoints { get; set; }
    }

    public class SignInRewardRule
    {
        public int DayNumber { get; set; }
        public int Points { get; set; }
        public int Experience { get; set; }
        public bool HasCoupon { get; set; }
    }
}


