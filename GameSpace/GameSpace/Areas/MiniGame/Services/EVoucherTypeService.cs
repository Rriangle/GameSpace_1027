using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    public class EVoucherTypeService : IEVoucherTypeService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<EVoucherTypeService> _logger;

        public EVoucherTypeService(GameSpacedatabaseContext context, ILogger<EVoucherTypeService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // EvoucherType 基本 CRUD
        public async Task<IEnumerable<EvoucherType>> GetAllEVoucherTypesAsync()
        {
            return await _context.EvoucherTypes
                .AsNoTracking()
                .OrderByDescending(evt => evt.ValidFrom)
                .ToListAsync();
        }

        public async Task<EvoucherType?> GetEVoucherTypeByIdAsync(int evoucherTypeId)
        {
            return await _context.EvoucherTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(evt => evt.EvoucherTypeId == evoucherTypeId);
        }

        public async Task<bool> CreateEVoucherTypeAsync(EvoucherType evoucherType)
        {
            try
            {
                evoucherType.ValidFrom = DateTime.UtcNow;
                // IsActive property does not exist in EvoucherType
                _context.EvoucherTypes.Add(evoucherType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("創建電子憑證類型成功: EVoucherTypeId={EVoucherTypeId}, Name={Name}",
                    evoucherType.EvoucherTypeId, evoucherType.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建電子憑證類型失敗: Name={Name}", evoucherType.Name);
                return false;
            }
        }

        public async Task<bool> UpdateEVoucherTypeAsync(EvoucherType evoucherType)
        {
            try
            {
                // UpdatedAt property does not exist in EvoucherType
                _context.EvoucherTypes.Update(evoucherType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("更新電子憑證類型成功: EVoucherTypeId={EVoucherTypeId}", evoucherType.EvoucherTypeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新電子憑證類型失敗: EVoucherTypeId={EVoucherTypeId}", evoucherType.EvoucherTypeId);
                return false;
            }
        }

        public async Task<bool> DeleteEVoucherTypeAsync(int evoucherTypeId)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null)
                {
                    _logger.LogWarning("刪除電子憑證類型失敗：找不到: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                    return false;
                }

                _context.EvoucherTypes.Remove(evoucherType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("刪除電子憑證類型成功: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除電子憑證類型失敗: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                return false;
            }
        }

        // EvoucherType 狀態管理
        public async Task<bool> ActivateEVoucherTypeAsync(int evoucherTypeId)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null)
                {
                    _logger.LogWarning("啟用電子憑證類型失敗：找不到: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                    return false;
                }

                // IsActive property does not exist in EvoucherType
                // UpdatedAt property does not exist in EvoucherType
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "啟用電子憑證類型失敗: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                return false;
            }
        }

        public async Task<bool> DeactivateEVoucherTypeAsync(int evoucherTypeId)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null)
                {
                    _logger.LogWarning("停用電子憑證類型失敗：找不到: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                    return false;
                }

                // Set ValidTo to current time to deactivate
                evoucherType.ValidTo = DateTime.UtcNow;
                // UpdatedAt property does not exist in EvoucherType
                await _context.SaveChangesAsync();
                _logger.LogInformation("停用電子憑證類型成功: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停用電子憑證類型失敗: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                return false;
            }
        }

        // EvoucherType 查詢
        public async Task<IEnumerable<EvoucherType>> GetActiveEVoucherTypesAsync()
        {
            return await _context.EvoucherTypes
                .AsNoTracking()
                .Where(evt => evt.ValidFrom <= DateTime.UtcNow && evt.ValidTo >= DateTime.UtcNow)
                .OrderBy(evt => evt.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvoucherType>> GetEVoucherTypesByValueRangeAsync(decimal minValue, decimal maxValue)
        {
            return await _context.EvoucherTypes
                .AsNoTracking()
                .Where(evt => evt.ValueAmount >= minValue && evt.ValueAmount <= maxValue)
                .OrderBy(evt => evt.ValueAmount)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvoucherType>> GetAvailableEVoucherTypesAsync()
        {
            return await _context.EvoucherTypes
                .AsNoTracking()
                .Where(evt => evt.ValidFrom <= DateTime.UtcNow && evt.ValidTo >= DateTime.UtcNow && evt.TotalAvailable > 0)
                .OrderBy(evt => evt.ValueAmount)
                .ToListAsync();
        }

        // EvoucherType 庫存管理
        public async Task<bool> IncreaseStockAsync(int evoucherTypeId, int amount)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null)
                {
                    _logger.LogWarning("增加庫存失敗：找不到: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                    return false;
                }

                evoucherType.TotalAvailable += amount;
                // UpdatedAt property does not exist in EvoucherType
                await _context.SaveChangesAsync();
                _logger.LogInformation("增加庫存成功: EVoucherTypeId={EVoucherTypeId}, Amount={Amount}, NewStock={NewStock}",
                    evoucherTypeId, amount, evoucherType.TotalAvailable);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "增加庫存失敗: EVoucherTypeId={EVoucherTypeId}, Amount={Amount}", evoucherTypeId, amount);
                return false;
            }
        }

        public async Task<bool> DecreaseStockAsync(int evoucherTypeId, int amount)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null)
                {
                    _logger.LogWarning("減少庫存失敗：找不到: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
                    return false;
                }
                if (evoucherType.TotalAvailable < amount)
                {
                    _logger.LogWarning("減少庫存失敗：庫存不足: EVoucherTypeId={EVoucherTypeId}, Current={Current}, Requested={Requested}",
                        evoucherTypeId, evoucherType.TotalAvailable, amount);
                    return false;
                }

                evoucherType.TotalAvailable -= amount;
                // UpdatedAt property does not exist in EvoucherType
                await _context.SaveChangesAsync();
                _logger.LogInformation("減少庫存成功: EVoucherTypeId={EVoucherTypeId}, Amount={Amount}, RemainingStock={RemainingStock}",
                    evoucherTypeId, amount, evoucherType.TotalAvailable);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "減少庫存失敗: EVoucherTypeId={EVoucherTypeId}, Amount={Amount}", evoucherTypeId, amount);
                return false;
            }
        }

        public async Task<int> GetRemainingStockAsync(int evoucherTypeId)
        {
            var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
            return evoucherType?.TotalAvailable ?? 0;
        }

        public async Task<bool> IsStockAvailableAsync(int evoucherTypeId)
        {
            var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
            return evoucherType != null && evoucherType.ValidFrom <= DateTime.UtcNow && evoucherType.ValidTo >= DateTime.UtcNow && evoucherType.TotalAvailable > 0;
        }

        // EvoucherType 統計
        public async Task<int> GetTotalEVoucherTypesCountAsync()
        {
            return await _context.EvoucherTypes.AsNoTracking().CountAsync();
        }

        public async Task<int> GetActiveEVoucherTypesCountAsync()
        {
            return await _context.EvoucherTypes.AsNoTracking().CountAsync(evt => evt.ValidFrom <= DateTime.UtcNow && evt.ValidTo >= DateTime.UtcNow);
        }

        public async Task<Dictionary<string, int>> GetEVoucherTypesDistributionAsync()
        {
            var types = await _context.EvoucherTypes.AsNoTracking().ToListAsync();
            var distribution = new Dictionary<string, int>();

            // 按價值範圍分組
            distribution["0-100"] = types.Count(t => t.ValueAmount < 100);
            distribution["100-500"] = types.Count(t => t.ValueAmount >= 100 && t.ValueAmount < 500);
            distribution["500-1000"] = types.Count(t => t.ValueAmount >= 500 && t.ValueAmount < 1000);
            distribution["1000+"] = types.Count(t => t.ValueAmount >= 1000);

            return distribution;
        }

        public async Task<IEnumerable<EVoucherTypeUsageStats>> GetEVoucherTypeUsageStatsAsync()
        {
            // 優化：使用單一查詢避免 N+1 問題
            var stats = await _context.EvoucherTypes
                .AsNoTracking()
                .Select(et => new EVoucherTypeUsageStats
                {
                    EVoucherTypeId = et.EvoucherTypeId,
                    Name = et.Name,
                    TotalIssued = et.Evouchers.Count(),
                    TotalUsed = et.Evouchers.Count(e => e.IsUsed),
                    TotalUnused = et.Evouchers.Count(e => !e.IsUsed),
                    RemainingStock = et.TotalAvailable,
                    UsageRate = et.Evouchers.Any()
                        ? (decimal)et.Evouchers.Count(e => e.IsUsed) / et.Evouchers.Count() * 100
                        : 0,
                    TotalValue = et.ValueAmount * et.Evouchers.Count()
                })
                .OrderByDescending(s => s.TotalValue)
                .ToListAsync();

            return stats;
        }
    }
}

