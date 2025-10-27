using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(GameSpacedatabaseContext context, ILogger<DashboardService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 總覽統計
        public async Task<DashboardOverview> GetDashboardOverviewAsync()
        {
            var today = DateTime.UtcNow.Date;

            var overview = new DashboardOverview
            {
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(),
                ActiveUsers = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.UserRight)
                    .CountAsync(u => u.UserRight != null && u.UserRight.UserStatus == true),
                TotalGames = 0, // 需要 Game 表
                TodayGames = 0, // 需要 GameHistory 表
                TotalRevenue = 0, // 需要收入記錄
                TodayRevenue = 0, // 需要收入記錄
                OnlineUsers = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.UserRight)
                    .CountAsync(u => u.UserRight != null && u.UserRight.UserStatus == true)
            };

            return overview;
        }

        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddMonths(-1);

            var stats = new UserStatistics
            {
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(),
                ActiveUsers = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.UserRight)
                    .CountAsync(u => u.UserRight != null && u.UserRight.UserStatus == true),
                NewUsersToday = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.UserIntroduce)
                    .CountAsync(u => u.UserIntroduce != null && u.UserIntroduce.CreateAccount >= today),
                NewUsersThisWeek = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.UserIntroduce)
                    .CountAsync(u => u.UserIntroduce != null && u.UserIntroduce.CreateAccount >= weekAgo),
                NewUsersThisMonth = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.UserIntroduce)
                    .CountAsync(u => u.UserIntroduce != null && u.UserIntroduce.CreateAccount >= monthAgo),
                AverageSessionTime = 0 // 需要 Session 記錄表
            };

            return stats;
        }

        public async Task<GameStatistics> GetGameStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddMonths(-1);

            // 簡化版：使用簽到作為遊戲活動指標
            var stats = new GameStatistics
            {
                TotalGamesPlayed = await _context.UserSignInStats.AsNoTracking().CountAsync(),
                GamesToday = await _context.UserSignInStats.AsNoTracking().CountAsync(s => s.SignTime >= today),
                GamesThisWeek = await _context.UserSignInStats.AsNoTracking().CountAsync(s => s.SignTime >= weekAgo),
                GamesThisMonth = await _context.UserSignInStats.AsNoTracking().CountAsync(s => s.SignTime >= monthAgo),
                AverageGameDuration = 0,
                MostPopularGame = "寵物養成"
            };

            return stats;
        }

        public async Task<RevenueStatistics> GetRevenueStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddMonths(-1);

            // 使用點數消耗作為收入指標
            var allHistory = await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.PointsChanged < 0)
                .ToListAsync();

            var stats = new RevenueStatistics
            {
                TotalRevenue = Math.Abs(allHistory.Sum(h => h.PointsChanged)),
                RevenueToday = Math.Abs(allHistory.Where(h => h.ChangeTime >= today).Sum(h => h.PointsChanged)),
                RevenueThisWeek = Math.Abs(allHistory.Where(h => h.ChangeTime >= weekAgo).Sum(h => h.PointsChanged)),
                RevenueThisMonth = Math.Abs(allHistory.Where(h => h.ChangeTime >= monthAgo).Sum(h => h.PointsChanged)),
                AverageRevenuePerUser = 0
            };

            var userCount = await _context.Users.AsNoTracking().CountAsync();
            if (userCount > 0)
            {
                stats.AverageRevenuePerUser = stats.TotalRevenue / userCount;
            }

            return stats;
        }

        // 趨勢數據
        public async Task<Dictionary<string, int>> GetUserGrowthTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserIntroduce)
                .Where(u => u.UserIntroduce != null && u.UserIntroduce.CreateAccount >= startDate)
                .GroupBy(u => u.UserIntroduce.CreateAccount.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return users.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => x.Count
            );
        }

        public async Task<Dictionary<string, int>> GetGamePlayTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var signIns = await _context.UserSignInStats
                .AsNoTracking()
                .Where(s => s.SignTime >= startDate)
                .GroupBy(s => s.SignTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return signIns.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => x.Count
            );
        }

        public async Task<Dictionary<string, decimal>> GetRevenueTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var revenue = await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.ChangeTime >= startDate && h.PointsChanged < 0)
                .GroupBy(h => h.ChangeTime.Date)
                .Select(g => new { Date = g.Key, Total = Math.Abs(g.Sum(h => h.PointsChanged)) })
                .ToListAsync();

            return revenue.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => (decimal)x.Total
            );
        }

        // 排行榜
        public async Task<IEnumerable<TopUser>> GetTopUsersAsync(int count = 10)
        {
            var topUsers = await _context.Users
                .AsNoTracking()
                .Join(_context.UserWallets,
                    u => u.UserId,
                    w => w.UserId,
                    (u, w) => new { User = u, Wallet = w })
                .GroupJoin(_context.Pets,
                    uw => uw.User.UserId,
                    p => p.UserId,
                    (uw, pets) => new { uw.User, uw.Wallet, Pet = pets.FirstOrDefault() })
                .GroupJoin(_context.UserSignInStats,
                    uwp => uwp.User.UserId,
                    s => s.UserId,
                    (uwp, signIns) => new TopUser
                    {
                        UserId = uwp.User.UserId,
                        UserName = uwp.User.UserAccount,
                        Points = uwp.Wallet.UserPoint,
                        Level = uwp.Pet != null ? uwp.Pet.Level : 1,
                        GamesPlayed = signIns.Count()
                    })
                .OrderByDescending(u => u.Points)
                .ThenByDescending(u => u.Level)
                .Take(count)
                .ToListAsync();

            return topUsers;
        }

        public async Task<IEnumerable<TopGame>> GetTopGamesAsync(int count = 10)
        {
            // 簡化版：使用寵物互動作為遊戲指標
            var topGames = new List<TopGame>
            {
                new TopGame
                {
                    GameId = 1,
                    GameName = "寵物養成",
                    PlayCount = await _context.Pets.AsNoTracking().CountAsync(),
                    AverageScore = 0
                },
                new TopGame
                {
                    GameId = 2,
                    GameName = "每日簽到",
                    PlayCount = await _context.UserSignInStats.AsNoTracking().CountAsync(),
                    AverageScore = 0
                }
            };

            return topGames.OrderByDescending(g => g.PlayCount).Take(count);
        }

        // 最近活動
        public async Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(int count = 20)
        {
            var activities = new List<RecentActivity>();

            // 最近簽到
            var recentSignIns = await _context.UserSignInStats
                .AsNoTracking()
                .OrderByDescending(s => s.SignTime)
                .Take(count / 2)
                .ToListAsync();

            // 手動載入用戶資料
            var userIds = recentSignIns.Select(s => s.UserId).Distinct().ToList();
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId);

            activities.AddRange(recentSignIns.Select(s =>
            {
                users.TryGetValue(s.UserId, out var userData);
                return new RecentActivity
                {
                    ActivityType = "SignIn",
                    Description = $"完成簽到，獲得 {s.PointsGained} 點數",
                    UserName = userData?.UserAccount ?? "未知用戶",
                    Timestamp = s.SignTime
                };
            }));

            // 最近錢包變動
            var recentWallet = await _context.WalletHistories
                .AsNoTracking()
                .OrderByDescending(h => h.ChangeTime)
                .Take(count / 2)
                .ToListAsync();

            // 手動載入錢包用戶資料
            var walletUserIds = recentWallet.Select(h => h.UserId).Distinct().ToList();
            var walletUsers = await _context.Users
                .AsNoTracking()
                .Where(u => walletUserIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId);

            activities.AddRange(recentWallet.Select(h =>
            {
                walletUsers.TryGetValue(h.UserId, out var userData);
                return new RecentActivity
                {
                    ActivityType = h.ChangeType,
                    Description = $"{h.Description} ({(h.PointsChanged > 0 ? "+" : "")}{h.PointsChanged} 點數)",
                    UserName = userData?.UserAccount ?? "未知用戶",
                    Timestamp = h.ChangeTime
                };
            }));

            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(count);
        }

        // 警報
        public async Task<IEnumerable<SystemAlert>> GetActiveAlertsAsync()
        {
            var alerts = new List<SystemAlert>();

            // 檢查資料庫連線
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                alerts.Add(new SystemAlert
                {
                    Level = "Critical",
                    Message = "資料庫連線失敗",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 檢查低點數使用者
            var lowPointUsers = await _context.UserWallets
                .AsNoTracking()
                .CountAsync(w => w.UserPoint < 10);
            if (lowPointUsers > 0)
            {
                alerts.Add(new SystemAlert
                {
                    Level = "Warning",
                    Message = $"有 {lowPointUsers} 位使用者點數低於 10",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 檢查優惠券庫存
            var expiringSoon = await _context.Coupons
                .AsNoTracking()
                .Include(c => c.CouponType)
                .CountAsync(c => !c.IsUsed && c.CouponType.ValidTo <= DateTime.UtcNow.AddDays(7));
            if (expiringSoon > 0)
            {
                alerts.Add(new SystemAlert
                {
                    Level = "Info",
                    Message = $"有 {expiringSoon} 張優惠券將在 7 天內到期",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return alerts.OrderByDescending(a => a.Level).ThenByDescending(a => a.CreatedAt);
        }
    }
}

