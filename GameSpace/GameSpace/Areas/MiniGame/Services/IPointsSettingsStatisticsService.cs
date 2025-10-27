namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 點數設定統計服務介面
    /// </summary>
    public interface IPointsSettingsStatisticsService
    {
        /// <summary>
        /// 獲取點數統計資訊
        /// </summary>
        Task<Dictionary<string, object>> GetStatisticsAsync();

        /// <summary>
        /// 獲取點數分佈資訊
        /// </summary>
        Task<Dictionary<string, int>> GetPointsDistributionAsync();

        /// <summary>
        /// 獲取點數使用趨勢
        /// </summary>
        Task<Dictionary<string, int>> GetPointsUsageTrendAsync(int days = 30);

        /// <summary>
        /// 獲取顏色設定總數
        /// </summary>
        Task<int> GetTotalColorSettingsAsync();

        /// <summary>
        /// 獲取背景設定總數
        /// </summary>
        Task<int> GetTotalBackgroundSettingsAsync();

        /// <summary>
        /// 獲取啟用的顏色設定數量
        /// </summary>
        Task<int> GetActiveColorSettingsAsync();

        /// <summary>
        /// 獲取啟用的背景設定數量
        /// </summary>
        Task<int> GetActiveBackgroundSettingsAsync();

        /// <summary>
        /// 獲取所有顏色變更總點數
        /// </summary>
        Task<int> GetTotalColorPointsAsync();

        /// <summary>
        /// 獲取所有背景變更總點數
        /// </summary>
        Task<int> GetTotalBackgroundPointsAsync();

        /// <summary>
        /// 獲取總點數（顏色+背景）
        /// </summary>
        Task<int> GetTotalPointsAsync();
    }
}

