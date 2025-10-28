using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物換色點數設定 ViewModel
    /// </summary>
    public class PetColorChangeSettingsViewModel
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 設定ID（別名屬性，為了向後相容）
        /// </summary>
        public int SettingId
        {
            get => Id;
            set => Id = value;
        }

        /// <summary>
        /// 顏色名稱
        /// </summary>
        [Required(ErrorMessage = "顏色名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "顏色名稱長度不能超過100個字元")]
        [Display(Name = "顏色名稱")]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 換色所需點數
        /// </summary>
        [Required(ErrorMessage = "換色所需點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "換色所需點數必須大於等於0")]
        [Display(Name = "換色所需點數")]
        public int RequiredPoints { get; set; } = 2000;

        /// <summary>
        /// 顏色代碼（十六進位，例如：#FF5733）
        /// </summary>
        [Required(ErrorMessage = "顏色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "顏色代碼必須為7個字元（包含#）")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確，必須為 #RRGGBB 格式")]
        [Display(Name = "顏色代碼")]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 建立者ID
        /// </summary>
        [Display(Name = "建立者ID")]
        public int CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        [Display(Name = "更新者ID")]
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = "備註長度不能超過500個字元")]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }

        /// <summary>
        /// 所需點數（別名屬性，為了向後相容）
        /// </summary>
        public int PointsRequired
        {
            get => RequiredPoints;
            set => RequiredPoints = value;
        }

        /// <summary>
        /// 設定名稱（別名屬性，為了向後相容）
        /// </summary>
        public string SettingName
        {
            get => ColorName;
            set => ColorName = value;
        }
    }

    /// <summary>
    /// 寵物換色點數設定索引 ViewModel
    /// </summary>
    public class PetColorChangeSettingsIndexViewModel
    {
        /// <summary>
        /// 設定列表
        /// </summary>
        public List<PetColorChangeSettingsViewModel> Settings { get; set; } = new List<PetColorChangeSettingsViewModel>();

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

