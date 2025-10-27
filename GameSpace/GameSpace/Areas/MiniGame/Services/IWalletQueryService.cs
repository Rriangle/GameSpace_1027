using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// ?Ｗ??亥岷??隞
    /// </summary>
    public interface IWalletQueryService
    {
        /// <summary>
        /// ?亥岷?暺
        /// </summary>
        Task<PagedResult<WalletPointRecord>> QueryUserPointsAsync(WalletQueryModel query);

        /// <summary>
        /// ?亥岷??芣???
        /// </summary>
        Task<PagedResult<UserCouponReadModel>> QueryUserCouponsAsync(CouponQueryModel query);

        /// <summary>
        /// ?亥岷??餃?蝳桀
        /// </summary>
        Task<PagedResult<Models.ViewModels.EVoucherReadModel>> QueryUserEVouchersAsync(EVoucherQueryModel query);

        /// <summary>
        /// ?亥岷?Ｗ?鈭斗?甇瑕
        /// </summary>
        Task<PagedResult<WalletHistoryRecord>> QueryWalletHistoryAsync(WalletHistoryQueryModel query);

        /// <summary>
        /// ???冽?桀?暺
        /// </summary>
        Task<int> GetUserPointsAsync(int userId);

        /// <summary>
        /// ???冽?舐?芣??豢??
        /// </summary>
        Task<int> GetUserAvailableCouponsCountAsync(int userId);

        /// <summary>
        /// ???冽?舐?餃?蝳桀?賊?
        /// </summary>
        Task<int> GetUserAvailableEVouchersCountAsync(int userId);
    }
}
