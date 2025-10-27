using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物變更服務介面（Admin 專用）
    /// </summary>
    public interface IPetMutationService
    {
        /// <summary>
        /// 更新寵物系統整體規則
        /// </summary>
        /// <param name="model">規則輸入模型</param>
        /// <param name="operatorId">操作者 ID</param>
        /// <returns>操作結果</returns>
        Task<PetMutationResult> UpdatePetSystemRulesAsync(PetSystemRulesInputModel model, int operatorId);

        /// <summary>
        /// 更新寵物基本資料（名稱）
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="model">基本資料輸入模型</param>
        /// <param name="operatorId">操作者 ID</param>
        /// <returns>操作結果</returns>
        Task<PetMutationResult> UpdatePetBasicInfoAsync(int petId, PetBasicInfoInputModel model, int operatorId);

        /// <summary>
        /// 更新寵物外觀（膚色/背景色）
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="model">外觀輸入模型</param>
        /// <param name="operatorId">操作者 ID</param>
        /// <returns>操作結果</returns>
        Task<PetMutationResult> UpdatePetAppearanceAsync(int petId, PetAppearanceInputModel model, int operatorId);

        /// <summary>
        /// 更新寵物屬性（經驗值、等級、狀態值）
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="model">屬性輸入模型</param>
        /// <param name="operatorId">操作者 ID</param>
        /// <returns>操作結果</returns>
        Task<PetMutationResult> UpdatePetStatsAsync(int petId, PetStatsInputModel model, int operatorId);

        /// <summary>
        /// 重置寵物狀態（將所有狀態值重置為預設值）
        /// </summary>
        /// <param name="petId">寵物 ID</param>
        /// <param name="operatorId">操作者 ID</param>
        /// <returns>操作結果</returns>
        Task<PetMutationResult> ResetPetStatsAsync(int petId, int operatorId);
    }

    /// <summary>
    /// 寵物變更操作結果
    /// </summary>
    public class PetMutationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object>? Data { get; set; }

        public static PetMutationResult Succeeded(string message = "操作成功", Dictionary<string, object>? data = null)
        {
            return new PetMutationResult
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static PetMutationResult Failed(string message)
        {
            return new PetMutationResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
