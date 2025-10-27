using GameSpace.Areas.MiniGame.Models.Settings;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景變更設定服務介面
    /// </summary>
    public interface IPetBackgroundChangeSettingsService
    {
        /// <summary>
        /// 獲取背景變更設定
        /// </summary>
        Task<PetSettings?> GetSettingsAsync();

        /// <summary>
        /// 更新背景變更設定
        /// </summary>
        Task<bool> UpdateSettingsAsync(PetSettings settings);

        /// <summary>
        /// 獲取背景變更所需點數
        /// </summary>
        Task<int> GetBackgroundChangePointCostAsync();

        /// <summary>
        /// 驗證用戶是否有足夠點數變更背景
        /// </summary>
        Task<bool> CanUserChangeBackgroundAsync(int userId);

        /// <summary>
        /// 獲取所有背景變更設定
        /// </summary>
        Task<IEnumerable<PetBackgroundChangeSettings>> GetAllAsync();

        /// <summary>
        /// 根據ID獲取背景變更設定
        /// </summary>
        Task<PetBackgroundChangeSettings?> GetByIdAsync(int id);

        /// <summary>
        /// 建立背景變更設定
        /// </summary>
        Task<PetBackgroundChangeSettings> CreateAsync(PetBackgroundChangeSettings settings);

        /// <summary>
        /// 更新背景變更設定
        /// </summary>
        Task<PetBackgroundChangeSettings> UpdateAsync(int id, PetBackgroundChangeSettings settings);

        /// <summary>
        /// 刪除背景變更設定
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 切換背景變更設定啟用狀態
        /// </summary>
        Task<bool> ToggleActiveAsync(int id);
    }
}

