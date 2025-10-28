using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface ICouponTypeService
    {
        // CouponType 基本 CRUD
        Task<IEnumerable<CouponType>> GetAllCouponTypesAsync();
        Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId);
        Task<bool> CreateCouponTypeAsync(CouponType couponType);
        Task<bool> UpdateCouponTypeAsync(CouponType couponType);
        Task<bool> DeleteCouponTypeAsync(int couponTypeId);

        // CouponType 狀態管理
        Task<bool> ActivateCouponTypeAsync(int couponTypeId);
        Task<bool> DeactivateCouponTypeAsync(int couponTypeId);

        // CouponType 查詢
        Task<IEnumerable<CouponType>> GetActiveCouponTypesAsync();
        Task<IEnumerable<CouponType>> GetCouponTypesByDiscountTypeAsync(string discountType);
        Task<IEnumerable<CouponType>> GetCouponTypesByPointsCostAsync(int minPoints, int maxPoints);

        // CouponType 統計
        Task<int> GetTotalCouponTypesCountAsync();
        Task<int> GetActiveCouponTypesCountAsync();
        Task<Dictionary<string, int>> GetCouponTypesDistributionAsync();
        Task<IEnumerable<CouponTypeUsageStats>> GetCouponTypeUsageStatsAsync();
    }

    public class CouponTypeUsageStats
    {
        public int CouponTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalIssued { get; set; }
        public int TotalUsed { get; set; }
        public int TotalUnused { get; set; }
        public decimal UsageRate { get; set; }
    }
}

