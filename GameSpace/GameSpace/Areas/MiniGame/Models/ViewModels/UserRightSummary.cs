namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 用戶權限摘要
    /// </summary>
    public class UserRightSummary
    {
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
        /// 總權限數
        /// </summary>
        public int TotalRights { get; set; }

        /// <summary>
        /// 最後權限更新時間
        /// </summary>
        public DateTime? LastRightUpdate { get; set; }

        /// <summary>
        /// 啟用中的權限數
        /// </summary>
        public int ActiveRights { get; set; }

        /// <summary>
        /// 已過期的權限數
        /// </summary>
        public int ExpiredRights { get; set; }

        /// <summary>
        /// 最高權限等級
        /// </summary>
        public int HighestRightLevel { get; set; }

        /// <summary>
        /// 權限類型列表
        /// </summary>
        public List<string> RightTypes { get; set; } = new();

        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// 即將過期的權限數（30天內）
        /// </summary>
        public int ExpiringRights { get; set; }
    }
}
