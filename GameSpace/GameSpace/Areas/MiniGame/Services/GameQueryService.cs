using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 遊戲查詢服務，提供遊戲相關的查詢功能給 Admin 使用
    /// </summary>
    public class GameQueryService : IGameQueryService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IAppClock _appClock;

        public GameQueryService(GameSpacedatabaseContext context, IAppClock appClock)
        {
            _context = context;
            _appClock = appClock;
        }

        /// <summary>
        /// 取得遊戲規則設定，包含基本設定和關卡設定
        /// </summary>
        public async Task<GameRuleViewModel> GetGameRulesAsync()
        {
            // 預設遊戲規則，Adventure Game 冒險遊戲
            var viewModel = new GameRuleViewModel
            {
                GameName = "Adventure Game - 冒險遊戲",
                Description = "經典冒險遊戲，通過擊敗怪物來獲得經驗值和點數獎勵",
                DailyPlayLimit = 3,
                IsActive = true,
                LevelSettings = new List<GameLevelSettingViewModel>
                {
                    new GameLevelSettingViewModel
                    {
                        Level = 1,
                        MonsterCount = 6,
                        SpeedMultiplier = 1.0m,
                        WinPointsReward = 10,
                        WinExpReward = 100,
                        WinCouponReward = 0,
                        LosePointsReward = 5,
                        LoseExpReward = 50,
                        AbortPointsReward = 0,
                        AbortExpReward = 0,
                        Description = "關卡1 擊敗6個怪物"
                    },
                    new GameLevelSettingViewModel
                    {
                        Level = 2,
                        MonsterCount = 8,
                        SpeedMultiplier = 1.5m,
                        WinPointsReward = 20,
                        WinExpReward = 200,
                        WinCouponReward = 0,
                        LosePointsReward = 10,
                        LoseExpReward = 100,
                        AbortPointsReward = 0,
                        AbortExpReward = 0,
                        Description = "關卡2 擊敗8個怪物"
                    },
                    new GameLevelSettingViewModel
                    {
                        Level = 3,
                        MonsterCount = 10,
                        SpeedMultiplier = 2.0m,
                        WinPointsReward = 30,
                        WinExpReward = 300,
                        WinCouponReward = 1,
                        LosePointsReward = 15,
                        LoseExpReward = 150,
                        AbortPointsReward = 0,
                        AbortExpReward = 0,
                        Description = "Pass 3 checkpoints to win, lose when HP reaches 0",
                    }
                }
            };

            // 計算統計數據
            var totalGames = await _context.MiniGames.CountAsync();
            // Count games started today in Asia/Taipei timezone
            var taipeiNow = _appClock.ToAppTime(_appClock.UtcNow);
            var startUtc = _appClock.ToUtc(taipeiNow.Date);
            var endUtc = _appClock.ToUtc(taipeiNow.Date.AddDays(1));
            var activeToday = await _context.MiniGames
                .Where(g => g.StartTime >= startUtc && g.StartTime < endUtc)
                .CountAsync();

            viewModel.TotalGamesPlayed = totalGames;
            viewModel.TodayGamesPlayed = activeToday;
            viewModel.LastUpdated = DateTime.UtcNow;

            return viewModel;
        }

        /// <summary>
        /// 查詢遊戲記錄，支援多種篩選條件和分頁
        /// </summary>
        public async Task<GameRecordsListViewModel> QueryGameRecordsAsync(GameRecordQueryModel query)
        {
            // 基礎查詢
            var baseQuery = _context.MiniGames
                .Include(m => m.User)
                .AsNoTracking()
                .AsQueryable();

            // 應用篩選條件
            if (query.UserId.HasValue)
            {
                baseQuery = baseQuery.Where(m => m.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                var searchTerm = query.UserName.Trim().ToLower();
                baseQuery = baseQuery.Where(m => m.User.UserName.ToLower().Contains(searchTerm));
            }

            if (query.PetId.HasValue)
            {
                baseQuery = baseQuery.Where(m => m.PetId == query.PetId.Value);
            }

            if (query.Level.HasValue)
            {
                baseQuery = baseQuery.Where(m => m.Level == query.Level.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Result))
            {
                baseQuery = baseQuery.Where(m => m.Result == query.Result);
            }

            if (query.StartDate.HasValue)
            {
                baseQuery = baseQuery.Where(m => m.StartTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                // 包含結束日期的整天
                var endOfDay = query.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                baseQuery = baseQuery.Where(m => m.StartTime <= endOfDay);
            }

            // 計算總數
            var totalCount = await baseQuery.CountAsync();

            // 排序
            var sortedQuery = query.SortBy?.ToLower() switch
            {
                "userid" => query.SortOrder?.ToLower() == "asc"
                    ? baseQuery.OrderBy(m => m.UserId)
                    : baseQuery.OrderByDescending(m => m.UserId),
                "level" => query.SortOrder?.ToLower() == "asc"
                    ? baseQuery.OrderBy(m => m.Level)
                    : baseQuery.OrderByDescending(m => m.Level),
                "result" => query.SortOrder?.ToLower() == "asc"
                    ? baseQuery.OrderBy(m => m.Result)
                    : baseQuery.OrderByDescending(m => m.Result),
                "pointsgained" => query.SortOrder?.ToLower() == "asc"
                    ? baseQuery.OrderBy(m => m.PointsGained)
                    : baseQuery.OrderByDescending(m => m.PointsGained),
                "expgained" => query.SortOrder?.ToLower() == "asc"
                    ? baseQuery.OrderBy(m => m.ExpGained)
                    : baseQuery.OrderByDescending(m => m.ExpGained),
                _ => query.SortOrder?.ToLower() == "asc"
                    ? baseQuery.OrderBy(m => m.StartTime)
                    : baseQuery.OrderByDescending(m => m.StartTime)
            };

            // 分頁
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 100 ? 100 : query.PageSize);

            var records = await sortedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new GameRecordItemViewModel
                {
                    PlayId = m.PlayId,
                    UserId = m.UserId,
                    UserName = m.User.UserName ?? "未知",
                    PetId = m.PetId,
                    Level = m.Level,
                    MonsterCount = m.MonsterCount,
                    SpeedMultiplier = m.SpeedMultiplier,
                    Result = m.Result,
                    ExpGained = m.ExpGained,
                    PointsGained = m.PointsGained,
                    CouponGained = m.CouponGained,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    Aborted = m.Aborted,
                    Duration = m.EndTime.HasValue
                        ? (int)(m.EndTime.Value - m.StartTime).TotalSeconds
                        : null
                })
                .ToListAsync();

            return new GameRecordsListViewModel
            {
                Records = records,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Query = query
            };
        }

        /// <summary>
        /// 取得單一遊戲記錄的詳細資訊
        /// </summary>
        public async Task<GameRecordDetailViewModel?> GetGameRecordDetailAsync(int playId)
        {
            var record = await _context.MiniGames
                .Include(m => m.User)
                .AsNoTracking()
                .Where(m => m.PlayId == playId)
                .Select(m => new GameRecordDetailViewModel
                {
                    PlayId = m.PlayId,
                    UserId = m.UserId,
                    UserName = m.User.UserName ?? "未知",
                    UserEmail = m.User.UserIntroduce != null ? m.User.UserIntroduce.Email : string.Empty,
                    PetId = m.PetId,
                    Level = m.Level,
                    MonsterCount = m.MonsterCount,
                    SpeedMultiplier = m.SpeedMultiplier,
                    Result = m.Result,
                    ExpGained = m.ExpGained,
                    ExpGainedTime = m.ExpGainedTime,
                    PointsGained = m.PointsGained,
                    PointsGainedTime = m.PointsGainedTime,
                    CouponGained = m.CouponGained,
                    CouponGainedTime = m.CouponGainedTime,
                    HungerDelta = m.HungerDelta,
                    MoodDelta = m.MoodDelta,
                    StaminaDelta = m.StaminaDelta,
                    CleanlinessDelta = m.CleanlinessDelta,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    Aborted = m.Aborted,
                    Duration = m.EndTime.HasValue
                        ? (int)(m.EndTime.Value - m.StartTime).TotalSeconds
                        : null
                })
                .FirstOrDefaultAsync();

            return record;
        }

        /// <summary>
        /// 取得遊戲統計數據
        /// </summary>
        public async Task<Models.ViewModels.GameStatisticsViewModel> GetGameStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.MiniGames.AsNoTracking().AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(m => m.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(m => m.StartTime <= endOfDay);
            }

            var totalGames = await query.CountAsync();
            var winGames = await query.Where(m => m.Result == "Win").CountAsync();
            var loseGames = await query.Where(m => m.Result == "Lose").CountAsync();
            var abortGames = await query.Where(m => m.Aborted).CountAsync();

            var totalPoints = await query.SumAsync(m => (int?)m.PointsGained) ?? 0;
            var totalExp = await query.SumAsync(m => (int?)m.ExpGained) ?? 0;
            var avgPointsPerGame = totalGames > 0 ? (decimal)totalPoints / totalGames : 0;
            var avgExpPerGame = totalGames > 0 ? (decimal)totalExp / totalGames : 0;

            // 關卡統計
            var levelStats = await query
                .GroupBy(m => m.Level)
                .Select(g => new GameLevelStatViewModel
                {
                    Level = g.Key,
                    TotalPlays = g.Count(),
                    WinCount = g.Count(m => m.Result == "Win"),
                    LoseCount = g.Count(m => m.Result == "Lose"),
                    AbortCount = g.Count(m => m.Aborted),
                    WinRate = g.Count() > 0 ? (decimal)g.Count(m => m.Result == "Win") / g.Count() * 100 : 0
                })
                .OrderBy(s => s.Level)
                .ToListAsync();

            return new Models.ViewModels.GameStatisticsViewModel
            {
                TotalGames = totalGames,
                WinGames = winGames,
                LoseGames = loseGames,
                AbortGames = abortGames,
                WinRate = totalGames > 0 ? (decimal)winGames / totalGames * 100 : 0,
                TotalPointsAwarded = totalPoints,
                TotalExpAwarded = totalExp,
                AveragePointsPerGame = avgPointsPerGame,
                AverageExpPerGame = avgExpPerGame,
                LevelStatistics = levelStats,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        /// <summary>
        /// 取得用戶今日遊戲次數
        /// </summary>
        public async Task<int> GetUserTodayGameCountAsync(int userId)
        {
            var nowTaiwan = _appClock.ToAppTime(_appClock.UtcNow);
            var todayTaiwan = nowTaiwan.Date;
            var startUtc = _appClock.ToUtc(todayTaiwan);
            var endUtc = _appClock.ToUtc(todayTaiwan.AddDays(1));

            return await _context.MiniGames
                .AsNoTracking()
                .Where(m => m.UserId == userId && m.StartTime >= startUtc && m.StartTime < endUtc)
                .CountAsync();
        }

        /// <summary>
        /// 取得關卡設定列表
        /// </summary>
        public async Task<List<GameLevelSettingViewModel>> GetGameLevelSettingsAsync()
        {
            // 預設關卡設定，與 GetGameRulesAsync 保持一致
            return await Task.FromResult(new List<GameLevelSettingViewModel>
            {
                new GameLevelSettingViewModel
                {
                    Level = 1,
                    MonsterCount = 6,
                    SpeedMultiplier = 1.0m,
                    WinPointsReward = 10,
                    WinExpReward = 100,
                    WinCouponReward = 0,
                    LosePointsReward = 5,
                    LoseExpReward = 50,
                    AbortPointsReward = 0,
                    AbortExpReward = 0,
                    Description = "關卡1 擊敗6個怪物"
                },
                new GameLevelSettingViewModel
                {
                    Level = 2,
                    MonsterCount = 8,
                    SpeedMultiplier = 1.5m,
                    WinPointsReward = 20,
                    WinExpReward = 200,
                    WinCouponReward = 0,
                    LosePointsReward = 10,
                    LoseExpReward = 100,
                    AbortPointsReward = 0,
                    AbortExpReward = 0,
                    Description = "關卡2 擊敗8個怪物"
                },
                new GameLevelSettingViewModel
                {
                    Level = 3,
                    MonsterCount = 10,
                    SpeedMultiplier = 2.0m,
                    WinPointsReward = 30,
                    WinExpReward = 300,
                    WinCouponReward = 1,
                    LosePointsReward = 15,
                    LoseExpReward = 150,
                    AbortPointsReward = 0,
                    AbortExpReward = 0,
                    Description = "Pass 3 checkpoints to win, lose when HP reaches 0",
                }
            });
        }
    }
}