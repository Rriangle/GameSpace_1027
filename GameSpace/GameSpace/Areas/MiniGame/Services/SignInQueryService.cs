using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// Service implementation for querying sign-in rules and records
    /// </summary>
    public class SignInQueryService : ISignInQueryService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<SignInQueryService> _logger;
        private static readonly TimeZoneInfo TaipeiTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        private const string SignInRuleSelectSql = "SELECT Id, SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive, CreatedAt, UpdatedAt, Description, IsDeleted, DeletedAt, DeletedBy, DeleteReason FROM SignInRule WHERE IsDeleted = 0";

        public SignInQueryService(GameSpacedatabaseContext context, ILogger<SignInQueryService> logger)
        {
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

        /// <summary>
        /// Get paginated list of sign-in rules
        /// </summary>
        public async Task<PagedResult<SignInRuleViewModel>> GetSignInRulesAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 10, 100);

                var query = _context.Set<SignInRule>()
                    .FromSqlRaw(SignInRuleSelectSql)
                    .AsNoTracking()
                    .OrderBy(r => r.SignInDay);

                var totalCount = await query.CountAsync();

                var rules = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new SignInRuleViewModel
                    {
                        RuleId = r.Id,
                        ConsecutiveDays = r.SignInDay,
                        RewardPoints = r.Points,
                        RewardPetExp = r.Experience,
                        CouponTypeId = null,
                        CouponTypeName = r.CouponTypeCode,
                        EvoucherTypeId = null,
                        EvoucherTypeName = null,
                        IsActive = r.IsActive,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToListAsync();

                return new PagedResult<SignInRuleViewModel>
                {
                    Items = rules,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到規則列表失敗: Page={Page}, PageSize={PageSize}", page, pageSize);
                return new PagedResult<SignInRuleViewModel>
                {
                    Items = new List<SignInRuleViewModel>(),
                    TotalCount = 0,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// Get sign-in rule by ID
        /// </summary>
        public async Task<SignInRuleViewModel?> GetSignInRuleByIdAsync(int id)
        {
            try
            {
                var rule = await _context.Set<SignInRule>()
                    .FromSqlRaw(SignInRuleSelectSql)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rule == null)
                    return null;

                return new SignInRuleViewModel
                {
                    RuleId = rule.Id,
                    ConsecutiveDays = rule.SignInDay,
                    RewardPoints = rule.Points,
                    RewardPetExp = rule.Experience,
                    CouponTypeId = null,
                    CouponTypeName = rule.CouponTypeCode,
                    EvoucherTypeId = null,
                    EvoucherTypeName = null,
                    IsActive = rule.IsActive,
                    CreatedAt = rule.CreatedAt,
                    UpdatedAt = rule.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到規則失敗: RuleId={RuleId}", id);
                return null;
            }
        }

        /// <summary>
        /// Query sign-in records with filters
        /// </summary>
        public async Task<PagedResult<SignInRecordViewModel>> QuerySignInRecordsAsync(
            int? userId = null,
            string? userAccount = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? minPoints = null,
            int? maxPoints = null,
            string sortBy = "SignInDate",
            string sortOrder = "desc",
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 10, 100);

                var query = _context.UserSignInStats
                    .AsNoTracking()
                    .Include(s => s.User)
                    .AsQueryable();

                // Apply filters
                if (userId.HasValue)
                {
                    query = query.Where(s => s.UserId == userId.Value);
                }

                if (!string.IsNullOrWhiteSpace(userAccount))
                {
                    query = query.Where(s => s.User != null && s.User.UserAccount.Contains(userAccount));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(s => s.SignTime >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(s => s.SignTime <= endDateTime);
                }

                if (minPoints.HasValue)
                {
                    query = query.Where(s => s.PointsGained >= minPoints.Value);
                }

                if (maxPoints.HasValue)
                {
                    query = query.Where(s => s.PointsGained <= maxPoints.Value);
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "userid" => sortOrder?.ToLower() == "asc"
                        ? query.OrderBy(s => s.UserId)
                        : query.OrderByDescending(s => s.UserId),
                    "pointsgained" => sortOrder?.ToLower() == "asc"
                        ? query.OrderBy(s => s.PointsGained)
                        : query.OrderByDescending(s => s.PointsGained),
                    "expgained" => sortOrder?.ToLower() == "asc"
                        ? query.OrderBy(s => s.ExpGained)
                        : query.OrderByDescending(s => s.ExpGained),
                    _ => sortOrder?.ToLower() == "asc"
                        ? query.OrderBy(s => s.SignTime)
                        : query.OrderByDescending(s => s.SignTime)
                };

                var totalCount = await query.CountAsync();

                // Calculate consecutive days for each user
                var userIds = await query
                    .Select(s => s.UserId)
                    .Distinct()
                    .ToListAsync();

                var consecutiveDaysMap = new Dictionary<int, Dictionary<DateTime, int>>();
                foreach (var uid in userIds)
                {
                    consecutiveDaysMap[uid] = await CalculateConsecutiveDaysForUserAsync(uid);
                }

                // Execute paginated query
                var records = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new
                    {
                        s.LogId,
                        s.UserId,
                        UserAccount = s.User != null ? s.User.UserAccount : null,
                        UserName = s.User != null ? s.User.UserName : null,
                        s.SignTime,
                        s.PointsGained,
                        s.ExpGained,
                        s.CouponGained
                    })
                    .ToListAsync();

                var viewModels = records.Select(s => new SignInRecordViewModel
                {
                    RecordId = s.LogId,
                    UserId = s.UserId,
                    UserAccount = s.UserAccount ?? string.Empty,
                    UserName = s.UserName ?? string.Empty,
                    SignInDate = s.SignTime,
                    ConsecutiveDays = GetConsecutiveDaysForDate(consecutiveDaysMap, s.UserId, s.SignTime),
                    PointsRewarded = s.PointsGained,
                    PetExpRewarded = s.ExpGained,
                    CouponTypeId = null,
                    CouponTypeName = s.CouponGained,
                    EvoucherTypeId = null,
                    EvoucherTypeName = null,
                    CreatedAt = s.SignTime
                }).ToList();

                return new PagedResult<SignInRecordViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢簽到記錄失敗: UserId={UserId}, Page={Page}", userId, page);
                return new PagedResult<SignInRecordViewModel>
                {
                    Items = new List<SignInRecordViewModel>(),
                    TotalCount = 0,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// Get sign-in record by ID
        /// </summary>
        public async Task<SignInRecordViewModel?> GetSignInRecordByIdAsync(int logId)
        {
            try
            {
                var record = await _context.UserSignInStats
                    .AsNoTracking()
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.LogId == logId);

                if (record == null)
                    return null;

                var consecutiveDays = await GetUserConsecutiveDaysAsync(record.UserId, record.SignTime);

                return new SignInRecordViewModel
                {
                    RecordId = record.LogId,
                    UserId = record.UserId,
                    UserAccount = record.User?.UserAccount ?? string.Empty,
                    UserName = record.User?.UserName ?? string.Empty,
                    SignInDate = record.SignTime,
                    ConsecutiveDays = consecutiveDays,
                    PointsRewarded = record.PointsGained,
                    PetExpRewarded = record.ExpGained,
                    CouponTypeId = null,
                    CouponTypeName = record.CouponGained,
                    EvoucherTypeId = null,
                    EvoucherTypeName = null,
                    CreatedAt = record.SignTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到記錄失敗: LogId={LogId}", logId);
                return null;
            }
        }

        /// <summary>
        /// Get sign-in statistics summary
        /// </summary>
        public async Task<SignInStatsViewModel> GetSignInStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
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

                var query = _context.UserSignInStats.AsNoTracking();

                if (startDate.HasValue)
                    query = query.Where(s => s.SignTime >= startDate.Value);

                if (endDate.HasValue)
                {
                    var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(s => s.SignTime <= endDateTime);
                }

                var allRecords = await query.ToListAsync();

                var todayRecords = allRecords.Where(s => s.SignTime >= todayStartUtc && s.SignTime < todayEndUtc).ToList();
                var weekRecords = allRecords.Where(s => s.SignTime >= weekStartUtc).ToList();
                var monthRecords = allRecords.Where(s => s.SignTime >= monthStartUtc).ToList();

                var activeUsers = allRecords.Select(s => s.UserId).Distinct().Count();

                return new SignInStatsViewModel
                {
                    TotalSignIns = allRecords.Count,
                    TodaySignIns = todayRecords.Count,
                    WeekSignIns = weekRecords.Count,
                    MonthSignIns = monthRecords.Count,
                    AverageConsecutiveDays = 0, // Would need complex calculation
                    MaxConsecutiveDays = 0, // Would need complex calculation
                    TotalPointsRewarded = allRecords.Sum(s => s.PointsGained),
                    TotalPetExpRewarded = allRecords.Sum(s => s.ExpGained),
                    TotalCouponsRewarded = allRecords.Count(s => !string.IsNullOrEmpty(s.CouponGained)),
                    TotalEvouchersRewarded = 0,
                    ActiveUsersCount = activeUsers,
                    StatsGeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到統計失敗");
                return new SignInStatsViewModel
                {
                    StatsGeneratedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Get active sign-in rules
        /// </summary>
        public async Task<List<SignInRuleViewModel>> GetActiveSignInRulesAsync()
        {
            try
            {
                var rules = await _context.Set<SignInRule>()
                    .FromSqlRaw(SignInRuleSelectSql)
                    .AsNoTracking()
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.SignInDay)
                    .Select(r => new SignInRuleViewModel
                    {
                        RuleId = r.Id,
                        ConsecutiveDays = r.SignInDay,
                        RewardPoints = r.Points,
                        RewardPetExp = r.Experience,
                        CouponTypeId = null,
                        CouponTypeName = r.CouponTypeCode,
                        EvoucherTypeId = null,
                        EvoucherTypeName = null,
                        IsActive = r.IsActive,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToListAsync();

                return rules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得活動簽到規則失敗");
                return new List<SignInRuleViewModel>();
            }
        }

        /// <summary>
        /// Get user's consecutive sign-in days
        /// </summary>
        public async Task<int> GetUserConsecutiveDaysAsync(int userId)
        {
            try
            {
                var signIns = await _context.UserSignInStats
                    .AsNoTracking()
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SignTime)
                    .Select(s => s.SignTime.Date)
                    .ToListAsync();

                if (!signIns.Any())
                    return 0;

                int consecutiveDays = 1;
                var currentDate = signIns.First();

                for (int i = 1; i < signIns.Count; i++)
                {
                    var previousDate = signIns[i];
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
                _logger.LogError(ex, "取得用戶連續簽到天數失敗: UserId={UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// Get user's consecutive sign-in days at a specific date
        /// </summary>
        private async Task<int> GetUserConsecutiveDaysAsync(int userId, DateTime atDate)
        {
            try
            {
                // Convert atDate to Taipei timezone and get its date part
                var atDateTaiwan = TimeZoneInfo.ConvertTimeFromUtc(atDate, TaipeiTimeZone).Date;
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(atDateTaiwan.AddDays(1), TaipeiTimeZone);

                var signIns = await _context.UserSignInStats
                    .AsNoTracking()
                    .Where(s => s.UserId == userId && s.SignTime < endUtc)
                    .OrderByDescending(s => s.SignTime)
                    .Select(s => s.SignTime.Date)
                    .ToListAsync();

                if (!signIns.Any())
                    return 0;

                int consecutiveDays = 1;
                var currentDate = signIns.First();

                for (int i = 1; i < signIns.Count; i++)
                {
                    var previousDate = signIns[i];
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
                _logger.LogError(ex, "取得用戶特定日期連續簽到天數失敗: UserId={UserId}, AtDate={AtDate}", userId, atDate);
                return 0;
            }
        }

        /// <summary>
        /// Calculate consecutive days for all sign-in records of a user
        /// </summary>
        private async Task<Dictionary<DateTime, int>> CalculateConsecutiveDaysForUserAsync(int userId)
        {
            var result = new Dictionary<DateTime, int>();

            try
            {
                var signIns = await _context.UserSignInStats
                    .AsNoTracking()
                    .Where(s => s.UserId == userId)
                    .OrderBy(s => s.SignTime)
                    .Select(s => s.SignTime.Date)
                    .Distinct()
                    .ToListAsync();

                if (!signIns.Any())
                    return result;

                int consecutiveDays = 1;
                result[signIns[0]] = consecutiveDays;

                for (int i = 1; i < signIns.Count; i++)
                {
                    if (signIns[i] == signIns[i - 1].AddDays(1))
                    {
                        consecutiveDays++;
                    }
                    else
                    {
                        consecutiveDays = 1;
                    }
                    result[signIns[i]] = consecutiveDays;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算用戶連續簽到天數失敗: UserId={UserId}", userId);
                return result;
            }
        }

        /// <summary>
        /// Get consecutive days for a specific date from pre-calculated map
        /// </summary>
        private int GetConsecutiveDaysForDate(Dictionary<int, Dictionary<DateTime, int>> consecutiveDaysMap, int userId, DateTime date)
        {
            if (consecutiveDaysMap.ContainsKey(userId))
            {
                var userMap = consecutiveDaysMap[userId];
                var dateKey = date.Date;
                if (userMap.ContainsKey(dateKey))
                {
                    return userMap[dateKey];
                }
            }
            return 1; // Default to 1 if not found
        }
    }
}
