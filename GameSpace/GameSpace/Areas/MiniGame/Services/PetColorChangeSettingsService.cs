using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色點數設定服務實作
    /// </summary>
    public class PetColorChangeSettingsService : IPetColorChangeSettingsService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetColorChangeSettingsService> _logger;
        private static readonly Regex ColorCodeRegex = new Regex(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        public PetColorChangeSettingsService(GameSpacedatabaseContext context, ILogger<PetColorChangeSettingsService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 取得所有寵物換色點數設定
        /// </summary>
        /// <returns>寵物換色點數設定清單</returns>
        public async Task<IEnumerable<PetColorChangeSettings>> GetAllSettingsAsync()
        {
            try
            {
                var settings = await _context.Set<PetColorChangeSettings>()
                    .AsNoTracking()
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("成功取得 {Count} 筆寵物換色點數設定", settings.Count);
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "操作失敗: {Operation}", nameof(GetAllSettingsAsync));
                throw new Exception("取得所有寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 取得所有寵物換色點數設定 (Alias for GetAllSettingsAsync)
        /// </summary>
        /// <returns>寵物換色點數設定清單</returns>
        public async Task<IEnumerable<PetColorChangeSettings>> GetAllAsync()
        {
            try
            {
                var settings = await _context.Set<PetColorChangeSettings>()
                    .AsNoTracking()
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("成功取得 {Count} 筆寵物換色點數設定", settings.Count);
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "操作失敗: {Operation}", nameof(GetAllAsync));
                throw new Exception("取得所有寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 根據ID取得寵物換色點數設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>寵物換色點數設定</returns>
        public async Task<PetColorChangeSettings?> GetSettingByIdAsync(int id)
        {
            try
            {
                return await _context.Set<PetColorChangeSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SettingId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"根據ID {id} 取得寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 新增寵物換色點數設定
        /// </summary>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>新增的設定</returns>
        public async Task<PetColorChangeSettings> CreateSettingAsync(PetColorChangeSettings setting)
        {
            // 1. 點數範圍驗證（0-10000）
            if (setting.RequiredPoints < 0 || setting.RequiredPoints > 10000)
            {
                throw new ArgumentException("所需點數必須在 0-10000 之間");
            }

            // 2. 顏色代碼格式驗證（Hex 格式 #RRGGBB）
            if (!ColorCodeRegex.IsMatch(setting.ColorCode))
            {
                throw new ArgumentException("顏色代碼格式不正確，應為 #RRGGBB 格式");
            }

            // 3. 顏色名稱唯一性檢查
            var exists = await _context.Set<PetColorChangeSettings>()
                .AsNoTracking()
                .AnyAsync(s => s.ColorName == setting.ColorName);
            if (exists)
            {
                throw new ArgumentException($"顏色名稱 '{setting.ColorName}' 已存在");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                setting.CreatedAt = DateTime.UtcNow;
                _context.Set<PetColorChangeSettings>().Add(setting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return setting;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("新增寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 更新寵物換色點數設定
        /// </summary>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>更新後的設定</returns>
        public async Task<PetColorChangeSettings> UpdateSettingAsync(PetColorChangeSettings setting)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                setting.UpdatedAt = DateTime.UtcNow;
                _context.Set<PetColorChangeSettings>().Update(setting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return setting;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("更新寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 刪除寵物換色點數設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否刪除成功</returns>
        public async Task<bool> DeleteSettingAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var setting = await _context.Set<PetColorChangeSettings>()
                    .FirstOrDefaultAsync(s => s.SettingId == id);

                if (setting == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                _context.Set<PetColorChangeSettings>().Remove(setting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"刪除ID {id} 的寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 取得目前啟用的換色點數設定
        /// </summary>
        /// <returns>啟用的換色點數設定</returns>
        public async Task<PetColorChangeSettings?> GetActiveSettingAsync()
        {
            try
            {
                return await _context.Set<PetColorChangeSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.IsActive);
            }
            catch (Exception ex)
            {
                throw new Exception("取得目前啟用的換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 切換設定的啟用狀態
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否切換成功</returns>
        public async Task<bool> ToggleActiveAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var setting = await _context.Set<PetColorChangeSettings>()
                    .FirstOrDefaultAsync(s => s.SettingId == id);

                if (setting == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                setting.IsActive = !setting.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"切換ID {id} 的設定啟用狀態時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 根據ID取得寵物換色點數設定 (Alias for GetSettingByIdAsync)
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>寵物換色點數設定</returns>
        public async Task<PetColorChangeSettings?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<PetColorChangeSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SettingId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"根據ID {id} 取得寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 新增寵物換色點數設定 (Alias for CreateSettingAsync)
        /// </summary>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>新增的設定</returns>
        public async Task<PetColorChangeSettings> CreateAsync(PetColorChangeSettings setting)
        {
            // 1. 點數範圍驗證（0-10000）
            if (setting.RequiredPoints < 0 || setting.RequiredPoints > 10000)
            {
                throw new ArgumentException("所需點數必須在 0-10000 之間");
            }

            // 2. 顏色代碼格式驗證（Hex 格式 #RRGGBB）
            if (!ColorCodeRegex.IsMatch(setting.ColorCode))
            {
                throw new ArgumentException("顏色代碼格式不正確，應為 #RRGGBB 格式");
            }

            // 3. 顏色名稱唯一性檢查
            var exists = await _context.Set<PetColorChangeSettings>()
                .AsNoTracking()
                .AnyAsync(s => s.ColorName == setting.ColorName);
            if (exists)
            {
                throw new ArgumentException($"顏色名稱 '{setting.ColorName}' 已存在");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                setting.CreatedAt = DateTime.UtcNow;
                _context.Set<PetColorChangeSettings>().Add(setting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return setting;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("新增寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 更新寵物換色點數設定 (Overload with ID parameter)
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>更新後的設定</returns>
        public async Task<PetColorChangeSettings> UpdateAsync(int id, PetColorChangeSettings setting)
        {
            // 1. 點數範圍驗證（0-10000）
            if (setting.RequiredPoints < 0 || setting.RequiredPoints > 10000)
            {
                throw new ArgumentException("所需點數必須在 0-10000 之間");
            }

            // 2. 顏色代碼格式驗證（Hex 格式 #RRGGBB）
            if (!ColorCodeRegex.IsMatch(setting.ColorCode))
            {
                throw new ArgumentException("顏色代碼格式不正確，應為 #RRGGBB 格式");
            }

            // 3. 顏色名稱唯一性檢查（排除自己）
            var exists = await _context.Set<PetColorChangeSettings>()
                .AsNoTracking()
                .AnyAsync(s => s.ColorName == setting.ColorName && s.Id != setting.Id);
            if (exists)
            {
                throw new ArgumentException($"顏色名稱 '{setting.ColorName}' 已存在");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 檢查設定是否存在
                var existingSetting = await _context.Set<PetColorChangeSettings>()
                    .FirstOrDefaultAsync(s => s.SettingId == id);

                if (existingSetting == null)
                {
                    await transaction.RollbackAsync();
                    throw new ArgumentException($"找不到ID為 {id} 的設定");
                }

                // 更新字段
                existingSetting.SettingName = setting.SettingName;
                existingSetting.ColorCode = setting.ColorCode;
                existingSetting.PointsRequired = setting.PointsRequired;
                existingSetting.IsActive = setting.IsActive;
                existingSetting.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(setting.Remarks))
                {
                    existingSetting.Remarks = setting.Remarks;
                }

                _context.Set<PetColorChangeSettings>().Update(existingSetting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return existingSetting;
            }
            catch (ArgumentException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"更新ID為 {id} 的寵物換色點數設定時發生錯誤", ex);
            }
        }

        /// <summary>
        /// 刪除寵物換色點數設定 (Alias for DeleteSettingAsync)
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否刪除成功</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var setting = await _context.Set<PetColorChangeSettings>()
                    .FirstOrDefaultAsync(s => s.SettingId == id);

                if (setting == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                _context.Set<PetColorChangeSettings>().Remove(setting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"刪除ID {id} 的寵物換色點數設定時發生錯誤", ex);
            }
        }
    }
}

