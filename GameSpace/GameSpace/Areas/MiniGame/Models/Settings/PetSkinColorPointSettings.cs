using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// �d�������I�Ƴ]�w
    /// </summary>
    public class PetSkinColorPointSettings
    {
        [Key]
        public int SettingId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public int RequiredLevel { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Property aliases for compatibility
        [NotMapped]
        public int Level
        {
            get => RequiredLevel;
            set => RequiredLevel = value;
        }

        [NotMapped]
        public int PointsCost
        {
            get => PointCost;
            set => PointCost = value;
        }
    }
}
