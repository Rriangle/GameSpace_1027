using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IWalletService
    {
        // 錢包基本操作
        Task<UserWallet?> GetWalletByUserIdAsync(int userId);
        Task<UserWallet?> GetWalletByUserIdReadOnlyAsync(int userId);
        Task<int> GetUserPointsAsync(int userId);
        Task<bool> AddPointsAsync(int userId, int points, string description, string itemCode = "");
        Task<bool> DeductPointsAsync(int userId, int points, string description, string itemCode = "");
        Task<bool> TransferPointsAsync(int fromUserId, int toUserId, int points, string description);

        // 錢包歷史查詢
        Task<IEnumerable<GameSpace.Models.WalletHistory>> GetWalletHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<GameSpace.Models.WalletHistory>> GetWalletHistoryByTypeAsync(int userId, string changeType);
        Task<IEnumerable<GameSpace.Models.WalletHistory>> GetWalletHistoryByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<GameSpace.Models.WalletHistory?> GetHistoryDetailAsync(int logId);
        Task<int> GetTotalHistoryCountAsync(int userId);

        // 錢包統計
        Task<Dictionary<string, int>> GetPointsSummaryAsync(int userId);
        Task<Dictionary<string, int>> GetPointsStatsByTypeAsync(int userId);
        Task<int> GetTotalPointsEarnedAsync(int userId);
        Task<int> GetTotalPointsSpentAsync(int userId);

        // 批次操作
        Task<bool> GrantPointsToMultipleUsersAsync(IEnumerable<int> userIds, int points, string description);
        Task<IEnumerable<UserWallet>> GetTopWalletsAsync(int count = 10);

        // 管理功能
        Task<bool> AdjustUserPointsAsync(int userId, int points, string reason, int? managerId = null);
        Task<bool> ResetUserPointsAsync(int userId, string reason, int? managerId = null);
        Task<IEnumerable<GameSpace.Models.WalletHistory>> GetAllHistoryAsync(int pageNumber = 1, int pageSize = 50);
    }
}

