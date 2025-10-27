using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 小遊戲系統寫入/調整服務 (Admin)
    /// 負責遊戲規則設定、關卡配置、每日次數限制的調整操作
    /// </summary>
    public interface IGameMutationService
    {
        /// <summary>
        /// 更新遊戲規則（整體設定）
        /// 包含遊戲名稱、描述、每日次數限制、啟用狀態等
        /// </summary>
        /// <param name="input">遊戲規則輸入模型</param>
        /// <param name="operatorId">操作者管理員 ID</param>
        /// <returns>是否成功</returns>
        Task<(bool success, string message)> UpdateGameRulesAsync(GameRulesInputModel input, int operatorId);

        /// <summary>
        /// 更新單一關卡設定
        /// 包含怪物數量、移動速度、各項獎勵（點數、經驗值、優惠券）
        /// </summary>
        /// <param name="input">關卡設定輸入模型</param>
        /// <param name="operatorId">操作者管理員 ID</param>
        /// <returns>是否成功</returns>
        Task<(bool success, string message)> UpdateLevelSettingsAsync(LevelSettingsInputModel input, int operatorId);

        /// <summary>
        /// 更新每日遊戲次數限制（全域設定）
        /// </summary>
        /// <param name="input">每日次數限制輸入模型</param>
        /// <param name="operatorId">操作者管理員 ID</param>
        /// <returns>是否成功</returns>
        Task<(bool success, string message)> UpdateDailyLimitAsync(DailyLimitInputModel input, int operatorId);

        /// <summary>
        /// 手動調整遊戲紀錄（補發獎勵或修正錯誤）
        /// </summary>
        /// <param name="playId">遊戲紀錄 ID</param>
        /// <param name="input">調整資料輸入模型</param>
        /// <param name="operatorId">操作者管理員 ID</param>
        /// <returns>是否成功</returns>
        Task<(bool success, string message)> ManualAdjustGameRecordAsync(int playId, GameRecordAdjustmentInputModel input, int operatorId);

        /// <summary>
        /// 驗證關卡設定的合理性
        /// 檢查怪物數量、速度倍率、獎勵範圍是否合理
        /// </summary>
        /// <param name="level">關卡等級</param>
        /// <param name="monsterCount">怪物數量</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="winPoints">勝利點數</param>
        /// <param name="winExp">勝利經驗值</param>
        /// <returns>驗證結果（是否有效，錯誤訊息）</returns>
        (bool isValid, string errorMessage) ValidateLevelSettings(int level, int monsterCount, decimal speedMultiplier, int winPoints, int winExp);

        /// <summary>
        /// 批量更新所有關卡設定
        /// </summary>
        /// <param name="levelInputs">所有關卡設定列表</param>
        /// <param name="operatorId">操作者管理員 ID</param>
        /// <returns>是否成功</returns>
        Task<(bool success, string message)> BatchUpdateLevelSettingsAsync(List<LevelSettingsInputModel> levelInputs, int operatorId);
    }
}
