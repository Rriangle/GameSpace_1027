using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物等級提升規則服務介面
    /// </summary>
    public interface IPetLevelUpRuleService
    {
        /// <summary>
        /// 獲取所有等級提升規則
        /// </summary>
        Task<List<PetRuleReadModel>> GetAllRulesAsync();

        /// <summary>
        /// 根據ID獲取規則
        /// </summary>
        Task<PetRuleReadModel?> GetRuleByIdAsync(int ruleId);

        /// <summary>
        /// 創建新規則
        /// </summary>
        Task<bool> CreateRuleAsync(PetRuleReadModel rule);

        /// <summary>
        /// 更新規則
        /// </summary>
        Task<bool> UpdateRuleAsync(PetRuleReadModel rule);

        /// <summary>
        /// 刪除規則
        /// </summary>
        Task<bool> DeleteRuleAsync(int ruleId);

        /// <summary>
        /// 計算寵物升級所需經驗值
        /// </summary>
        Task<int> CalculateExpRequiredAsync(int currentLevel);

        /// <summary>
        /// 驗證規則
        /// </summary>
        Task<(bool IsValid, string ErrorMessage)> ValidateRuleAsync(PetRuleReadModel rule);

        // 控制器專用方法
        /// <summary>
        /// 獲取所有升級規則（返回實體）
        /// </summary>
        Task<List<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>> GetAllLevelUpRulesAsync();

        /// <summary>
        /// 根據ID獲取升級規則（返回實體）
        /// </summary>
        Task<GameSpace.Areas.MiniGame.Models.PetLevelUpRule?> GetLevelUpRuleByIdAsync(int id);

        /// <summary>
        /// 創建升級規則
        /// </summary>
        Task<bool> CreateLevelUpRuleAsync(PetLevelUpRuleCreateViewModel model, int createdBy);

        /// <summary>
        /// 更新升級規則
        /// </summary>
        Task<bool> UpdateLevelUpRuleAsync(PetLevelUpRuleEditViewModel model, int updatedBy);

        /// <summary>
        /// 刪除升級規則
        /// </summary>
        Task<bool> DeleteLevelUpRuleAsync(int id);

        /// <summary>
        /// 切換升級規則狀態
        /// </summary>
        Task<bool> ToggleLevelUpRuleStatusAsync(int id);
    }
}

