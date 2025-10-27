using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物升級規則
    /// </summary>
    public class PetLevelUpRule
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 等級
        /// </summary>
        [Required]
        public int Level { get; set; }

        /// <summary>
        /// 升級所需經驗值
        /// </summary>
        [Required]
        public int RequiredExp { get; set; }

        /// <summary>
        /// 升級獎勵點數
        /// </summary>
        public int RewardPoints { get; set; }

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
