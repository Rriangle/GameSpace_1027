using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 小遊戲首頁視圖模型
    /// </summary>
    public class MiniGameIndexViewModel
    {
        /// <summary>
        /// 小遊戲記錄列表
        /// </summary>
        public List<GameSpace.Models.MiniGame> MiniGames { get; set; } = new();
        
        /// <summary>
        /// 寵物資料
        /// </summary>
        public GameSpace.Models.Pet Pet { get; set; } = new();
        
        /// <summary>
        /// 用戶錢包
        /// </summary>
        public UserWallet Wallet { get; set; } = new();
        
        /// <summary>
        /// 是否可以遊玩
        /// </summary>
        public bool CanPlay { get; set; }
        
        /// <summary>
        /// 今日遊戲次數
        /// </summary>
        public int TodayGames { get; set; }
        
        /// <summary>
        /// 剩餘遊戲次數
        /// </summary>
        public int RemainingGames { get; set; }
        
        /// <summary>
        /// 發送者ID
        /// </summary>
        public int SenderID { get; set; }
        
        /// <summary>
        /// 遊戲規則
        /// </summary>
        public GameRuleReadModel GameRules { get; set; } = new();
        
        /// <summary>
        /// 遊戲統計
        /// </summary>
        public MiniGameStatistics Statistics { get; set; } = new();
        
        /// <summary>
        /// 可用難度選項
        /// </summary>
        public List<GameDifficultyOption> AvailableDifficulties { get; set; } = new();
    }

    /// <summary>
    /// 小遊戲詳情視圖模型
    /// </summary>
    public class MiniGameDetailsViewModel
    {
        /// <summary>
        /// 小遊戲記錄
        /// </summary>
        public GameSpace.Models.MiniGame MiniGame { get; set; } = new();
        
        /// <summary>
        /// 寵物資料
        /// </summary>
        public GameSpace.Models.Pet Pet { get; set; } = new();
        
        /// <summary>
        /// 用戶錢包
        /// </summary>
        public UserWallet Wallet { get; set; } = new();
        
        /// <summary>
        /// 發送者ID
        /// </summary>
        public int SenderID { get; set; }
        
        /// <summary>
        /// 遊戲詳情
        /// </summary>
        public GameDetails GameDetails { get; set; } = new();
        
        /// <summary>
        /// 獎勵詳情
        /// </summary>
        public GameRewardDetails RewardDetails { get; set; } = new();
    }

    /// <summary>
    /// 小遊戲遊玩視圖模型
    /// </summary>
    public class MiniGamePlayViewModel
    {
        /// <summary>
        /// 寵物資料
        /// </summary>
        public GameSpace.Models.Pet Pet { get; set; } = new();
        
        /// <summary>
        /// 用戶錢包
        /// </summary>
        public UserWallet Wallet { get; set; } = new();
        
        /// <summary>
        /// 發送者ID
        /// </summary>
        public int SenderID { get; set; }
        
        /// <summary>
        /// 難度等級
        /// </summary>
        [Required(ErrorMessage = "請選擇難度")]
        [Range(1, 5, ErrorMessage = "難度必須在1-5之間")]
        public int Difficulty { get; set; } = 1;
        
        /// <summary>
        /// 遊戲會話ID
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// 最大遊戲時間（秒）
        /// </summary>
        public int MaxPlayTime { get; set; }
        
        /// <summary>
        /// 剩餘時間（秒）
        /// </summary>
        public int RemainingTime { get; set; }
        
        /// <summary>
        /// 是否活躍
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// 遊戲配置
        /// </summary>
        public GameConfiguration GameConfig { get; set; } = new();
        
        /// <summary>
        /// 寵物狀態
        /// </summary>
        public PetGameStatus PetStatus { get; set; } = new();
    }

    /// <summary>
    /// 小遊戲結果視圖模型
    /// </summary>
    public class MiniGameResultViewModel
    {
        /// <summary>
        /// 小遊戲記錄
        /// </summary>
        public GameSpace.Models.MiniGame MiniGame { get; set; } = new();
        
        /// <summary>
        /// 寵物資料
        /// </summary>
        public GameSpace.Models.Pet Pet { get; set; } = new();
        
        /// <summary>
        /// 用戶錢包
        /// </summary>
        public UserWallet Wallet { get; set; } = new();
        
        /// <summary>
        /// 發送者ID
        /// </summary>
        public int SenderID { get; set; }
        
        /// <summary>
        /// 是否勝利
        /// </summary>
        public bool IsWin { get; set; }
        
        /// <summary>
        /// 獲得點數
        /// </summary>
        public int PointsGained { get; set; }
        
        /// <summary>
        /// 獲得經驗
        /// </summary>
        public int ExpGained { get; set; }
        
        /// <summary>
        /// 遊戲結果詳情
        /// </summary>
        public GameResultDetails ResultDetails { get; set; } = new();
        
        /// <summary>
        /// 獎勵詳情
        /// </summary>
        public GameRewardDetails RewardDetails { get; set; } = new();
        
        /// <summary>
        /// 下次遊戲時間
        /// </summary>
        public DateTime? NextGameTime { get; set; }
    }

    /// <summary>
    /// 小遊戲統計視圖模型
    /// </summary>
    public class MiniGameStatsViewModel
    {
        /// <summary>
        /// 小遊戲記錄列表
        /// </summary>
        public List<GameSpace.Models.MiniGame> MiniGames { get; set; } = new();
        
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
        public double WinRate { get; set; }
        
        /// <summary>
        /// 總獲得點數
        /// </summary>
        public int TotalPoints { get; set; }
        
        /// <summary>
        /// 總獲得經驗
        /// </summary>
        public int TotalExp { get; set; }
        
        /// <summary>
        /// 發送者ID
        /// </summary>
        public int SenderID { get; set; }
        
        /// <summary>
        /// 統計詳情
        /// </summary>
        public MiniGameStatistics Statistics { get; set; } = new();
        
        /// <summary>
        /// 勝率顯示文字
        /// </summary>
        public string WinRateDisplay => $"{WinRate:P1}";
    }

    /// <summary>
    /// 管理員小遊戲管理視圖模型
    /// </summary>
    public class AdminMiniGameManagementViewModel
    {
        /// <summary>
        /// 遊戲記錄列表
        /// </summary>
        public List<GameSpace.Models.MiniGame> GameRecords { get; set; } = new();
        
        /// <summary>
        /// 用戶列表
        /// </summary>
        public List<User> Users { get; set; } = new();
        
        /// <summary>
        /// 查詢條件
        /// </summary>
        public MiniGameRecordQueryModel Query { get; set; } = new();
        
        /// <summary>
        /// 統計數據
        /// </summary>
        public GameStatisticsReadModel Statistics { get; set; } = new();
        
        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 頁碼
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// 分頁結果
        /// </summary>
        public PagedResult<GameSpace.Models.MiniGame> PagedResults { get; set; } = new();
    }

    /// <summary>
    /// 小遊戲規則管理視圖模型
    /// </summary>
    public class MiniGameRuleManagementViewModel
    {
        /// <summary>
        /// 當前規則
        /// </summary>
        public GameRuleReadModel CurrentRule { get; set; } = new();
        
        /// <summary>
        /// 更新模型
        /// </summary>
        public MiniGameRulesUpdateModel UpdateModel { get; set; } = new();
        
        /// <summary>
        /// 規則歷史
        /// </summary>
        public List<GameRule> RuleHistory { get; set; } = new();
        
        /// <summary>
        /// 規則統計
        /// </summary>
        public GameRuleStatistics RuleStatistics { get; set; } = new();
    }

    /// <summary>
    /// 小遊戲會話視圖模型
    /// </summary>
    public class MiniGameSessionViewModel
    {
        /// <summary>
        /// 難度等級
        /// </summary>
        [Required(ErrorMessage = "請選擇難度")]
        [Range(1, 5, ErrorMessage = "難度必須在1-5之間")]
        public int Difficulty { get; set; } = 1;
        
        /// <summary>
        /// 會話ID
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// 最大遊戲時間（秒）
        /// </summary>
        public int MaxPlayTime { get; set; }
        
        /// <summary>
        /// 剩餘時間（秒）
        /// </summary>
        public int RemainingTime { get; set; }
        
        /// <summary>
        /// 是否活躍
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// 遊戲狀態
        /// </summary>
        public string GameStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// 遊戲配置
        /// </summary>
        public GameConfiguration GameConfig { get; set; } = new();
    }

    /// <summary>
    /// 小遊戲排行榜視圖模型
    /// </summary>
    public class MiniGameLeaderboardViewModel
    {
        /// <summary>
        /// 頂級玩家列表
        /// </summary>
        public List<MiniGameLeaderboardEntry> TopPlayers { get; set; } = new();
        
        /// <summary>
        /// 最近遊戲記錄
        /// </summary>
        public List<MiniGameLeaderboardEntry> RecentGames { get; set; } = new();
        
        /// <summary>
        /// 用戶排名
        /// </summary>
        public MiniGameLeaderboardEntry? UserRank { get; set; }

        /// <summary>
        /// 總玩家數
        /// </summary>
        public int TotalPlayers { get; set; }

        /// <summary>
        /// 排行榜類型
        /// </summary>
        public string LeaderboardType { get; set; } = "weekly";
        
        /// <summary>
        /// 排行榜統計
        /// </summary>
        public LeaderboardStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// 小遊戲排行榜條目
    /// </summary>
    public class MiniGameLeaderboardEntry
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
        /// 總遊戲次數
        /// </summary>
        public int TotalGames { get; set; }
        
        /// <summary>
        /// 勝利次數
        /// </summary>
        public int WinCount { get; set; }
        
        /// <summary>
        /// 勝率
        /// </summary>
        public double WinRate { get; set; }
        
        /// <summary>
        /// 總獲得點數
        /// </summary>
        public int TotalPoints { get; set; }
        
        /// <summary>
        /// 總獲得經驗
        /// </summary>
        public int TotalExperience { get; set; }
        
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }
        
        /// <summary>
        /// 最後遊玩時間
        /// </summary>
        public DateTime LastPlayed { get; set; }
        
        /// <summary>
        /// 勝率顯示文字
        /// </summary>
        public string WinRateDisplay => $"{WinRate:P1}";
        
        /// <summary>
        /// 排名顯示文字
        /// </summary>
        public string RankDisplay => Rank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"#{Rank}"
        };
    }

    /// <summary>
    /// 小遊戲獎勵視圖模型
    /// </summary>
    public class MiniGameRewardViewModel
    {
        /// <summary>
        /// 遊戲ID
        /// </summary>
        public int GameId { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// 遊戲結果
        /// </summary>
        public string GameResult { get; set; } = string.Empty;
        
        /// <summary>
        /// 獲得點數
        /// </summary>
        public int PointsEarned { get; set; }
        
        /// <summary>
        /// 獲得經驗
        /// </summary>
        public int ExperienceEarned { get; set; }
        
        /// <summary>
        /// 額外獎勵
        /// </summary>
        public List<string> BonusRewards { get; set; } = new();
        
        /// <summary>
        /// 獎勵發放時間
        /// </summary>
        public DateTime AwardedDate { get; set; }
        
        /// <summary>
        /// 獎勵詳情
        /// </summary>
        public GameRewardDetails RewardDetails { get; set; } = new();
    }

    /// <summary>
    /// 小遊戲統計模型
    /// </summary>
    public class MiniGameStatistics
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
        public double WinRate { get; set; }
        
        /// <summary>
        /// 平均分數
        /// </summary>
        public double AverageScore { get; set; }
        
        /// <summary>
        /// 最高分數
        /// </summary>
        public int HighestScore { get; set; }
        
        /// <summary>
        /// 總獲得點數
        /// </summary>
        public int TotalPointsEarned { get; set; }
        
        /// <summary>
        /// 總獲得經驗
        /// </summary>
        public int TotalExperienceEarned { get; set; }
        
        /// <summary>
        /// 平均遊戲時間（秒）
        /// </summary>
        public double AverageGameDuration { get; set; }
        
        /// <summary>
        /// 勝率顯示文字
        /// </summary>
        public string WinRateDisplay => $"{WinRate:P1}";
        
        /// <summary>
        /// 平均分數顯示文字
        /// </summary>
        public string AverageScoreDisplay => $"{AverageScore:F1}";
    }

    /// <summary>
    /// 遊戲難度選項
    /// </summary>
    public class GameDifficultyOption
    {
        /// <summary>
        /// 難度等級
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// 難度名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 難度描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 怪物數量
        /// </summary>
        public int MonsterCount { get; set; }
        
        /// <summary>
        /// 怪物速度
        /// </summary>
        public double MonsterSpeed { get; set; }
        
        /// <summary>
        /// 獎勵倍率
        /// </summary>
        public double RewardMultiplier { get; set; }
        
        /// <summary>
        /// 是否解鎖
        /// </summary>
        public bool IsUnlocked { get; set; }
        
        /// <summary>
        /// 解鎖條件
        /// </summary>
        public string UnlockCondition { get; set; } = string.Empty;
    }

    /// <summary>
    /// 遊戲配置
    /// </summary>
    public class GameConfiguration
    {
        /// <summary>
        /// 怪物數量
        /// </summary>
        public int MonsterCount { get; set; }
        
        /// <summary>
        /// 怪物速度
        /// </summary>
        public double MonsterSpeed { get; set; }
        
        /// <summary>
        /// 遊戲時間限制（秒）
        /// </summary>
        public int TimeLimit { get; set; }
        
        /// <summary>
        /// 勝利條件
        /// </summary>
        public string WinCondition { get; set; } = string.Empty;
        
        /// <summary>
        /// 失敗條件
        /// </summary>
        public string LoseCondition { get; set; } = string.Empty;
        
        /// <summary>
        /// 特殊規則
        /// </summary>
        public List<string> SpecialRules { get; set; } = new();
    }

    /// <summary>
    /// 寵物遊戲狀態
    /// </summary>
    public class PetGameStatus
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
        /// 當前生命值
        /// </summary>
        public int CurrentHealth { get; set; }
        
        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHealth { get; set; }
        
        /// <summary>
        /// 當前能量
        /// </summary>
        public int CurrentEnergy { get; set; }
        
        /// <summary>
        /// 最大能量
        /// </summary>
        public int MaxEnergy { get; set; }
        
        /// <summary>
        /// 攻擊力
        /// </summary>
        public int AttackPower { get; set; }
        
        /// <summary>
        /// 防禦力
        /// </summary>
        public int DefensePower { get; set; }
        
        /// <summary>
        /// 速度
        /// </summary>
        public int Speed { get; set; }
        
        /// <summary>
        /// 生命值百分比
        /// </summary>
        public double HealthPercentage => MaxHealth > 0 ? (double)CurrentHealth / MaxHealth * 100 : 0;
        
        /// <summary>
        /// 能量百分比
        /// </summary>
        public double EnergyPercentage => MaxEnergy > 0 ? (double)CurrentEnergy / MaxEnergy * 100 : 0;
    }

    /// <summary>
    /// 遊戲詳情
    /// </summary>
    public class GameDetails
    {
        /// <summary>
        /// 遊戲ID
        /// </summary>
        public int GameId { get; set; }
        
        /// <summary>
        /// 遊戲開始時間
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 遊戲結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// 遊戲持續時間（秒）
        /// </summary>
        public int Duration => EndTime.HasValue ? (int)(EndTime.Value - StartTime).TotalSeconds : 0;
        
        /// <summary>
        /// 遊戲結果
        /// </summary>
        public string Result { get; set; } = string.Empty;
        
        /// <summary>
        /// 難度等級
        /// </summary>
        public int Difficulty { get; set; }
        
        /// <summary>
        /// 怪物數量
        /// </summary>
        public int MonsterCount { get; set; }
        
        /// <summary>
        /// 怪物速度
        /// </summary>
        public double MonsterSpeed { get; set; }
        
        /// <summary>
        /// 是否中止
        /// </summary>
        public bool IsAborted { get; set; }
        
        /// <summary>
        /// 遊戲狀態顯示文字
        /// </summary>
        public string StatusDisplay => Result switch
        {
            "Win" => "勝利",
            "Lose" => "失敗",
            "Abort" => "中止",
            _ => "進行中"
        };
    }

    /// <summary>
    /// 遊戲結果詳情
    /// </summary>
    public class GameResultDetails
    {
        /// <summary>
        /// 是否勝利
        /// </summary>
        public bool IsWin { get; set; }
        
        /// <summary>
        /// 是否失敗
        /// </summary>
        public bool IsLose { get; set; }
        
        /// <summary>
        /// 是否中止
        /// </summary>
        public bool IsAbort { get; set; }
        
        /// <summary>
        /// 遊戲分數
        /// </summary>
        public int Score { get; set; }
        
        /// <summary>
        /// 擊敗怪物數量
        /// </summary>
        public int MonstersDefeated { get; set; }
        
        /// <summary>
        /// 遊戲時間（秒）
        /// </summary>
        public int GameTime { get; set; }
        
        /// <summary>
        /// 結果描述
        /// </summary>
        public string ResultDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// 結果顏色
        /// </summary>
        public string ResultColor => IsWin ? "success" : IsLose ? "danger" : "warning";
    }

    /// <summary>
    /// 遊戲獎勵詳情
    /// </summary>
    public class GameRewardDetails
    {
        /// <summary>
        /// 基礎點數獎勵
        /// </summary>
        public int BasePoints { get; set; }
        
        /// <summary>
        /// 基礎經驗獎勵
        /// </summary>
        public int BaseExperience { get; set; }
        
        /// <summary>
        /// 難度倍率
        /// </summary>
        public double DifficultyMultiplier { get; set; }
        
        /// <summary>
        /// 連擊倍率
        /// </summary>
        public double ComboMultiplier { get; set; }
        
        /// <summary>
        /// 最終點數獎勵
        /// </summary>
        public int FinalPoints { get; set; }
        
        /// <summary>
        /// 最終經驗獎勵
        /// </summary>
        public int FinalExperience { get; set; }
        
        /// <summary>
        /// 額外獎勵
        /// </summary>
        public List<string> ExtraRewards { get; set; } = new();
        
        /// <summary>
        /// 獎勵描述
        /// </summary>
        public string RewardDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// 排行榜統計
    /// </summary>
    public class LeaderboardStatistics
    {
        /// <summary>
        /// 總玩家數
        /// </summary>
        public int TotalPlayers { get; set; }
        
        /// <summary>
        /// 活躍玩家數
        /// </summary>
        public int ActivePlayers { get; set; }
        
        /// <summary>
        /// 平均勝率
        /// </summary>
        public double AverageWinRate { get; set; }
        
        /// <summary>
        /// 平均分數
        /// </summary>
        public double AverageScore { get; set; }
        
        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 遊戲規則統計
    /// </summary>
    public class GameRuleStatistics
    {
        /// <summary>
        /// 規則使用次數
        /// </summary>
        public int RuleUsageCount { get; set; }
        
        /// <summary>
        /// 規則生效時間
        /// </summary>
        public DateTime RuleEffectiveTime { get; set; }
        
        /// <summary>
        /// 規則修改次數
        /// </summary>
        public int RuleModificationCount { get; set; }
        
        /// <summary>
        /// 最後修改時間
        /// </summary>
        public DateTime LastModifiedTime { get; set; }
        
        /// <summary>
        /// 規則狀態
        /// </summary>
        public string RuleStatus { get; set; } = string.Empty;
    }
}

