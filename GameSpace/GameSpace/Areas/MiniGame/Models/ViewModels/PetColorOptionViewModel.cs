using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物顏色選項 ViewModel
    /// </summary>
    public class PetColorOptionViewModel
    {
        /// <summary>
        /// 顏色選項ID
        /// </summary>
        public int ColorOptionId { get; set; }

        /// <summary>
        /// 顏色名稱
        /// </summary>
        [Required(ErrorMessage = "顏色名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "顏色名稱長度不能超過50個字元")]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 顏色代碼（十六進位，如 #FF5733）
        /// </summary>
        [Required(ErrorMessage = "顏色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "顏色代碼必須為7個字元（包含#）")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確，應為 #RRGGBB 格式")]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 顏色類型 (Skin 或 Background)
        /// </summary>
        [Required(ErrorMessage = "顏色類型為必填欄位")]
        [StringLength(20)]
        public string ColorType { get; set; } = "Skin";

        /// <summary>
        /// 更換所需點數
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "更換所需點數必須大於等於0")]
        public int PointsCost { get; set; } = 2000;

        /// <summary>
        /// 解鎖所需等級
        /// </summary>
        [Range(1, 100, ErrorMessage = "解鎖等級必須在 1-100 之間")]
        public int UnlockLevel { get; set; } = 1;

        /// <summary>
        /// 是否為預設顏色
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        [Range(0, 9999)]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 顏色分類標籤（如：暖色系、冷色系、自然色）
        /// </summary>
        [StringLength(50)]
        public string? CategoryTag { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// 縮圖URL（可選）
        /// </summary>
        [StringLength(500)]
        [Url(ErrorMessage = "縮圖URL格式不正確")]
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// 是否為特殊/稀有顏色
        /// </summary>
        public bool IsSpecial { get; set; } = false;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 使用此顏色的寵物數量
        /// </summary>
        public int UsageCount { get; set; } = 0;
    }
}
