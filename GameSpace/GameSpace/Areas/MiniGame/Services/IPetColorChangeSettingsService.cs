using GameSpace.Areas.MiniGame.Models.Settings;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色點數設定服務介面
    /// </summary>
    public interface IPetColorChangeSettingsService
    {
        /// <summary>
        /// 取得所有寵物換色點數設定
        /// </summary>
        /// <returns>寵物換色點數設定清單</returns>
        Task<IEnumerable<PetColorChangeSettings>> GetAllSettingsAsync();

        /// <summary>
        /// 根據ID取得寵物換色點數設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>寵物換色點數設定</returns>
        Task<PetColorChangeSettings?> GetSettingByIdAsync(int id);

        /// <summary>
        /// 新增寵物換色點數設定
        /// </summary>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>新增的設定</returns>
        Task<PetColorChangeSettings> CreateSettingAsync(PetColorChangeSettings setting);

        /// <summary>
        /// 更新寵物換色點數設定
        /// </summary>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>更新後的設定</returns>
        Task<PetColorChangeSettings> UpdateSettingAsync(PetColorChangeSettings setting);

        /// <summary>
        /// 刪除寵物換色點數設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeleteSettingAsync(int id);

        /// <summary>
        /// 取得目前啟用的換色點數設定
        /// </summary>
        /// <returns>啟用的換色點數設定</returns>
        Task<PetColorChangeSettings?> GetActiveSettingAsync();

        /// <summary>
        /// 切換設定的啟用狀態
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否切換成功</returns>
        Task<bool> ToggleActiveAsync(int id);

        /// <summary>
        /// 取得所有寵物換色點數設定 (Alias for GetAllSettingsAsync)
        /// </summary>
        /// <returns>寵物換色點數設定清單</returns>
        Task<IEnumerable<PetColorChangeSettings>> GetAllAsync();

        /// <summary>
        /// 根據ID取得寵物換色點數設定 (Alias for GetSettingByIdAsync)
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>寵物換色點數設定</returns>
        Task<PetColorChangeSettings?> GetByIdAsync(int id);

        /// <summary>
        /// 新增寵物換色點數設定 (Alias for CreateSettingAsync)
        /// </summary>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>新增的設定</returns>
        Task<PetColorChangeSettings> CreateAsync(PetColorChangeSettings setting);

        /// <summary>
        /// 更新寵物換色點數設定 (Overload with ID parameter)
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>更新後的設定</returns>
        Task<PetColorChangeSettings> UpdateAsync(int id, PetColorChangeSettings setting);

        /// <summary>
        /// 刪除寵物換色點數設定 (Alias for DeleteSettingAsync)
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeleteAsync(int id);
    }
}

