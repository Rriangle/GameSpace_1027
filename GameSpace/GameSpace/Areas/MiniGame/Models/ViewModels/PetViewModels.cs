﻿﻿using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物顏色選項視圖模型
    /// </summary>
    public class PetColorOptions
    {
        /// <summary>
        /// 顏色選項ID
        /// </summary>
        public int ColorOptionId { get; set; }

        /// <summary>
        /// 顏色名稱
        /// </summary>
        [Required(ErrorMessage = "顏色名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "顏色名稱長度不能超過50個字元")]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 顏色代碼（十六進位）
        /// </summary>
        [Required(ErrorMessage = "顏色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "顏色代碼必須為7個字元（包含#）")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確")]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 更換所需點數
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "更換所需點數必須大於等於0")]
        public int PointsCost { get; set; } = 2000;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 顏色類型 (Skin/Background)
        /// </summary>
        [StringLength(20)]
        public string ColorType { get; set; } = "Skin";

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物互動獎勵規則
    /// </summary>
    [Table("PetInteractionBonusRules")]
    public class PetInteractionBonusRules
    {
        /// <summary>
        /// 規則ID
        /// </summary>
        [Key]
        public int RuleId { get; set; }

        /// <summary>
        /// 互動類型 (Feed/Play/Clean)
        /// </summary>
        [Required(ErrorMessage = "互動類型為必填欄位")]
        [StringLength(20, ErrorMessage = "互動類型長度不能超過20個字元")]
        public string InteractionType { get; set; } = string.Empty;

        /// <summary>
        /// 最小獎勵值
        /// </summary>
        [Range(0, 100, ErrorMessage = "最小獎勵值必須在0-100之間")]
        public int MinBonus { get; set; } = 5;

        /// <summary>
        /// 最大獎勵值
        /// </summary>
        [Range(0, 100, ErrorMessage = "最大獎勵值必須在0-100之間")]
        public int MaxBonus { get; set; } = 15;

        /// <summary>
        /// 所需點數
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "所需點數必須大於等於0")]
        public int PointsCost { get; set; } = 10;

        /// <summary>
        /// 冷卻時間（分鐘）
        /// </summary>
        [Range(0, 1440, ErrorMessage = "冷卻時間必須在0-1440分鐘之間")]
        public int CooldownMinutes { get; set; } = 60;

        /// <summary>
        /// 經驗值獎勵
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "經驗值獎勵必須大於等於0")]
        public int ExpReward { get; set; } = 5;

        /// <summary>
        /// 飢餓度加成
        /// </summary>
        [Range(0, 100, ErrorMessage = "飢餓度加成必須在0-100之間")]
        public int HungerBonus { get; set; } = 0;

        /// <summary>
        /// 快樂度加成
        /// </summary>
        [Range(0, 100, ErrorMessage = "快樂度加成必須在0-100之間")]
        public int HappinessBonus { get; set; } = 0;

        /// <summary>
        /// 精力加成
        /// </summary>
        [Range(0, 100, ErrorMessage = "精力加成必須在0-100之間")]
        public int EnergyBonus { get; set; } = 0;

        /// <summary>
        /// 清潔度加成
        /// </summary>
        [Range(0, 100, ErrorMessage = "清潔度加成必須在0-100之間")]
        public int CleanlinessBonus { get; set; } = 0;

        /// <summary>
        /// 健康度加成
        /// </summary>
        [Range(0, 100, ErrorMessage = "健康度加成必須在0-100之間")]
        public int HealthBonus { get; set; } = 0;

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物規則讀取模型
    /// </summary>
    public class PetRuleReadModel
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LevelUpExpRequired { get; set; }
        public decimal ExpMultiplier { get; set; }
        public int MaxLevel { get; set; }
        public int ColorChangePointCost { get; set; }
        public int BackgroundChangePointCost { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 規則名稱（別名屬性，為了向後相容）
        /// </summary>
        public string Name
        {
            get => RuleName;
            set => RuleName = value;
        }

        /// <summary>
        /// 最大寵物數量（每個用戶可擁有的寵物上限）
        /// </summary>
        public int MaxPets { get; set; } = 1;

        /// <summary>
        /// 餵食成本（每次餵食所需點數）
        /// </summary>
        public int FeedingCost { get; set; } = 10;

        /// <summary>
        /// 餵食獎勵（每次餵食獲得的經驗值或其他獎勵）
        /// </summary>
        public int FeedingReward { get; set; } = 5;
    }

    /// <summary>
    /// 寵物狀態視圖模型（用於互動返回結果）
    /// </summary>
    public class PetStatsViewModel
    {
        public int PetId { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Cleanliness { get; set; }
        public int Health { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
    }

    /// <summary>
    /// 寵物摘要模型
    /// </summary>
    public class PetSummary
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExp { get; set; }
        public string CurrentSkinColor { get; set; } = string.Empty;
        public string CurrentBackground { get; set; } = string.Empty;
        public int Health { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Cleanliness { get; set; }
        public int Loyalty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastInteractionTime { get; set; }
        public int TotalColorChanges { get; set; }
        public int TotalBackgroundChanges { get; set; }

        // Additional properties for compatibility
        public int TotalPets { get; set; }
        public decimal AverageLevel { get; set; }
    }

    /// <summary>
    /// 寵物膚色變更記錄
    /// </summary>
    public class PetSkinColorChangeLog
    {
        public int LogId { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string OldColor { get; set; } = string.Empty;
        public string NewColor { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 寵物背景變更記錄
    /// </summary>
    public class PetBackgroundColorChangeLog
    {
        public int LogId { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string OldBackground { get; set; } = string.Empty;
        public string NewBackground { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 寵物背景選項
    /// </summary>
    public class PetBackgroundOption
    {
        public int OptionId { get; set; }
        public string BackgroundName { get; set; } = string.Empty;
        public string BackgroundCode { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int PointCost { get; set; }
        public int UnlockLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }

        // Additional properties for compatibility
        public int BackgroundId { get; set; }
        public int RequiredPoints { get; set; }
        public bool IsUnlocked { get; set; }
    }

    /// <summary>
    /// 寵物顏色歷史查詢 ViewModel
    /// </summary>
    public class PetColorChangeHistoryViewModel
    {
        public int? PetId { get; set; }
        public string? ChangeType { get; set; } // "Skin" or "Background"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public List<PetSkinColorChangeLog> SkinChanges { get; set; } = new();
        public List<PetBackgroundColorChangeLog> BackgroundChanges { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

    /// <summary>
    /// 寵物詳細資料 ViewModel
    /// </summary>
    public class PetDetailsViewModel
    {
        public PetSummary Pet { get; set; } = new();
        public List<PetSkinColorChangeLog> RecentColorChanges { get; set; } = new();
        public List<PetBackgroundColorChangeLog> RecentBackgroundChanges { get; set; } = new();
        public PetRuleReadModel CurrentRules { get; set; } = new();
        public List<PetBackgroundOption> AvailableBackgrounds { get; set; } = new();
    }

    /// <summary>
    /// 寵物更新模型
    /// </summary>
    public class PetUpdateModel
    {
        [Required(ErrorMessage = "寵物ID為必填")]
        public int PetId { get; set; }

        [StringLength(50, ErrorMessage = "寵物名稱長度不可超過 50 字元")]
        public string? PetName { get; set; }

        [Range(1, 100, ErrorMessage = "等級必須在 1-100 之間")]
        public int? Level { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "經驗值必須大於等於 0")]
        public int? Experience { get; set; }

        [Range(0, 100, ErrorMessage = "飽食度必須在 0-100 之間")]
        public int? Hunger { get; set; }

        [Range(0, 100, ErrorMessage = "心情必須在 0-100 之間")]
        public int? Mood { get; set; }

        [Range(0, 100, ErrorMessage = "體力必須在 0-100 之間")]
        public int? Stamina { get; set; }

        [Range(0, 100, ErrorMessage = "乾淨度必須在 0-100 之間")]
        public int? Cleanliness { get; set; }

        [Range(0, 100, ErrorMessage = "健康度必須在 0-100 之間")]
        public int? Health { get; set; }

        [StringLength(10, ErrorMessage = "膚色代碼長度不可超過 10 字元")]
        public string? SkinColor { get; set; }

        [StringLength(20, ErrorMessage = "背景顏色代碼長度不可超過 20 字元")]
        public string? BackgroundColor { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物規則更新模型
    /// </summary>
    public class PetRuleUpdateModel
    {
        [Required(ErrorMessage = "規則ID為必填")]
        public int RuleId { get; set; }

        [Required(ErrorMessage = "規則名稱為必填")]
        [StringLength(100, ErrorMessage = "規則名稱長度不可超過 100 字元")]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "升級所需經驗值為必填")]
        [Range(1, 100000, ErrorMessage = "升級所需經驗值必須在 1-100000 之間")]
        public int LevelUpExpRequired { get; set; }

        [Range(0.1, 10.0, ErrorMessage = "經驗值倍率必須在 0.1-10.0 之間")]
        public decimal ExpMultiplier { get; set; } = 1.0m;

        [Range(1, 100, ErrorMessage = "最大等級必須在 1-100 之間")]
        public int MaxLevel { get; set; } = 100;

        [Range(0, 100000, ErrorMessage = "膚色變更點數成本必須在 0-100000 之間")]
        public int ColorChangePointCost { get; set; } = 2000;

        [Range(0, 100000, ErrorMessage = "背景變更點數成本必須在 0-100000 之間")]
        public int BackgroundChangePointCost { get; set; } = 3000;

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物升級規則創建 ViewModel
    /// </summary>
    public class PetLevelUpRuleCreateViewModel
    {
        [Required(ErrorMessage = "等級為必填")]
        [Range(1, 100, ErrorMessage = "等級必須在 1-100 之間")]
        public int Level { get; set; }

        [Required(ErrorMessage = "所需經驗值為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "所需經驗值必須大於等於 0")]
        public int RequiredExp { get; set; }

        /// <summary>
        /// 所需經驗值（別名，用於向後相容）
        /// </summary>
        public int ExperienceRequired
        {
            get => RequiredExp;
            set => RequiredExp = value;
        }

        [Range(0, 100000, ErrorMessage = "點數獎勵必須在 0-100000 之間")]
        public int PointsReward { get; set; } = 0;

        /// <summary>
        /// 經驗值獎勵（升級後獲得的額外經驗值）
        /// </summary>
        [Range(0, 10000, ErrorMessage = "經驗值獎勵必須在 0-10000 之間")]
        public int ExpReward { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int HealthBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int HungerBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int MoodBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int StaminaBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int CleanlinessBonus { get; set; } = 0;

        [StringLength(200, ErrorMessage = "描述長度不可超過 200 字元")]
        public string? Description { get; set; }

        public bool IsSpecialLevel { get; set; } = false;

        [StringLength(100, ErrorMessage = "特殊獎勵描述長度不可超過 100 字元")]
        public string? SpecialReward { get; set; }
    }

    /// <summary>
    /// 寵物升級規則編輯 ViewModel
    /// </summary>
    public class PetLevelUpRuleEditViewModel
    {
        [Required(ErrorMessage = "規則ID為必填")]
        public int Id { get; set; }

        [Required(ErrorMessage = "等級為必填")]
        [Range(1, 100, ErrorMessage = "等級必須在 1-100 之間")]
        public int Level { get; set; }

        [Required(ErrorMessage = "所需經驗值為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "所需經驗值必須大於等於 0")]
        public int RequiredExp { get; set; }

        /// <summary>
        /// 所需經驗值（別名，用於向後相容）
        /// </summary>
        public int ExperienceRequired
        {
            get => RequiredExp;
            set => RequiredExp = value;
        }

        [Range(0, 100000, ErrorMessage = "點數獎勵必須在 0-100000 之間")]
        public int PointsReward { get; set; } = 0;

        /// <summary>
        /// 經驗值獎勵（升級後獲得的額外經驗值）
        /// </summary>
        [Range(0, 10000, ErrorMessage = "經驗值獎勵必須在 0-10000 之間")]
        public int ExpReward { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int HealthBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int HungerBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int MoodBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int StaminaBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int CleanlinessBonus { get; set; } = 0;

        [StringLength(200, ErrorMessage = "描述長度不可超過 200 字元")]
        public string? Description { get; set; }

        public bool IsSpecialLevel { get; set; } = false;

        [StringLength(100, ErrorMessage = "特殊獎勵描述長度不可超過 100 字元")]
        public string? SpecialReward { get; set; }

        /// <summary>
        /// 備註（別名，用於向後相容）
        /// </summary>
        public string? Remarks
        {
            get => SpecialReward;
            set => SpecialReward = value;
        }

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物背景變更設定 ViewModel
    /// </summary>
    public class PetBackgroundChangeSettingsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "背景顏色為必填")]
        [StringLength(20, ErrorMessage = "背景顏色長度不可超過 20 字元")]
        public string BackgroundColor { get; set; } = string.Empty;

        [Required(ErrorMessage = "所需點數為必填")]
        [Range(0, 100000, ErrorMessage = "所需點數必須在 0-100000 之間")]
        public int PointsRequired { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(200, ErrorMessage = "描述長度不可超過 200 字元")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物系統規則輸入模型
    /// </summary>
    public class PetSystemRulesInputModel
    {
        [Display(Name = "最大飢餓度")]
        [Required(ErrorMessage = "最大飢餓度為必填欄位")]
        [Range(0, 100, ErrorMessage = "最大飢餓度必須在 0-100 之間")]
        public int MaxHunger { get; set; } = 100;

        [Display(Name = "最大快樂度")]
        [Required(ErrorMessage = "最大快樂度為必填欄位")]
        [Range(0, 100, ErrorMessage = "最大快樂度必須在 0-100 之間")]
        public int MaxHappiness { get; set; } = 100;

        [Display(Name = "最大健康度")]
        [Required(ErrorMessage = "最大健康度為必填欄位")]
        [Range(0, 100, ErrorMessage = "最大健康度必須在 0-100 之間")]
        public int MaxHealth { get; set; } = 100;

        [Display(Name = "飢餓度衰減率")]
        [Required(ErrorMessage = "飢餓度衰減率為必填欄位")]
        [Range(1, 10, ErrorMessage = "飢餓度衰減率必須在 1-10 之間")]
        public int DecayRateHunger { get; set; } = 1;

        [Display(Name = "快樂度衰減率")]
        [Required(ErrorMessage = "快樂度衰減率為必填欄位")]
        [Range(1, 10, ErrorMessage = "快樂度衰減率必須在 1-10 之間")]
        public int DecayRateHappiness { get; set; } = 1;

        [Display(Name = "升級基礎經驗值")]
        [Required(ErrorMessage = "升級基礎經驗值為必填欄位")]
        [Range(1, 100000, ErrorMessage = "升級基礎經驗值必須在 1-100000 之間")]
        public int LevelUpExpBase { get; set; } = 100;

        [Display(Name = "升級經驗值公式")]
        [StringLength(100, ErrorMessage = "升級經驗值公式長度不能超過 100 個字元")]
        public string? LevelUpFormula { get; set; } = "Level 1-10: 40×level+60; 11-100: 0.8×level²+380; ≥101: 285.69×1.06^level";

        [Display(Name = "餵食獎勵值 (飢餓)")]
        [Required(ErrorMessage = "餵食獎勵值為必填欄位")]
        [Range(1, 50, ErrorMessage = "餵食獎勵值必須在 1-50 之間")]
        public int FeedBonus { get; set; } = 10;

        [Display(Name = "清潔獎勵值 (洗澡)")]
        [Required(ErrorMessage = "清潔獎勵值為必填欄位")]
        [Range(1, 50, ErrorMessage = "清潔獎勵值必須在 1-50 之間")]
        public int CleanBonus { get; set; } = 10;

        [Display(Name = "玩耍獎勵值 (心情)")]
        [Required(ErrorMessage = "玩耍獎勵值為必填欄位")]
        [Range(1, 50, ErrorMessage = "玩耍獎勵值必須在 1-50 之間")]
        public int PlayBonus { get; set; } = 10;

        [Display(Name = "哄睡獎勵值 (體力)")]
        [Required(ErrorMessage = "哄睡獎勵值為必填欄位")]
        [Range(1, 50, ErrorMessage = "哄睡獎勵值必須在 1-50 之間")]
        public int SleepBonus { get; set; } = 10;

        [Display(Name = "經驗值獎勵倍數")]
        [Required(ErrorMessage = "經驗值獎勵倍數為必填欄位")]
        [Range(1, 10, ErrorMessage = "經驗值獎勵倍數必須在 1-10 之間")]
        public int ExpBonus { get; set; } = 1;

        [Display(Name = "顏色更換所需點數")]
        [Required(ErrorMessage = "顏色更換所需點數為必填欄位")]
        [Range(0, 10000, ErrorMessage = "顏色更換所需點數必須在 0-10000 之間")]
        public int ColorChangePoints { get; set; } = 2000;

        [Display(Name = "背景更換所需點數")]
        [Required(ErrorMessage = "背景更換所需點數為必填欄位")]
        [Range(0, 10000, ErrorMessage = "背景更換所需點數必須在 0-10000 之間")]
        public int BackgroundChangePoints { get; set; } = 1000;

        [Display(Name = "可用顏色列表")]
        public string? AvailableColors { get; set; } = "#FFFFFF,#FFD700,#FF6B6B,#4ECDC4,#45B7D1,#FFA07A,#98D8C8,#F7DC6F,#BB8FCE,#85C1E2";

        [Display(Name = "可用背景列表")]
        public string? AvailableBackgrounds { get; set; } = "#FFFFFF,#F0F0F0,#E8F5E9,#E3F2FD,#FFF3E0,#FCE4EC,#F3E5F5,#E0F2F1,#FFF8E1,#EFEBE9";
    }

    /// <summary>
    /// 寵物基本資訊輸入模型
    /// </summary>
    public class PetBasicInfoInputModel
    {
        [Required(ErrorMessage = "寵物ID為必填欄位")]
        [Display(Name = "寵物ID")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "寵物名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "寵物名稱長度不能超過 50 個字元")]
        [Display(Name = "寵物名稱")]
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "用戶ID為必填欄位")]
        [Display(Name = "用戶ID")]
        public int UserId { get; set; }
    }

    /// <summary>
    /// 寵物外觀輸入模型
    /// </summary>
    public class PetAppearanceInputModel
    {
        [Required(ErrorMessage = "寵物ID為必填欄位")]
        [Display(Name = "寵物ID")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "膚色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "膚色代碼必須為 7 個字元（包含#）")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "膚色代碼格式不正確，必須為 #RRGGBB 格式")]
        [Display(Name = "膚色代碼")]
        public string SkinColorCode { get; set; } = "#FFD700";

        [Display(Name = "膚色")]
        public string? SkinColor
        {
            get => SkinColorCode;
            set => SkinColorCode = value ?? "#FFD700";
        }

        [Required(ErrorMessage = "背景顏色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "背景顏色代碼必須為 7 個字元（包含#）")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "背景顏色代碼格式不正確，必須為 #RRGGBB 格式")]
        [Display(Name = "背景顏色代碼")]
        public string BackgroundColorCode { get; set; } = "#FFFFFF";

        [Display(Name = "背景顏色")]
        public string? BackgroundColor
        {
            get => BackgroundColorCode;
            set => BackgroundColorCode = value ?? "#FFFFFF";
        }

        [Display(Name = "所需點數")]
        [Range(0, int.MaxValue, ErrorMessage = "所需點數必須大於等於 0")]
        public int PointsCost { get; set; } = 0;

        [Display(Name = "描述")]
        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 寵物屬性輸入模型
    /// </summary>
    public class PetStatsInputModel
    {
        [Required(ErrorMessage = "寵物ID為必填欄位")]
        [Display(Name = "寵物ID")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "等級必須在 1-100 之間")]
        [Display(Name = "等級")]
        public int Level { get; set; } = 1;

        [Required(ErrorMessage = "經驗值為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "經驗值必須大於等於 0")]
        [Display(Name = "經驗值")]
        public int Experience { get; set; } = 0;

        [Required(ErrorMessage = "飢餓度為必填欄位")]
        [Range(0, 100, ErrorMessage = "飢餓度必須在 0-100 之間")]
        [Display(Name = "飢餓度")]
        public int Hunger { get; set; } = 100;

        [Required(ErrorMessage = "快樂度為必填欄位")]
        [Range(0, 100, ErrorMessage = "快樂度必須在 0-100 之間")]
        [Display(Name = "快樂度")]
        public int Happiness { get; set; } = 100;

        [Required(ErrorMessage = "心情為必填欄位")]
        [Range(0, 100, ErrorMessage = "心情必須在 0-100 之間")]
        [Display(Name = "心情")]
        public int Mood { get; set; } = 100;

        [Required(ErrorMessage = "體力為必填欄位")]
        [Range(0, 100, ErrorMessage = "體力必須在 0-100 之間")]
        [Display(Name = "體力")]
        public int Stamina { get; set; } = 100;

        [Required(ErrorMessage = "乾淨度為必填欄位")]
        [Range(0, 100, ErrorMessage = "乾淨度必須在 0-100 之間")]
        [Display(Name = "乾淨度")]
        public int Cleanliness { get; set; } = 100;

        [Required(ErrorMessage = "健康度為必填欄位")]
        [Range(0, 100, ErrorMessage = "健康度必須在 0-100 之間")]
        [Display(Name = "健康度")]
        public int Health { get; set; } = 100;
    }

    /// <summary>
    /// 寵物管理列表查詢模型
    /// </summary>
    public class PetAdminListQueryModel
    {
        [Display(Name = "會員ID")]
        public int? UserId { get; set; }

        [Display(Name = "寵物名稱")]
        [StringLength(50, ErrorMessage = "寵物名稱長度不能超過 50 個字元")]
        public string? PetName { get; set; }

        [Display(Name = "最低等級")]
        [Range(1, 100, ErrorMessage = "最低等級必須在 1-100 之間")]
        public int? MinLevel { get; set; }

        [Display(Name = "最高等級")]
        [Range(1, 100, ErrorMessage = "最高等級必須在 1-100 之間")]
        public int? MaxLevel { get; set; }

        [Display(Name = "最低經驗值")]
        [Range(0, int.MaxValue, ErrorMessage = "最低經驗值必須大於等於 0")]
        public int? MinExperience { get; set; }

        [Display(Name = "最高經驗值")]
        [Range(0, int.MaxValue, ErrorMessage = "最高經驗值必須大於等於 0")]
        public int? MaxExperience { get; set; }

        [Display(Name = "膚色")]
        [StringLength(10, ErrorMessage = "膚色代碼長度不能超過 10 個字元")]
        public string? SkinColor { get; set; }

        [Display(Name = "背景顏色")]
        [StringLength(20, ErrorMessage = "背景顏色代碼長度不能超過 20 個字元")]
        public string? BackgroundColor { get; set; }

        [Display(Name = "搜尋關鍵字")]
        [StringLength(100, ErrorMessage = "搜尋關鍵字長度不能超過 100 個字元")]
        public string? SearchTerm { get; set; }

        [Display(Name = "排序欄位")]
        [StringLength(50, ErrorMessage = "排序欄位長度不能超過 50 個字元")]
        public string SortBy { get; set; } = "name";

        [Display(Name = "排序順序")]
        [StringLength(10, ErrorMessage = "排序順序長度不能超過 10 個字元")]
        public string? SortOrder { get; set; } = "asc";

        [Display(Name = "頁碼")]
        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於等於 1")]
        public int Page { get; set; } = 1;

        [Display(Name = "頁碼")]
        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於等於 1")]
        public int PageNumber { get; set; } = 1;

        [Display(Name = "每頁筆數")]
        [Range(1, 100, ErrorMessage = "每頁筆數必須在 1-100 之間")]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 寵物管理列表項目 ViewModel
    /// </summary>
    public class PetAdminListItemViewModel
    {
        [Display(Name = "寵物ID")]
        public int PetId { get; set; }

        [Display(Name = "寵物名稱")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "用戶ID")]
        public int UserId { get; set; }

        [Display(Name = "用戶名稱")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "等級")]
        public int Level { get; set; }

        [Display(Name = "經驗值")]
        public int Experience { get; set; }

        [Display(Name = "飢餓度")]
        public int Hunger { get; set; }

        [Display(Name = "快樂度")]
        public int Happiness { get; set; }

        [Display(Name = "心情")]
        public int Mood { get; set; }

        [Display(Name = "體力")]
        public int Stamina { get; set; }

        [Display(Name = "乾淨度")]
        public int Cleanliness { get; set; }

        [Display(Name = "健康度")]
        public int Health { get; set; }

        [Display(Name = "膚色")]
        public string SkinColor { get; set; } = string.Empty;

        [Display(Name = "背景顏色")]
        public string BackgroundColor { get; set; } = string.Empty;

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "最後互動時間")]
        public DateTime? LastInteraction { get; set; }
    }

    /// <summary>
    /// 寵物管理列表分頁結果
    /// </summary>
    public class PetAdminListPagedResult
    {
        [Display(Name = "寵物列表")]
        public List<PetAdminListItemViewModel> Items { get; set; } = new();

        [Display(Name = "總筆數")]
        public int TotalCount { get; set; }

        [Display(Name = "當前頁碼")]
        public int Page { get; set; }

        [Display(Name = "當前頁碼")]
        public int PageNumber { get; set; }

        [Display(Name = "每頁筆數")]
        public int PageSize { get; set; }

        [Display(Name = "總頁數")]
        public int TotalPages { get; set; }

        [Display(Name = "是否有上一頁")]
        public bool HasPreviousPage => Page > 1 || PageNumber > 1;

        [Display(Name = "是否有下一頁")]
        public bool HasNextPage => (Page > 0 ? Page : PageNumber) < TotalPages;
    }

    /// <summary>
    /// 寵物管理詳細資料 ViewModel
    /// </summary>
    public class PetAdminDetailViewModel
    {
        [Display(Name = "寵物ID")]
        public int PetId { get; set; }

        [Display(Name = "寵物名稱")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "用戶ID")]
        public int UserId { get; set; }

        [Display(Name = "用戶名稱")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "用戶Email")]
        public string? UserEmail { get; set; }

        [Display(Name = "等級")]
        public int Level { get; set; }

        [Display(Name = "升級時間")]
        public DateTime? LevelUpTime { get; set; }

        [Display(Name = "經驗值")]
        public int Experience { get; set; }

        [Display(Name = "飢餓度")]
        public int Hunger { get; set; }

        [Display(Name = "快樂度")]
        public int Happiness { get; set; }

        [Display(Name = "心情")]
        public int Mood { get; set; }

        [Display(Name = "體力")]
        public int Stamina { get; set; }

        [Display(Name = "乾淨度")]
        public int Cleanliness { get; set; }

        [Display(Name = "健康度")]
        public int Health { get; set; }

        [Display(Name = "膚色")]
        public string SkinColor { get; set; } = string.Empty;

        [Display(Name = "膚色變更時間")]
        public DateTime? SkinColorChangedTime { get; set; }

        [Display(Name = "背景顏色")]
        public string BackgroundColor { get; set; } = string.Empty;

        [Display(Name = "背景顏色變更時間")]
        public DateTime? BackgroundColorChangedTime { get; set; }

        [Display(Name = "膚色變更消耗點數")]
        public int? PointsChangedSkinColor { get; set; }

        [Display(Name = "背景變更消耗點數")]
        public int? PointsChangedBackgroundColor { get; set; }

        [Display(Name = "升級獲得點數")]
        public int? PointsGainedLevelUp { get; set; }

        [Display(Name = "升級獲得點數時間")]
        public DateTime? PointsGainedTimeLevelUp { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "最後互動時間")]
        public DateTime? LastInteraction { get; set; }

        [Display(Name = "總互動次數")]
        public int TotalInteractions { get; set; }

        [Display(Name = "總顏色變更次數")]
        public int TotalColorChanges { get; set; }

        [Display(Name = "總背景變更次數")]
        public int TotalBackgroundChanges { get; set; }
    }

    /// <summary>
    /// 寵物顏色變更歷史查詢模型
    /// </summary>
    public class PetColorChangeHistoryQueryModel
    {
        [Display(Name = "用戶ID")]
        public int? UserId { get; set; }

        [Display(Name = "寵物ID")]
        public int? PetId { get; set; }

        [Display(Name = "開始日期")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "結束日期")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "排序順序")]
        [StringLength(10, ErrorMessage = "排序順序長度不能超過 10 個字元")]
        public string SortOrder { get; set; } = "desc";

        [Display(Name = "頁碼")]
        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於等於 1")]
        public int PageNumber { get; set; } = 1;

        [Display(Name = "每頁筆數")]
        [Range(1, 100, ErrorMessage = "每頁筆數必須在 1-100 之間")]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 寵物顏色變更歷史分頁結果
    /// </summary>
    public class PetColorChangeHistoryPagedResult
    {
        [Display(Name = "變更記錄列表")]
        public List<ColorChangeHistoryItemViewModel> Items { get; set; } = new();

        [Display(Name = "總筆數")]
        public int TotalCount { get; set; }

        [Display(Name = "當前頁碼")]
        public int PageNumber { get; set; }

        [Display(Name = "每頁筆數")]
        public int PageSize { get; set; }

        [Display(Name = "總頁數")]
        public int TotalPages { get; set; }

        [Display(Name = "是否有上一頁")]
        public bool HasPreviousPage => PageNumber > 1;

        [Display(Name = "是否有下一頁")]
        public bool HasNextPage => PageNumber < TotalPages;

        // Query filter properties for maintaining search state
        [Display(Name = "用戶ID")]
        public int? UserId { get; set; }

        [Display(Name = "寵物ID")]
        public int? PetId { get; set; }

        [Display(Name = "更換類型")]
        public string? ChangeType { get; set; }

        [Display(Name = "開始日期")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "結束日期")]
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// 寵物顏色變更歷史項目
    /// </summary>
    public class ColorChangeHistoryItemViewModel
    {
        [Display(Name = "變更ID")]
        public int LogId { get; set; }

        [Display(Name = "寵物ID")]
        public int? PetId { get; set; }

        [Display(Name = "寵物名稱")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "用戶ID")]
        public int UserId { get; set; }

        [Display(Name = "用戶名稱")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "更換類型")]
        public string ChangeType { get; set; } = string.Empty;

        [Display(Name = "原顏色代碼")]
        public string OldColor { get; set; } = string.Empty;

        [Display(Name = "新顏色代碼")]
        public string NewColor { get; set; } = string.Empty;

        [Display(Name = "消耗點數")]
        public int PointsCost { get; set; }

        [Display(Name = "變更時間")]
        public DateTime ChangedAt { get; set; }

        [Display(Name = "描述")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 寵物背景變更歷史查詢模型
    /// </summary>
    public class PetBackgroundChangeHistoryQueryModel
    {
        [Display(Name = "用戶ID")]
        public int? UserId { get; set; }

        [Display(Name = "寵物ID")]
        public int? PetId { get; set; }

        [Display(Name = "開始日期")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "結束日期")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "排序順序")]
        [StringLength(10, ErrorMessage = "排序順序長度不能超過 10 個字元")]
        public string SortOrder { get; set; } = "desc";

        [Display(Name = "頁碼")]
        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於等於 1")]
        public int PageNumber { get; set; } = 1;

        [Display(Name = "每頁筆數")]
        [Range(1, 100, ErrorMessage = "每頁筆數必須在 1-100 之間")]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 寵物背景變更歷史分頁結果
    /// </summary>
    public class PetBackgroundChangeHistoryPagedResult
    {
        [Display(Name = "變更記錄列表")]
        public List<BackgroundChangeHistoryItemViewModel> Items { get; set; } = new();

        [Display(Name = "總筆數")]
        public int TotalCount { get; set; }

        [Display(Name = "當前頁碼")]
        public int PageNumber { get; set; }

        [Display(Name = "每頁筆數")]
        public int PageSize { get; set; }

        [Display(Name = "總頁數")]
        public int TotalPages { get; set; }

        [Display(Name = "是否有上一頁")]
        public bool HasPreviousPage => PageNumber > 1;

        [Display(Name = "是否有下一頁")]
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// 寵物背景變更歷史項目
    /// </summary>
    public class BackgroundChangeHistoryItemViewModel
    {
        [Display(Name = "變更ID")]
        public int LogId { get; set; }

        [Display(Name = "寵物ID")]
        public int? PetId { get; set; }

        [Display(Name = "寵物名稱")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "用戶ID")]
        public int UserId { get; set; }

        [Display(Name = "用戶名稱")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "原背景代碼")]
        public string OldBackground { get; set; } = string.Empty;

        [Display(Name = "新背景代碼")]
        public string NewBackground { get; set; } = string.Empty;

        [Display(Name = "消耗點數")]
        public int PointsCost { get; set; }

        [Display(Name = "變更時間")]
        public DateTime ChangedAt { get; set; }

        [Display(Name = "描述")]
        public string? Description { get; set; }
    }
}






