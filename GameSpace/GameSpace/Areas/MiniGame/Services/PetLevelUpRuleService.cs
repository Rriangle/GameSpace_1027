using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物等級提升規則服務實作
    /// </summary>
    public class PetLevelUpRuleService : IPetLevelUpRuleService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetLevelUpRuleService> _logger;

        public PetLevelUpRuleService(GameSpacedatabaseContext context, ILogger<PetLevelUpRuleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region IPetLevelUpRuleService 介面方法實作

        public async Task<List<PetRuleReadModel>> GetAllRulesAsync()
        {
            try
            {
                var rules = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .OrderBy(r => r.Level)
                    .ToListAsync();

                return rules.Select(MapToReadModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有寵物等級規則時發生錯誤");
                return new List<PetRuleReadModel>();
            }
        }

        public async Task<PetRuleReadModel?> GetRuleByIdAsync(int ruleId)
        {
            try
            {
                var rule = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .FirstOrDefaultAsync(r => r.Id == ruleId);

                return rule != null ? MapToReadModel(rule) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取寵物等級規則 {RuleId} 時發生錯誤", ruleId);
                return null;
            }
        }

        public async Task<bool> CreateRuleAsync(PetRuleReadModel rule)
        {
            try
            {
                var entity = new GameSpace.Areas.MiniGame.Models.PetLevelUpRule
                {
                    Level = rule.RuleId, // 假設 RuleId 對應到 Level
                    RequiredExp = rule.LevelUpExpRequired,
                    RewardPoints = rule.ColorChangePointCost, // 根據實際需求調整
                    IsActive = rule.IsActive
                };

                _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>().Add(entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建寵物等級規則時發生錯誤");
                return false;
            }
        }

        public async Task<bool> UpdateRuleAsync(PetRuleReadModel rule)
        {
            try
            {
                var entity = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .FirstOrDefaultAsync(r => r.Id == rule.RuleId);

                if (entity == null)
                {
                    _logger.LogWarning("找不到 ID 為 {RuleId} 的寵物等級規則", rule.RuleId);
                    return false;
                }

                entity.RequiredExp = rule.LevelUpExpRequired;
                entity.RewardPoints = rule.ColorChangePointCost;
                entity.IsActive = rule.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物等級規則時發生錯誤");
                return false;
            }
        }

        public async Task<bool> DeleteRuleAsync(int ruleId)
        {
            try
            {
                var entity = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .FirstOrDefaultAsync(r => r.Id == ruleId);

                if (entity == null)
                {
                    _logger.LogWarning("找不到 ID 為 {RuleId} 的寵物等級規則", ruleId);
                    return false;
                }

                _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>().Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物等級規則 {RuleId} 時發生錯誤", ruleId);
                return false;
            }
        }

        public async Task<int> CalculateExpRequiredAsync(int currentLevel)
        {
            try
            {
                // 首先查找資料庫中的規則
                var rule = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .Where(r => r.Level == currentLevel + 1 && r.IsActive)
                    .Select(r => r.RequiredExp)
                    .FirstOrDefaultAsync();

                if (rule > 0)
                {
                    return rule;
                }

                // 如果資料庫中沒有，使用與 PetService 相同的三段式公式
                // Level 1-10: 40 * level + 60
                // Level 11-100: 0.8 * level² + 380
                // Level 101+: 285.69 * 1.06^level
                var nextLevel = currentLevel + 1;
                if (nextLevel <= 1) return 0;
                if (nextLevel <= 10) return 40 * nextLevel + 60;
                else if (nextLevel <= 100) return (int)(0.8 * nextLevel * nextLevel + 380);
                else return (int)(285.69 * Math.Pow(1.06, nextLevel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算等級 {Level} 所需經驗值時發生錯誤", currentLevel);
                return 0;
            }
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateRuleAsync(PetRuleReadModel rule)
        {
            try
            {
                if (rule.LevelUpExpRequired <= 0)
                    return (false, "升級所需經驗值必須大於0");

                if (rule.MaxLevel <= 0 || rule.MaxLevel > 100)
                    return (false, "最高等級必須在1-100之間");

                if (rule.ColorChangePointCost < 0)
                    return (false, "換色所需點數不能為負數");

                if (rule.BackgroundChangePointCost < 0)
                    return (false, "換背景所需點數不能為負數");

                // 檢查該等級是否已存在（創建時）
                if (rule.RuleId == 0)
                {
                    var exists = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                        .AnyAsync(r => r.Level == rule.RuleId);

                    if (exists)
                        return (false, "該等級規則已存在");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證寵物等級規則時發生錯誤");
                return (false, "驗證規則時發生錯誤");
            }
        }

        #endregion

        #region 控制器需要的額外方法（非介面定義）

        /// <summary>
        /// 獲取所有升級規則（控制器專用）
        /// </summary>
        public async Task<List<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>> GetAllLevelUpRulesAsync()
        {
            try
            {
                return await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .OrderBy(r => r.Level)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有升級規則時發生錯誤");
                return new List<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>();
            }
        }

        /// <summary>
        /// 根據 ID 獲取升級規則（控制器專用）
        /// </summary>
        public async Task<GameSpace.Areas.MiniGame.Models.PetLevelUpRule?> GetLevelUpRuleByIdAsync(int id)
        {
            try
            {
                return await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取升級規則 {Id} 時發生錯誤", id);
                return null;
            }
        }

        /// <summary>
        /// 創建升級規則（控制器專用）
        /// </summary>
        public async Task<bool> CreateLevelUpRuleAsync(PetLevelUpRuleCreateViewModel model, int createdBy)
        {
            try
            {
                // 檢查等級是否已存在
                var exists = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .AnyAsync(r => r.Level == model.Level);

                if (exists)
                {
                    _logger.LogWarning("等級 {Level} 的規則已存在", model.Level);
                    return false;
                }

                var entity = new GameSpace.Areas.MiniGame.Models.PetLevelUpRule
                {
                    Level = model.Level,
                    RequiredExp = model.RequiredExp,
                    RewardPoints = model.PointsReward,
                    Description = model.SpecialReward,
                    IsActive = true
                };

                _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>().Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功創建等級 {Level} 的升級規則，創建者：{CreatedBy}", model.Level, createdBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建升級規則時發生錯誤");
                return false;
            }
        }

        /// <summary>
        /// 更新升級規則（控制器專用）
        /// </summary>
        public async Task<bool> UpdateLevelUpRuleAsync(PetLevelUpRuleEditViewModel model, int updatedBy)
        {
            try
            {
                var entity = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .FirstOrDefaultAsync(r => r.Id == model.Id);

                if (entity == null)
                {
                    _logger.LogWarning("找不到 ID 為 {Id} 的升級規則", model.Id);
                    return false;
                }

                // 如果修改了等級，檢查新等級是否已被其他規則使用
                if (entity.Level != model.Level)
                {
                    var exists = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                        .AnyAsync(r => r.Level == model.Level && r.Id != model.Id);

                    if (exists)
                    {
                        _logger.LogWarning("等級 {Level} 已被其他規則使用", model.Level);
                        return false;
                    }
                }

                // 更新實體屬性
                entity.Level = model.Level;
                entity.RequiredExp = model.RequiredExp;
                entity.RewardPoints = model.PointsReward;
                entity.Description = model.SpecialReward;
                entity.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新 ID 為 {Id} 的升級規則，更新者：{UpdatedBy}", model.Id, updatedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新升級規則時發生錯誤");
                return false;
            }
        }

        /// <summary>
        /// 刪除升級規則（控制器專用）
        /// </summary>
        public async Task<bool> DeleteLevelUpRuleAsync(int id)
        {
            try
            {
                var entity = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (entity == null)
                {
                    _logger.LogWarning("找不到 ID 為 {Id} 的升級規則", id);
                    return false;
                }

                _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>().Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除 ID 為 {Id} 的升級規則", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除升級規則 {Id} 時發生錯誤", id);
                return false;
            }
        }

        /// <summary>
        /// 切換升級規則狀態（控制器專用）
        /// </summary>
        public async Task<bool> ToggleLevelUpRuleStatusAsync(int id)
        {
            try
            {
                var entity = await _context.Set<GameSpace.Areas.MiniGame.Models.PetLevelUpRule>()
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (entity == null)
                {
                    _logger.LogWarning("找不到 ID 為 {Id} 的升級規則", id);
                    return false;
                }

                entity.IsActive = !entity.IsActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功切換 ID 為 {Id} 的升級規則狀態為 {IsActive}", id, entity.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換升級規則狀態時發生錯誤");
                return false;
            }
        }

        #endregion

        #region 私有輔助方法

        /// <summary>
        /// 將 PetLevelUpRule 實體映射為 PetRuleReadModel
        /// </summary>
        private PetRuleReadModel MapToReadModel(GameSpace.Areas.MiniGame.Models.PetLevelUpRule rule)
        {
            return new PetRuleReadModel
            {
                RuleId = rule.Id,
                RuleName = $"等級 {rule.Level} 升級規則",
                Description = rule.Description ?? string.Empty,
                LevelUpExpRequired = rule.RequiredExp,
                ExpMultiplier = 1.0m, // 預設值
                MaxLevel = 100, // 預設最高等級
                ColorChangePointCost = rule.RewardPoints,
                BackgroundChangePointCost = rule.RewardPoints,
                IsActive = rule.IsActive,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt
            };
        }

        #endregion
    }
}

