using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    public class WalletService : IWalletService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(GameSpacedatabaseContext context, ILogger<WalletService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Wallet 基本操作
        public async Task<UserWallet?> GetWalletByUserIdAsync(int userId)
        {
            return await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<UserWallet?> GetWalletByUserIdReadOnlyAsync(int userId)
        {
            return await _context.UserWallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<int> GetUserPointsAsync(int userId)
        {
            var wallet = await GetWalletByUserIdReadOnlyAsync(userId);
            return wallet?.UserPoint ?? 0;
        }

        public async Task<bool> AddPointsAsync(int userId, int points, string description, string itemCode = "")
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                wallet.UserPoint += points;

                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Add",
                    PointsChanged = points,
                    ItemCode = itemCode,
                    Description = description,
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "添加點數失敗: UserId={UserId}, Points={Points}", userId, points);
                return false;
            }
        }

        public async Task<bool> DeductPointsAsync(int userId, int points, string description, string itemCode = "")
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null || wallet.UserPoint < points) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                wallet.UserPoint -= points;

                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Deduct",
                    PointsChanged = -points,
                    ItemCode = itemCode,
                    Description = description,
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "扣除點數失敗: UserId={UserId}, Points={Points}", userId, points);
                return false;
            }
        }

        public async Task<bool> TransferPointsAsync(int fromUserId, int toUserId, int points, string description)
        {
            var fromWallet = await GetWalletByUserIdAsync(fromUserId);
            var toWallet = await GetWalletByUserIdAsync(toUserId);

            if (fromWallet == null || toWallet == null || fromWallet.UserPoint < points)
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                fromWallet.UserPoint -= points;
                toWallet.UserPoint += points;

                var historyFrom = new WalletHistory
                {
                    UserId = fromUserId,
                    ChangeType = "Transfer_Out",
                    PointsChanged = -points,
                    ItemCode = $"TO_USER_{toUserId}",
                    Description = $"轉帳給用戶 {toUserId}: {description}",
                    ChangeTime = DateTime.UtcNow
                };

                var historyTo = new WalletHistory
                {
                    UserId = toUserId,
                    ChangeType = "Transfer_In",
                    PointsChanged = points,
                    ItemCode = $"FROM_USER_{fromUserId}",
                    Description = $"收到來自用戶 {fromUserId} 的轉帳: {description}",
                    ChangeTime = DateTime.UtcNow
                };

                _context.WalletHistories.Add(historyFrom);
                _context.WalletHistories.Add(historyTo);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "轉帳失敗: FromUserId={FromUserId}, ToUserId={ToUserId}, Points={Points}", fromUserId, toUserId, points);
                return false;
            }
        }

        // Wallet 歷史記錄查詢
        public async Task<IEnumerable<WalletHistory>> GetWalletHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20)
        {
            return await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.ChangeTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<WalletHistory>> GetWalletHistoryByTypeAsync(int userId, string changeType)
        {
            return await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.UserId == userId && h.ChangeType == changeType)
                .OrderByDescending(h => h.ChangeTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<WalletHistory>> GetWalletHistoryByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.UserId == userId && h.ChangeTime >= startDate && h.ChangeTime <= endDate)
                .OrderByDescending(h => h.ChangeTime)
                .ToListAsync();
        }

        public async Task<WalletHistory?> GetHistoryDetailAsync(int logId)
        {
            return await _context.WalletHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.LogId == logId);
        }

        public async Task<int> GetTotalHistoryCountAsync(int userId)
        {
            return await _context.WalletHistories
                .Where(h => h.UserId == userId)
                .CountAsync();
        }

        // Wallet 統計數據
        public async Task<Dictionary<string, int>> GetPointsSummaryAsync(int userId)
        {
            var wallet = await GetWalletByUserIdReadOnlyAsync(userId);
            var history = await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.UserId == userId)
                .ToListAsync();

            var totalEarned = history
                .Where(h => h.PointsChanged > 0)
                .Sum(h => h.PointsChanged);

            var totalSpent = history
                .Where(h => h.PointsChanged < 0)
                .Sum(h => Math.Abs(h.PointsChanged));

            return new Dictionary<string, int>
            {
                { "CurrentPoints", wallet?.UserPoint ?? 0 },
                { "TotalEarned", totalEarned },
                { "TotalSpent", totalSpent },
                { "TransactionCount", history.Count }
            };
        }

        public async Task<Dictionary<string, int>> GetPointsStatsByTypeAsync(int userId)
        {
            var history = await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.UserId == userId)
                .GroupBy(h => h.ChangeType)
                .Select(g => new { Type = g.Key, Total = g.Sum(h => h.PointsChanged) })
                .ToListAsync();

            return history.ToDictionary(x => x.Type, x => x.Total);
        }

        public async Task<int> GetTotalPointsEarnedAsync(int userId)
        {
            return await _context.WalletHistories
                .Where(h => h.UserId == userId && h.PointsChanged > 0)
                .SumAsync(h => h.PointsChanged);
        }

        public async Task<int> GetTotalPointsSpentAsync(int userId)
        {
            return await _context.WalletHistories
                .Where(h => h.UserId == userId && h.PointsChanged < 0)
                .SumAsync(h => Math.Abs(h.PointsChanged));
        }

        // 管理員功能
        public async Task<bool> AdjustUserPointsAsync(int userId, int points, string reason, int? managerId = null)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                wallet.UserPoint += points;

                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = points > 0 ? "Admin_Add" : "Admin_Deduct",
                    PointsChanged = points,
                    ItemCode = $"ADMIN_{managerId ?? 0}",
                    Description = $"管理員調整: {reason}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "調整用戶點數失敗: UserId={UserId}, Points={Points}", userId, points);
                return false;
            }
        }

        public async Task<bool> ResetUserPointsAsync(int userId, string reason, int? managerId = null)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentPoints = wallet.UserPoint;
                wallet.UserPoint = 0;

                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Admin_Reset",
                    PointsChanged = -currentPoints,
                    ItemCode = $"ADMIN_RESET_{managerId ?? 0}",
                    Description = $"管理員重置: {reason} (原有點數: {currentPoints})",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "重置用戶點數失敗: UserId={UserId}", userId);
                return false;
            }
        }

        public async Task<IEnumerable<WalletHistory>> GetAllHistoryAsync(int pageNumber = 1, int pageSize = 50)
        {
            return await _context.WalletHistories
                .AsNoTracking()
                .OrderByDescending(h => h.ChangeTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // 批次操作
        public async Task<bool> GrantPointsToMultipleUsersAsync(IEnumerable<int> userIds, int points, string description)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var userId in userIds)
                {
                    var wallet = await GetWalletByUserIdAsync(userId);
                    if (wallet != null)
                    {
                        wallet.UserPoint += points;

                        var history = new WalletHistory
                        {
                            UserId = userId,
                            ChangeType = "Batch_Grant",
                            PointsChanged = points,
                            ItemCode = "BATCH_OPERATION",
                            Description = $"批次發放: {description}",
                            ChangeTime = DateTime.UtcNow
                        };
                        _context.WalletHistories.Add(history);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次發放點數失敗: UserCount={UserCount}, Points={Points}", userIds.Count(), points);
                return false;
            }
        }

        public async Task<IEnumerable<UserWallet>> GetTopWalletsAsync(int count = 10)
        {
            return await _context.UserWallets
                .AsNoTracking()
                .OrderByDescending(w => w.UserPoint)
                .Take(count)
                .ToListAsync();
        }
    }
}

