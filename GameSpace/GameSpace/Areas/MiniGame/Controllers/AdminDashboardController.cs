using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminDashboardController : MiniGameBaseController
    {
        private readonly IDashboardService _dashboardService;
        private readonly ISignInService _signInService;

        public AdminDashboardController(GameSpacedatabaseContext context, IDashboardService dashboardService, ISignInService signInService)
            : base(context)
        {
            _dashboardService = dashboardService;
            _signInService = signInService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel();

            try
            {
                // 獲取總覽數據
                var overview = await _dashboardService.GetDashboardOverviewAsync();
                viewModel.TotalUsers = overview.TotalUsers;
                viewModel.ActiveUsers = overview.ActiveUsers;
                viewModel.TotalPets = 0; // 暫不可用
                viewModel.TotalMiniGames = overview.TotalGames;
                viewModel.TotalCoupons = 0; // 暫不可用
                viewModel.TotalEVouchers = 0; // 暫不可用

                // 今日統計
                viewModel.TodaySignIns = overview.TodayGames; // 使用遊戲數代替
                viewModel.TodayGames = overview.TodayGames;

                // 本月統計
                var gameStats = await _dashboardService.GetGameStatisticsAsync();
                viewModel.ThisMonthSignIns = gameStats.GamesThisMonth;
                viewModel.ThisMonthGames = gameStats.GamesThisMonth;

                // 活躍用戶統計
                var userStats = await _dashboardService.GetUserStatisticsAsync();
                viewModel.ActiveUsersLast7Days = userStats.NewUsersThisWeek;

                // 寵物統計（暫時使用假數據）
                viewModel.PetsWithMaxLevel = 0;
                viewModel.PetsWithMaxHappiness = 0;

                // 錢包統計
                var revenueStats = await _dashboardService.GetRevenueStatisticsAsync();
                viewModel.TotalPointsInCirculation = (int)revenueStats.TotalRevenue;
                viewModel.AveragePointsPerUser = viewModel.TotalUsers > 0
                    ? (int)(revenueStats.TotalRevenue / viewModel.TotalUsers)
                    : 0;

                // 優惠券統計（暫時使用假數據）
                viewModel.UsedCoupons = 0;
                viewModel.AvailableCoupons = 0;

                // 電子券統計（暫時使用假數據）
                viewModel.UsedEVouchers = 0;
                viewModel.AvailableEVouchers = 0;

                // 最近活動
                var recentActivities = await _dashboardService.GetRecentActivitiesAsync(10);
                viewModel.RecentSignIns = recentActivities
                    .Where(a => a.ActivityType == "SignIn")
                    .Take(5)
                    .Select(a => new RecentActivityViewModel
                    {
                        UserName = a.UserName,
                        Activity = a.Description,
                        Time = a.Timestamp,
                        PointsEarned = 0 // 從描述中解析
                    })
                    .ToList();

                viewModel.RecentGames = recentActivities
                    .Where(a => a.ActivityType != "SignIn")
                    .Take(5)
                    .Select(a => new RecentActivityViewModel
                    {
                        UserName = a.UserName,
                        Activity = a.Description,
                        Time = a.Timestamp,
                        PointsEarned = 0
                    })
                    .ToList();

                // 圖表數據
                viewModel.SignInChartData = await GetSignInChartDataAsync();
                viewModel.GameChartData = await GetGameChartDataAsync();
                viewModel.PointsChartData = await GetPointsChartDataAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                TempData["ErrorMessage"] = $"載入儀表板時發生錯誤：{ex.Message}";
                return View(viewModel);
            }
        }

        private async Task<List<ChartData>> GetSignInChartDataAsync()
        {
            var today = DateTime.Today;
            var chartData = new List<ChartData>();

            var trendData = await _signInService.GetSignInTrendDataAsync(7);

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateKey = date.ToString("yyyy-MM-dd");

                var count = trendData.ContainsKey(dateKey) ? trendData[dateKey] : 0;

                chartData.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Value = count
                });
            }

            return chartData;
        }

        private async Task<List<ChartData>> GetGameChartDataAsync()
        {
            var today = DateTime.Today;
            var chartData = new List<ChartData>();

            var trendData = await _dashboardService.GetGamePlayTrendAsync(7);

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateKey = date.ToString("yyyy-MM-dd");

                var count = trendData.ContainsKey(dateKey) ? trendData[dateKey] : 0;

                chartData.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Value = count
                });
            }

            return chartData;
        }

        private async Task<List<ChartData>> GetPointsChartDataAsync()
        {
            var today = DateTime.Today;
            var chartData = new List<ChartData>();

            var trendData = await _dashboardService.GetRevenueTrendAsync(7);

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateKey = date.ToString("yyyy-MM-dd");

                var points = trendData.ContainsKey(dateKey) ? (int)trendData[dateKey] : 0;

                chartData.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Value = points
                });
            }

            return chartData;
        }

        // API endpoints for dynamic data
        [HttpGet]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _dashboardService.GetDashboardOverviewAsync();
            return Json(overview);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserStatistics()
        {
            var stats = await _dashboardService.GetUserStatisticsAsync();
            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetGameStatistics()
        {
            var stats = await _dashboardService.GetGameStatisticsAsync();
            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueStatistics()
        {
            var stats = await _dashboardService.GetRevenueStatisticsAsync();
            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopUsers(int count = 10)
        {
            var topUsers = await _dashboardService.GetTopUsersAsync(count);
            return Json(topUsers);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopGames(int count = 10)
        {
            var topGames = await _dashboardService.GetTopGamesAsync(count);
            return Json(topGames);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentActivities(int count = 20)
        {
            var activities = await _dashboardService.GetRecentActivitiesAsync(count);
            return Json(activities);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveAlerts()
        {
            var alerts = await _dashboardService.GetActiveAlertsAsync();
            return Json(alerts);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserGrowthTrend(int days = 30)
        {
            var trend = await _dashboardService.GetUserGrowthTrendAsync(days);
            return Json(trend);
        }

        [HttpGet]
        public async Task<IActionResult> GetGamePlayTrend(int days = 30)
        {
            var trend = await _dashboardService.GetGamePlayTrendAsync(days);
            return Json(trend);
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueTrend(int days = 30)
        {
            var trend = await _dashboardService.GetRevenueTrendAsync(days);
            return Json(trend);
        }
    }
}

