using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class PetLevelRewardSettingService : IPetLevelRewardSettingService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetLevelRewardSettingService> _logger;

        public PetLevelRewardSettingService(GameSpacedatabaseContext context, ILogger<PetLevelRewardSettingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PetLevelRewardSettingListViewModel>> GetAllAsync()
        {
            try
            {
                var settings = await _context.Set<PetLevelRewardSetting>()
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.LevelRangeStart)
                    .Select(s => new PetLevelRewardSettingListViewModel
                    {
                        SettingId = s.SettingId,
                        LevelRangeStart = s.LevelRangeStart,
                        LevelRangeEnd = s.LevelRangeEnd,
                        PointsReward = s.PointsReward,
                        Description = s.Description,
                        IsActive = s.IsActive,
                        DisplayOrder = s.DisplayOrder,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        UpdatedBy = s.UpdatedBy
                    })
                    .ToListAsync();

                _logger.LogInformation("成功取得所有寵物升級獎勵設定，共 {Count} 筆", settings.Count());
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有寵物升級獎勵設定時發生錯誤");
                throw;
            }
        }

        public async Task<PetLevelRewardSettingViewModel?> GetByIdAsync(int id)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>()
                    .Where(s => s.SettingId == id && !s.IsDeleted)
                    .Select(s => new PetLevelRewardSettingViewModel
                    {
                        SettingId = s.SettingId,
                        LevelRangeStart = s.LevelRangeStart,
                        LevelRangeEnd = s.LevelRangeEnd,
                        PointsReward = s.PointsReward,
                        Description = s.Description,
                        IsActive = s.IsActive,
                        DisplayOrder = s.DisplayOrder,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        UpdatedBy = s.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (setting != null)
                {
                    _logger.LogInformation("成功取得寵物升級獎勵設定 ID: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("找不到寵物升級獎勵設定 ID: {Id}", id);
                }

                return setting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物升級獎勵設定 ID: {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<PetLevelRewardSettingViewModel?> CreateAsync(PetLevelRewardSettingCreateViewModel model)
        {
            try
            {
                // 驗證等級範圍是否有效
                if (model.LevelRangeStart > model.LevelRangeEnd)
                {
                    _logger.LogWarning("等級範圍無效：起始 {Start} 大於結束 {End}", model.LevelRangeStart, model.LevelRangeEnd);
                    return null;
                }

                // 驗證等級範圍是否與現有設定重疊
                if (!await ValidateLevelRangeAsync(model.LevelRangeStart, model.LevelRangeEnd))
                {
                    _logger.LogWarning("等級範圍 {Start}-{End} 與現有設定重疊", model.LevelRangeStart, model.LevelRangeEnd);
                    return null;
                }

                var setting = new PetLevelRewardSetting
                {
                    LevelRangeStart = model.LevelRangeStart,
                    LevelRangeEnd = model.LevelRangeEnd,
                    PointsReward = model.PointsReward,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    UpdatedBy = null,
                    IsDeleted = false
                };

                _context.Set<PetLevelRewardSetting>().Add(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功建立寵物升級獎勵設定，等級範圍: {Start}-{End}, 獎勵點數: {Points}",
                    model.LevelRangeStart, model.LevelRangeEnd, model.PointsReward);

                return new PetLevelRewardSettingViewModel
                {
                    SettingId = setting.SettingId,
                    LevelRangeStart = setting.LevelRangeStart,
                    LevelRangeEnd = setting.LevelRangeEnd,
                    PointsReward = setting.PointsReward,
                    Description = setting.Description,
                    IsActive = setting.IsActive,
                    DisplayOrder = setting.DisplayOrder,
                    CreatedAt = setting.CreatedAt,
                    UpdatedAt = setting.UpdatedAt,
                    UpdatedBy = setting.UpdatedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立寵物升級獎勵設定時發生錯誤");
                throw;
            }
        }

        public async Task<PetLevelRewardSettingViewModel?> UpdateAsync(PetLevelRewardSettingEditViewModel model)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>()
                    .FirstOrDefaultAsync(s => s.SettingId == model.SettingId && !s.IsDeleted);

                if (setting == null)
                {
                    _logger.LogWarning("找不到要更新的寵物升級獎勵設定 ID: {Id}", model.SettingId);
                    return null;
                }

                // 驗證等級範圍是否有效
                if (model.LevelRangeStart > model.LevelRangeEnd)
                {
                    _logger.LogWarning("等級範圍無效：起始 {Start} 大於結束 {End}", model.LevelRangeStart, model.LevelRangeEnd);
                    return null;
                }

                // 驗證等級範圍是否與現有設定重疊（排除自己）
                if (!await ValidateLevelRangeAsync(model.LevelRangeStart, model.LevelRangeEnd, model.SettingId))
                {
                    _logger.LogWarning("等級範圍 {Start}-{End} 與現有設定重疊", model.LevelRangeStart, model.LevelRangeEnd);
                    return null;
                }

                setting.LevelRangeStart = model.LevelRangeStart;
                setting.LevelRangeEnd = model.LevelRangeEnd;
                setting.PointsReward = model.PointsReward;
                setting.Description = model.Description;
                setting.IsActive = model.IsActive;
                setting.DisplayOrder = model.DisplayOrder;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新寵物升級獎勵設定 ID: {Id}", model.SettingId);

                return new PetLevelRewardSettingViewModel
                {
                    SettingId = setting.SettingId,
                    LevelRangeStart = setting.LevelRangeStart,
                    LevelRangeEnd = setting.LevelRangeEnd,
                    PointsReward = setting.PointsReward,
                    Description = setting.Description,
                    IsActive = setting.IsActive,
                    DisplayOrder = setting.DisplayOrder,
                    CreatedAt = setting.CreatedAt,
                    UpdatedAt = setting.UpdatedAt,
                    UpdatedBy = setting.UpdatedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物升級獎勵設定 ID: {Id} 時發生錯誤", model.SettingId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>()
                    .FirstOrDefaultAsync(s => s.SettingId == id && !s.IsDeleted);

                if (setting == null)
                {
                    _logger.LogWarning("找不到要刪除的寵物升級獎勵設定 ID: {Id}", id);
                    return false;
                }

                // 軟刪除
                setting.IsDeleted = true;
                setting.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除寵物升級獎勵設定 ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物升級獎勵設定 ID: {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>()
                    .FirstOrDefaultAsync(s => s.SettingId == id && !s.IsDeleted);

                if (setting == null)
                {
                    _logger.LogWarning("找不到要切換狀態的寵物升級獎勵設定 ID: {Id}", id);
                    return false;
                }

                setting.IsActive = !setting.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功切換寵物升級獎勵設定 ID: {Id} 狀態為: {IsActive}",
                    id, setting.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換寵物升級獎勵設定 ID: {Id} 狀態時發生錯誤", id);
                throw;
            }
        }

        public async Task<IEnumerable<PetLevelRewardSettingListViewModel>> SearchAsync(PetLevelRewardSettingSearchViewModel searchModel)
        {
            try
            {
                var query = _context.Set<PetLevelRewardSetting>()
                    .Where(s => !s.IsDeleted)
                    .AsQueryable();

                if (searchModel.LevelRangeStart.HasValue)
                {
                    query = query.Where(s => s.LevelRangeStart >= searchModel.LevelRangeStart.Value);
                }

                if (searchModel.LevelRangeEnd.HasValue)
                {
                    query = query.Where(s => s.LevelRangeEnd <= searchModel.LevelRangeEnd.Value);
                }

                if (searchModel.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == searchModel.IsActive.Value);
                }

                var settings = await query
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.LevelRangeStart)
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(s => new PetLevelRewardSettingListViewModel
                    {
                        SettingId = s.SettingId,
                        LevelRangeStart = s.LevelRangeStart,
                        LevelRangeEnd = s.LevelRangeEnd,
                        PointsReward = s.PointsReward,
                        Description = s.Description,
                        IsActive = s.IsActive,
                        DisplayOrder = s.DisplayOrder,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        UpdatedBy = s.UpdatedBy
                    })
                    .ToListAsync();

                _logger.LogInformation("搜尋寵物升級獎勵設定完成，找到 {Count} 筆結果", settings.Count());
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋寵物升級獎勵設定時發生錯誤");
                throw;
            }
        }

        public async Task<(int TotalCount, int TotalPages)> GetPaginationInfoAsync(PetLevelRewardSettingSearchViewModel searchModel)
        {
            try
            {
                var query = _context.Set<PetLevelRewardSetting>()
                    .Where(s => !s.IsDeleted)
                    .AsQueryable();

                if (searchModel.LevelRangeStart.HasValue)
                {
                    query = query.Where(s => s.LevelRangeStart >= searchModel.LevelRangeStart.Value);
                }

                if (searchModel.LevelRangeEnd.HasValue)
                {
                    query = query.Where(s => s.LevelRangeEnd <= searchModel.LevelRangeEnd.Value);
                }

                if (searchModel.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == searchModel.IsActive.Value);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / searchModel.PageSize);

                return (totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得分頁資訊時發生錯誤");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            try
            {
                var allSettings = await _context.Set<PetLevelRewardSetting>()
                    .Where(s => !s.IsDeleted)
                    .ToListAsync();

                var totalCount = allSettings.Count;
                var enabledCount = allSettings.Count(s => s.IsActive);
                var disabledCount = totalCount - enabledCount;

                var statistics = new Dictionary<string, object>
                {
                    ["TotalCount"] = totalCount,
                    ["EnabledCount"] = enabledCount,
                    ["DisabledCount"] = disabledCount,
                    ["TotalPointsReward"] = allSettings.Where(s => s.IsActive).Sum(s => s.PointsReward),
                    ["AveragePointsReward"] = enabledCount > 0 ? allSettings.Where(s => s.IsActive).Average(s => s.PointsReward) : 0,
                    ["MinLevelCovered"] = allSettings.Any() ? allSettings.Min(s => s.LevelRangeStart) : 0,
                    ["MaxLevelCovered"] = allSettings.Any() ? allSettings.Max(s => s.LevelRangeEnd) : 0
                };

                _logger.LogInformation("成功取得寵物升級獎勵設定統計資料");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得統計資料時發生錯誤");
                throw;
            }
        }

        public async Task<bool> ValidateLevelRangeAsync(int start, int end, int? excludeId = null)
        {
            try
            {
                var query = _context.Set<PetLevelRewardSetting>()
                    .Where(s => !s.IsDeleted);

                if (excludeId.HasValue)
                {
                    query = query.Where(s => s.SettingId != excludeId.Value);
                }

                // 檢查範圍是否與現有設定重疊
                var hasOverlap = await query.AnyAsync(s =>
                    (start >= s.LevelRangeStart && start <= s.LevelRangeEnd) ||
                    (end >= s.LevelRangeStart && end <= s.LevelRangeEnd) ||
                    (start <= s.LevelRangeStart && end >= s.LevelRangeEnd)
                );

                return !hasOverlap; // 如果沒有重疊則返回 true（可以建立）
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證等級範圍 {Start}-{End} 時發生錯誤", start, end);
                throw;
            }
        }

        public async Task<int> GetRewardPointsForLevelAsync(int level)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>()
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Where(s => level >= s.LevelRangeStart && level <= s.LevelRangeEnd)
                    .FirstOrDefaultAsync();

                if (setting != null)
                {
                    _logger.LogInformation("等級 {Level} 的獎勵點數: {Points}", level, setting.PointsReward);
                    return setting.PointsReward;
                }

                _logger.LogWarning("找不到等級 {Level} 對應的獎勵設定", level);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得等級 {Level} 獎勵點數時發生錯誤", level);
                throw;
            }
        }
    }
}
