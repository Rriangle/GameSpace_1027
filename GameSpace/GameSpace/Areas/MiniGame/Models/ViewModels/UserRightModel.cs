using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 用戶權限模型
    /// </summary>
    public class UserRightModel
    {
        /// <summary>
        /// 用戶權限ID
        /// </summary>
        public int UserRightId { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 用戶帳號
        /// </summary>
        public string UserAccount { get; set; } = string.Empty;

        /// <summary>
        /// 用戶Email
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// 權限名稱
        /// </summary>
        public string RightName { get; set; } = string.Empty;

        /// <summary>
        /// 權限類型
        /// </summary>
        public string RightType { get; set; } = string.Empty;

        /// <summary>
        /// 權限顯示名稱
        /// </summary>
        public string RightDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 權限等級
        /// </summary>
        public int RightLevel { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 是否已過期
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.Now;

        /// <summary>
        /// 到期時間
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 授予者ID
        /// </summary>
        public int? GrantedByManagerId { get; set; }

        /// <summary>
        /// 授予者名稱
        /// </summary>
        public string? GrantedByManagerName { get; set; }

        /// <summary>
        /// 權限範圍
        /// </summary>
        public string? RightScope { get; set; }
    }
}
