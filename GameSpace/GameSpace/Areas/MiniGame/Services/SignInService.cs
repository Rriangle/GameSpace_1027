using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SignInRuleEntity = GameSpace.Models.SignInRule;

namespace GameSpace.Areas.MiniGame.Services
{
    public class SignInService : ISignInService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IAppClock _appClock;
        private readonly ITaiwanHolidayService _holidayService;
        private readonly ILogger<SignInService> _logger;

        public SignInService(
            GameSpacedatabaseContext context,
            IAppClock appClock,
            ITaiwanHolidayService holidayService,
            ILogger<SignInService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
            _holidayService = holidayService ?? throw new ArgumentNullException(nameof(holidayService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 簽到基本操作
        public async Task<bool> SignInAsync(int userId)
        {
            // 檢查今天是否已簽到
            if (!await CanSignInTodayAsync(userId))
                return false;

            // Use Taiwan timezone
            var nowUtc = _appClock.UtcNow;
            var todayTaiwan = _appClock.ToAppTime(nowUtc).Date;
            var lastSignIn = await GetLastSignInDateAsync(userId);
            var consecutiveDays = await GetConsecutiveDaysAsync(userId);

            // 計算連續天數
            DateTime? lastSignInTaiwan = null;
            if (lastSignIn.HasValue)
            {
                lastSignInTaiwan = _appClock.ToAppTime(lastSignIn.Value).Date;
            }

            if (lastSignInTaiwan.HasValue && lastSignInTaiwan.Value == todayTaiwan.AddDays(-1))
            {
                consecutiveDays++;
            }
            else if (!lastSignInTaiwan.HasValue || lastSignInTaiwan.Value < todayTaiwan.AddDays(-1))
            {
                consecutiveDays = 1;
            }

            // 計算獎勵
            var reward = await CalculateSignInRewardAsync(userId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 新增簽到記錄
                var signInLog = new UserSignInStat
                {
                    UserId = userId,
                    SignTime = nowUtc,
                    PointsGained = reward.Points,
                    ExpGained = reward.Experience,
                    CouponGained = reward.CouponCode ?? string.Empty,
                    PointsGainedTime = nowUtc,
                    ExpGainedTime = nowUtc,
                    CouponGainedTime = nowUtc
                };
                _context.UserSignInStats.Add(signInLog);

                // 發放獎勵
                await GrantSignInRewardAsync(userId, reward);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "簽到失敗: UserId={UserId}", userId);
                return false;
            }
        }

        public async Task<bool> CanSignInTodayAsync(int userId)
        {
            var todayTaiwan = _appClock.ToAppTime(_appClock.UtcNow).Date;

            var userSignIns = await _context.UserSignInStats
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var signedInToday = userSignIns
                .Any(s => _appClock.ToAppTime(s.SignTime).Date == todayTaiwan);

            return !signedInToday;
        }

        public async Task<DateTime?> GetLastSignInDateAsync(int userId)
        {
            var lastSignIn = await _context.UserSignInStats
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .FirstOrDefaultAsync();

            return lastSignIn?.SignTime;
        }

        public async Task<int> GetConsecutiveDaysAsync(int userId)
        {
            var signIns = await _context.UserSignInStats
                .AsNoTracking()
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

        // 簽到記錄查詢
        public async Task<IEnumerable<UserSignInStats>> GetSignInHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20)
        {
            var signIns = await _context.UserSignInStats
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 轉換為ViewModel
            return signIns.Select(s => new UserSignInStats
            {
                StatsID = s.LogId,
                UserID = s.UserId,
                SignTime = s.SignTime,
                PointsEarned = s.PointsGained,
                PetExpEarned = s.ExpGained,
                CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? (int?)null : int.TryParse(s.CouponGained, out var couponId) ? couponId : (int?)null,
                ConsecutiveDays = 0 // 需要動態計算
            });
        }

        public async Task<IEnumerable<UserSignInStats>> GetAllSignInsAsync(int pageNumber = 1, int pageSize = 50)
        {
            var signIns = await _context.UserSignInStats
                .AsNoTracking()
                .Include(s => s.User)
                .OrderByDescending(s => s.SignTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 轉換為ViewModel
            return signIns.Select(s => new UserSignInStats
            {
                StatsID = s.LogId,
                UserID = s.UserId,
                SignTime = s.SignTime,
                PointsEarned = s.PointsGained,
                PetExpEarned = s.ExpGained,
                CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? (int?)null : int.TryParse(s.CouponGained, out var couponId) ? couponId : (int?)null,
                ConsecutiveDays = 0 // 需要動態計算
            });
        }

        public async Task<IEnumerable<UserSignInStats>> GetSignInsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var signIns = await _context.UserSignInStats
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.SignTime >= startDate && s.SignTime <= endDate)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();

            // 轉換為ViewModel
            return signIns.Select(s => new UserSignInStats
            {
                StatsID = s.LogId,
                UserID = s.UserId,
                SignTime = s.SignTime,
                PointsEarned = s.PointsGained,
                PetExpEarned = s.ExpGained,
                CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? (int?)null : int.TryParse(s.CouponGained, out var couponId) ? couponId : (int?)null,
                ConsecutiveDays = 0 // 需要動態計算
            });
        }

        public async Task<UserSignInStats?> GetSignInDetailAsync(int logId)
        {
            var signIn = await _context.UserSignInStats
                .AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.LogId == logId);

            if (signIn == null) return null;

            // 轉換為ViewModel
            return new UserSignInStats
            {
                StatsID = signIn.LogId,
                UserID = signIn.UserId,
                SignTime = signIn.SignTime,
                PointsEarned = signIn.PointsGained,
                PetExpEarned = signIn.ExpGained,
                CouponEarned = string.IsNullOrEmpty(signIn.CouponGained) ? (int?)null : int.TryParse(signIn.CouponGained, out var couponId) ? couponId : (int?)null,
                ConsecutiveDays = await GetConsecutiveDaysAsync(signIn.UserId)
            };
        }

        // 簽到獎勵
        public async Task<SignInReward> CalculateSignInRewardAsync(int userId)
        {
            var consecutiveDays = await GetConsecutiveDaysAsync(userId);
            var nextDay = consecutiveDays + 1;

            // Get current Taiwan date
            var nowTaiwan = _appClock.ToAppTime(_appClock.UtcNow);
            var isHoliday = _holidayService.IsHoliday(nowTaiwan);

            // Initialize reward based on weekday/holiday
            // 平日簽到: +20 點數, +0 經驗
            // 假日簽到: +30 點數, +200 經驗
            var reward = new SignInReward
            {
                Points = isHoliday ? 30 : 20,
                Experience = isHoliday ? 200 : 0,
                ConsecutiveDayBonus = 0
            };

            // 連續 7 天: 額外 +40 點數, +300 經驗
            if (nextDay % 7 == 0)
            {
                reward.Points += 40;
                reward.Experience += 300;
                reward.ConsecutiveDayBonus = 40;
            }

            // 當月全勤: 額外 +200 點數, +2000 經驗, +1 張商城優惠券
            bool hasPerfectAttendance = await CheckMonthlyPerfectAttendanceAsync(userId, nowTaiwan);
            if (hasPerfectAttendance)
            {
                reward.Points += 200;
                reward.Experience += 2000;
                reward.CouponCode = $"PERFECT_ATTENDANCE_{nowTaiwan:yyyyMM}";
            }

            return reward;
        }

        /// <summary>
        /// Check if user has perfect attendance for the current month
        /// Perfect attendance means signing in every day of the current month up to today
        /// </summary>
        private async Task<bool> CheckMonthlyPerfectAttendanceAsync(int userId, DateTime currentDateTaiwan)
        {
            var monthStart = new DateTime(currentDateTaiwan.Year, currentDateTaiwan.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(currentDateTaiwan.Year, currentDateTaiwan.Month);

            // If it's not the last day of the month, we can't award perfect attendance yet
            if (currentDateTaiwan.Day != daysInMonth)
            {
                return false;
            }

            // Get all sign-ins for this month
            var allUserSignIns = await _context.UserSignInStats
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var signInsThisMonth = allUserSignIns
                .Select(s => _appClock.ToAppTime(s.SignTime).Date)
                .Where(d => d.Year == currentDateTaiwan.Year && d.Month == currentDateTaiwan.Month)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Check if user has signed in every day of the month (including today)
            // Since we're calling this during sign-in, we need to check if all days from 1 to today are covered
            // For the last day of month, it should be all days from 1 to daysInMonth
            var expectedDays = daysInMonth; // Including today which will be added

            // We expect daysInMonth - 1 sign-ins already recorded (today's will be added after this check)
            return signInsThisMonth.Count == expectedDays - 1 &&
                   signInsThisMonth.First().Day == 1; // Started from day 1
        }

        public async Task<bool> GrantSignInRewardAsync(int userId, SignInReward reward)
        {
            var nowUtc = _appClock.UtcNow;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 發放點數
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet != null)
                {
                    wallet.UserPoint += reward.Points;

                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "SignIn",
                        PointsChanged = reward.Points,
                        ItemCode = "SIGNIN_REWARD",
                        Description = $"簽到獎勵 (連續 {(reward.ConsecutiveDayBonus > 0 ? "+" + reward.ConsecutiveDayBonus : "")}天加成)",
                        ChangeTime = nowUtc
                    };
                    _context.WalletHistories.Add(history);
                }

                // 發放經驗給寵物
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);
                if (pet != null)
                {
                    pet.Experience += reward.Experience;
                }

                // 發放優惠券
                if (!string.IsNullOrEmpty(reward.CouponCode))
                {
                    // 查找「全勤獎勵」專用優惠券類型
                    // 優先查找名稱包含"全勤"的優惠券類型，如果找不到則查找名稱包含"簽到"的類型
                    var couponType = await _context.CouponTypes
                        .Where(ct => ct.ValidFrom <= nowUtc && ct.ValidTo >= nowUtc)
                        .Where(ct => ct.Name.Contains("全勤") || ct.Name.Contains("完美出勤") || ct.Name.Contains("Perfect Attendance"))
                        .OrderBy(ct => ct.CouponTypeId)
                        .FirstOrDefaultAsync();

                    // 如果找不到全勤專用優惠券，嘗試查找簽到獎勵優惠券
                    if (couponType == null)
                    {
                        couponType = await _context.CouponTypes
                            .Where(ct => ct.ValidFrom <= nowUtc && ct.ValidTo >= nowUtc)
                            .Where(ct => ct.Name.Contains("簽到獎勵") || ct.Name.Contains("Sign-In Reward"))
                            .OrderBy(ct => ct.CouponTypeId)
                            .FirstOrDefaultAsync();
                    }

                    if (couponType != null)
                    {
                        var coupon = new Coupon
                        {
                            UserId = userId,
                            CouponTypeId = couponType.CouponTypeId,
                            CouponCode = $"{reward.CouponCode}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                            AcquiredTime = nowUtc,
                            IsUsed = false
                        };
                        _context.Coupons.Add(coupon);

                        // 記錄優惠券發放歷史
                        var history = new WalletHistory
                        {
                            UserId = userId,
                            ChangeType = "Coupon",
                            PointsChanged = 0,
                            ItemCode = coupon.CouponCode,
                            Description = $"全勤獎勵優惠券：{couponType.Name}",
                            ChangeTime = nowUtc
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
                await transaction.RollbackAsync();
                _logger.LogError(ex, "發放簽到獎勵失敗: UserId={UserId}", userId);
                return false;
            }
        }

        public Task<IEnumerable<SignInRewardRule>> GetSignInRewardRulesAsync()
        {
            // 預設簽到獎勵規則
            return Task.FromResult<IEnumerable<SignInRewardRule>>(new List<SignInRewardRule>
            {
                new SignInRewardRule { DayNumber = 1, Points = 10, Experience = 5, HasCoupon = false },
                new SignInRewardRule { DayNumber = 3, Points = 15, Experience = 8, HasCoupon = false },
                new SignInRewardRule { DayNumber = 7, Points = 25, Experience = 15, HasCoupon = true },
                new SignInRewardRule { DayNumber = 14, Points = 40, Experience = 25, HasCoupon = true },
                new SignInRewardRule { DayNumber = 30, Points = 100, Experience = 50, HasCoupon = true }
            });
        }

        // 簽到統計
        public async Task<SignInStatistics> GetUserSignInStatisticsAsync(int userId)
        {
            var allSignIns = await _context.UserSignInStats
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();

            var stats = new SignInStatistics
            {
                TotalSignIns = allSignIns.Count,
                CurrentStreak = await GetConsecutiveDaysAsync(userId),
                LastSignInDate = await GetLastSignInDateAsync(userId),
                TotalPointsEarned = allSignIns.Sum(s => s.PointsGained),
                TotalExpEarned = allSignIns.Sum(s => s.ExpGained),
                TotalCouponsEarned = allSignIns.Count(s => !string.IsNullOrEmpty(s.CouponGained))
            };

            // 計算最長連續簽到 (需要遍歷所有記錄計算)
            stats.LongestStreak = await GetConsecutiveDaysAsync(userId);

            return stats;
        }

        public async Task<SignInStatistics> GetGlobalSignInStatisticsAsync()
        {
            var allSignIns = await _context.UserSignInStats.AsNoTracking().ToListAsync();

            return new SignInStatistics
            {
                TotalSignIns = allSignIns.Count,
                CurrentStreak = 0, // 全域統計不適用
                LongestStreak = 0, // 全域統計中最長連續需要針對每個用戶計算
                TotalPointsEarned = allSignIns.Sum(s => s.PointsGained),
                TotalExpEarned = allSignIns.Sum(s => s.ExpGained),
                TotalCouponsEarned = allSignIns.Count(s => !string.IsNullOrEmpty(s.CouponGained)),
                LastSignInDate = allSignIns.Any() ? allSignIns.Max(s => s.SignTime) : null
            };
        }

        public async Task<Dictionary<string, int>> GetSignInTrendDataAsync(int days = 30)
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

        public async Task<IEnumerable<UserSignInRanking>> GetSignInLeaderboardAsync(int count = 10)
        {
            // 優化：使用 GroupBy 避免 N+1 問題
            var rankings = await _context.UserSignInStats
                .AsNoTracking()
                .Include(s => s.User)
                .GroupBy(s => new { s.UserId, s.User.UserAccount })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserAccount,
                    SignInCount = g.Count(),
                    TotalPoints = g.Sum(s => s.PointsGained),
                    LastSignInDate = g.Max(s => s.SignTime),
                    SignIns = g.OrderByDescending(s => s.SignTime).ToList()
                })
                .OrderByDescending(r => r.SignInCount)
                .Take(count)
                .ToListAsync();

            // 計算連續天數（使用已載入的資料）
            var result = rankings.Select(r =>
            {
                int consecutiveDays = CalculateConsecutiveDaysFromList(r.SignIns);
                return new UserSignInRanking
                {
                    UserId = r.UserId,
                    UserName = r.UserName,
                    SignInCount = r.SignInCount,
                    ConsecutiveDays = consecutiveDays,
                    TotalPoints = r.TotalPoints
                };
            }).ToList();

            return result;
        }

        // 輔助方法：從簽到記錄列表計算連續天數
        private int CalculateConsecutiveDaysFromList(List<UserSignInStat> signIns)
        {
            if (!signIns.Any()) return 0;

            // 按日期排序（最新到最舊）
            var dates = signIns
                .Select(s => _appClock.ToAppTime(s.SignTime).Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (!dates.Any()) return 0;

            // 從最新日期開始計算連續天數
            var todayTaiwan = _appClock.ToAppTime(_appClock.UtcNow).Date;
            int consecutiveDays = 0;
            DateTime expectedDate = todayTaiwan;

            foreach (var date in dates)
            {
                if (date == expectedDate || date == expectedDate.AddDays(-1))
                {
                    consecutiveDays++;
                    expectedDate = date.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        // 簽到規則管理
        public async Task<IEnumerable<SignInRule>> GetAllSignInRulesAsync()
        {
            var dbRules = await _context.Set<SignInRuleEntity>()
                .AsNoTracking()
                .OrderBy(r => r.SignInDay)
                .ToListAsync();

            // Map database entity to DTO
            return dbRules.Select(r => new SignInRule
            {
                Id = r.Id,
                Description = r.Description ?? string.Empty,
                SignInDay = r.SignInDay,
                Points = r.Points,
                Experience = r.Experience,
                CouponTypeCode = r.CouponTypeCode,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });
        }

        public async Task<SignInRule?> GetSignInRuleByIdAsync(int ruleId)
        {
            var dbRule = await _context.Set<SignInRuleEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == ruleId);

            if (dbRule == null) return null;

            return new SignInRule
            {
                Id = dbRule.Id,
                Description = dbRule.Description ?? string.Empty,
                SignInDay = dbRule.SignInDay,
                Points = dbRule.Points,
                Experience = dbRule.Experience,
                CouponTypeCode = dbRule.CouponTypeCode,
                IsActive = dbRule.IsActive,
                CreatedAt = dbRule.CreatedAt,
                UpdatedAt = dbRule.UpdatedAt
            };
        }

        public async Task<bool> CreateSignInRuleAsync(SignInRule rule)
        {
            try
            {
                var dbRule = new SignInRuleEntity
                {
                    SignInDay = rule.SignInDay,
                    Points = rule.Points,
                    Experience = rule.Experience,
                    HasCoupon = !string.IsNullOrEmpty(rule.CouponTypeCode),
                    CouponTypeCode = rule.CouponTypeCode,
                    IsActive = rule.IsActive,
                    Description = rule.Description,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Add(dbRule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSignInRuleAsync(SignInRule rule)
        {
            try
            {
                var dbRule = await _context.Set<SignInRuleEntity>()
                    .FirstOrDefaultAsync(r => r.Id == rule.Id);
                if (dbRule == null) return false;

                dbRule.SignInDay = rule.SignInDay;
                dbRule.Points = rule.Points;
                dbRule.Experience = rule.Experience;
                dbRule.HasCoupon = !string.IsNullOrEmpty(rule.CouponTypeCode);
                dbRule.CouponTypeCode = rule.CouponTypeCode;
                dbRule.IsActive = rule.IsActive;
                dbRule.Description = rule.Description;
                dbRule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSignInRuleAsync(int ruleId)
        {
            try
            {
                var dbRule = await _context.Set<SignInRuleEntity>()
                    .FirstOrDefaultAsync(r => r.Id == ruleId);
                if (dbRule == null) return false;

                _context.Remove(dbRule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleSignInRuleStatusAsync(int ruleId)
        {
            try
            {
                var dbRule = await _context.Set<SignInRuleEntity>()
                    .FirstOrDefaultAsync(r => r.Id == ruleId);
                if (dbRule == null) return false;

                dbRule.IsActive = !dbRule.IsActive;
                dbRule.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}




