using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物換背景點數設定
    /// </summary>
    [Table("PetBackgroundChangeSettings")]
    public class PetBackgroundChangeSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string BackgroundName { get; set; } = "";

        /// <summary>
        /// 背景颜色 - Alias for BackgroundCode for view compatibility
        /// </summary>
        [NotMapped]
        public string BackgroundColor
        {
            get => BackgroundCode;
            set => BackgroundCode = value;
        }

        /// <summary>
        /// 所需點數 - Alias for RequiredPoints for view compatibility
        /// </summary>
        [NotMapped]
        public int PointsRequired
        {
            get => RequiredPoints;
            set => RequiredPoints = value;
        }

        [Required]
        public int RequiredPoints { get; set; }

        [StringLength(7)]
        public string BackgroundCode { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

