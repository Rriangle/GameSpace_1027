namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 權限類型資訊
    /// </summary>
    public class RightTypeInfo
    {
        /// <summary>
        /// 權限類型ID
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// 權限類型名稱
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 權限類型代碼
        /// </summary>
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 分類 (如: System, User, Content)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 預設權限等級
        /// </summary>
        public int DefaultLevel { get; set; } = 1;

        /// <summary>
        /// 最大權限等級
        /// </summary>
        public int MaxLevel { get; set; } = 10;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 使用此權限的用戶數
        /// </summary>
        public int UserCount { get; set; }

        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 圖示類別 (Font Awesome class)
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 是否可設定過期時間
        /// </summary>
        public bool CanExpire { get; set; } = true;

        /// <summary>
        /// 是否可設定範圍
        /// </summary>
        public bool CanScope { get; set; } = false;
    }
}
