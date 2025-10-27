namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物每日屬性衰減服務介面
    /// 處理每日凌晨 00:00 (Asia/Taipei) 的屬性衰減
    /// </summary>
    public interface IPetDailyDecayService
    {
        /// <summary>
        /// 應用每日屬性衰減至指定寵物
        /// 規格 7.6：每日凌晨 00:00
        /// - 飢餓值 -20
        /// - 心情值 -30
        /// - 體力值 -10
        /// - 清潔值 -20
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <returns>操作結果</returns>
        Task<PetDecayResult> ApplyDailyDecayAsync(int petId);

        /// <summary>
        /// 應用每日屬性衰減至所有寵物
        /// </summary>
        /// <returns>處理的寵物數量及結果</returns>
        Task<PetDecayBatchResult> ApplyDailyDecayToAllPetsAsync();

        /// <summary>
        /// 檢查是否需要執行每日衰減（用於排程檢查）
        /// </summary>
        /// <returns>true 表示需要執行衰減</returns>
        Task<bool> ShouldApplyDailyDecayAsync();

        /// <summary>
        /// 取得上次執行衰減的時間（台北時區）
        /// </summary>
        Task<DateTime?> GetLastDecayTimeAsync();
    }

    /// <summary>
    /// 寵物衰減操作結果
    /// </summary>
    public class PetDecayResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 結果訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 寵物 ID
        /// </summary>
        public int PetId { get; set; }

        /// <summary>
        /// 衰減後的屬性值
        /// </summary>
        public Dictionary<string, int> UpdatedStats { get; set; } = new();

        public static PetDecayResult Succeeded(int petId, Dictionary<string, int> stats, string message = "衰減成功")
        {
            return new PetDecayResult
            {
                Success = true,
                PetId = petId,
                UpdatedStats = stats,
                Message = message
            };
        }

        public static PetDecayResult Failed(int petId, string message)
        {
            return new PetDecayResult
            {
                Success = false,
                PetId = petId,
                Message = message
            };
        }
    }

    /// <summary>
    /// 批次寵物衰減操作結果
    /// </summary>
    public class PetDecayBatchResult
    {
        /// <summary>
        /// 處理成功的寵物數量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 處理失敗的寵物數量
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 總處理寵物數量
        /// </summary>
        public int TotalCount => SuccessCount + FailureCount;

        /// <summary>
        /// 個別結果
        /// </summary>
        public List<PetDecayResult> Results { get; set; } = new();

        /// <summary>
        /// 執行時間（台北時區）
        /// </summary>
        public DateTime ExecutedAt { get; set; }
    }
}
