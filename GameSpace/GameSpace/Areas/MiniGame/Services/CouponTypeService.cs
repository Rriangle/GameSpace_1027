using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    public class CouponTypeService : ICouponTypeService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<CouponTypeService> _logger;

        public CouponTypeService(GameSpacedatabaseContext context, ILogger<CouponTypeService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // CouponType 基本 CRUD
        public async Task<IEnumerable<CouponType>> GetAllCouponTypesAsync()
        {
            return await _context.CouponTypes
                .AsNoTracking()
                .OrderByDescending(ct => ct.ValidFrom)
                .ToListAsync();
        }

        public async Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId)
        {
            return await _context.CouponTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.CouponTypeId == couponTypeId);
        }

        public async Task<bool> CreateCouponTypeAsync(CouponType couponType)
        {
            try
            {
                couponType.ValidFrom = DateTime.UtcNow;
                // IsActive property does not exist in CouponType
                _context.CouponTypes.Add(couponType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("創建優惠券類型成功: CouponTypeId={CouponTypeId}, Name={Name}",
                    couponType.CouponTypeId, couponType.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建優惠券類型失敗: Name={Name}", couponType.Name);
                return false;
            }
        }

        public async Task<bool> UpdateCouponTypeAsync(CouponType couponType)
        {
            try
            {
                // UpdatedAt property does not exist in CouponType
                _context.CouponTypes.Update(couponType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("更新優惠券類型成功: CouponTypeId={CouponTypeId}", couponType.CouponTypeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新優惠券類型失敗: CouponTypeId={CouponTypeId}", couponType.CouponTypeId);
                return false;
            }
        }

        public async Task<bool> DeleteCouponTypeAsync(int couponTypeId)
        {
            try
            {
                var couponType = await GetCouponTypeByIdAsync(couponTypeId);
                if (couponType == null)
                {
                    _logger.LogWarning("刪除優惠券類型失敗：找不到: CouponTypeId={CouponTypeId}", couponTypeId);
                    return false;
                }

                _context.CouponTypes.Remove(couponType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("刪除優惠券類型成功: CouponTypeId={CouponTypeId}", couponTypeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除優惠券類型失敗: CouponTypeId={CouponTypeId}", couponTypeId);
                return false;
            }
        }

        // CouponType 狀態管理
        public async Task<bool> ActivateCouponTypeAsync(int couponTypeId)
        {
            try
            {
                var couponType = await GetCouponTypeByIdAsync(couponTypeId);
                if (couponType == null)
                {
                    _logger.LogWarning("啟用優惠券類型失敗：找不到: CouponTypeId={CouponTypeId}", couponTypeId);
                    return false;
                }

                // IsActive property does not exist in CouponType
                // UpdatedAt property does not exist in CouponType
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "啟用優惠券類型失敗: CouponTypeId={CouponTypeId}", couponTypeId);
                return false;
            }
        }

        public async Task<bool> DeactivateCouponTypeAsync(int couponTypeId)
        {
            try
            {
                var couponType = await GetCouponTypeByIdAsync(couponTypeId);
                if (couponType == null)
                {
                    _logger.LogWarning("停用優惠券類型失敗：找不到: CouponTypeId={CouponTypeId}", couponTypeId);
                    return false;
                }

                // IsActive property does not exist
                // UpdatedAt property does not exist in CouponType
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停用優惠券類型失敗: CouponTypeId={CouponTypeId}", couponTypeId);
                return false;
            }
        }

        // CouponType 查詢
        public async Task<IEnumerable<CouponType>> GetActiveCouponTypesAsync()
        {
            return await _context.CouponTypes
                .AsNoTracking()
                .Where(ct => (ct.ValidFrom <= DateTime.UtcNow && ct.ValidTo >= DateTime.UtcNow))
                .OrderBy(ct => ct.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CouponType>> GetCouponTypesByDiscountTypeAsync(string discountType)
        {
            return await _context.CouponTypes
                .AsNoTracking()
                .Where(ct => ct.DiscountType == discountType)
                .OrderByDescending(ct => ct.ValidFrom)
                .ToListAsync();
        }

        public async Task<IEnumerable<CouponType>> GetCouponTypesByPointsCostAsync(int minPoints, int maxPoints)
        {
            return await _context.CouponTypes
                .AsNoTracking()
                .Where(ct => ct.PointsCost >= minPoints && ct.PointsCost <= maxPoints)
                .OrderBy(ct => ct.PointsCost)
                .ToListAsync();
        }

        // CouponType 統計
        public async Task<int> GetTotalCouponTypesCountAsync()
        {
            return await _context.CouponTypes.AsNoTracking().CountAsync();
        }

        public async Task<int> GetActiveCouponTypesCountAsync()
        {
            return await _context.CouponTypes.AsNoTracking().CountAsync(ct => (ct.ValidFrom <= DateTime.UtcNow && ct.ValidTo >= DateTime.UtcNow));
        }

        public async Task<Dictionary<string, int>> GetCouponTypesDistributionAsync()
        {
            var distribution = await _context.CouponTypes
                .AsNoTracking()
                .GroupBy(ct => ct.DiscountType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            return distribution.ToDictionary(x => x.Type, x => x.Count);
        }

        public async Task<IEnumerable<CouponTypeUsageStats>> GetCouponTypeUsageStatsAsync()
        {
            try
            {
                // 優化：使用單一查詢避免 N+1 問題
                var stats = await _context.CouponTypes
                    .AsNoTracking()
                    .Select(ct => new CouponTypeUsageStats
                    {
                        CouponTypeId = ct.CouponTypeId,
                        Name = ct.Name,
                        TotalIssued = ct.Coupons.Count(),
                        TotalUsed = ct.Coupons.Count(c => c.IsUsed),
                        TotalUnused = ct.Coupons.Count(c => !c.IsUsed),
                        UsageRate = ct.Coupons.Any()
                            ? (decimal)ct.Coupons.Count(c => c.IsUsed) / ct.Coupons.Count() * 100
                            : 0
                    })
                    .OrderByDescending(s => s.TotalIssued)
                    .ToListAsync();

                _logger.LogInformation("取得優惠券類型使用統計成功，共 {Count} 個類型", stats.Count);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得優惠券類型使用統計失敗");
                return new List<CouponTypeUsageStats>();
            }
        }
    }
}

