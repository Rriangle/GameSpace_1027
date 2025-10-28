using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景成本設定服務介面
    /// </summary>
    public interface IPetBackgroundCostSettingService
    {
        // 基本 CRUD
        Task<IEnumerable<PetBackgroundCostSetting>> GetAllAsync();
        Task<PetBackgroundCostSetting?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PetBackgroundCostSetting setting);
        Task<bool> UpdateAsync(PetBackgroundCostSetting setting);
        Task<bool> DeleteAsync(int id);

        // 查詢功能
        Task<IEnumerable<PetBackgroundCostSetting>> GetActiveSettingsAsync();
        Task<PetBackgroundCostSetting?> GetByBackgroundCodeAsync(string backgroundCode);
        Task<PetBackgroundCostSetting?> GetByBackgroundNameAsync(string backgroundName);
        Task<int> GetCostByBackgroundCodeAsync(string backgroundCode);
        Task<bool> ExistsByBackgroundCodeAsync(string backgroundCode);

        // 排序與分頁
        Task<IEnumerable<PetBackgroundCostSetting>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();

        // 狀態管理
        Task<bool> ToggleActiveStatusAsync(int id);
        Task<bool> SetActiveStatusAsync(int id, bool isActive);
        Task<bool> ToggleActiveAsync(int settingId);

        // 批次操作
        Task<bool> UpdateMultipleCostsAsync(Dictionary<int, int> costMapping);
    }
}
