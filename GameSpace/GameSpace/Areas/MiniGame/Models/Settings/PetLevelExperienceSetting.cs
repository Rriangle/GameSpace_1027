using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物等級經驗值對應設定
    /// </summary>
    public class PetLevelExperienceSetting
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 等級
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 所需經驗值
        /// </summary>
        public int RequiredExperience { get; set; }

        /// <summary>
        /// 等級描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

