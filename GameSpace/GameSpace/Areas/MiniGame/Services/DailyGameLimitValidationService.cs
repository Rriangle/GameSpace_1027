using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 每日遊戲次數限制驗證服務
    /// </summary>
    public class DailyGameLimitValidationService : IDailyGameLimitValidationService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<DailyGameLimitValidationService> _logger;
        private readonly GameSpace.Infrastructure.Time.IAppClock _appClock;

        public DailyGameLimitValidationService(GameSpacedatabaseContext context, ILogger<DailyGameLimitValidationService> logger, GameSpace.Infrastructure.Time.IAppClock appClock)
        {
            _context = context;
            _logger = logger;
            _appClock = appClock;
        }

        /// <summary>
        /// 驗證每日遊戲次數限制設定
        /// </summary>
        public async Task<ValidationResult> ValidateDailyGameLimitAsync(DailyGameLimitCreateViewModel model)
        {
            var result = new ValidationResult();

            try
            {
                // 驗證每日限制範圍
                if (model.DailyLimit < 1 || model.DailyLimit > 100)
                {
                    result.AddError("每日遊戲次數限制必須在1-100之間");
                }

                // 驗證設定名稱唯一性
                var existingSetting = await _context.Set<DailyGameLimit>()
                    .FirstOrDefaultAsync(x => x.SettingName == model.SettingName);
                
                if (existingSetting != null)
                {
                    result.AddError("設定名稱已存在，請使用其他名稱");
                }

                // 驗證描述長度
                if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 500)
                {
                    result.AddError("設定描述長度不能超過500個字元");
                }

                if (result.IsValid)
                {
                    _logger.LogInformation("每日遊戲次數限制設定驗證通過");
                }
                else
                {
                    _logger.LogWarning("每日遊戲次數限制設定驗證失敗: {Errors}", string.Join(", ", result.Errors));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證每日遊戲次數限制設定時發生錯誤");
                result.AddError("驗證時發生錯誤，請稍後再試");
                return result;
            }
        }

        /// <summary>
        /// 驗證編輯每日遊戲次數限制設定
        /// </summary>
        public async Task<ValidationResult> ValidateDailyGameLimitEditAsync(DailyGameLimitEditViewModel model)
        {
            var result = new ValidationResult();

            try
            {
                // 驗證每日限制範圍
                if (model.DailyLimit < 1 || model.DailyLimit > 100)
                {
                    result.AddError("每日遊戲次數限制必須在1-100之間");
                }

                // 驗證設定名稱唯一性（排除自己）
                var existingSetting = await _context.Set<DailyGameLimit>()
                    .FirstOrDefaultAsync(x => x.SettingName == model.SettingName && x.Id != model.Id);
                
                if (existingSetting != null)
                {
                    result.AddError("設定名稱已存在，請使用其他名稱");
                }

                // 驗證描述長度
                if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 500)
                {
                    result.AddError("設定描述長度不能超過500個字元");
                }

                // 驗證設定是否存在
                var setting = await _context.Set<DailyGameLimit>().FindAsync(model.Id);
                if (setting == null)
                {
                    result.AddError("找不到要編輯的設定");
                }

                if (result.IsValid)
                {
                    _logger.LogInformation("每日遊戲次數限制設定編輯驗證通過 ID: {Id}", model.Id);
                }
                else
                {
                    _logger.LogWarning("每日遊戲次數限制設定編輯驗證失敗 ID: {Id}, 錯誤: {Errors}", 
                        model.Id, string.Join(", ", result.Errors));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證每日遊戲次數限制設定編輯 ID: {Id} 時發生錯誤", model.Id);
                result.AddError("驗證時發生錯誤，請稍後再試");
                return result;
            }
        }

        /// <summary>
        /// 驗證使用者是否可以進行遊戲
        /// </summary>
        public async Task<GameValidationResult> ValidateUserGameAccessAsync(int userId)
        {
            var result = new GameValidationResult();

            try
            {
                // 取得目前啟用的設定
                var currentSetting = await _context.Set<DailyGameLimit>()
                    .Where(x => x.IsEnabled)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (currentSetting == null)
                {
                    // 如果沒有設定，使用預設值（一天三次）
                    result.DailyLimit = 3;
                    result.IsDefaultLimit = true;
                }
                else
                {
                    result.DailyLimit = currentSetting.DailyLimit;
                    result.IsDefaultLimit = false;
                }

                // 取得使用者今日已玩遊戲次數
                var todayGameCount = await GetTodayGameCountAsync(userId);
                result.TodayGameCount = todayGameCount;
                result.RemainingGames = Math.Max(0, result.DailyLimit - todayGameCount);

                // 檢查是否超過限制
                if (todayGameCount >= result.DailyLimit)
                {
                    result.IsExceeded = true;
                    result.AddError($"今日遊戲次數已達上限 ({result.DailyLimit} 次)，請明天再來");
                }
                else
                {
                    result.IsExceeded = false;
                }

                _logger.LogInformation("驗證使用者 {UserId} 遊戲權限，今日已玩: {TodayCount}, 限制: {DailyLimit}, 剩餘: {Remaining}", 
                    userId, todayGameCount, result.DailyLimit, result.RemainingGames);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證使用者 {UserId} 遊戲權限時發生錯誤", userId);
                result.AddError("驗證遊戲權限時發生錯誤，請稍後再試");
                return result;
            }
        }

        /// <summary>
        /// 取得使用者今日已玩遊戲次數
        /// </summary>
        private async Task<int> GetTodayGameCountAsync(int userId)
        {
            try
            {
                var taipeiNow = _appClock.ToAppTime(_appClock.UtcNow);
                var startUtc = _appClock.ToUtc(taipeiNow.Date);
                var endUtc = _appClock.ToUtc(taipeiNow.Date.AddDays(1));

                var todayGameCountTaipei = await _context.MiniGames
                    .Where(x => x.UserId == userId && x.StartTime >= startUtc && x.StartTime < endUtc)
                    .CountAsync();
                return todayGameCountTaipei;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 今日遊戲次數時發生錯誤", userId);
                throw;
            }
        }
    }

    /// <summary>
    /// 遊戲驗證結果
    /// </summary>
    public class GameValidationResult : ValidationResult
    {
        public int DailyLimit { get; set; }
        public int TodayGameCount { get; set; }
        public int RemainingGames { get; set; }
        public bool IsExceeded { get; set; }
        public bool IsDefaultLimit { get; set; }
    }
}

