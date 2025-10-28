using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IPetLevelUpRuleValidationService
    {
        Task<ValidationResult> ValidateLevelUpRuleAsync(PetLevelUpRuleCreateViewModel model);
        Task<ValidationResult> ValidateLevelUpRuleUpdateAsync(PetLevelUpRuleEditViewModel model);
        Task<ValidationResult> ValidateLevelUpRuleBulkAsync(List<PetLevelUpRuleEditViewModel> models);
        Task<ValidationResult> ValidateLevelUpRuleConsistencyAsync();
        Task<ValidationResult> ValidateLevelUpRuleLogicAsync(int level, int experienceRequired, int pointsReward, int expReward);
        Task<List<string>> GetValidationErrorsAsync(PetLevelUpRuleCreateViewModel model);
        Task<List<string>> GetValidationErrorsAsync(PetLevelUpRuleEditViewModel model);
    }

    public class PetLevelUpRuleValidationService : IPetLevelUpRuleValidationService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetLevelUpRuleValidationService> _logger;

        public PetLevelUpRuleValidationService(GameSpacedatabaseContext context, ILogger<PetLevelUpRuleValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateLevelUpRuleAsync(PetLevelUpRuleCreateViewModel model)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // 基本欄位驗證
                if (model.Level <= 0)
                {
                    result.Errors.Add("等級必須大於 0");
                    result.IsValid = false;
                }

                if (model.ExperienceRequired < 0)
                {
                    result.Errors.Add("所需經驗值不能為負數");
                    result.IsValid = false;
                }

                if (model.PointsReward < 0)
                {
                    result.Errors.Add("點數獎勵不能為負數");
                    result.IsValid = false;
                }

                if (model.ExpReward < 0)
                {
                    result.Errors.Add("經驗獎勵不能為負數");
                    result.IsValid = false;
                }

                // 檢查等級是否已存在
                var existingRule = await _context.Set<PetLevelUpRule>()
                    .FirstOrDefaultAsync(x => x.Level == model.Level);
                
                if (existingRule != null)
                {
                    result.Errors.Add($"等級 {model.Level} 已存在");
                    result.IsValid = false;
                }

                // 邏輯驗證
                var logicResult = await ValidateLevelUpRuleLogicAsync(model.Level, model.ExperienceRequired, model.PointsReward, model.ExpReward);
                if (!logicResult.IsValid)
                {
                    result.Errors.AddRange(logicResult.Errors);
                    result.IsValid = false;
                }

                // 一致性檢查
                var consistencyResult = await ValidateLevelUpRuleConsistencyAsync();
                if (!consistencyResult.IsValid)
                {
                    result.Warnings.AddRange(consistencyResult.Errors);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證升級規則時發生錯誤");
                result.IsValid = false;
                result.Errors.Add("驗證過程中發生錯誤");
                return result;
            }
        }

        public async Task<ValidationResult> ValidateLevelUpRuleUpdateAsync(PetLevelUpRuleEditViewModel model)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // 基本欄位驗證
                if (model.Level <= 0)
                {
                    result.Errors.Add("等級必須大於 0");
                    result.IsValid = false;
                }

                if (model.ExperienceRequired < 0)
                {
                    result.Errors.Add("所需經驗值不能為負數");
                    result.IsValid = false;
                }

                if (model.PointsReward < 0)
                {
                    result.Errors.Add("點數獎勵不能為負數");
                    result.IsValid = false;
                }

                if (model.ExpReward < 0)
                {
                    result.Errors.Add("經驗獎勵不能為負數");
                    result.IsValid = false;
                }

                // 檢查等級是否被其他規則使用
                var existingRule = await _context.Set<PetLevelUpRule>()
                    .FirstOrDefaultAsync(x => x.Level == model.Level && x.Id != model.Id);
                
                if (existingRule != null)
                {
                    result.Errors.Add($"等級 {model.Level} 已被其他規則使用");
                    result.IsValid = false;
                }

                // 邏輯驗證
                var logicResult = await ValidateLevelUpRuleLogicAsync(model.Level, model.ExperienceRequired, model.PointsReward, model.ExpReward);
                if (!logicResult.IsValid)
                {
                    result.Errors.AddRange(logicResult.Errors);
                    result.IsValid = false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證升級規則更新時發生錯誤");
                result.IsValid = false;
                result.Errors.Add("驗證過程中發生錯誤");
                return result;
            }
        }

        public async Task<ValidationResult> ValidateLevelUpRuleBulkAsync(List<PetLevelUpRuleEditViewModel> models)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // 檢查是否有重複等級
                var levels = models.Select(m => m.Level).ToList();
                var duplicateLevels = levels.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                
                if (duplicateLevels.Any())
                {
                    result.Errors.Add($"批量更新中包含重複等級: {string.Join(", ", duplicateLevels)}");
                    result.IsValid = false;
                }

                // 逐一驗證每個規則
                foreach (var model in models)
                {
                    var modelResult = await ValidateLevelUpRuleUpdateAsync(model);
                    if (!modelResult.IsValid)
                    {
                        result.Errors.AddRange(modelResult.Errors.Select(e => $"等級 {model.Level}: {e}"));
                        result.IsValid = false;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證批量升級規則時發生錯誤");
                result.IsValid = false;
                result.Errors.Add("驗證過程中發生錯誤");
                return result;
            }
        }

        public async Task<ValidationResult> ValidateLevelUpRuleConsistencyAsync()
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                var rules = await _context.Set<PetLevelUpRule>()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Level)
                    .ToListAsync();

                if (!rules.Any())
                {
                    result.Warnings.Add("沒有啟用的升級規則");
                    return result;
                }

                // 檢查等級連續性
                for (int i = 1; i < rules.Count; i++)
                {
                    if (rules[i].Level - rules[i-1].Level > 1)
                    {
                        result.Warnings.Add($"等級 {rules[i-1].Level} 和 {rules[i].Level} 之間有間隔");
                    }
                }

                // 檢查經驗值遞增性
                for (int i = 1; i < rules.Count; i++)
                {
                    if (rules[i].RequiredExp <= rules[i-1].RequiredExp)
                    {
                        result.Errors.Add($"等級 {rules[i].Level} 的經驗值 ({rules[i].RequiredExp}) 應該大於等級 {rules[i-1].Level} 的經驗值 ({rules[i-1].RequiredExp})");
                        result.IsValid = false;
                    }
                }

                // 檢查獎勵合理性
                foreach (var rule in rules)
                {
                    if (rule.RewardPoints > rule.RequiredExp)
                    {
                        result.Warnings.Add($"等級 {rule.Level} 的點數獎勵 ({rule.RewardPoints}) 大於所需經驗值 ({rule.RequiredExp})");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證升級規則一致性時發生錯誤");
                result.IsValid = false;
                result.Errors.Add("驗證過程中發生錯誤");
                return result;
            }
        }

        public async Task<ValidationResult> ValidateLevelUpRuleLogicAsync(int level, int experienceRequired, int pointsReward, int expReward)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // 等級範圍檢查
                if (level < 1 || level > 100)
                {
                    result.Errors.Add("等級必須在 1-100 之間");
                    result.IsValid = false;
                }

                // 經驗值合理性檢查
                if (experienceRequired < level * 10)
                {
                    result.Warnings.Add($"等級 {level} 的經驗值 ({experienceRequired}) 可能過低，建議至少 {level * 10}");
                }

                if (experienceRequired > level * 1000)
                {
                    result.Warnings.Add($"等級 {level} 的經驗值 ({experienceRequired}) 可能過高，建議最多 {level * 1000}");
                }

                // 獎勵合理性檢查
                if (pointsReward > experienceRequired * 0.1)
                {
                    result.Warnings.Add($"等級 {level} 的點數獎勵 ({pointsReward}) 可能過高，建議不超過經驗值的 10%");
                }

                if (expReward > experienceRequired * 0.05)
                {
                    result.Warnings.Add($"等級 {level} 的經驗獎勵 ({expReward}) 可能過高，建議不超過經驗值的 5%");
                }

                // 檢查與現有規則的關係
                var existingRules = await _context.Set<PetLevelUpRule>()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Level)
                    .ToListAsync();

                if (existingRules.Any())
                {
                    var lowerRules = existingRules.Where(x => x.Level < level).OrderByDescending(x => x.Level).ToList();
                    var higherRules = existingRules.Where(x => x.Level > level).OrderBy(x => x.Level).ToList();

                    // 檢查與較低等級的關係
                    if (lowerRules.Any())
                    {
                        var maxLowerExp = lowerRules.First().RequiredExp;
                        if (experienceRequired <= maxLowerExp)
                        {
                            result.Errors.Add($"等級 {level} 的經驗值 ({experienceRequired}) 應該大於較低等級的最大經驗值 ({maxLowerExp})");
                            result.IsValid = false;
                        }
                    }

                    // 檢查與較高等級的關係
                    if (higherRules.Any())
                    {
                        var minHigherExp = higherRules.First().RequiredExp;
                        if (experienceRequired >= minHigherExp)
                        {
                            result.Errors.Add($"等級 {level} 的經驗值 ({experienceRequired}) 應該小於較高等級的最小經驗值 ({minHigherExp})");
                            result.IsValid = false;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證升級規則邏輯時發生錯誤");
                result.IsValid = false;
                result.Errors.Add("驗證過程中發生錯誤");
                return result;
            }
        }

        public async Task<List<string>> GetValidationErrorsAsync(PetLevelUpRuleCreateViewModel model)
        {
            var result = await ValidateLevelUpRuleAsync(model);
            return result.Errors;
        }

        public async Task<List<string>> GetValidationErrorsAsync(PetLevelUpRuleEditViewModel model)
        {
            var result = await ValidateLevelUpRuleUpdateAsync(model);
            return result.Errors;
        }
    }
}

