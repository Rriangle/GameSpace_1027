using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景選項服務介面
    /// </summary>
    public interface IPetBackgroundOptionService
    {
        // 基本 CRUD (Entity-based)
        Task<IEnumerable<PetBackgroundOptionEntity>> GetAllAsync();
        Task<PetBackgroundOptionEntity?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PetBackgroundOptionEntity option);
        Task<bool> UpdateAsync(PetBackgroundOptionEntity option);
        Task<bool> DeleteAsync(int id);

        // 查詢功能 (Entity-based)
        Task<IEnumerable<PetBackgroundOptionEntity>> GetActiveOptionsAsync();
        Task<IEnumerable<PetBackgroundOptionEntity>> GetByBackgroundCodeAsync(string backgroundCode);
        Task<PetBackgroundOptionEntity?> GetByBackgroundNameAsync(string backgroundName);
        Task<bool> ExistsByBackgroundCodeAsync(string backgroundCode);
        Task<bool> ExistsByBackgroundNameAsync(string backgroundName);

        // 排序與分頁 (Entity-based)
        Task<IEnumerable<PetBackgroundOptionEntity>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();

        // 狀態管理 (Entity-based)
        Task<bool> ToggleActiveStatusAsync(int id);
        Task<bool> SetActiveStatusAsync(int id, bool isActive);

        // 排序管理 (Entity-based)
        Task<bool> UpdateSortOrderAsync(int id, int newSortOrder);
        Task<bool> ReorderOptionsAsync(Dictionary<int, int> orderMapping);

        // ViewModel-based 方法
        Task<IEnumerable<PetBackgroundOptionViewModel>> GetAllBackgroundOptionsAsync();
        Task<IEnumerable<PetBackgroundOptionViewModel>> GetActiveBackgroundOptionsAsync();
        Task<PetBackgroundOptionViewModel?> GetBackgroundOptionByIdAsync(int id);
        Task<bool> CreateBackgroundOptionAsync(PetBackgroundOptionViewModel viewModel, int managerId);
        Task<bool> UpdateBackgroundOptionAsync(PetBackgroundOptionViewModel viewModel, int managerId);
        Task<bool> DeleteBackgroundOptionAsync(int id);
        Task<bool> ToggleBackgroundOptionStatusAsync(int id, int managerId);
        Task<IEnumerable<PetBackgroundOptionViewModel>> SearchBackgroundOptionsAsync(string keyword, bool includeInactive);
        Task<object> GetBackgroundOptionStatisticsAsync();
    }
}
