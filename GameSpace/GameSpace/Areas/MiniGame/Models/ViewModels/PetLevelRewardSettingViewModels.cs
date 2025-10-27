using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    public class PetLevelRewardSettingViewModel
    {
        public int SettingId { get; set; }

        [Required(ErrorMessage = "等級範圍起始為必填欄位")]
        [Range(1, 200, ErrorMessage = "等級範圍起始必須在1-200之間")]
        [Display(Name = "等級範圍起始")]
        public int LevelRangeStart { get; set; }

        [Required(ErrorMessage = "等級範圍結束為必填欄位")]
        [Range(1, 200, ErrorMessage = "等級範圍結束必須在1-200之間")]
        [Display(Name = "等級範圍結束")]
        public int LevelRangeEnd { get; set; }

        [Required(ErrorMessage = "獎勵點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "獎勵點數必須大於等於0")]
        [Display(Name = "獎勵點數")]
        public int PointsReward { get; set; }

        [StringLength(500, ErrorMessage = "獎勵描述長度不能超過500個字元")]
        [Display(Name = "獎勵描述")]
        public string? Description { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "顯示順序")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "更新者")]
        public int? UpdatedBy { get; set; }
    }

    public class PetLevelRewardSettingCreateViewModel
    {
        [Required(ErrorMessage = "等級範圍起始為必填欄位")]
        [Range(1, 200, ErrorMessage = "等級範圍起始必須在1-200之間")]
        [Display(Name = "等級範圍起始")]
        public int LevelRangeStart { get; set; }

        [Required(ErrorMessage = "等級範圍結束為必填欄位")]
        [Range(1, 200, ErrorMessage = "等級範圍結束必須在1-200之間")]
        [Display(Name = "等級範圍結束")]
        public int LevelRangeEnd { get; set; }

        [Required(ErrorMessage = "獎勵點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "獎勵點數必須大於等於0")]
        [Display(Name = "獎勵點數")]
        public int PointsReward { get; set; }

        [StringLength(500, ErrorMessage = "獎勵描述長度不能超過500個字元")]
        [Display(Name = "獎勵描述")]
        public string? Description { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "顯示順序")]
        public int DisplayOrder { get; set; } = 0;
    }

    public class PetLevelRewardSettingEditViewModel
    {
        public int SettingId { get; set; }

        [Required(ErrorMessage = "等級範圍起始為必填欄位")]
        [Range(1, 200, ErrorMessage = "等級範圍起始必須在1-200之間")]
        [Display(Name = "等級範圍起始")]
        public int LevelRangeStart { get; set; }

        [Required(ErrorMessage = "等級範圍結束為必填欄位")]
        [Range(1, 200, ErrorMessage = "等級範圍結束必須在1-200之間")]
        [Display(Name = "等級範圍結束")]
        public int LevelRangeEnd { get; set; }

        [Required(ErrorMessage = "獎勵點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "獎勵點數必須大於等於0")]
        [Display(Name = "獎勵點數")]
        public int PointsReward { get; set; }

        [StringLength(500, ErrorMessage = "獎勵描述長度不能超過500個字元")]
        [Display(Name = "獎勵描述")]
        public string? Description { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "顯示順序")]
        public int DisplayOrder { get; set; } = 0;
    }

    public class PetLevelRewardSettingListViewModel
    {
        public int SettingId { get; set; }
        public int LevelRangeStart { get; set; }
        public int LevelRangeEnd { get; set; }
        public int PointsReward { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class PetLevelRewardSettingSearchViewModel
    {
        [Display(Name = "等級範圍起始")]
        public int? LevelRangeStart { get; set; }

        [Display(Name = "等級範圍結束")]
        public int? LevelRangeEnd { get; set; }

        [Display(Name = "是否啟用")]
        public bool? IsActive { get; set; }

        [Display(Name = "頁碼")]
        public int Page { get; set; } = 1;

        [Display(Name = "每頁筆數")]
        public int PageSize { get; set; } = 10;
    }
}
