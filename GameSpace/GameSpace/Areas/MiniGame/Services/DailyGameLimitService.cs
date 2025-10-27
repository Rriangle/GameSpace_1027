using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 每日遊戲次數限制設定服務實作
    /// </summary>
    public class DailyGameLimitService : IDailyGameLimitService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<DailyGameLimitService> _logger;
        private readonly GameSpace.Infrastructure.Time.IAppClock _appClock;

        public DailyGameLimitService(GameSpacedatabaseContext context, ILogger<DailyGameLimitService> logger, GameSpace.Infrastructure.Time.IAppClock appClock)
        {
            _context = context;
            _logger = logger;
            _appClock = appClock;
        }

        /// <summary>
        /// 取得所有每日遊戲次數限制設定
        /// </summary>
        public async Task<(List<DailyGameLimitListViewModel> Items, int TotalCount)> GetAllAsync(DailyGameLimitSearchViewModel searchModel)
        {
            try
            {
                // ✅ 使用 Set<T>() 不需要在 DbContext 定義 DbSet（避免越界）
                var query = _context.Set<DailyGameLimit>().AsQueryable();

                // 搜尋條件
                if (!string.IsNullOrEmpty(searchModel.SettingName))
                {
                    query = query.Where(x => x.SettingName.Contains(searchModel.SettingName));
                }

                if (searchModel.IsEnabled.HasValue)
                {
                    query = query.Where(x => x.IsEnabled == searchModel.IsEnabled.Value);
                }

                // 取得總數
                var totalCount = await query.CountAsync();

                // 分頁
                var items = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(x => new DailyGameLimitListViewModel
                    {
                        Id = x.Id,
                        DailyLimit = x.DailyLimit,
                        SettingName = x.SettingName,
                        Description = x.Description,
                        IsEnabled = x.IsEnabled,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        CreatedBy = x.CreatedBy,
                        UpdatedBy = x.UpdatedBy
                    })
                    .ToListAsync();

                _logger.LogInformation("成功取得每日遊戲次數限制設定列表，共 {Count} 筆", items.Count);
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得每日遊戲次數限制設定列表時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 根據ID取得每日遊戲次數限制設定
        /// </summary>
        public async Task<DailyGameLimit?> GetByIdAsync(int id)
        {
            try
            {
                var setting = await _context.Set<DailyGameLimit>().FindAsync(id);
                if (setting != null)
                {
                    _logger.LogInformation("成功取得每日遊戲次數限制設定 ID: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("找不到每日遊戲次數限制設定 ID: {Id}", id);
                }
                return setting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得每日遊戲次數限制設定 ID: {Id} 時發生錯誤", id);
                throw;
            }
        }

        /// <summary>
        /// 取得目前的每日遊戲次數限制設定
        /// </summary>
        public async Task<DailyGameLimit?> GetCurrentSettingAsync()
        {
            try
            {
                var setting = await _context.Set<DailyGameLimit>()
                    .Where(x => x.IsEnabled)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (setting != null)
                {
                    _logger.LogInformation("成功取得目前每日遊戲次數限制設定，限制次數: {DailyLimit}", setting.DailyLimit);
                }
                else
                {
                    _logger.LogWarning("找不到啟用的每日遊戲次數限制設定");
                }
                return setting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得目前每日遊戲次數限制設定時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 建立每日遊戲次數限制設定
        /// </summary>
        public async Task<bool> CreateAsync(DailyGameLimitCreateViewModel model, string? createdBy = null)
        {
            try
            {
                var setting = new DailyGameLimit
                {
                    DailyLimit = model.DailyLimit,
                    SettingName = model.SettingName,
                    Description = model.Description,
                    IsEnabled = model.IsEnabled,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Set<DailyGameLimit>().Add(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功建立每日遊戲次數限制設定，ID: {Id}, 限制次數: {DailyLimit}", setting.Id, setting.DailyLimit);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立每日遊戲次數限制設定時發生錯誤");
                return false;
            }
        }

        /// <summary>
        /// 更新每日遊戲次數限制設定
        /// </summary>
        public async Task<bool> UpdateAsync(DailyGameLimitEditViewModel model, string? updatedBy = null)
        {
            try
            {
                var setting = await _context.Set<DailyGameLimit>().FindAsync(model.Id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到每日遊戲次數限制設定 ID: {Id}", model.Id);
                    return false;
                }

                setting.DailyLimit = model.DailyLimit;
                setting.SettingName = model.SettingName;
                setting.Description = model.Description;
                setting.IsEnabled = model.IsEnabled;
                setting.UpdatedBy = updatedBy;
                setting.UpdatedAt = DateTime.UtcNow;

                _context.Set<DailyGameLimit>().Update(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新每日遊戲次數限制設定 ID: {Id}, 限制次數: {DailyLimit}", setting.Id, setting.DailyLimit);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新每日遊戲次數限制設定 ID: {Id} 時發生錯誤", model.Id);
                return false;
            }
        }

        /// <summary>
        /// 刪除每日遊戲次數限制設定
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var setting = await _context.Set<DailyGameLimit>().FindAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到每日遊戲次數限制設定 ID: {Id}", id);
                    return false;
                }

                _context.Set<DailyGameLimit>().Remove(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除每日遊戲次數限制設定 ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除每日遊戲次數限制設定 ID: {Id} 時發生錯誤", id);
                return false;
            }
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        public async Task<bool> ToggleStatusAsync(int id, string? updatedBy = null)
        {
            try
            {
                var setting = await _context.Set<DailyGameLimit>().FindAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到每日遊戲次數限制設定 ID: {Id}", id);
                    return false;
                }

                setting.IsEnabled = !setting.IsEnabled;
                setting.UpdatedBy = updatedBy;
                setting.UpdatedAt = DateTime.UtcNow;

                _context.Set<DailyGameLimit>().Update(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功切換每日遊戲次數限制設定狀態 ID: {Id}, 新狀態: {IsEnabled}", id, setting.IsEnabled);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換每日遊戲次數限制設定狀態 ID: {Id} 時發生錯誤", id);
                return false;
            }
        }

        /// <summary>
        /// 取得統計資料
        /// </summary>
        public async Task<DailyGameLimitStatisticsViewModel> GetStatisticsAsync()
        {
            try
            {
                var totalSettings = await _context.Set<DailyGameLimit>().CountAsync();
                var enabledSettings = await _context.Set<DailyGameLimit>().CountAsync(x => x.IsEnabled);
                var disabledSettings = totalSettings - enabledSettings;
                
                var currentSetting = await GetCurrentSettingAsync();
                var currentDailyLimit = currentSetting?.DailyLimit ?? 3;
                
                var lastUpdated = await _context.Set<DailyGameLimit>()
                    .OrderByDescending(x => x.UpdatedAt)
                    .Select(x => x.UpdatedAt)
                    .FirstOrDefaultAsync();

                var statistics = new DailyGameLimitStatisticsViewModel
                {
                    TotalSettings = totalSettings,
                    EnabledSettings = enabledSettings,
                    DisabledSettings = disabledSettings,
                    CurrentDailyLimit = currentDailyLimit,
                    LastUpdated = lastUpdated
                };

                _logger.LogInformation("成功取得每日遊戲次數限制設定統計資料");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得每日遊戲次數限制設定統計資料時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 檢查使用者今日遊戲次數是否超過限制
        /// </summary>
        public async Task<bool> IsDailyLimitExceededAsync(int userId)
        {
            try
            {
                var currentSetting = await GetCurrentSettingAsync();
                if (currentSetting == null)
                {
                    // 如果沒有設定，使用預設值（一天三次）
                    return await GetTodayGameCountAsync(userId) >= 3;
                }

                var todayGameCount = await GetTodayGameCountAsync(userId);
                var isExceeded = todayGameCount >= currentSetting.DailyLimit;

                _logger.LogInformation("檢查使用者 {UserId} 今日遊戲次數限制，已玩: {TodayCount}, 限制: {DailyLimit}, 超過限制: {IsExceeded}", 
                    userId, todayGameCount, currentSetting.DailyLimit, isExceeded);

                return isExceeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查使用者 {UserId} 今日遊戲次數限制時發生錯誤", userId);
                throw;
            }
        }

        /// <summary>
        /// 取得使用者今日已玩遊戲次數
        /// </summary>
        public async Task<int> GetTodayGameCountAsync(int userId)
        {
            try
            {
                var taipeiNow = _appClock.ToAppTime(_appClock.UtcNow);
                var startUtc = _appClock.ToUtc(taipeiNow.Date);
                var endUtc = _appClock.ToUtc(taipeiNow.Date.AddDays(1));

                var todayGameCount = await _context.MiniGames
                    .Where(x => x.UserId == userId && x.StartTime >= startUtc && x.StartTime < endUtc)
                    .CountAsync();

                _logger.LogInformation("使用者 {UserId} 今日已玩遊戲次數: {Count}", userId, todayGameCount);
                return todayGameCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 今日遊戲次數時發生錯誤", userId);
                throw;
            }
        }
    }
}

