using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 每日遊戲次數限制設定服務介面
    /// </summary>
    public interface IDailyGameLimitService
    {
        /// <summary>
        /// 取得所有每日遊戲次數限制設定
        /// </summary>
        /// <param name="searchModel">搜尋條件</param>
        /// <returns>每日遊戲次數限制設定列表</returns>
        Task<(List<DailyGameLimitListViewModel> Items, int TotalCount)> GetAllAsync(DailyGameLimitSearchViewModel searchModel);

        /// <summary>
        /// 根據ID取得每日遊戲次數限制設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>每日遊戲次數限制設定</returns>
        Task<Models.DailyGameLimit?> GetByIdAsync(int id);

        /// <summary>
        /// 取得目前的每日遊戲次數限制設定
        /// </summary>
        /// <returns>每日遊戲次數限制設定</returns>
        Task<Models.DailyGameLimit?> GetCurrentSettingAsync();

        /// <summary>
        /// 建立每日遊戲次數限制設定
        /// </summary>
        /// <param name="model">建立模型</param>
        /// <param name="createdBy">建立者</param>
        /// <returns>是否成功</returns>
        Task<bool> CreateAsync(DailyGameLimitCreateViewModel model, string? createdBy = null);

        /// <summary>
        /// 更新每日遊戲次數限制設定
        /// </summary>
        /// <param name="model">編輯模型</param>
        /// <param name="updatedBy">更新者</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateAsync(DailyGameLimitEditViewModel model, string? updatedBy = null);

        /// <summary>
        /// 刪除每日遊戲次數限制設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <param name="updatedBy">更新者</param>
        /// <returns>是否成功</returns>
        Task<bool> ToggleStatusAsync(int id, string? updatedBy = null);

        /// <summary>
        /// 取得統計資料
        /// </summary>
        /// <returns>統計資料</returns>
        Task<DailyGameLimitStatisticsViewModel> GetStatisticsAsync();

        /// <summary>
        /// 檢查使用者今日遊戲次數是否超過限制
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>是否超過限制</returns>
        Task<bool> IsDailyLimitExceededAsync(int userId);

        /// <summary>
        /// 取得使用者今日已玩遊戲次數
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>已玩遊戲次數</returns>
        Task<int> GetTodayGameCountAsync(int userId);
    }

    /// <summary>
    /// 每日遊戲次數限制設定統計資料模型
    /// </summary>
    public class DailyGameLimitStatisticsViewModel
    {
        public int TotalSettings { get; set; }
        public int EnabledSettings { get; set; }
        public int DisabledSettings { get; set; }
        public int CurrentDailyLimit { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

