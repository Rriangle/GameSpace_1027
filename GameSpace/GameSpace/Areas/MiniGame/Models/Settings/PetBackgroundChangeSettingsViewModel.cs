using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物換背景點數設定 ViewModel
    /// </summary>
    public class PetBackgroundChangeSettingsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "背景名稱為必填")]
        [StringLength(50, ErrorMessage = "背景名稱長度不能超過50字")]
        [Display(Name = "背景名稱")]
        public string BackgroundName { get; set; } = "";

        [Required(ErrorMessage = "背景顏色為必填")]
        [StringLength(20, ErrorMessage = "背景顏色長度不能超過20字")]
        [Display(Name = "背景顏色")]
        public string BackgroundColor { get; set; } = "";

        [Required(ErrorMessage = "所需點數為必填")]
        [Range(0, 10000, ErrorMessage = "所需點數必須在0-10000之間")]
        [Display(Name = "所需點數")]
        public int PointsRequired { get; set; }

        [StringLength(7, ErrorMessage = "背景代碼長度不能超過7字")]
        [Display(Name = "背景代碼")]
        public string BackgroundCode { get; set; } = "";

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物換背景點數設定索引 ViewModel
    /// </summary>
    public class PetBackgroundChangeSettingsIndexViewModel
    {
        /// <summary>
        /// 背景設定列表
        /// </summary>
        public List<PetBackgroundChangeSettingsViewModel> Settings { get; set; } = new List<PetBackgroundChangeSettingsViewModel>();

        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 頁碼
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}

