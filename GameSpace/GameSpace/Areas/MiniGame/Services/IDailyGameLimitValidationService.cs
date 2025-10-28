using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 每日遊戲次數限制驗證服務介面
    /// </summary>
    public interface IDailyGameLimitValidationService
    {
        /// <summary>
        /// 驗證每日遊戲次數限制設定
        /// </summary>
        Task<ValidationResult> ValidateDailyGameLimitAsync(DailyGameLimitCreateViewModel model);

        /// <summary>
        /// 驗證編輯每日遊戲次數限制設定
        /// </summary>
        Task<ValidationResult> ValidateDailyGameLimitEditAsync(DailyGameLimitEditViewModel model);

        /// <summary>
        /// 驗證使用者是否可以進行遊戲
        /// </summary>
        Task<GameValidationResult> ValidateUserGameAccessAsync(int userId);
    }
}
