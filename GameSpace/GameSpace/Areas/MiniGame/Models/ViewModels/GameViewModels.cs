using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// ?閮?瑼Ｚ?璅∪?
    /// </summary>
    public class GameRecordViewModel
    {
        /// <summary>
        /// 閮?ID
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        /// 雿輻?D
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 雿輻??蝔?
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// ?ID
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// ??迂
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// ?蝯? (Win, Lose, Abort)
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// ?脣?暺
        /// </summary>
        public int PointsEarned { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        public DateTime PlayedAt { get; set; }

        /// <summary>
        /// ????? (蝘?
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// ??
        /// </summary>
        public int? Score { get; set; }

        /// <summary>
        /// ?酉
        /// </summary>
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// ??亥岷璅∪?
    /// </summary>
    public class GameQueryModel
    {
        /// <summary>
        /// 雿輻?D (?詨‵)
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 雿輻??蝔?(璅∠???)
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// ?ID (?詨‵)
        /// </summary>
        public int? GameId { get; set; }

        /// <summary>
        /// ?蝯? (Win, Lose, Abort)
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// ???交?
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 蝯??交?
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// ?嗅??Ⅳ
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 瘥?蝑
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// ??甈? (PlayedAt, PointsEarned, Score)
        /// </summary>
        public string SortBy { get; set; } = "PlayedAt";

        /// <summary>
        /// ???孵? (asc, desc)
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// ?閬?瑼Ｚ?璅∪?
    /// </summary>
    public class GameRulesViewModel
    {
        /// <summary>
        /// ?ID
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// ??迂
        /// </summary>
        [Required(ErrorMessage = "Game name is required")]
        [StringLength(100, ErrorMessage = "Game name cannot exceed 100 characters")]
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// ??膩
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// 瘥?甈⊥?
        /// </summary>
        [Required(ErrorMessage = "Daily play limit is required")]
        [Range(1, 100, ErrorMessage = "Daily play limit must be between 1 and 100")]
        public int DailyPlayLimit { get; set; } = 3;

        /// <summary>
        /// ??暺
        /// </summary>
        [Required(ErrorMessage = "Win points is required")]
        [Range(0, 10000, ErrorMessage = "Win points must be between 0 and 10000")]
        public int WinPoints { get; set; }

        /// <summary>
        /// 憭望??暺
        /// </summary>
        [Range(0, 10000, ErrorMessage = "Lose points must be between 0 and 10000")]
        public int LosePoints { get; set; }

        /// <summary>
        /// 銝剜??暺
        /// </summary>
        [Range(0, 10000, ErrorMessage = "Abort points must be between 0 and 10000")]
        public int AbortPoints { get; set; }

        /// <summary>
        /// ??臬?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ??內URL
        /// </summary>
        [StringLength(500, ErrorMessage = "Icon URL cannot exceed 500 characters")]
        public string? IconUrl { get; set; }

        /// <summary>
        /// ?撠?RL
        /// </summary>
        [StringLength(500, ErrorMessage = "Cover image URL cannot exceed 500 characters")]
        public string? CoverImageUrl { get; set; }

        /// <summary>
        /// ??剝??脫???(蝘?
        /// </summary>
        [Range(0, 3600, ErrorMessage = "Min duration must be between 0 and 3600 seconds")]
        public int? MinDurationSeconds { get; set; }

        /// <summary>
        /// ??琿??脫???(蝘?
        /// </summary>
        [Range(0, 3600, ErrorMessage = "Max duration must be between 0 and 3600 seconds")]
        public int? MaxDurationSeconds { get; set; }

        /// <summary>
        /// 撱箇???
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// ?湔??
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 撱箇??D
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// ?湔?D
        /// </summary>
        public int? UpdatedBy { get; set; }
    }

    /// <summary>
    /// ?閬? ViewModel嚗???∟身摰??萸??塚?
    /// </summary>
    public class GameRuleViewModel
    {
        /// <summary>
        /// ??迂
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// ??膩
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 瘥?甈⊥?
        /// </summary>
        public int DailyPlayLimit { get; set; } = 3;

        /// <summary>
        /// ??臬?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ?閮剖??”
        /// </summary>
        public List<GameLevelSettingViewModel> LevelSettings { get; set; } = new List<GameLevelSettingViewModel>();

        /// <summary>
        /// 蝮賡??脫活?貊絞閮?
        /// </summary>
        public int TotalGamesPlayed { get; set; }

        /// <summary>
        /// 隞?甈⊥蝯梯?
        /// </summary>
        public int TodayGamesPlayed { get; set; }

        /// <summary>
        /// ?敺?唳???
        /// </summary>
        public DateTime? LastUpdated { get; set; }
    }

    /// <summary>
    /// ??閮剖? ViewModel
    /// </summary>
    public class GameLevelSettingViewModel
    {
        /// <summary>
        /// ?蝑?
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// ?芰?賊?
        /// </summary>
        public int MonsterCount { get; set; }

        /// <summary>
        /// 蝘餃??漲?
        /// </summary>
        public decimal SpeedMultiplier { get; set; }

        /// <summary>
        /// ??暺
        /// </summary>
        public int WinPointsReward { get; set; }

        /// <summary>
        /// ??蝬???
        /// </summary>
        public int WinExpReward { get; set; }

        /// <summary>
        /// ???芣??豢??
        /// </summary>
        public int WinCouponReward { get; set; }

        /// <summary>
        /// 憭望??暺
        /// </summary>
        public int LosePointsReward { get; set; }

        /// <summary>
        /// 憭望??蝬???
        /// </summary>
        public int LoseExpReward { get; set; }

        /// <summary>
        /// 銝剜?暺
        /// </summary>
        public int AbortPointsReward { get; set; }

        /// <summary>
        /// 銝剜?蝬???
        /// </summary>
        public int AbortExpReward { get; set; }

        /// <summary>
        /// ??膩
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// ?蝝?閰Ｘ芋??
    /// </summary>
    public class GameRecordQueryModel
    {
        /// <summary>
        /// 雿輻?D嚗憛恬?
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 雿輻??蝔梧?璅∠???嚗?
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 撖萇ID嚗憛恬?
        /// </summary>
        public int? PetId { get; set; }

        /// <summary>
        /// ?蝑?嚗憛恬?
        /// </summary>
        public int? Level { get; set; }

        /// <summary>
        /// ?蝯?嚗in/Lose/Abort嚗?
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// ???交?
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 蝯??交?
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// ?嗅??Ⅳ
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 瘥?蝑
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// ??甈?
        /// </summary>
        public string? SortBy { get; set; } = "StartTime";

        /// <summary>
        /// ???孵?嚗sc/desc嚗?
        /// </summary>
        public string? SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// ?蝝????ViewModel
    /// </summary>
    public class GameRecordItemViewModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int PetId { get; set; }
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public string Result { get; set; } = string.Empty;
        public int ExpGained { get; set; }
        public int PointsGained { get; set; }
        public string CouponGained { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Aborted { get; set; }
        public int? Duration { get; set; }
    }

    /// <summary>
    /// ?蝝??銵?ViewModel
    /// </summary>
    public class GameRecordsListViewModel
    {
        public List<GameRecordItemViewModel> Records { get; set; } = new List<GameRecordItemViewModel>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public GameRecordQueryModel Query { get; set; } = new GameRecordQueryModel();
    }

    /// <summary>
    /// ?蝝?底蝝?ViewModel
    /// </summary>
    public class GameRecordDetailViewModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int PetId { get; set; }
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public string Result { get; set; } = string.Empty;
        public int ExpGained { get; set; }
        public DateTime ExpGainedTime { get; set; }
        public int PointsGained { get; set; }
        public DateTime PointsGainedTime { get; set; }
        public string CouponGained { get; set; } = string.Empty;
        public DateTime CouponGainedTime { get; set; }
        public int HungerDelta { get; set; }
        public int MoodDelta { get; set; }
        public int StaminaDelta { get; set; }
        public int CleanlinessDelta { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Aborted { get; set; }
        public int? Duration { get; set; }
    }

    /// <summary>
    /// ?蝯梯? ViewModel
    /// </summary>
    public class GameStatisticsViewModel
    {
        public int TotalGames { get; set; }
        public int WinGames { get; set; }
        public int LoseGames { get; set; }
        public int AbortGames { get; set; }
        public decimal WinRate { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalExpAwarded { get; set; }
        public decimal AveragePointsPerGame { get; set; }
        public decimal AverageExpPerGame { get; set; }
        public List<GameLevelStatViewModel> LevelStatistics { get; set; } = new List<GameLevelStatViewModel>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// ??蝯梯? ViewModel
    /// </summary>
    public class GameLevelStatViewModel
    {
        public int Level { get; set; }
        public int TotalPlays { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int AbortCount { get; set; }
        public decimal WinRate { get; set; }
    }

    // ==================== Input Models for Mutation Operations ====================

    /// <summary>
    /// 遊戲規則輸入模型（用於更新整體遊戲設定）
    /// </summary>
    public class GameRulesInputModel
    {
        /// <summary>
        /// 遊戲名稱
        /// </summary>
        [Required(ErrorMessage = "遊戲名稱為必填")]
        [StringLength(100, ErrorMessage = "遊戲名稱不可超過100個字元")]
        public string GameName { get; set; } = "冒險小遊戲";

        /// <summary>
        /// 遊戲描述
        /// </summary>
        [StringLength(500, ErrorMessage = "遊戲描述不可超過500個字元")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 圖示URL
        /// </summary>
        [StringLength(500, ErrorMessage = "圖示URL不可超過500個字元")]
        [Url(ErrorMessage = "圖示URL格式不正確")]
        public string? IconUrl { get; set; }

        /// <summary>
        /// 封面圖片URL
        /// </summary>
        [StringLength(500, ErrorMessage = "封面圖片URL不可超過500個字元")]
        [Url(ErrorMessage = "封面圖片URL格式不正確")]
        public string? CoverImageUrl { get; set; }

        /// <summary>
        /// 最小遊戲時長（秒）
        /// </summary>
        [Range(0, 3600, ErrorMessage = "最小時長必須在0-3600秒之間")]
        public int? MinDurationSeconds { get; set; }

        /// <summary>
        /// 最大遊戲時長（秒）
        /// </summary>
        [Range(0, 3600, ErrorMessage = "最大時長必須在0-3600秒之間")]
        public int? MaxDurationSeconds { get; set; }
    }

    /// <summary>
    /// 關卡設定輸入模型（用於更新單一關卡）
    /// </summary>
    public class LevelSettingsInputModel
    {
        /// <summary>
        /// 關卡等級
        /// </summary>
        [Required(ErrorMessage = "關卡等級為必填")]
        [Range(1, 10, ErrorMessage = "關卡等級必須在1-10之間")]
        public int Level { get; set; }

        /// <summary>
        /// 怪物數量
        /// </summary>
        [Required(ErrorMessage = "怪物數量為必填")]
        [Range(1, 50, ErrorMessage = "怪物數量必須在1-50之間")]
        public int MonsterCount { get; set; }

        /// <summary>
        /// 移動速度倍率
        /// </summary>
        [Required(ErrorMessage = "速度倍率為必填")]
        [Range(0.5, 5.0, ErrorMessage = "速度倍率必須在0.5-5.0之間")]
        public decimal SpeedMultiplier { get; set; }

        /// <summary>
        /// 獲勝點數獎勵
        /// </summary>
        [Required(ErrorMessage = "獲勝點數為必填")]
        [Range(0, 10000, ErrorMessage = "獲勝點數必須在0-10000之間")]
        public int WinPointsReward { get; set; }

        /// <summary>
        /// 獲勝經驗值獎勵
        /// </summary>
        [Required(ErrorMessage = "獲勝經驗值為必填")]
        [Range(0, 10000, ErrorMessage = "獲勝經驗值必須在0-10000之間")]
        public int WinExpReward { get; set; }

        /// <summary>
        /// 獲勝優惠券獎勵數量
        /// </summary>
        [Range(0, 10, ErrorMessage = "優惠券數量必須在0-10之間")]
        public int WinCouponReward { get; set; } = 0;

        /// <summary>
        /// 失敗點數獎勵
        /// </summary>
        [Range(0, 10000, ErrorMessage = "失敗點數必須在0-10000之間")]
        public int LosePointsReward { get; set; } = 0;

        /// <summary>
        /// 失敗經驗值獎勵
        /// </summary>
        [Range(0, 10000, ErrorMessage = "失敗經驗值必須在0-10000之間")]
        public int LoseExpReward { get; set; } = 0;

        /// <summary>
        /// 中止點數獎勵
        /// </summary>
        [Range(0, 10000, ErrorMessage = "中止點數必須在0-10000之間")]
        public int AbortPointsReward { get; set; } = 0;

        /// <summary>
        /// 中止經驗值獎勵
        /// </summary>
        [Range(0, 10000, ErrorMessage = "中止經驗值必須在0-10000之間")]
        public int AbortExpReward { get; set; } = 0;

        /// <summary>
        /// 關卡描述
        /// </summary>
        [StringLength(200, ErrorMessage = "關卡描述不可超過200個字元")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 每日次數限制輸入模型
    /// </summary>
    public class DailyLimitInputModel
    {
        /// <summary>
        /// 每日最大遊戲次數
        /// </summary>
        [Required(ErrorMessage = "每日次數為必填")]
        [Range(1, 100, ErrorMessage = "每日次數必須在1-100之間")]
        public int MaxPlaysPerDay { get; set; } = 3;

        /// <summary>
        /// 設定說明
        /// </summary>
        [StringLength(200, ErrorMessage = "說明不可超過200個字元")]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 遊戲紀錄調整輸入模型（用於手動補發獎勵或修正錯誤）
    /// </summary>
    public class GameRecordAdjustmentInputModel
    {
        /// <summary>
        /// 調整點數（正數為補發，負數為扣除）
        /// </summary>
        [Range(-10000, 10000, ErrorMessage = "調整點數必須在-10000到10000之間")]
        public int? AdjustPoints { get; set; }

        /// <summary>
        /// 調整經驗值（正數為補發，負數為扣除）
        /// </summary>
        [Range(-10000, 10000, ErrorMessage = "調整經驗值必須在-10000到10000之間")]
        public int? AdjustExp { get; set; }

        /// <summary>
        /// 是否補發優惠券
        /// </summary>
        public bool IssueCoupon { get; set; } = false;

        /// <summary>
        /// 優惠券類型代碼（當 IssueCoupon 為 true 時必填）
        /// </summary>
        [StringLength(50, ErrorMessage = "優惠券類型代碼不可超過50個字元")]
        public string? CouponTypeCode { get; set; }

        /// <summary>
        /// 調整原因
        /// </summary>
        [Required(ErrorMessage = "調整原因為必填")]
        [StringLength(500, ErrorMessage = "調整原因不可超過500個字元")]
        public string Reason { get; set; } = string.Empty;
    }
}
