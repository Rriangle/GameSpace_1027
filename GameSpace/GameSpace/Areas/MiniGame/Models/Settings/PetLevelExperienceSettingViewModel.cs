using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    public class PetLevelExperienceSettingViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "等級為必填項目")]
        [Range(1, 100, ErrorMessage = "等級必須在 1-100 之間")]
        public int Level { get; set; }
        
        [Required(ErrorMessage = "所需經驗值為必填項目")]
        [Range(1, int.MaxValue, ErrorMessage = "所需經驗值必須大於 0")]
        public int RequiredExperience { get; set; }
        
        [StringLength(500, ErrorMessage = "描述長度不能超過 500 字")]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    public class PetLevelExperienceSettingListViewModel
    {
        public List<PetLevelExperienceSettingViewModel> Settings { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}

