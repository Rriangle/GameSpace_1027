using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    public class CouponService : ICouponService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<CouponService> _logger;

        public CouponService(GameSpacedatabaseContext context, ILogger<CouponService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            try
            {
                return await _context.Coupons
                    .AsNoTracking()
                    .Include(c => c.CouponType)
                    .OrderByDescending(c => c.AcquiredTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有優惠券失敗");
                return Enumerable.Empty<Coupon>();
            }
        }

        public async Task<IEnumerable<Coupon>> GetCouponsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Coupons
                    .AsNoTracking()
                    .Include(c => c.CouponType)
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.AcquiredTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者優惠券失敗: UserId={UserId}", userId);
                return Enumerable.Empty<Coupon>();
            }
        }

        public async Task<Coupon?> GetCouponByIdAsync(int couponId)
        {
            try
            {
                return await _context.Coupons
                    .AsNoTracking()
                    .Include(c => c.CouponType)
                    .FirstOrDefaultAsync(c => c.CouponId == couponId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得優惠券失敗: CouponId={CouponId}", couponId);
                return null;
            }
        }

        public async Task<Coupon?> GetCouponByCodeAsync(string couponCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(couponCode))
                {
                    _logger.LogWarning("優惠券代碼為空");
                    return null;
                }

                if (!ValidateCouponCodeFormat(couponCode))
                {
                    _logger.LogWarning("優惠券代碼格式無效: {CouponCode}", couponCode);
                    return null;
                }

                return await _context.Coupons
                    .AsNoTracking()
                    .Include(c => c.CouponType)
                    .FirstOrDefaultAsync(c => c.CouponCode == couponCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得優惠券失敗: CouponCode={CouponCode}", couponCode);
                return null;
            }
        }

        public async Task<bool> CreateCouponAsync(Coupon coupon)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (coupon == null)
                {
                    _logger.LogWarning("嘗試建立 null 優惠券");
                    return false;
                }

                if (!ValidateCouponCodeFormat(coupon.CouponCode))
                {
                    _logger.LogWarning("優惠券代碼格式無效: {CouponCode}", coupon.CouponCode);
                    return false;
                }

                coupon.AcquiredTime = DateTime.UtcNow;

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("優惠券建立成功: CouponId={CouponId}, CouponCode={CouponCode}",
                    coupon.CouponId, coupon.CouponCode);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "建立優惠券失敗: CouponCode={CouponCode}", coupon?.CouponCode);
                return false;
            }
        }

        public async Task<bool> UpdateCouponAsync(Coupon coupon)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (coupon == null)
                {
                    _logger.LogWarning("嘗試更新 null 優惠券");
                    return false;
                }

                var existingCoupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == coupon.CouponId);

                if (existingCoupon == null)
                {
                    _logger.LogWarning("優惠券不存在: CouponId={CouponId}", coupon.CouponId);
                    return false;
                }

                existingCoupon.CouponCode = coupon.CouponCode;
                existingCoupon.CouponTypeId = coupon.CouponTypeId;
                existingCoupon.UserId = coupon.UserId;
                existingCoupon.IsUsed = coupon.IsUsed;
                existingCoupon.AcquiredTime = coupon.AcquiredTime;
                existingCoupon.UsedTime = coupon.UsedTime;
                existingCoupon.UsedInOrderId = coupon.UsedInOrderId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("優惠券更新成功: CouponId={CouponId}", coupon.CouponId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "更新優惠券失敗: CouponId={CouponId}", coupon?.CouponId);
                return false;
            }
        }

        public async Task<bool> DeleteCouponAsync(int couponId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == couponId);

                if (coupon == null)
                {
                    _logger.LogWarning("優惠券不存在: CouponId={CouponId}", couponId);
                    return false;
                }

                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("優惠券刪除成功: CouponId={CouponId}", couponId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "刪除優惠券失敗: CouponId={CouponId}", couponId);
                return false;
            }
        }

        public async Task<bool> UseCouponAsync(int couponId, int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var coupon = await _context.Coupons
                    .Include(c => c.CouponType)
                    .FirstOrDefaultAsync(c => c.CouponId == couponId);

                if (coupon == null)
                {
                    _logger.LogWarning("優惠券不存在: CouponId={CouponId}", couponId);
                    return false;
                }

                if (coupon.IsUsed)
                {
                    _logger.LogWarning("優惠券已使用: CouponId={CouponId}", couponId);
                    return false;
                }

                if (coupon.CouponType != null)
                {
                    var now = DateTime.UtcNow;
                    if (now < coupon.CouponType.ValidFrom || now > coupon.CouponType.ValidTo)
                    {
                        _logger.LogWarning("優惠券已過期或尚未生效: CouponId={CouponId}, ValidFrom={ValidFrom}, ValidTo={ValidTo}",
                            couponId, coupon.CouponType.ValidFrom, coupon.CouponType.ValidTo);
                        return false;
                    }
                }

                coupon.IsUsed = true;
                coupon.UsedTime = DateTime.UtcNow;
                coupon.UsedInOrderId = orderId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("優惠券使用成功: CouponId={CouponId}, OrderId={OrderId}", couponId, orderId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "使用優惠券失敗: CouponId={CouponId}, OrderId={OrderId}", couponId, orderId);
                return false;
            }
        }

        public async Task<IEnumerable<CouponType>> GetAllCouponTypesAsync()
        {
            try
            {
                return await _context.CouponTypes
                    .AsNoTracking()
                    .OrderBy(ct => ct.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有優惠券類型失敗");
                return Enumerable.Empty<CouponType>();
            }
        }

        public async Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId)
        {
            try
            {
                return await _context.CouponTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct => ct.CouponTypeId == couponTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得優惠券類型失敗: CouponTypeId={CouponTypeId}", couponTypeId);
                return null;
            }
        }

        public async Task<bool> CreateCouponTypeAsync(CouponType couponType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (couponType == null)
                {
                    _logger.LogWarning("嘗試建立 null 優惠券類型");
                    return false;
                }

                _context.CouponTypes.Add(couponType);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("優惠券類型建立成功: CouponTypeId={CouponTypeId}, Name={Name}",
                    couponType.CouponTypeId, couponType.Name);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "建立優惠券類型失敗: Name={Name}", couponType?.Name);
                return false;
            }
        }

        public async Task<bool> UpdateCouponTypeAsync(CouponType couponType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (couponType == null)
                {
                    _logger.LogWarning("嘗試更新 null 優惠券類型");
                    return false;
                }

                var existingCouponType = await _context.CouponTypes
                    .FirstOrDefaultAsync(ct => ct.CouponTypeId == couponType.CouponTypeId);

                if (existingCouponType == null)
                {
                    _logger.LogWarning("優惠券類型不存在: CouponTypeId={CouponTypeId}", couponType.CouponTypeId);
                    return false;
                }

                existingCouponType.Name = couponType.Name;
                existingCouponType.DiscountType = couponType.DiscountType;
                existingCouponType.DiscountValue = couponType.DiscountValue;
                existingCouponType.MinSpend = couponType.MinSpend;
                existingCouponType.ValidFrom = couponType.ValidFrom;
                existingCouponType.ValidTo = couponType.ValidTo;
                existingCouponType.PointsCost = couponType.PointsCost;
                existingCouponType.Description = couponType.Description;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("優惠券類型更新成功: CouponTypeId={CouponTypeId}", couponType.CouponTypeId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "更新優惠券類型失敗: CouponTypeId={CouponTypeId}", couponType?.CouponTypeId);
                return false;
            }
        }

        public async Task<bool> DeleteCouponTypeAsync(int couponTypeId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var couponType = await _context.CouponTypes
                    .FirstOrDefaultAsync(ct => ct.CouponTypeId == couponTypeId);

                if (couponType == null)
                {
                    _logger.LogWarning("優惠券類型不存在: CouponTypeId={CouponTypeId}", couponTypeId);
                    return false;
                }

                _context.CouponTypes.Remove(couponType);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("優惠券類型刪除成功: CouponTypeId={CouponTypeId}", couponTypeId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "刪除優惠券類型失敗: CouponTypeId={CouponTypeId}", couponTypeId);
                return false;
            }
        }

        public async Task<bool> GrantCouponToUserAsync(int userId, int couponTypeId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var couponType = await _context.CouponTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct => ct.CouponTypeId == couponTypeId);

                if (couponType == null)
                {
                    _logger.LogWarning("優惠券類型不存在: CouponTypeId={CouponTypeId}", couponTypeId);
                    return false;
                }

                var couponCode = GenerateCouponCode();

                var coupon = new Coupon
                {
                    CouponCode = couponCode,
                    CouponTypeId = couponTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                };

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("發放優惠券成功: UserId={UserId}, CouponTypeId={CouponTypeId}, CouponCode={CouponCode}",
                    userId, couponTypeId, couponCode);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "發放優惠券失敗: UserId={UserId}, CouponTypeId={CouponTypeId}", userId, couponTypeId);
                return false;
            }
        }

        private string GenerateCouponCode()
        {
            var random = new Random();
            var yearMonth = DateTime.UtcNow.ToString("yyMM");
            var randomCode = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"CPN-{yearMonth}-{randomCode}";
        }

        private bool ValidateCouponCodeFormat(string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return false;

            var parts = couponCode.Split('-');
            if (parts.Length != 3)
                return false;

            if (parts[0] != "CPN")
                return false;

            if (parts[1].Length != 4 || !parts[1].All(char.IsDigit))
                return false;

            if (parts[2].Length != 6 || !parts[2].All(c => char.IsLetterOrDigit(c)))
                return false;

            return true;
        }
    }
}
