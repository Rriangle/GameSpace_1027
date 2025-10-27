using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 每日遊戲次數限制設定
    /// </summary>
    [Table("DailyGameLimits")]
    public class DailyGameLimit
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 每日遊戲次數限制
        /// </summary>
        [Required(ErrorMessage = "每日遊戲次數限制為必填欄位")]
        [Range(1, 100, ErrorMessage = "每日遊戲次數限制必須在1-100之間")]
        [Display(Name = "每日遊戲次數限制")]
        public int DailyLimit { get; set; } = 3;

        /// <summary>
        /// 設定名稱
        /// </summary>
        [Required(ErrorMessage = "設定名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "設定名稱長度不能超過100個字元")]
        [Display(Name = "設定名稱")]
        public string SettingName { get; set; } = "每日遊戲次數限制";

        /// <summary>
        /// 設定描述
        /// </summary>
        [StringLength(500, ErrorMessage = "設定描述長度不能超過500個字元")]
        [Display(Name = "設定描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 建立者
        /// </summary>
        [StringLength(50)]
        [Display(Name = "建立者")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [StringLength(50)]
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }
    }
}

