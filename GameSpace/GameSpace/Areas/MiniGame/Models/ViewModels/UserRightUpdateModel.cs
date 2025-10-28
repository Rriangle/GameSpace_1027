using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 用戶權限更新模型
    /// </summary>
    public class UserRightUpdateModel
    {
        /// <summary>
        /// 用戶權限ID
        /// </summary>
        [Required(ErrorMessage = "用戶權限ID為必填")]
        public int UserRightId { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填")]
        public int UserId { get; set; }

        /// <summary>
        /// 權限名稱
        /// </summary>
        [Required(ErrorMessage = "權限名稱為必填")]
        [StringLength(100, ErrorMessage = "權限名稱長度不可超過 100 字元")]
        public string RightName { get; set; } = string.Empty;

        /// <summary>
        /// 權限類型
        /// </summary>
        [Required(ErrorMessage = "權限類型為必填")]
        [StringLength(50, ErrorMessage = "權限類型長度不可超過 50 字元")]
        public string RightType { get; set; } = string.Empty;

        /// <summary>
        /// 權限等級 (1-10)
        /// </summary>
        [Range(1, 10, ErrorMessage = "權限等級必須在 1-10 之間")]
        public int RightLevel { get; set; } = 1;

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        /// <summary>
        /// 到期時間
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 權限範圍
        /// </summary>
        [StringLength(200, ErrorMessage = "權限範圍長度不可超過 200 字元")]
        public string? RightScope { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
