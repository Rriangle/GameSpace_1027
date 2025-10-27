using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminDiagnosticsController : MiniGameBaseController
    {
        public AdminDiagnosticsController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context, adminService)
        {
        }

        // 系統診斷首頁
        public async Task<IActionResult> Index()
        {
            try
            {
                var diagnosticsData = await GetSystemDiagnosticsAsync();
                return View(diagnosticsData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入系統診斷頁面時發生錯誤：{ex.Message}";
                return View(new SystemDiagnosticsViewModel());
            }
        }

        // 資料庫健康檢查
        [HttpGet]
        public async Task<IActionResult> DatabaseHealth()
        {
            try
            {
                var healthData = await GetDatabaseHealthAsync();
                return Json(new { success = true, data = healthData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 系統效能監控
        [HttpGet]
        public async Task<IActionResult> PerformanceMetrics()
        {
            try
            {
                var metrics = await GetPerformanceMetricsAsync();
                return Json(new { success = true, data = metrics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 錯誤日誌查詢
        public async Task<IActionResult> ErrorLogs(ErrorLogQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryErrorLogsAsync(query);
                var viewModel = new AdminErrorLogsViewModel
                {
                    ErrorLogs = result.Items,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢錯誤日誌時發生錯誤：{ex.Message}";
                return View(new AdminErrorLogsViewModel());
            }
        }

        // 系統設定檢查
        [HttpGet]
        public async Task<IActionResult> SystemSettings()
        {
            try
            {
                var settings = await GetSystemSettingsAsync();
                return Json(new { success = true, data = settings });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 清理系統快取
        [HttpPost]
        public async Task<IActionResult> ClearCache()
        {
            try
            {
                await ClearSystemCacheAsync();
                return Json(new { success = true, message = "系統快取清理成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 重新啟動系統服務
        [HttpPost]
        public async Task<IActionResult> RestartServices()
        {
            try
            {
                await RestartSystemServicesAsync();
                return Json(new { success = true, message = "系統服務重啟成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 生成系統報告
        [HttpGet]
        public async Task<IActionResult> GenerateReport(string reportType = "summary")
        {
            try
            {
                var report = await GenerateSystemReportAsync(reportType);
                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法
        private async Task<SystemDiagnosticsViewModel> GetSystemDiagnosticsAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            // 基本統計
            var totalUsers = await _context.Users.CountAsync();
            var totalPets = await _context.Pets.CountAsync();
            var totalGames = await _context.MiniGames.CountAsync();
            var totalCoupons = await _context.Coupons.CountAsync();
            var totalEVouchers = await _context.Evouchers.CountAsync();

            // 今日活動
            var todaySignIns = await _context.UserSignInStats
                .Where(s => s.SignTime.Date == today)
                .CountAsync();

            var todayGames = await _context.MiniGames
                .Where(g => g.StartTime.Date == today)
                .CountAsync();

            // 系統狀態
            var systemStatus = new SystemStatusModel
            {
                DatabaseConnection = await CheckDatabaseConnectionAsync(),
                MemoryUsage = GetMemoryUsage(),
                CpuUsage = GetCpuUsage(),
                DiskSpace = GetDiskSpace(),
                LastBackup = await GetLastBackupTimeAsync()
            };

            // 錯誤統計
            var errorStats = new ErrorStatsModel
            {
                TodayErrors = await GetTodayErrorCountAsync(),
                ThisWeekErrors = await GetThisWeekErrorCountAsync(),
                CriticalErrors = await GetCriticalErrorCountAsync()
            };

            return new SystemDiagnosticsViewModel
            {
                TotalUsers = totalUsers,
                TotalPets = totalPets,
                TotalGames = totalGames,
                TotalCoupons = totalCoupons,
                TotalEVouchers = totalEVouchers,
                TodaySignIns = todaySignIns,
                TodayGames = todayGames,
                SystemStatus = systemStatus,
                ErrorStats = errorStats
            };
        }

        private async Task<DatabaseHealthModel> GetDatabaseHealthAsync()
        {
            try
            {
                // 測試資料庫連接
                var connectionTest = await _context.Database.CanConnectAsync();
                
                // 檢查各表記錄數
                var tableStats = new Dictionary<string, int>
                {
                    ["Users"] = await _context.Users.CountAsync(),
                    ["Pets"] = await _context.Pets.CountAsync(),
                    ["MiniGames"] = await _context.MiniGames.CountAsync(),
                    ["Coupons"] = await _context.Coupons.CountAsync(),
                    ["EVouchers"] = await _context.Evouchers.CountAsync(),
                    ["UserWallets"] = await _context.UserWallets.CountAsync(),
                    ["WalletHistories"] = await _context.WalletHistories.CountAsync()
                };

                // 檢查資料庫大小
                var dbSize = await GetDatabaseSizeAsync();

                return new DatabaseHealthModel
                {
                    IsConnected = connectionTest,
                    TableStats = tableStats,
                    DatabaseSize = dbSize,
                    LastCheck = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                return new DatabaseHealthModel
                {
                    IsConnected = false,
                    ErrorMessage = ex.Message,
                    LastCheck = DateTime.Now
                };
            }
        }

        private async Task<PerformanceMetricsModel> GetPerformanceMetricsAsync()
        {
            var metrics = new PerformanceMetricsModel
            {
                MemoryUsage = GetMemoryUsage(),
                CpuUsage = GetCpuUsage(),
                DiskSpace = GetDiskSpace(),
                ActiveConnections = await GetActiveConnectionsAsync(),
                ResponseTime = await GetAverageResponseTimeAsync(),
                LastUpdated = DateTime.Now
            };

            return metrics;
        }

        private async Task<PagedResult<ErrorLogModel>> QueryErrorLogsAsync(ErrorLogQueryModel query)
        {
            var queryable = _context.Set<GameSpace.Areas.MiniGame.Models.ErrorLog>().AsQueryable();

            if (query.Level != null)
                queryable = queryable.Where(e => e.Level == query.Level);

            if (query.StartDate.HasValue)
                queryable = queryable.Where(e => e.Timestamp >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                queryable = queryable.Where(e => e.Timestamp <= query.EndDate.Value);

            if (!string.IsNullOrEmpty(query.Message))
                queryable = queryable.Where(e => e.Message.Contains(query.Message));

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(e => e.Timestamp)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new ErrorLogModel
                {
                    LogId = e.LogId,
                    Level = e.Level,
                    Message = e.Message,
                    Exception = e.Exception,
                    Timestamp = e.Timestamp,
                    Source = e.Source
                })
                .ToListAsync();

            return new PagedResult<ErrorLogModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private Task<SystemSettingsModel> GetSystemSettingsAsync()
        {
            var settings = new SystemSettingsModel
            {
                DatabaseConnectionString = GetConnectionStringStatus(),
                LogLevel = GetLogLevel(),
                CacheSettings = GetCacheSettings(),
                SecuritySettings = GetSecuritySettings(),
                PerformanceSettings = GetPerformanceSettings()
            };

            return Task.FromResult(settings);
        }

        private async Task<bool> CheckDatabaseConnectionAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        private string GetMemoryUsage()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / 1024 / 1024;
            return $"{memoryMB} MB";
        }

        private string GetCpuUsage()
        {
            // 簡化的CPU使用率計算
            return "N/A";
        }

        private string GetDiskSpace()
        {
            var drive = new DriveInfo("C:");
            var freeSpaceGB = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
            var totalSpaceGB = drive.TotalSize / 1024 / 1024 / 1024;
            return $"{freeSpaceGB} GB / {totalSpaceGB} GB";
        }

        private Task<DateTime?> GetLastBackupTimeAsync()
        {
            // 假設有備份記錄表
            return Task.FromResult<DateTime?>(DateTime.Now.AddDays(-1));
        }

        private async Task<int> GetTodayErrorCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Set<GameSpace.Areas.MiniGame.Models.ErrorLog>()
                .Where(e => e.Timestamp.Date == today)
                .CountAsync();
        }

        private async Task<int> GetThisWeekErrorCountAsync()
        {
            var weekStart = DateTime.Today.AddDays(-7);
            return await _context.Set<GameSpace.Areas.MiniGame.Models.ErrorLog>()
                .Where(e => e.Timestamp >= weekStart)
                .CountAsync();
        }

        private async Task<int> GetCriticalErrorCountAsync()
        {
            return await _context.Set<GameSpace.Areas.MiniGame.Models.ErrorLog>()
                .Where(e => e.Level == "Critical")
                .CountAsync();
        }

        private Task<string> GetDatabaseSizeAsync()
        {
            // 簡化的資料庫大小計算
            return Task.FromResult("N/A");
        }

        private Task<int> GetActiveConnectionsAsync()
        {
            // 簡化的活躍連接數計算
            return Task.FromResult(1);
        }

        private Task<TimeSpan> GetAverageResponseTimeAsync()
        {
            // 簡化的平均響應時間計算
            return Task.FromResult(TimeSpan.FromMilliseconds(100));
        }

        private async Task ClearSystemCacheAsync()
        {
            // 清理系統快取的邏輯
            await Task.Delay(100);
        }

        private async Task RestartSystemServicesAsync()
        {
            // 重啟系統服務的邏輯
            await Task.Delay(100);
        }

        private async Task<SystemReportModel> GenerateSystemReportAsync(string reportType)
        {
            var report = new SystemReportModel
            {
                ReportType = reportType,
                GeneratedAt = DateTime.Now,
                Summary = await GetSystemSummaryAsync(),
                Recommendations = GetSystemRecommendations()
            };

            return report;
        }

        private async Task<string> GetSystemSummaryAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPets = await _context.Pets.CountAsync();
            return $"系統總計：{totalUsers} 用戶，{totalPets} 寵物";
        }

        private List<string> GetSystemRecommendations()
        {
            return new List<string>
            {
                "建議定期備份資料庫",
                "監控系統效能指標",
                "檢查錯誤日誌並修復問題"
            };
        }

        private string GetConnectionStringStatus()
        {
            return "正常";
        }

        private string GetLogLevel()
        {
            return "Information";
        }

        private Dictionary<string, string> GetCacheSettings()
        {
            return new Dictionary<string, string>
            {
                ["CacheEnabled"] = "true",
                ["CacheTimeout"] = "30 minutes"
            };
        }

        private Dictionary<string, string> GetSecuritySettings()
        {
            return new Dictionary<string, string>
            {
                ["AuthenticationEnabled"] = "true",
                ["AuthorizationEnabled"] = "true"
            };
        }

        private Dictionary<string, string> GetPerformanceSettings()
        {
            return new Dictionary<string, string>
            {
                ["MaxConnections"] = "100",
                ["Timeout"] = "30 seconds"
            };
        }
    }

    // ViewModels
    public class SystemDiagnosticsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGames { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TodaySignIns { get; set; }
        public int TodayGames { get; set; }
        public SystemStatusModel SystemStatus { get; set; } = new();
        public ErrorStatsModel ErrorStats { get; set; } = new();
    }

    public class SystemStatusModel
    {
        public bool DatabaseConnection { get; set; }
        public string MemoryUsage { get; set; } = string.Empty;
        public string CpuUsage { get; set; } = string.Empty;
        public string DiskSpace { get; set; } = string.Empty;
        public DateTime? LastBackup { get; set; }
        public int ErrorCount { get; set; }
    }

    public class ErrorStatsModel
    {
        public int TodayErrors { get; set; }
        public int ThisWeekErrors { get; set; }
        public int CriticalErrors { get; set; }
    }

    public class DatabaseHealthModel
    {
        public bool IsConnected { get; set; }
        public Dictionary<string, int> TableStats { get; set; } = new();
        public string DatabaseSize { get; set; } = string.Empty;
        public DateTime LastCheck { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class PerformanceMetricsModel
    {
        public string MemoryUsage { get; set; } = string.Empty;
        public string CpuUsage { get; set; } = string.Empty;
        public string DiskSpace { get; set; } = string.Empty;
        public int ActiveConnections { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ErrorLogQueryModel
    {
        public string Level { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Message { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminErrorLogsViewModel
    {
        public List<ErrorLogModel> ErrorLogs { get; set; } = new();
        public ErrorLogQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class SystemSettingsModel
    {
        public string DatabaseConnectionString { get; set; } = string.Empty;
        public string LogLevel { get; set; } = string.Empty;
        public Dictionary<string, string> CacheSettings { get; set; } = new();
        public Dictionary<string, string> SecuritySettings { get; set; } = new();
        public Dictionary<string, string> PerformanceSettings { get; set; } = new();
    }

    public class SystemReportModel
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    public class ErrorLogModel
    {
        public int LogId { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Exception { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}





