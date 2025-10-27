using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
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
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 換色所需點數
        /// </summary>
        [Required(ErrorMessage = "換色所需點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "換色所需點數必須大於等於0")]
        public int RequiredPoints { get; set; }

        /// <summary>
        /// 顏色代碼（十六進位）
        /// </summary>
        [Required(ErrorMessage = "顏色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "顏色代碼必須為7個字元（包含#）")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確")]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = "備註長度不能超過500個字元")]
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
        /// 顏色選項列表
        /// </summary>
        public IEnumerable<PetColorChangeSettingsViewModel> ColorOptions { get; set; } = new List<PetColorChangeSettingsViewModel>();

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

    /// <summary>
    /// 點數設定統計 ViewModel
    /// </summary>
    public class PointsSettingsStatisticsViewModel
    {
        /// <summary>
        /// 總點數
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// 啟用的顏色選項數量
        /// </summary>
        public int ActiveColorOptions { get; set; }

        /// <summary>
        /// 停用的顏色選項數量
        /// </summary>
        public int InactiveColorOptions { get; set; }

        /// <summary>
        /// 平均所需點數
        /// </summary>
        public double AveragePointsCost { get; set; }

        /// <summary>
        /// 最高點數成本
        /// </summary>
        public int MaxPointsCost { get; set; }

        /// <summary>
        /// 最低點數成本
        /// </summary>
        public int MinPointsCost { get; set; }
    }
}
