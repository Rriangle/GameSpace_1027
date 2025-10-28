using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物互動服務介面
    /// 處理寵物餵食、洗澡、玩耍、哄睡等互動操作
    /// </summary>
    public interface IPetInteractionService
    {
        /// <summary>
        /// 餵食寵物
        /// 效果：飢餓值 +10
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="userId">使用者 ID（用於權限檢查）</param>
        /// <returns>操作結果，包含更新後的寵物狀態</returns>
        Task<PetInteractionResult> FeedAsync(int petId, int userId);

        /// <summary>
        /// 為寵物洗澡
        /// 效果：清潔值 +10
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="userId">使用者 ID（用於權限檢查）</param>
        /// <returns>操作結果，包含更新後的寵物狀態</returns>
        Task<PetInteractionResult> BathAsync(int petId, int userId);

        /// <summary>
        /// 哄寵物睡覺
        /// 效果：心情值 +10
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="userId">使用者 ID（用於權限檢查）</param>
        /// <returns>操作結果，包含更新後的寵物狀態</returns>
        Task<PetInteractionResult> PlayAsync(int petId, int userId);

        /// <summary>
        /// 讓寵物休息
        /// 效果：體力值 +10
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="userId">使用者 ID（用於權限檢查）</param>
        /// <returns>操作結果，包含更新後的寵物狀態</returns>
        Task<PetInteractionResult> SleepAsync(int petId, int userId);

        /// <summary>
        /// 檢查並獎勵每日全滿狀態獎勵
        /// 當飢餓、心情、體力、清潔四項值均達到 100 時：
        /// 1. 額外獲得 100 點寵物經驗值（每日首次）
        /// 2. 健康值恢復至 100
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <returns>是否發放了獎勵</returns>
        Task<bool> CheckAndAwardDailyFullStatsBonusAsync(int petId);
    }

    /// <summary>
    /// 寵物互動操作結果
    /// </summary>
    public class PetInteractionResult
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
        /// 更新後的寵物狀態
        /// </summary>
        public PetStatsViewModel? PetStats { get; set; }

        /// <summary>
        /// 是否觸發了每日全滿獎勵
        /// </summary>
        public bool DailyBonusAwarded { get; set; }

        /// <summary>
        /// 是否恢復了健康值
        /// </summary>
        public bool HealthRestored { get; set; }

        public static PetInteractionResult Succeeded(string message, PetStatsViewModel? stats = null, bool dailyBonus = false, bool healthRestored = false)
        {
            return new PetInteractionResult
            {
                Success = true,
                Message = message,
                PetStats = stats,
                DailyBonusAwarded = dailyBonus,
                HealthRestored = healthRestored
            };
        }

        public static PetInteractionResult Failed(string message)
        {
            return new PetInteractionResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
