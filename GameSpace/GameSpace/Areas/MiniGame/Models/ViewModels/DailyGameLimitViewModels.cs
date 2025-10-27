using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 每日遊戲次數限制設定檢視模型
    /// </summary>
    public class DailyGameLimitViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "每日遊戲次數限制為必填欄位")]
        [Range(1, 100, ErrorMessage = "每日遊戲次數限制必須在1-100之間")]
        [Display(Name = "每日遊戲次數限制")]
        public int DailyLimit { get; set; } = 3;

        [Required(ErrorMessage = "設定名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "設定名稱長度不能超過100個字元")]
        [Display(Name = "設定名稱")]
        public string SettingName { get; set; } = "每日遊戲次數限制";

        [StringLength(500, ErrorMessage = "設定描述長度不能超過500個字元")]
        [Display(Name = "設定描述")]
        public string? Description { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "建立者")]
        public string? CreatedBy { get; set; }

        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// 每日遊戲次數限制設定建立模型
    /// </summary>
    public class DailyGameLimitCreateViewModel
    {
        [Required(ErrorMessage = "每日遊戲次數限制為必填欄位")]
        [Range(1, 100, ErrorMessage = "每日遊戲次數限制必須在1-100之間")]
        [Display(Name = "每日遊戲次數限制")]
        public int DailyLimit { get; set; } = 3;

        [Required(ErrorMessage = "設定名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "設定名稱長度不能超過100個字元")]
        [Display(Name = "設定名稱")]
        public string SettingName { get; set; } = "每日遊戲次數限制";

        [StringLength(500, ErrorMessage = "設定描述長度不能超過500個字元")]
        [Display(Name = "設定描述")]
        public string? Description { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 每日遊戲次數限制設定編輯模型
    /// </summary>
    public class DailyGameLimitEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "每日遊戲次數限制為必填欄位")]
        [Range(1, 100, ErrorMessage = "每日遊戲次數限制必須在1-100之間")]
        [Display(Name = "每日遊戲次數限制")]
        public int DailyLimit { get; set; } = 3;

        [Required(ErrorMessage = "設定名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "設定名稱長度不能超過100個字元")]
        [Display(Name = "設定名稱")]
        public string SettingName { get; set; } = "每日遊戲次數限制";

        [StringLength(500, ErrorMessage = "設定描述長度不能超過500個字元")]
        [Display(Name = "設定描述")]
        public string? Description { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 每日遊戲次數限制設定搜尋模型
    /// </summary>
    public class DailyGameLimitSearchViewModel
    {
        [Display(Name = "設定名稱")]
        public string? SettingName { get; set; }

        [Display(Name = "是否啟用")]
        public bool? IsEnabled { get; set; }

        [Display(Name = "頁碼")]
        public int Page { get; set; } = 1;

        [Display(Name = "每頁筆數")]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 每日遊戲次數限制設定列表模型
    /// </summary>
    public class DailyGameLimitListViewModel
    {
        public int Id { get; set; }
        public int DailyLimit { get; set; }
        public string SettingName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// 每日遊戲次數限制設定詳細模型
    /// </summary>
    public class DailyGameLimitDetailsViewModel
    {
        public int Id { get; set; }
        public int DailyLimit { get; set; }
        public string SettingName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// 每日遊戲次數限制設定刪除模型
    /// </summary>
    public class DailyGameLimitDeleteViewModel
    {
        public int Id { get; set; }
        public int DailyLimit { get; set; }
        public string SettingName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// 每日遊戲次數限制設定統計模型
    /// </summary>
    public class DailyGameLimitStatisticsViewModel
    {
        public int TotalSettings { get; set; }
        public int EnabledSettings { get; set; }
        public int DisabledSettings { get; set; }
        public int AverageDailyLimit { get; set; }
        public int MinDailyLimit { get; set; }
        public int MaxDailyLimit { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

