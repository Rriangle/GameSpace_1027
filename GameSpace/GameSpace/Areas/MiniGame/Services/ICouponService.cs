using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface ICouponService
    {
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();
        Task<IEnumerable<Coupon>> GetCouponsByUserIdAsync(int userId);
        Task<Coupon?> GetCouponByIdAsync(int couponId);
        Task<Coupon?> GetCouponByCodeAsync(string couponCode);
        Task<bool> CreateCouponAsync(Coupon coupon);
        Task<bool> UpdateCouponAsync(Coupon coupon);
        Task<bool> DeleteCouponAsync(int couponId);
        Task<bool> UseCouponAsync(int couponId, int orderId);
        Task<IEnumerable<CouponType>> GetAllCouponTypesAsync();
        Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId);
        Task<bool> CreateCouponTypeAsync(CouponType couponType);
        Task<bool> UpdateCouponTypeAsync(CouponType couponType);
        Task<bool> DeleteCouponTypeAsync(int couponTypeId);
        Task<bool> GrantCouponToUserAsync(int userId, int couponTypeId);
    }
}

