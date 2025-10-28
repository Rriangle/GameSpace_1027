using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 小遊戲玩法服務介面
    /// 負責遊戲開始、結束、難度進程、寵物狀態檢查等核心遊戲邏輯
    /// </summary>
    public interface IGamePlayService
    {
        /// <summary>
        /// 檢查寵物健康狀態是否允許開始冒險
        /// 規則: 飢餓、心情、體力、清潔、健康任一屬性值為 0 則無法開始冒險
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <returns>是否允許開始、錯誤訊息</returns>
        Task<(bool canStart, string message)> CheckPetHealthForAdventureAsync(int petId);

        /// <summary>
        /// 獲取寵物當前應該挑戰的關卡等級
        /// 規則: 首次從第 1 關開始；勝利升級、失敗保持、最高第 3 關
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="petId">寵物 ID</param>
        /// <returns>關卡等級 (1-3)</returns>
        Task<int> GetCurrentLevelForPetAsync(int userId, int petId);

        /// <summary>
        /// 開始遊戲並記錄遊戲狀態
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="petId">寵物 ID</param>
        /// <returns>遊戲記錄 ID、關卡等級</returns>
        Task<(bool success, string message, int? playId, int level)> StartAdventureAsync(int userId, int petId);

        /// <summary>
        /// 結束遊戲並處理結果影響
        /// 規則: 勝利時飢餓-20、心情+30、體力-20、清潔-20
        ///      失敗時飢餓-20、心情-30、體力-20、清潔-20
        /// </summary>
        /// <param name="playId">遊戲記錄 ID</param>
        /// <param name="isWin">是否勝利</param>
        /// <param name="pointsEarned">獲得點數</param>
        /// <param name="expEarned">獲得經驗值</param>
        /// <param name="couponEarned">獲得優惠券編號</param>
        /// <returns>是否成功、訊息</returns>
        Task<(bool success, string message)> EndAdventureAsync(
            int playId,
            bool isWin,
            int pointsEarned,
            int expEarned,
            string? couponEarned = null);

        /// <summary>
        /// 中止遊戲
        /// </summary>
        /// <param name="playId">遊戲記錄 ID</param>
        /// <returns>是否成功、訊息</returns>
        Task<(bool success, string message)> AbortAdventureAsync(int playId);

        /// <summary>
        /// 更新寵物難度等級(基於遊戲結果)
        /// 規則: 勝利時升級(最高第3關)、失敗時保持原等級
        /// </summary>
        /// <param name="userId">用戶 ID</param>
        /// <param name="petId">寵物 ID</param>
        /// <param name="currentLevel">當前關卡</param>
        /// <param name="isWin">是否勝利</param>
        /// <returns>新的關卡等級</returns>
        Task<int> UpdatePetDifficultyLevelAsync(int userId, int petId, int currentLevel, bool isWin);

        /// <summary>
        /// 應用遊戲結果對寵物屬性的影響
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="isWin">是否勝利</param>
        /// <returns>是否成功、訊息</returns>
        Task<(bool success, string message)> ApplyGameResultToPetStatsAsync(int petId, bool isWin);
    }
}
