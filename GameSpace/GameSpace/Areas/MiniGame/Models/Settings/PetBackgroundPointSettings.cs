using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// �d���I���I�Ƴ]�w
    /// </summary>
    public class PetBackgroundPointSettings
    {
        [Key]
        public int SettingId { get; set; }
        public string BackgroundName { get; set; } = string.Empty;
        public string BackgroundCode { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public int RequiredLevel { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Property aliases for compatibility with legacy MiniGame code
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
