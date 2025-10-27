using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物膚色成本設定服務介面
    /// </summary>
    public interface IPetSkinColorCostSettingService
    {
        // 基本 CRUD
        Task<IEnumerable<PetSkinColorCostSetting>> GetAllAsync();
        Task<PetSkinColorCostSetting?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PetSkinColorCostSetting setting);
        Task<bool> UpdateAsync(PetSkinColorCostSetting setting);
        Task<bool> DeleteAsync(int id);

        // 查詢功能
        Task<IEnumerable<PetSkinColorCostSetting>> GetActiveSettingsAsync();
        Task<PetSkinColorCostSetting?> GetByColorCodeAsync(string colorCode);
        Task<PetSkinColorCostSetting?> GetByColorNameAsync(string colorName);
        Task<int> GetCostByColorCodeAsync(string colorCode);
        Task<bool> ExistsByColorCodeAsync(string colorCode);

        // 排序與分頁
        Task<IEnumerable<PetSkinColorCostSetting>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();

        // 狀態管理
        Task<bool> ToggleActiveStatusAsync(int id);
        Task<bool> SetActiveStatusAsync(int id, bool isActive);
        Task<bool> ToggleActiveAsync(int settingId);

        // 批次操作
        Task<bool> UpdateMultipleCostsAsync(Dictionary<int, int> costMapping);
    }
}
