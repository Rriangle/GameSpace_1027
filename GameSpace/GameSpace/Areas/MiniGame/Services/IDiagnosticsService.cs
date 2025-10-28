using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IDiagnosticsService
    {
        // 系統診斷
        Task<SystemDiagnostics> GetSystemDiagnosticsAsync();
        Task<DatabaseStatus> CheckDatabaseConnectionAsync();
        Task<Dictionary<string, bool>> CheckServicesHealthAsync();

        // 效能監控
        Task<PerformanceMetrics> GetPerformanceMetricsAsync();
        Task<IEnumerable<SlowQuery>> GetSlowQueriesAsync(int count = 10);

        // 錯誤日誌
        Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 50);
        Task<ErrorLog?> GetErrorDetailAsync(int errorId);
        Task<bool> ClearOldErrorLogsAsync(int daysToKeep = 30);

        // 系統資訊
        Task<Dictionary<string, string>> GetSystemInfoAsync();
        Task<Dictionary<string, long>> GetDatabaseSizeInfoAsync();
    }

    public class SystemDiagnostics
    {
        public bool IsHealthy { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; }
        public long UptimeSeconds { get; set; }
        public DatabaseStatus Database { get; set; } = new();
        public Dictionary<string, bool> Services { get; set; } = new();
    }

    public class DatabaseStatus
    {
        public bool IsConnected { get; set; }
        public int ConnectionCount { get; set; }
        public long TotalSize { get; set; }
        public int TableCount { get; set; }
    }

    public class PerformanceMetrics
    {
        public double CpuUsage { get; set; }
        public long MemoryUsage { get; set; }
        public int ActiveConnections { get; set; }
        public double AverageResponseTime { get; set; }
        public int RequestsPerSecond { get; set; }
    }

    public class SlowQuery
    {
        public string Query { get; set; } = string.Empty;
        public double ExecutionTimeMs { get; set; }
        public DateTime ExecutedAt { get; set; }
    }

    public class ErrorLog
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

