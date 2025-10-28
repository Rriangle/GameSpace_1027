using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景變更設定服務實作
    /// </summary>
    public class PetBackgroundChangeSettingsService : IPetBackgroundChangeSettingsService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetBackgroundChangeSettingsService> _logger;

        public PetBackgroundChangeSettingsService(
            GameSpacedatabaseContext context,
            ILogger<PetBackgroundChangeSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PetSettings?> GetSettingsAsync()
        {
            try
            {
                // 暫時返回預設設定
                var settings = await _context.Set<PetSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                return settings ?? new PetSettings
                {
                    SettingName = "PetSystemSettings",
                    IsActive = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取寵物背景變更設定時發生錯誤");
                return null;
            }
        }

        public async Task<bool> UpdateSettingsAsync(PetSettings settings)
        {
            try
            {
                // 暫時實作
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物背景變更設定時發生錯誤");
                return false;
            }
        }

        public async Task<int> GetBackgroundChangePointCostAsync()
        {
            try
            {
                // 從背景變更設定中獲取預設成本，如果沒有則返回3000
                var defaultSetting = await _context.Set<PetBackgroundChangeSettings>()
                    .AsNoTracking()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.RequiredPoints)
                    .FirstOrDefaultAsync();

                return defaultSetting?.RequiredPoints ?? 3000;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取背景變更點數成本時發生錯誤");
                return 3000;
            }
        }

        public async Task<bool> CanUserChangeBackgroundAsync(int userId)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null) return false;

                var cost = await GetBackgroundChangePointCostAsync();
                return wallet.UserPoint >= cost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶 {UserId} 是否可變更背景時發生錯誤", userId);
                return false;
            }
        }

        /// <summary>
        /// 獲取所有背景變更設定
        /// </summary>
        public async Task<IEnumerable<PetBackgroundChangeSettings>> GetAllAsync()
        {
            try
            {
                var settings = await _context.Set<PetBackgroundChangeSettings>()
                    .AsNoTracking()
                    .OrderBy(s => s.Id)
                    .ToListAsync();

                _logger.LogInformation("成功獲取 {Count} 筆背景變更設定", settings.Count);
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有背景變更設定時發生錯誤");
                return Enumerable.Empty<PetBackgroundChangeSettings>();
            }
        }

        /// <summary>
        /// 根據ID獲取背景變更設定
        /// </summary>
        public async Task<PetBackgroundChangeSettings?> GetByIdAsync(int id)
        {
            try
            {
                var setting = await _context.Set<PetBackgroundChangeSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (setting == null)
                {
                    _logger.LogWarning("找不到ID為 {Id} 的背景變更設定", id);
                }

                return setting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據ID {Id} 獲取背景變更設定時發生錯誤", id);
                return null;
            }
        }

        /// <summary>
        /// 建立背景變更設定
        /// </summary>
        public async Task<PetBackgroundChangeSettings> CreateAsync(PetBackgroundChangeSettings settings)
        {
            // 1. 點數範圍驗證（0-10000）
            if (settings.RequiredPoints < 0 || settings.RequiredPoints > 10000)
            {
                throw new ArgumentException("所需點數必須在 0-10000 之間");
            }

            // 2. 背景代碼驗證（長度限制）
            if (string.IsNullOrWhiteSpace(settings.BackgroundCode) || settings.BackgroundCode.Length > 7)
            {
                throw new ArgumentException("背景代碼不能為空且長度不能超過 7 個字元");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 設定建立時間
                settings.CreatedAt = DateTime.UtcNow;
                settings.UpdatedAt = null;

                // 驗證背景名稱不可重複
                var existingName = await _context.Set<PetBackgroundChangeSettings>()
                    .AsNoTracking()
                    .AnyAsync(s => s.BackgroundName == settings.BackgroundName);

                if (existingName)
                {
                    _logger.LogWarning("背景名稱 {Name} 已存在，無法建立重複的設定", settings.BackgroundName);
                    throw new InvalidOperationException($"背景名稱「{settings.BackgroundName}」已存在");
                }

                // 驗證背景代碼不可重複
                if (!string.IsNullOrWhiteSpace(settings.BackgroundCode))
                {
                    var existingCode = await _context.Set<PetBackgroundChangeSettings>()
                        .AsNoTracking()
                        .AnyAsync(s => s.BackgroundCode == settings.BackgroundCode);

                    if (existingCode)
                    {
                        _logger.LogWarning("背景代碼 {Code} 已存在，無法建立重複的設定", settings.BackgroundCode);
                        throw new InvalidOperationException($"背景代碼「{settings.BackgroundCode}」已存在");
                    }
                }

                _context.Set<PetBackgroundChangeSettings>().Add(settings);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("成功建立背景變更設定，ID: {Id}, 名稱: {Name}", settings.Id, settings.BackgroundName);
                return settings;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "建立背景變更設定時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 更新背景變更設定
        /// </summary>
        public async Task<PetBackgroundChangeSettings> UpdateAsync(int id, PetBackgroundChangeSettings settings)
        {
            // 1. 點數範圍驗證（0-10000）
            if (settings.RequiredPoints < 0 || settings.RequiredPoints > 10000)
            {
                throw new ArgumentException("所需點數必須在 0-10000 之間");
            }

            // 2. 背景代碼驗證（長度限制）
            if (string.IsNullOrWhiteSpace(settings.BackgroundCode) || settings.BackgroundCode.Length > 7)
            {
                throw new ArgumentException("背景代碼不能為空且長度不能超過 7 個字元");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingSetting = await _context.Set<PetBackgroundChangeSettings>()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (existingSetting == null)
                {
                    _logger.LogWarning("找不到ID為 {Id} 的背景變更設定", id);
                    throw new InvalidOperationException($"找不到ID為 {id} 的背景變更設定");
                }

                // 驗證背景名稱不可重複（排除自己）
                var duplicateName = await _context.Set<PetBackgroundChangeSettings>()
                    .AsNoTracking()
                    .AnyAsync(s => s.BackgroundName == settings.BackgroundName && s.Id != id);

                if (duplicateName)
                {
                    _logger.LogWarning("背景名稱 {Name} 已存在於其他設定中", settings.BackgroundName);
                    throw new InvalidOperationException($"背景名稱「{settings.BackgroundName}」已存在");
                }

                // 驗證背景代碼不可重複（排除自己）
                if (!string.IsNullOrWhiteSpace(settings.BackgroundCode))
                {
                    var duplicateCode = await _context.Set<PetBackgroundChangeSettings>()
                        .AsNoTracking()
                        .AnyAsync(s => s.BackgroundCode == settings.BackgroundCode && s.Id != id);

                    if (duplicateCode)
                    {
                        _logger.LogWarning("背景代碼 {Code} 已存在於其他設定中", settings.BackgroundCode);
                        throw new InvalidOperationException($"背景代碼「{settings.BackgroundCode}」已存在");
                    }
                }

                // 更新欄位
                existingSetting.BackgroundName = settings.BackgroundName;
                existingSetting.RequiredPoints = settings.RequiredPoints;
                existingSetting.BackgroundCode = settings.BackgroundCode;
                existingSetting.IsActive = settings.IsActive;
                existingSetting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("成功更新背景變更設定，ID: {Id}, 名稱: {Name}", id, existingSetting.BackgroundName);
                return existingSetting;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "更新背景變更設定時發生錯誤，ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// 刪除背景變更設定
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var setting = await _context.Set<PetBackgroundChangeSettings>()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (setting == null)
                {
                    _logger.LogWarning("找不到ID為 {Id} 的背景變更設定，無法刪除", id);
                    return false;
                }

                _context.Set<PetBackgroundChangeSettings>().Remove(setting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("成功刪除背景變更設定，ID: {Id}, 名稱: {Name}", id, setting.BackgroundName);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "刪除背景變更設定時發生錯誤，ID: {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// 切換背景變更設定啟用狀態
        /// </summary>
        public async Task<bool> ToggleActiveAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var setting = await _context.Set<PetBackgroundChangeSettings>()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (setting == null)
                {
                    _logger.LogWarning("找不到ID為 {Id} 的背景變更設定，無法切換狀態", id);
                    return false;
                }

                // 切換啟用狀態
                setting.IsActive = !setting.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "成功切換背景變更設定狀態，ID: {Id}, 名稱: {Name}, 新狀態: {IsActive}",
                    id,
                    setting.BackgroundName,
                    setting.IsActive ? "啟用" : "停用"
                );
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "切換背景變更設定狀態時發生錯誤，ID: {Id}", id);
                return false;
            }
        }
    }
}

