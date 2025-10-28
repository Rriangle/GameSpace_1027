using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IUserWalletService
    {
        Task<PagedResult<UserWallet>> GetUserWalletsAsync(WalletQueryModel query);
        Task<UserWallet> GetUserWalletAsync(int userId);
        Task<bool> UpdateUserPointsAsync(int userId, int points, string description);
        Task<bool> IssueCouponAsync(int userId, int couponTypeId, string description);
        Task<bool> IssueEVoucherAsync(int userId, int evoucherTypeId, string description);
        Task<PagedResult<WalletHistory>> GetWalletHistoryAsync(WalletHistoryQueryModel query);
        Task<bool> ExportWalletHistoryAsync(WalletHistoryQueryModel query, string filePath);
    }
}


