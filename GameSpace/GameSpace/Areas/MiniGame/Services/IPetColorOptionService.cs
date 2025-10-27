using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物顏色選項服務介面
    /// </summary>
    public interface IPetColorOptionService
    {
        // 基本 CRUD (Entity-based)
        Task<IEnumerable<PetColorOption>> GetAllAsync();
        Task<PetColorOption?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PetColorOption option);
        Task<bool> UpdateAsync(PetColorOption option);
        Task<bool> DeleteAsync(int id);

        // 查詢功能 (Entity-based)
        Task<IEnumerable<PetColorOption>> GetActiveOptionsAsync();
        Task<IEnumerable<PetColorOption>> GetByColorCodeAsync(string colorCode);
        Task<PetColorOption?> GetByColorNameAsync(string colorName);
        Task<bool> ExistsByColorCodeAsync(string colorCode);
        Task<bool> ExistsByColorNameAsync(string colorName);

        // 排序與分頁 (Entity-based)
        Task<IEnumerable<PetColorOption>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();

        // 狀態管理 (Entity-based)
        Task<bool> ToggleActiveStatusAsync(int id);
        Task<bool> SetActiveStatusAsync(int id, bool isActive);

        // 排序管理 (Entity-based)
        Task<bool> UpdateSortOrderAsync(int id, int newSortOrder);
        Task<bool> ReorderOptionsAsync(Dictionary<int, int> orderMapping);

        // ViewModel-based 方法
        Task<IEnumerable<PetColorOptionViewModel>> GetAllColorOptionsAsync();
        Task<IEnumerable<PetColorOptionViewModel>> GetActiveColorOptionsAsync();
        Task<PetColorOptionViewModel?> GetColorOptionByIdAsync(int id);
        Task<bool> CreateColorOptionAsync(PetColorOptionViewModel viewModel, int managerId);
        Task<bool> UpdateColorOptionAsync(PetColorOptionViewModel viewModel, int managerId);
        Task<bool> DeleteColorOptionAsync(int id);
        Task<bool> ToggleColorOptionStatusAsync(int id, int managerId);
        Task<IEnumerable<PetColorOptionViewModel>> SearchColorOptionsAsync(string keyword, bool includeInactive);
        Task<object> GetColorOptionStatisticsAsync();
    }
}
