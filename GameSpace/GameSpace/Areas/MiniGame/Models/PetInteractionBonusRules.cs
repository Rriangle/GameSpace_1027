using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物互動獎勵規則
    /// </summary>
    public class PetInteractionBonusRules
    {
        [Key]
        public int RuleId { get; set; }

        /// <summary>
        /// 互動類型 (Feed, Bath, Play, Sleep)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string InteractionType { get; set; } = string.Empty;

        /// <summary>
        /// 互動類型名稱（中文）
        /// </summary>
        [MaxLength(50)]
        public string InteractionName { get; set; } = string.Empty;

        /// <summary>
        /// 飢餓度變化
        /// </summary>
        public int HungerChange { get; set; }

        /// <summary>
        /// 心情度變化
        /// </summary>
        public int MoodChange { get; set; }

        /// <summary>
        /// 體力度變化
        /// </summary>
        public int StaminaChange { get; set; }

        /// <summary>
        /// 清潔度變化
        /// </summary>
        public int CleanlinessChange { get; set; }

        /// <summary>
        /// 經驗值獎勵
        /// </summary>
        public int ExpBonus { get; set; }

        /// <summary>
        /// 點數獎勵
        /// </summary>
        public int PointsBonus { get; set; }

        /// <summary>
        /// 冷卻時間（分鐘）
        /// </summary>
        public int CooldownMinutes { get; set; }

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
        /// 備註說明
        /// </summary>
        public string? Description { get; set; }
    }
}
