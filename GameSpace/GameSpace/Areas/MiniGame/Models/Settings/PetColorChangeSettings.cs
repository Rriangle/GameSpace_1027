using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物換色點數設定模型
    /// </summary>
    public class PetColorChangeSettings
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        [Key]
        public int SettingId { get; set; }

        /// <summary>
        /// ID - Alias for SettingId for view compatibility
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int Id
        {
            get => SettingId;
            set => SettingId = value;
        }

        /// <summary>
        /// 顏色名稱 - Alias for SettingName for view compatibility
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string ColorName
        {
            get => SettingName;
            set => SettingName = value;
        }

        /// <summary>
        /// 所需點數 - Alias for PointsRequired for view compatibility
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int RequiredPoints
        {
            get => PointsRequired;
            set => PointsRequired = value;
        }

        /// <summary>
        /// 設定名稱
        /// </summary>
        [Required(ErrorMessage = "設定名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "設定名稱長度不能超過100個字元")]
        public string SettingName { get; set; } = string.Empty;

        /// <summary>
        /// 顏色代碼 - for view compatibility
        /// </summary>
        [StringLength(7)]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 換色所需點數
        /// </summary>
        [Required(ErrorMessage = "換色所需點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "換色所需點數必須大於等於0")]
        public int PointsRequired { get; set; } = 2000;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

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
    }
}

