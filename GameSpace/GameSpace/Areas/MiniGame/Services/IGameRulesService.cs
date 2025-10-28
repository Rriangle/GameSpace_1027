using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IGameRulesService
    {
        // 每日遊戲限制
        Task<int> GetDailyGameLimitAsync();
        Task<bool> UpdateDailyGameLimitAsync(int limit);
        Task<int> GetUserGameCountTodayAsync(int userId);
        Task<bool> CanUserPlayGameAsync(int userId);

        // 遊戲獎勵設定
        Task<GameRewardSettings?> GetGameRewardSettingsAsync();
        Task<bool> UpdateGameRewardSettingsAsync(GameRewardSettings settings);
        Task<GameReward> CalculateGameRewardAsync(int gameId, int score, bool isWin);

        // 遊戲難度設定
        Task<IEnumerable<GameDifficultySettings>> GetAllDifficultySettingsAsync();
        Task<GameDifficultySettings?> GetDifficultySettingByLevelAsync(int level);
        Task<bool> UpdateDifficultySettingAsync(GameDifficultySettings setting);

        // 遊戲規則管理
        Task<IEnumerable<GameRule>> GetAllGameRulesAsync();
        Task<GameRule?> GetGameRuleByIdAsync(int ruleId);
        Task<bool> CreateGameRuleAsync(GameRule rule);
        Task<bool> UpdateGameRuleAsync(GameRule rule);
        Task<bool> DeleteGameRuleAsync(int ruleId);
        Task<bool> ToggleGameRuleAsync(int ruleId);

        // 特殊事件規則
        Task<IEnumerable<GameEventRule>> GetActiveGameEventsAsync();
        Task<bool> CreateGameEventAsync(GameEventRule eventRule);
        Task<bool> UpdateGameEventAsync(GameEventRule eventRule);
        Task<bool> EndGameEventAsync(int eventId);
    }

    public class GameRewardSettings
    {
        public int Id { get; set; }
        public decimal PointsRewardRate { get; set; }
        public decimal ExpRewardRate { get; set; }
        public decimal CouponRewardRate { get; set; }
        public bool PointsRewardEnabled { get; set; }
        public bool ExpRewardEnabled { get; set; }
        public bool CouponRewardEnabled { get; set; }
        public int MinPointsReward { get; set; }
        public int MaxPointsReward { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class GameReward
    {
        public int Points { get; set; }
        public int Experience { get; set; }
        public string? CouponCode { get; set; }
        public int BonusMultiplier { get; set; }
    }

    public class GameDifficultySettings
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public int RewardMultiplier { get; set; }
        public bool IsActive { get; set; }
    }

    public class GameRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public string RuleValue { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class GameEventRule
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public decimal RewardMultiplier { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}

