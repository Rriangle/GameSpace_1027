using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 錯誤日誌記錄
    /// </summary>
    [Table("ErrorLogs")]
    public class ErrorLog
    {
        /// <summary>
        /// 日誌ID
        /// </summary>
        [Key]
        public int LogId { get; set; }

        /// <summary>
        /// 錯誤等級 (Critical, Error, Warning, Information)
        /// </summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "錯誤等級")]
        public string Level { get; set; } = "Error";

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        [Required]
        [StringLength(1000)]
        [Display(Name = "錯誤訊息")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 例外詳細資訊
        /// </summary>
        [Display(Name = "例外詳細資訊")]
        public string? Exception { get; set; }

        /// <summary>
        /// 錯誤來源
        /// </summary>
        [StringLength(255)]
        [Display(Name = "錯誤來源")]
        public string? Source { get; set; }

        /// <summary>
        /// 發生時間
        /// </summary>
        [Required]
        [Display(Name = "發生時間")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 使用者ID (可選)
        /// </summary>
        [Display(Name = "使用者ID")]
        public int? UserId { get; set; }

        /// <summary>
        /// 請求路徑
        /// </summary>
        [StringLength(500)]
        [Display(Name = "請求路徑")]
        public string? RequestPath { get; set; }

        /// <summary>
        /// IP位址
        /// </summary>
        [StringLength(50)]
        [Display(Name = "IP位址")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 使用者代理字串
        /// </summary>
        [StringLength(500)]
        [Display(Name = "使用者代理")]
        public string? UserAgent { get; set; }
    }
}
