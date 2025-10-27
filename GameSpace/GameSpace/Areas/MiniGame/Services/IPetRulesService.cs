using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IPetRulesService
    {
        // Pet 升級規則
        Task<IEnumerable<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>> GetAllLevelUpRulesAsync();
        Task<GameSpace.Areas.MiniGame.Models.PetLevelUpRule?> GetLevelUpRuleByLevelAsync(int level);
        Task<bool> CreateLevelUpRuleAsync(GameSpace.Areas.MiniGame.Models.PetLevelUpRule rule);
        Task<bool> UpdateLevelUpRuleAsync(GameSpace.Areas.MiniGame.Models.PetLevelUpRule rule);
        Task<bool> DeleteLevelUpRuleAsync(int ruleId);
        Task<int> GetRequiredExpForLevelAsync(int level);

        // Pet 膚色變更規則
        Task<IEnumerable<PetSkinColorPointSettings>> GetAllSkinColorSettingsAsync();
        Task<PetSkinColorPointSettings?> GetSkinColorSettingByLevelAsync(int level);
        Task<bool> UpdateSkinColorSettingAsync(PetSkinColorPointSettings setting);
        Task<int> GetSkinColorCostForLevelAsync(int level);

        // Pet 背景變更規則
        Task<IEnumerable<PetBackgroundPointSettings>> GetAllBackgroundSettingsAsync();
        Task<PetBackgroundPointSettings?> GetBackgroundSettingByLevelAsync(int level);
        Task<bool> UpdateBackgroundSettingAsync(PetBackgroundPointSettings setting);
        Task<int> GetBackgroundCostForLevelAsync(int level);

        // Pet 互動獎勵規則
        Task<IEnumerable<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>> GetAllInteractionRulesAsync();
        Task<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules?> GetInteractionRuleByTypeAsync(string interactionType);
        Task<bool> CreateInteractionRuleAsync(GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules rule);
        Task<bool> UpdateInteractionRuleAsync(GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules rule);
        Task<bool> DeleteInteractionRuleAsync(int ruleId);
        Task<bool> ToggleInteractionRuleAsync(int ruleId);

        // Pet 顏色選項管理
        Task<IEnumerable<PetColorOption>> GetAllColorOptionsAsync();
        Task<IEnumerable<PetColorOption>> GetActiveColorOptionsAsync();
        Task<bool> CreateColorOptionAsync(PetColorOption option);
        Task<bool> UpdateColorOptionAsync(PetColorOption option);
        Task<bool> DeleteColorOptionAsync(int optionId);
        Task<bool> ToggleColorOptionAsync(int optionId);
    }
}

