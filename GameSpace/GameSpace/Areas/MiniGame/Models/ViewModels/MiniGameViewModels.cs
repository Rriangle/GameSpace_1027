﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 遊戲規則讀取模型
    /// </summary>
    public class GameRuleReadModel
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GameType { get; set; } = string.Empty;
        public int DailyPlayLimit { get; set; }
        public int PointsPerWin { get; set; }
        public int ExpPerWin { get; set; }
        public int? CouponTypeId { get; set; }
        public string? CouponTypeName { get; set; }
        public decimal CouponDropRate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 遊戲規則更新模型
    /// </summary>
    public class GameRuleUpdateModel
    {
        [Required(ErrorMessage = "規則ID為必填")]
        public int RuleId { get; set; }

        [Required(ErrorMessage = "規則名稱為必填")]
        [StringLength(100, ErrorMessage = "規則名稱長度不可超過 100 字元")]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "遊戲類型為必填")]
        [StringLength(50, ErrorMessage = "遊戲類型長度不可超過 50 字元")]
        public string GameType { get; set; } = string.Empty;

        [Required(ErrorMessage = "每日遊玩次數限制為必填")]
        [Range(1, 100, ErrorMessage = "每日遊玩次數限制必須在 1-100 之間")]
        public int DailyPlayLimit { get; set; } = 3;

        [Required(ErrorMessage = "勝利點數獎勵為必填")]
        [Range(0, 10000, ErrorMessage = "勝利點數獎勵必須在 0-10000 之間")]
        public int PointsPerWin { get; set; }

        [Range(0, 10000, ErrorMessage = "勝利經驗值獎勵必須在 0-10000 之間")]
        public int ExpPerWin { get; set; }

        public int? CouponTypeId { get; set; }

        [Range(0, 1, ErrorMessage = "優惠券掉落率必須在 0-1 之間")]
        public decimal CouponDropRate { get; set; } = 0.1m;

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 小遊戲規則更新模型
    /// </summary>
    public class MiniGameRulesUpdateModel
    {
        [Required(ErrorMessage = "每日遊玩次數限制為必填")]
        [Range(1, 100, ErrorMessage = "每日遊玩次數限制必須在 1-100 之間")]
        public int DailyGameLimit { get; set; } = 3;

        [Required(ErrorMessage = "勝利點數獎勵為必填")]
        [Range(0, 10000, ErrorMessage = "勝利點數獎勵必須在 0-10000 之間")]
        public int WinPointsReward { get; set; } = 100;

        [Range(0, 10000, ErrorMessage = "失敗點數獎勵必須在 0-10000 之間")]
        public int LosePointsReward { get; set; } = 10;

        [Range(0, 10000, ErrorMessage = "勝利經驗值獎勵必須在 0-10000 之間")]
        public int WinExpReward { get; set; } = 50;

        [Range(0, 10000, ErrorMessage = "失敗經驗值獎勵必須在 0-10000 之間")]
        public int LoseExpReward { get; set; } = 5;

        [Range(0, 1, ErrorMessage = "優惠券掉落率必須在 0-1 之間")]
        public decimal CouponDropRate { get; set; } = 0.1m;

        public int? WinCouponTypeId { get; set; }

        [Range(-100, 0, ErrorMessage = "飽食度消耗必須在 -100 到 0 之間")]
        public int HungerDelta { get; set; } = -10;

        [Range(-100, 100, ErrorMessage = "心情變化必須在 -100 到 100 之間")]
        public int MoodDelta { get; set; } = 5;

        [Range(-100, 0, ErrorMessage = "體力消耗必須在 -100 到 0 之間")]
        public int StaminaDelta { get; set; } = -15;

        [Range(-100, 0, ErrorMessage = "乾淨度減少必須在 -100 到 0 之間")]
        public int CleanlinessDelta { get; set; } = -5;

        [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 小遊戲記錄查詢模型
    /// </summary>
    public class MiniGameRecordQueryModel
    {
        public int? UserId { get; set; }
        public int? PetId { get; set; }
        public string? Result { get; set; } // "Win", "Lose", "Abort"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public string? SortBy { get; set; } = "StartTime";
        public bool Descending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 遊戲統計讀取模型
    /// </summary>
    public class GameStatisticsReadModel
    {
        public int TotalGames { get; set; }
        public int TotalWins { get; set; }
        public int TotalLoses { get; set; }
        public int TotalAborts { get; set; }
        public decimal WinRate { get; set; }
        public int TotalPointsDistributed { get; set; }
        public int TotalExpDistributed { get; set; }
        public int TotalCouponsDistributed { get; set; }
        public int AverageLevel { get; set; }
        public int MaxLevel { get; set; }
        public int AverageMonsterCount { get; set; }
        public decimal AverageSpeedMultiplier { get; set; }
        public DateTime? LastGameTime { get; set; }
        public Dictionary<string, int> ResultDistribution { get; set; } = new();
        public Dictionary<string, int> DailyGameTrend { get; set; } = new();
    }

    /// <summary>
    /// 遊戲規則模型
    /// </summary>
    public class GameRule
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DailyGameLimit { get; set; } = 3;
        public int WinPointsReward { get; set; } = 100;
        public int LosePointsReward { get; set; } = 10;
        public int WinExpReward { get; set; } = 50;
        public int LoseExpReward { get; set; } = 5;
        public decimal CouponDropRate { get; set; } = 0.1m;
        public int? WinCouponTypeId { get; set; }
        public string? WinCouponTypeName { get; set; }
        public int HungerDelta { get; set; } = -10;
        public int MoodDelta { get; set; } = 5;
        public int StaminaDelta { get; set; } = -15;
        public int CleanlinessDelta { get; set; } = -5;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 遊戲摘要模型
    /// </summary>
    public class GameSummary
    {
        public int GameId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public string Result { get; set; } = string.Empty; // "Win", "Lose", "Abort"
        public int ExpGained { get; set; }
        public int PointsGained { get; set; }
        public string? CouponGained { get; set; }
        public int HungerDelta { get; set; }
        public int MoodDelta { get; set; }
        public int StaminaDelta { get; set; }
        public int CleanlinessDelta { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }

        // Additional properties for compatibility
        public int TotalGames { get; set; }
        public bool Aborted { get; set; }
    }
}

