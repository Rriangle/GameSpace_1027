using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class PetRulesService : IPetRulesService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetRulesService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // Pet 升級規則
        public async Task<IEnumerable<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>> GetAllLevelUpRulesAsync()
        {
            return await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                .OrderBy(r => r.Level)
                .ToListAsync();
        }

        public async Task<GameSpace.Areas.MiniGame.Models.PetLevelUpRule?> GetLevelUpRuleByLevelAsync(int level)
        {
            return await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                .FirstOrDefaultAsync(r => r.Level == level);
        }

        public async Task<bool> CreateLevelUpRuleAsync(GameSpace.Areas.MiniGame.Models.PetLevelUpRule rule)
        {
            try
            {
                rule.IsActive = true;
                _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>().Add(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateLevelUpRuleAsync(GameSpace.Areas.MiniGame.Models.PetLevelUpRule rule)
        {
            try
            {
                _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>().Update(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteLevelUpRuleAsync(int RuleId)
        {
            try
            {
                var rule = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .FirstOrDefaultAsync(r => r.Id == RuleId);
                if (rule == null) return false;

                _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>().Remove(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetRequiredExpForLevelAsync(int level)
        {
            var rule = await GetLevelUpRuleByLevelAsync(level);
            if (rule != null)
            {
                return rule.RequiredExp;
            }

            // 備用公式：使用與 PetService 相同的三段式公式
            // Level 1-10: 40 * level + 60
            // Level 11-100: 0.8 * level² + 380
            // Level 101+: 285.69 * 1.06^level
            if (level <= 1) return 0;
            if (level <= 10) return 40 * level + 60;
            else if (level <= 100) return (int)(0.8 * level * level + 380);
            else return (int)(285.69 * Math.Pow(1.06, level));
        }

        // Pet 膚色變更規則
        public async Task<IEnumerable<PetSkinColorPointSettings>> GetAllSkinColorSettingsAsync()
        {
            return await _context.Set<PetSkinColorPointSettings>()
                .OrderBy(s => s.RequiredLevel)
                .ToListAsync();
        }

        public async Task<PetSkinColorPointSettings?> GetSkinColorSettingByLevelAsync(int level)
        {
            return await _context.Set<PetSkinColorPointSettings>()
                .FirstOrDefaultAsync(s => s.RequiredLevel == level);
        }

        public async Task<bool> UpdateSkinColorSettingAsync(PetSkinColorPointSettings setting)
        {
            try
            {
                _context.Set<PetSkinColorPointSettings>().Update(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetSkinColorCostForLevelAsync(int level)
        {
            var setting = await GetSkinColorSettingByLevelAsync(level);
            return setting?.PointCost ?? 50; // 預設 50 點數
        }

        // Pet 背景變更規則
        public async Task<IEnumerable<PetBackgroundPointSettings>> GetAllBackgroundSettingsAsync()
        {
            return await _context.Set<PetBackgroundPointSettings>()
                .OrderBy(s => s.RequiredLevel)
                .ToListAsync();
        }

        public async Task<PetBackgroundPointSettings?> GetBackgroundSettingByLevelAsync(int level)
        {
            return await _context.Set<PetBackgroundPointSettings>()
                .FirstOrDefaultAsync(s => s.RequiredLevel == level);
        }

        public async Task<bool> UpdateBackgroundSettingAsync(PetBackgroundPointSettings setting)
        {
            try
            {
                _context.Set<PetBackgroundPointSettings>().Update(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetBackgroundCostForLevelAsync(int level)
        {
            var setting = await GetBackgroundSettingByLevelAsync(level);
            return setting?.PointCost ?? 30; // 預設 30 點數
        }

        // Pet 互動獎勵規則
        public async Task<IEnumerable<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>> GetAllInteractionRulesAsync()
        {
            return await _context.Set<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>()
                .OrderBy(r => r.InteractionType)
                .ToListAsync();
        }

        public async Task<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules?> GetInteractionRuleByTypeAsync(string interactionType)
        {
            return await _context.Set<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>()
                .FirstOrDefaultAsync(r => r.InteractionType == interactionType);
        }

        public async Task<bool> CreateInteractionRuleAsync(GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules rule)
        {
            try
            {
                rule.IsActive = true;
                _context.Set<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>().Add(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateInteractionRuleAsync(GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules rule)
        {
            try
            {
                _context.Set<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>().Update(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteInteractionRuleAsync(int RuleId)
        {
            try
            {
                var rule = await _context.Set<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>()
                    .FirstOrDefaultAsync(r => r.RuleId == RuleId);
                if (rule == null) return false;

                _context.Set<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>().Remove(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleInteractionRuleAsync(int RuleId)
        {
            try
            {
                var rule = await _context.Set<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules>()
                    .FirstOrDefaultAsync(r => r.RuleId == RuleId);
                if (rule == null) return false;

                rule.IsActive = !rule.IsActive;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Pet 顏色選項管理
        public async Task<IEnumerable<PetColorOption>> GetAllColorOptionsAsync()
        {
            return await _context.Set<PetColorOption>()
                .OrderBy(o => o.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<PetColorOption>> GetActiveColorOptionsAsync()
        {
            return await _context.Set<PetColorOption>()
                .Where(o => o.IsActive)
                .OrderBy(o => o.SortOrder)
                .ToListAsync();
        }

        public async Task<bool> CreateColorOptionAsync(PetColorOption option)
        {
            try
            {
                option.IsActive = true;
                _context.Set<PetColorOption>().Add(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateColorOptionAsync(PetColorOption option)
        {
            try
            {
                _context.Set<PetColorOption>().Update(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteColorOptionAsync(int optionId)
        {
            try
            {
                var option = await _context.Set<PetColorOption>()
                    .FirstOrDefaultAsync(o => o.ColorOptionId == optionId);
                if (option == null) return false;

                _context.Set<PetColorOption>().Remove(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleColorOptionAsync(int optionId)
        {
            try
            {
                var option = await _context.Set<PetColorOption>()
                    .FirstOrDefaultAsync(o => o.ColorOptionId == optionId);
                if (option == null) return false;

                option.IsActive = !option.IsActive;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}


