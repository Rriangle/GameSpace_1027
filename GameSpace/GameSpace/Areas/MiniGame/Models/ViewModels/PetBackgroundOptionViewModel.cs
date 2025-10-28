﻿using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物背景選項 (簡化版)
    /// </summary>

    /// <summary>
    /// 寵物背景選項 ViewModel
    /// </summary>
    public class PetBackgroundOptionViewModel
    {
        /// <summary>
        /// 背景選項ID
        /// </summary>
        public int BackgroundOptionId { get; set; }

        /// <summary>
        /// 背景名稱
        /// </summary>
        [Required(ErrorMessage = "背景名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "背景名稱長度不能超過100個字元")]
        public string BackgroundName { get; set; } = string.Empty;

        /// <summary>
        /// 背景代碼/主題代碼
        /// </summary>
        [Required(ErrorMessage = "背景代碼為必填欄位")]
        [StringLength(50, ErrorMessage = "背景代碼長度不能超過50個字元")]
        public string BackgroundCode { get; set; } = string.Empty;

        /// <summary>
        /// 背景類型 (Color, Image, Pattern, Gradient)
        /// </summary>
        [Required(ErrorMessage = "背景類型為必填欄位")]
        [StringLength(20)]
        public string BackgroundType { get; set; } = "Color";

        /// <summary>
        /// 背景圖片URL（如果是圖片類型）
        /// </summary>
        [StringLength(500)]
        [Url(ErrorMessage = "圖片URL格式不正確")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// 背景顏色代碼（如果是純色類型）
        /// </summary>
        [StringLength(7)]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確，應為 #RRGGBB 格式")]
        public string? ColorCode { get; set; }

        /// <summary>
        /// 縮圖URL
        /// </summary>
        [StringLength(500)]
        [Url(ErrorMessage = "縮圖URL格式不正確")]
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// 更換所需點數
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "更換所需點數必須大於等於0")]
        public int PointCost { get; set; } = 3000;

        /// <summary>
        /// 解鎖所需等級
        /// </summary>
        [Range(1, 100, ErrorMessage = "解鎖等級必須在 1-100 之間")]
        public int UnlockLevel { get; set; } = 1;

        /// <summary>
        /// 是否為預設背景
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
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 背景分類（如：自然、城市、抽象、節日）
        /// </summary>
        [StringLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// 背景主題標籤（如：春天、海洋、星空）
        /// </summary>
        [StringLength(100)]
        public string? ThemeTags { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// 是否為季節限定
        /// </summary>
        public bool IsSeasonalLimited { get; set; } = false;

        /// <summary>
        /// 季節類型（如有）
        /// </summary>
        [StringLength(20)]
        public string? SeasonType { get; set; }

        /// <summary>
        /// 是否為特殊/稀有背景
        /// </summary>
        public bool IsSpecial { get; set; } = false;

        /// <summary>
        /// CSS樣式（額外的樣式設定）
        /// </summary>
        [StringLength(500)]
        public string? CssStyles { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 使用此背景的寵物數量
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// 預覽圖URL（完整尺寸）
        /// </summary>
        [StringLength(500)]
        [Url(ErrorMessage = "預覽圖URL格式不正確")]
        public string? PreviewUrl { get; set; }
    }
}
