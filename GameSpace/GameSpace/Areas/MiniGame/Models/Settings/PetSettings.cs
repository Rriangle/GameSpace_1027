using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物系統設定模型
    /// </summary>
    public class PetSettings
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        [Key]
        public int SettingId { get; set; }

        /// <summary>
        /// 設定名稱
        /// </summary>
        [Required(ErrorMessage = "設定名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "設定名稱長度不能超過100個字元")]
        public string SettingName { get; set; } = "PetSystemSettings";

        /// <summary>
        /// 最大健康值
        /// </summary>
        [Range(1, 1000, ErrorMessage = "最大健康值必須在1-1000之間")]
        public int MaxHealth { get; set; } = 100;

        /// <summary>
        /// 最大飽食度
        /// </summary>
        [Range(1, 1000, ErrorMessage = "最大飽食度必須在1-1000之間")]
        public int MaxHunger { get; set; } = 100;

        /// <summary>
        /// 最大心情值
        /// </summary>
        [Range(1, 1000, ErrorMessage = "最大心情值必須在1-1000之間")]
        public int MaxMood { get; set; } = 100;

        /// <summary>
        /// 最大乾淨度
        /// </summary>
        [Range(1, 1000, ErrorMessage = "最大乾淨度必須在1-1000之間")]
        public int MaxCleanliness { get; set; } = 100;

        /// <summary>
        /// 最大忠誠度
        /// </summary>
        [Range(1, 1000, ErrorMessage = "最大忠誠度必須在1-1000之間")]
        public int MaxLoyalty { get; set; } = 100;

        /// <summary>
        /// 健康值衰減率（每小時）
        /// </summary>
        [Range(0, 100, ErrorMessage = "健康值衰減率必須在0-100之間")]
        public int HealthDecayRate { get; set; } = 1;

        /// <summary>
        /// 飽食度衰減率（每小時）
        /// </summary>
        [Range(0, 100, ErrorMessage = "飽食度衰減率必須在0-100之間")]
        public int HungerDecayRate { get; set; } = 2;

        /// <summary>
        /// 心情值衰減率（每小時）
        /// </summary>
        [Range(0, 100, ErrorMessage = "心情值衰減率必須在0-100之間")]
        public int MoodDecayRate { get; set; } = 1;

        /// <summary>
        /// 乾淨度衰減率（每小時）
        /// </summary>
        [Range(0, 100, ErrorMessage = "乾淨度衰減率必須在0-100之間")]
        public int CleanlinessDecayRate { get; set; } = 2;

        /// <summary>
        /// 忠誠度衰減率（每小時）
        /// </summary>
        [Range(0, 100, ErrorMessage = "忠誠度衰減率必須在0-100之間")]
        public int LoyaltyDecayRate { get; set; } = 1;

        /// <summary>
        /// 餵食所需點數
        /// </summary>
        [Range(0, 10000, ErrorMessage = "餵食所需點數必須在0-10000之間")]
        public int FeedPointsCost { get; set; } = 10;

        /// <summary>
        /// 玩耍所需點數
        /// </summary>
        [Range(0, 10000, ErrorMessage = "玩耍所需點數必須在0-10000之間")]
        public int PlayPointsCost { get; set; } = 5;

        /// <summary>
        /// 清潔所需點數
        /// </summary>
        [Range(0, 10000, ErrorMessage = "清潔所需點數必須在0-10000之間")]
        public int CleanPointsCost { get; set; } = 5;

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

        /// <summary>
        /// 建立者ID
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = "備註長度不能超過500個字元")]
        public string? Remarks { get; set; }
    }
}

