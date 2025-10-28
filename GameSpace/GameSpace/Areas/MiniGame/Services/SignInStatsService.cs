using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GameSpace.Areas.MiniGame.Services
{
    public class SignInStatsService : ISignInStatsService
    {
        private readonly string _connectionString;
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<SignInStatsService> _logger;
        private static readonly TimeZoneInfo TaipeiTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

        public SignInStatsService(IConfiguration configuration, GameSpacedatabaseContext context, ILogger<SignInStatsService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private DateTime GetTaipeiNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TaipeiTimeZone);
        }

        private (DateTime startUtc, DateTime endUtc) GetTaipeiDateRange(DateTime taipeiDate)
        {
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(taipeiDate.Date, TaipeiTimeZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(taipeiDate.Date.AddDays(1), TaipeiTimeZone);
            return (startUtc, endUtc);
        }

        // ========== 介面方法實作 ==========

        public async Task<PagedResult<UserSignInStats>> GetSignInStatsAsync(SignInStatsQueryModel query)
        {
            try
            {
                // UserSignInStats (DbSet) 返回 UserSignInStat (實體)
                var queryable = _context.UserSignInStats.AsQueryable();

                // 篩選條件
                if (query.UserId.HasValue)
                    queryable = queryable.Where(s => s.UserId == query.UserId.Value);

                if (query.StartDate.HasValue)
                    queryable = queryable.Where(s => s.SignTime >= query.StartDate.Value);

                if (query.EndDate.HasValue)
                    queryable = queryable.Where(s => s.SignTime <= query.EndDate.Value);

                if (query.MinConsecutiveDays.HasValue)
                    queryable = queryable.Where(s => s.PointsGained >= query.MinConsecutiveDays.Value);

                var totalCount = await queryable.CountAsync();

                // 排序 - 使用 SortBy 屬性
                if (!string.IsNullOrEmpty(query.SortBy))
                {
                    if (query.Descending)
                        queryable = queryable.OrderByDescending(s => EF.Property<object>(s, query.SortBy));
                    else
                        queryable = queryable.OrderBy(s => EF.Property<object>(s, query.SortBy));
                }
                else
                {
                    queryable = queryable.OrderByDescending(s => s.SignTime);
                }

                // 分頁
                var dbItems = await queryable
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                // 將資料庫實體 UserSignInStat 轉換為 ViewModel UserSignInStats
                var items = dbItems.Select(s => new GameSpace.Areas.MiniGame.Models.UserSignInStats
                {
                    StatsID = s.LogId,
                    UserID = s.UserId,
                    SignTime = s.SignTime,
                    PointsEarned = s.PointsGained,
                    PetExpEarned = s.ExpGained,
                    CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? null : (int.TryParse(s.CouponGained, out var couponId) ? (int?)couponId : null),
                    ConsecutiveDays = 1 // 需要計算連續天數，暫時設為1
                }).ToList();

                return new PagedResult<GameSpace.Areas.MiniGame.Models.UserSignInStats>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到統計失敗: UserId={UserId}, PageNumber={PageNumber}",
                    query.UserId, query.PageNumber);
                return new PagedResult<GameSpace.Areas.MiniGame.Models.UserSignInStats>();
            }
        }

        public async Task<SignInStatsSummary> GetSignInStatsSummaryAsync()
        {
            try
            {
                var nowTaiwan = GetTaipeiNow();
                var todayTaiwan = nowTaiwan.Date;
                var (todayStartUtc, todayEndUtc) = GetTaipeiDateRange(todayTaiwan);

                var weekStart = todayTaiwan.AddDays(-(int)todayTaiwan.DayOfWeek);
                var weekStartUtc = TimeZoneInfo.ConvertTimeToUtc(weekStart, TaipeiTimeZone);

                var monthStart = new DateTime(todayTaiwan.Year, todayTaiwan.Month, 1);
                var monthStartUtc = TimeZoneInfo.ConvertTimeToUtc(monthStart, TaipeiTimeZone);

                var allStats = await _context.UserSignInStats.ToListAsync();

                return new SignInStatsSummary
                {
                    TodaySignInCount = allStats.Count(s => s.SignTime >= todayStartUtc && s.SignTime < todayEndUtc),
                    ThisWeekSignInCount = allStats.Count(s => s.SignTime >= weekStartUtc),
                    ThisMonthSignInCount = allStats.Count(s => s.SignTime >= monthStartUtc),
                    MaxConsecutiveDays = allStats.Any() ? allStats.Max(s => s.PointsGained) : 0,
                    PerfectAttendanceCount = allStats.Count(s => s.PointsGained >= DateTime.DaysInMonth(todayTaiwan.Year, todayTaiwan.Month)),
                    TotalPointsGranted = allStats.Sum(s => s.PointsGained),
                    TotalExpGranted = allStats.Sum(s => s.ExpGained),
                    TotalCouponsGranted = allStats.Count(s => !string.IsNullOrEmpty(s.CouponGained))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到統計摘要失敗");
                return new SignInStatsSummary();
            }
        }

        public async Task<bool> ConfigureSignInRulesAsync(SignInRulesModel rules)
        {
            await Task.CompletedTask;
            return false;
        }

        public async Task<SignInRulesModel> GetSignInRulesAsync()
        {
            await Task.CompletedTask;
            return new SignInRulesModel();
        }

        // ========== 原有方法 ==========

        public async Task<IEnumerable<UserSignInStats>> GetAllSignInStatsAsync()
        {
            try
            {
                var dbItems = await _context.UserSignInStats
                    .OrderByDescending(s => s.SignTime)
                    .ToListAsync();

                return dbItems.Select(s => new GameSpace.Areas.MiniGame.Models.UserSignInStats
                {
                    StatsID = s.LogId,
                    UserID = s.UserId,
                    SignTime = s.SignTime,
                    PointsEarned = s.PointsGained,
                    PetExpEarned = s.ExpGained,
                    CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? null : (int.TryParse(s.CouponGained, out var couponId) ? (int?)couponId : null),
                    ConsecutiveDays = 1
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有簽到統計失敗");
                return new List<GameSpace.Areas.MiniGame.Models.UserSignInStats>();
            }
        }

        public async Task<IEnumerable<UserSignInStats>> GetSignInStatsByUserIdAsync(int userId)
        {
            try
            {
                var dbItems = await _context.UserSignInStats
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SignTime)
                    .ToListAsync();

                return dbItems.Select(s => new GameSpace.Areas.MiniGame.Models.UserSignInStats
                {
                    StatsID = s.LogId,
                    UserID = s.UserId,
                    SignTime = s.SignTime,
                    PointsEarned = s.PointsGained,
                    PetExpEarned = s.ExpGained,
                    CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? null : (int.TryParse(s.CouponGained, out var couponId) ? (int?)couponId : null),
                    ConsecutiveDays = 1
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得用戶簽到統計失敗: UserId={UserId}", userId);
                return new List<GameSpace.Areas.MiniGame.Models.UserSignInStats>();
            }
        }

        public async Task<UserSignInStats?> GetSignInStatsByIdAsync(int statsId)
        {
            try
            {
                var dbItem = await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.LogId == statsId);

                if (dbItem == null) return null;

                return new GameSpace.Areas.MiniGame.Models.UserSignInStats
                {
                    StatsID = dbItem.LogId,
                    UserID = dbItem.UserId,
                    SignTime = dbItem.SignTime,
                    PointsEarned = dbItem.PointsGained,
                    PetExpEarned = dbItem.ExpGained,
                    CouponEarned = string.IsNullOrEmpty(dbItem.CouponGained) ? null : (int.TryParse(dbItem.CouponGained, out var couponId) ? (int?)couponId : null),
                    ConsecutiveDays = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到統計失敗: StatsId={StatsId}", statsId);
                return null;
            }
        }

        public async Task<bool> CreateSignInStatsAsync(UserSignInStats stats)
        {
            try
            {
                var signInLog = new UserSignInStat
                {
                    UserId = stats.UserID,
                    SignTime = stats.SignTime,
                    PointsGained = stats.PointsEarned,
                    ExpGained = stats.PetExpEarned,
                    CouponGained = stats.CouponEarned.HasValue ? stats.CouponEarned.Value.ToString() : string.Empty
                };

                _context.UserSignInStats.Add(signInLog);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建簽到統計失敗: UserId={UserId}", stats.UserID);
                return false;
            }
        }

        public async Task<bool> UpdateSignInStatsAsync(UserSignInStats stats)
        {
            try
            {
                var signInLog = await _context.UserSignInStats.FirstOrDefaultAsync(s => s.LogId == stats.StatsID);
                if (signInLog == null) return false;

                signInLog.UserId = stats.UserID;
                signInLog.SignTime = stats.SignTime;
                signInLog.PointsGained = stats.PointsEarned;
                signInLog.ExpGained = stats.PetExpEarned;
                signInLog.CouponGained = stats.CouponEarned.HasValue ? stats.CouponEarned.Value.ToString() : string.Empty;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新簽到統計失敗: StatsId={StatsId}", stats.StatsID);
                return false;
            }
        }

        public async Task<bool> DeleteSignInStatsAsync(int statsId)
        {
            try
            {
                var signInLog = await _context.UserSignInStats.FirstOrDefaultAsync(s => s.LogId == statsId);
                if (signInLog == null) return false;

                _context.UserSignInStats.Remove(signInLog);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除簽到統計失敗: StatsId={StatsId}", statsId);
                return false;
            }
        }

        public async Task<UserSignInStats?> GetTodaySignInAsync(int userId)
        {
            try
            {
                var nowTaiwan = GetTaipeiNow();
                var todayTaiwan = nowTaiwan.Date;
                var (todayStartUtc, todayEndUtc) = GetTaipeiDateRange(todayTaiwan);

                var dbItem = await _context.UserSignInStats
                    .Where(s => s.UserId == userId && s.SignTime >= todayStartUtc && s.SignTime < todayEndUtc)
                    .OrderByDescending(s => s.SignTime)
                    .FirstOrDefaultAsync();

                if (dbItem == null) return null;

                return new GameSpace.Areas.MiniGame.Models.UserSignInStats
                {
                    StatsID = dbItem.LogId,
                    UserID = dbItem.UserId,
                    SignTime = dbItem.SignTime,
                    PointsEarned = dbItem.PointsGained,
                    PetExpEarned = dbItem.ExpGained,
                    CouponEarned = string.IsNullOrEmpty(dbItem.CouponGained) ? null : (int.TryParse(dbItem.CouponGained, out var couponId) ? (int?)couponId : null),
                    ConsecutiveDays = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得今日簽到失敗: UserId={UserId}", userId);
                return null;
            }
        }

        public async Task<int> GetConsecutiveDaysAsync(int userId)
        {
            try
            {
                var signIns = await _context.UserSignInStats
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SignTime)
                    .ToListAsync();

                if (!signIns.Any()) return 0;

                int consecutiveDays = 1;
                var currentDate = signIns.First().SignTime.Date;

                for (int i = 1; i < signIns.Count; i++)
                {
                    var previousDate = signIns[i].SignTime.Date;
                    if (currentDate.AddDays(-1) == previousDate)
                    {
                        consecutiveDays++;
                        currentDate = previousDate;
                    }
                    else
                    {
                        break;
                    }
                }

                return consecutiveDays;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得連續簽到天數失敗: UserId={UserId}", userId);
                return 0;
            }
        }

        public async Task<bool> ProcessDailySignInAsync(int userId)
        {
            // Check if user already signed in today
            var todaySignIn = await GetTodaySignInAsync(userId);
            if (todaySignIn != null)
            {
                return false; // Already signed in today
            }

            // Get consecutive days
            var consecutiveDays = await GetConsecutiveDaysAsync(userId);
            
            // Check if user signed in yesterday
            var wasYesterday = await CheckYesterdaySignInAsync(userId);
            if (!wasYesterday)
            {
                consecutiveDays = 1; // Reset consecutive days
            }
            else
            {
                consecutiveDays++; // Increment consecutive days
            }

            // Calculate rewards based on consecutive days
            var pointsEarned = CalculatePointsReward(consecutiveDays);
            var petExpEarned = CalculatePetExpReward(consecutiveDays);
            var couponEarned = ShouldGrantCoupon(consecutiveDays) ? 1 : (int?)null;

            // Create sign-in record
            var signInStats = new UserSignInStats
            {
                UserID = userId,
                SignTime = DateTime.UtcNow,
                PointsEarned = pointsEarned,
                PetExpEarned = petExpEarned,
                CouponEarned = couponEarned,
                ConsecutiveDays = consecutiveDays
            };

            return await CreateSignInStatsAsync(signInStats);
        }

        private async Task<bool> CheckYesterdaySignInAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT COUNT(*) FROM UserSignInStats 
                WHERE UserID = @UserId 
                AND CAST(SignTime AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            var result = await command.ExecuteScalarAsync();
            return result != null && (int)result > 0;
        }

        private int CalculatePointsReward(int consecutiveDays)
        {
            // Base reward: 5 points, +2 points per consecutive day, max 30 points
            return Math.Min(5 + (consecutiveDays - 1) * 2, 30);
        }

        private int CalculatePetExpReward(int consecutiveDays)
        {
            // Base reward: 0 exp, +3 exp every 3 consecutive days, max 20 exp
            return Math.Min((consecutiveDays / 3) * 3, 20);
        }

        private bool ShouldGrantCoupon(int consecutiveDays)
        {
            // Grant coupon every 7 consecutive days
            return consecutiveDays % 7 == 0;
        }
    }
}

