using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GameSpace.Areas.MiniGame.Services
{
    public class DiagnosticsService : IDiagnosticsService
    {
        private readonly GameSpacedatabaseContext _context;
        private static readonly DateTime _startTime = DateTime.UtcNow;

        public DiagnosticsService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 系統診斷
        public async Task<SystemDiagnostics> GetSystemDiagnosticsAsync()
        {
            var diagnostics = new SystemDiagnostics
            {
                Version = "1.0.0",
                ServerTime = DateTime.UtcNow,
                UptimeSeconds = (long)(DateTime.UtcNow - _startTime).TotalSeconds,
                Database = await CheckDatabaseConnectionAsync(),
                Services = await CheckServicesHealthAsync()
            };

            diagnostics.IsHealthy = diagnostics.Database.IsConnected &&
                                   diagnostics.Services.All(s => s.Value);

            return diagnostics;
        }

        public async Task<DatabaseStatus> CheckDatabaseConnectionAsync()
        {
            var status = new DatabaseStatus();

            try
            {
                // 測試連線
                status.IsConnected = await _context.Database.CanConnectAsync();

                if (status.IsConnected)
                {
                    // 取得資料表數量
                    var tableNames = new[]
                    {
                        "Pet", "User_Wallet", "WalletHistory", "User_SignInStats",
                        "Coupon", "CouponType", "EVoucher", "EVoucherType",
                        "PetColorOptions", "PetBackgroundOptions", "Users", "Manager"
                    };
                    status.TableCount = tableNames.Length;

                    // 模擬連線數（實際需要資料庫特定查詢）
                    status.ConnectionCount = 1;

                    // 取得資料庫大小（簡化版）
                    status.TotalSize = 0; // 需要特定 SQL 查詢
                }
            }
            catch
            {
                status.IsConnected = false;
            }

            return status;
        }

        public async Task<Dictionary<string, bool>> CheckServicesHealthAsync()
        {
            var health = new Dictionary<string, bool>();

            try
            {
                // 檢查各個 Service 是否正常
                health["Database"] = await _context.Database.CanConnectAsync();
                health["PetService"] = await _context.Pets.AnyAsync() || !await _context.Pets.AnyAsync();
                health["WalletService"] = await _context.UserWallets.AnyAsync() || !await _context.UserWallets.AnyAsync();
                health["SignInService"] = await _context.UserSignInStats.AnyAsync() || !await _context.UserSignInStats.AnyAsync();
                health["CouponService"] = await _context.Coupons.AnyAsync() || !await _context.Coupons.AnyAsync();
            }
            catch
            {
                health["Unknown"] = false;
            }

            return health;
        }

        // 效能監控
        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            var metrics = new PerformanceMetrics();

            try
            {
                // CPU 使用率（需要 System.Diagnostics）
                using (var process = Process.GetCurrentProcess())
                {
                    metrics.CpuUsage = 0; // 簡化版，實際需要時間間隔計算
                    metrics.MemoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
                }

                // 資料庫連線數
                metrics.ActiveConnections = 1; // 簡化版

                // 平均回應時間（模擬）
                metrics.AverageResponseTime = 0;
                metrics.RequestsPerSecond = 0;

                await Task.CompletedTask;
            }
            catch
            {
                // 返回預設值
            }

            return metrics;
        }

        public async Task<IEnumerable<SlowQuery>> GetSlowQueriesAsync(int count = 10)
        {
            // 實際實作需要資料庫查詢日誌或 SQL Server Profiler
            // 這裡返回空列表作為預設實作
            await Task.CompletedTask;
            return new List<SlowQuery>();
        }

        // 錯誤日誌
        public async Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 50)
        {
            // 實際實作需要自訂錯誤日誌表
            // 這裡返回空列表作為預設實作
            await Task.CompletedTask;
            return new List<ErrorLog>();
        }

        public async Task<ErrorLog?> GetErrorDetailAsync(int errorId)
        {
            // 實際實作需要自訂錯誤日誌表
            await Task.CompletedTask;
            return null;
        }

        public async Task<bool> ClearOldErrorLogsAsync(int daysToKeep = 30)
        {
            try
            {
                // 實際實作需要自訂錯誤日誌表
                // 範例：
                // var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                // var oldLogs = await _context.ErrorLogs.Where(e => e.CreatedAt < cutoffDate).ToListAsync();
                // _context.ErrorLogs.RemoveRange(oldLogs);
                // await _context.SaveChangesAsync();

                await Task.CompletedTask;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 系統資訊
        public async Task<Dictionary<string, string>> GetSystemInfoAsync()
        {
            var info = new Dictionary<string, string>();

            try
            {
                info["OS"] = Environment.OSVersion.ToString();
                info["MachineName"] = Environment.MachineName;
                info["ProcessorCount"] = Environment.ProcessorCount.ToString();
                info["DotNetVersion"] = Environment.Version.ToString();
                info["CurrentDirectory"] = Environment.CurrentDirectory;
                info["UserName"] = Environment.UserName;
                info["Is64BitOS"] = Environment.Is64BitOperatingSystem.ToString();
                info["Is64BitProcess"] = Environment.Is64BitProcess.ToString();

                using (var process = Process.GetCurrentProcess())
                {
                    info["ProcessId"] = process.Id.ToString();
                    info["StartTime"] = process.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    info["TotalMemoryMB"] = (process.WorkingSet64 / 1024 / 1024).ToString();
                    info["ThreadCount"] = process.Threads.Count.ToString();
                }

                await Task.CompletedTask;
            }
            catch
            {
                info["Error"] = "Failed to retrieve system information";
            }

            return info;
        }

        public async Task<Dictionary<string, long>> GetDatabaseSizeInfoAsync()
        {
            var sizeInfo = new Dictionary<string, long>();

            try
            {
                // 各資料表記錄數
                sizeInfo["Pets"] = await _context.Pets.CountAsync();
                sizeInfo["UserWallets"] = await _context.UserWallets.CountAsync();
                sizeInfo["WalletHistories"] = await _context.WalletHistories.CountAsync();
                sizeInfo["UserSignInStats"] = await _context.UserSignInStats.CountAsync();
                sizeInfo["Coupons"] = await _context.Coupons.CountAsync();
                sizeInfo["CouponTypes"] = await _context.CouponTypes.CountAsync();
                sizeInfo["Evouchers"] = await _context.Evouchers.CountAsync();
                sizeInfo["EvoucherTypes"] = await _context.EvoucherTypes.CountAsync();
                sizeInfo["Users"] = await _context.Users.CountAsync();
                sizeInfo["ManagerData"] = await _context.ManagerData.CountAsync();
                sizeInfo["PetColorOptions"] = await _context.Set<PetColorOption>().CountAsync();
                sizeInfo["PetBackgroundOptions"] = await _context.Set<PetBackgroundOption>().CountAsync();

                // 總計
                sizeInfo["TotalRecords"] = sizeInfo.Values.Sum();
            }
            catch
            {
                sizeInfo["Error"] = -1;
            }

            return sizeInfo;
        }
    }
}

