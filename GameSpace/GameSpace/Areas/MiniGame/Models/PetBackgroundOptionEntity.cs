using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    public class PetBackgroundOptionEntity
    {
        [Key]
        public int BackgroundOptionId { get; set; }
        [Required]
        [StringLength(50)]
        public string BackgroundName { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        public string BackgroundCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Property aliases for compatibility
        [NotMapped]
        public int BackgroundId { get => BackgroundOptionId; set => BackgroundOptionId = value; }

        [NotMapped]
        public int RequiredPoints { get; set; }

        [NotMapped]
        public bool IsUnlocked { get; set; }
    }

    // Alias for compatibility
    public class PetBackgroundOption : PetBackgroundOptionEntity
    {
    }
}
