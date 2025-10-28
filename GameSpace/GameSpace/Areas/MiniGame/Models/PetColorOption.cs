using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物顏色選項模型
    /// </summary>
    public class PetColorOption
    {
        /// <summary>
        /// 顏色選項ID
        /// </summary>
        [Key]
        public int ColorOptionId { get; set; }

        /// <summary>
        /// 顏色名稱
        /// </summary>
        [Required(ErrorMessage = "顏色名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "顏色名稱長度不能超過50個字元")]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 顏色代碼（十六進位）
        /// </summary>
        [Required(ErrorMessage = "顏色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "顏色代碼必須為7個字元（包含#）")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確，必須為#開頭的6位十六進位數字")]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 建立者ID
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = "備註長度不能超過500個字元")]
        public string? Remarks { get; set; }

        // Property aliases for compatibility
        [NotMapped]
        public string ColorValue { get => ColorCode; set => ColorCode = value; }

        [NotMapped]
        public int DisplayOrder { get => SortOrder; set => SortOrder = value; }
    }
}
