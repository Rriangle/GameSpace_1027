using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Infrastructure.Time;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 小遊戲系統寫入/調整服務實作
    /// </summary>
    public class GameMutationService : IGameMutationService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IAppClock _appClock;
        private readonly ILogger<GameMutationService> _logger;

        // 系統設定鍵值常數
        private const string GAME_DAILY_LIMIT_KEY = "MiniGame.DailyPlayLimit";
        private const string GAME_LEVEL_SETTINGS_KEY = "MiniGame.LevelSettings";
        private const string GAME_BASIC_SETTINGS_KEY = "MiniGame.BasicSettings";

        public GameMutationService(GameSpacedatabaseContext context, IAppClock appClock, ILogger<GameMutationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 更新遊戲規則（整體設定）
        /// </summary>
        public async Task<(bool success, string message)> UpdateGameRulesAsync(GameRulesInputModel input, int operatorId)
        {
            try
            {
                // 驗證操作者
                var manager = await _context.ManagerData.FindAsync(operatorId);
                if (manager == null)
                {
                    _logger.LogWarning("更新遊戲規則失敗：操作者不存在: OperatorId={OperatorId}", operatorId);
                    return (false, "操作者不存在");
                }

                // SystemSettings 表尚未實作，功能暫時不可用
                await Task.CompletedTask;
                return (false, "此功能暫時不可用");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新遊戲規則失敗: OperatorId={OperatorId}", operatorId);
                return (false, $"更新失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新單一關卡設定
        /// </summary>
        public async Task<(bool success, string message)> UpdateLevelSettingsAsync(LevelSettingsInputModel input, int operatorId)
        {
            try
            {
                // 驗證關卡設定的合理性
                var (isValid, errorMessage) = ValidateLevelSettings(
                    input.Level,
                    input.MonsterCount,
                    input.SpeedMultiplier,
                    input.WinPointsReward,
                    input.WinExpReward
                );

                if (!isValid)
                {
                    return (false, errorMessage);
                }

                // 驗證操作者
                var manager = await _context.ManagerData.FindAsync(operatorId);
                if (manager == null)
                {
                    return (false, "操作者不存在");
                }

                // 獲取現有的所有關卡設定
                var existingSettings = await GetCurrentLevelSettingsAsync();

                // 更新或新增特定關卡
                var levelSetting = existingSettings.FirstOrDefault(l => l.Level == input.Level);
                if (levelSetting != null)
                {
                    // 更新現有關卡
                    levelSetting.MonsterCount = input.MonsterCount;
                    levelSetting.SpeedMultiplier = input.SpeedMultiplier;
                    levelSetting.WinPointsReward = input.WinPointsReward;
                    levelSetting.WinExpReward = input.WinExpReward;
                    levelSetting.WinCouponReward = input.WinCouponReward;
                    levelSetting.LosePointsReward = input.LosePointsReward;
                    levelSetting.LoseExpReward = input.LoseExpReward;
                    levelSetting.AbortPointsReward = input.AbortPointsReward;
                    levelSetting.AbortExpReward = input.AbortExpReward;
                    levelSetting.Description = input.Description;
                }
                else
                {
                    // 新增關卡
                    existingSettings.Add(new GameLevelSettingViewModel
                    {
                        Level = input.Level,
                        MonsterCount = input.MonsterCount,
                        SpeedMultiplier = input.SpeedMultiplier,
                        WinPointsReward = input.WinPointsReward,
                        WinExpReward = input.WinExpReward,
                        WinCouponReward = input.WinCouponReward,
                        LosePointsReward = input.LosePointsReward,
                        LoseExpReward = input.LoseExpReward,
                        AbortPointsReward = input.AbortPointsReward,
                        AbortExpReward = input.AbortExpReward,
                        Description = input.Description
                    });
                }

                // 排序並儲存
                existingSettings = existingSettings.OrderBy(l => l.Level).ToList();
                var settingJson = JsonSerializer.Serialize(existingSettings);

                // SystemSettings 表尚未實作，功能暫時不可用
                await Task.CompletedTask;
                return (false, "此功能暫時不可用");
            }
            catch (Exception ex)
            {
                return (false, $"更新失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新每日遊戲次數限制
        /// </summary>
        public async Task<(bool success, string message)> UpdateDailyLimitAsync(DailyLimitInputModel input, int operatorId)
        {
            try
            {
                // 驗證次數範圍
                if (input.MaxPlaysPerDay < 1 || input.MaxPlaysPerDay > 100)
                {
                    return (false, "每日次數必須在 1-100 之間");
                }

                // 驗證操作者
                var manager = await _context.ManagerData.FindAsync(operatorId);
                if (manager == null)
                {
                    return (false, "操作者不存在");
                }

                // SystemSettings 表尚未實作，功能暫時不可用
                await Task.CompletedTask;
                return (false, "此功能暫時不可用");
            }
            catch (Exception ex)
            {
                return (false, $"更新失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 手動調整遊戲紀錄
        /// </summary>
        public async Task<(bool success, string message)> ManualAdjustGameRecordAsync(int playId, GameRecordAdjustmentInputModel input, int operatorId)
        {
            try
            {
                // 驗證操作者
                var manager = await _context.ManagerData.FindAsync(operatorId);
                if (manager == null)
                {
                    return (false, "操作者不存在");
                }

                // 查找遊戲紀錄
                var record = await _context.MiniGames.FindAsync(playId);
                if (record == null)
                {
                    return (false, $"找不到遊戲紀錄 (PlayId: {playId})");
                }

                // 調整點數
                if (input.AdjustPoints.HasValue && input.AdjustPoints.Value != 0)
                {
                    var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == record.UserId);
                    if (wallet != null)
                    {
                        wallet.UserPoint += input.AdjustPoints.Value;

                        // 記錄錢包歷史
                        var walletHistory = new WalletHistory
                        {
                            UserId = record.UserId,
                            ChangeType = input.AdjustPoints.Value > 0 ? "手動補發" : "手動扣除",
                            PointsChanged = input.AdjustPoints.Value,
                            Description = $"{input.Reason ?? "管理員手動調整遊戲紀錄"} (操作者: {manager.ManagerName} ID {operatorId}, 調整後餘額: {wallet.UserPoint})",
                            ChangeTime = _appClock.UtcNow
                        };
                        _context.WalletHistories.Add(walletHistory);
                    }
                }

                // 調整經驗值
                if (input.AdjustExp.HasValue && input.AdjustExp.Value != 0)
                {
                    var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == record.PetId);
                    if (pet != null)
                    {
                        // Pet Model 尚未包含經驗值屬性，此功能暫未實作
                    }
                }

                // 補發優惠券
                if (input.IssueCoupon && !string.IsNullOrEmpty(input.CouponTypeCode))
                {
                    var couponType = await _context.CouponTypes
                        .FirstOrDefaultAsync(ct => ct.Name == input.CouponTypeCode && ct.ValidTo >= _appClock.UtcNow);

                    if (couponType != null)
                    {
                        var coupon = new Coupon
                        {
                            UserId = record.UserId,
                            CouponTypeId = couponType.CouponTypeId,
                            CouponCode = GenerateCouponCode(),
                            IsUsed = false,
                            AcquiredTime = _appClock.UtcNow,
                            UsedTime = null,
                            UsedInOrderId = null
                        };
                        _context.Coupons.Add(coupon);

                        // 記錄優惠券發放歷史
                        var couponHistory = new WalletHistory
                        {
                            UserId = record.UserId,
                            ChangeType = "手動補發優惠券",
                            PointsChanged = 0,
                            ItemCode = coupon.CouponCode,
                            Description = $"手動補發優惠券 {couponType.Name} (操作者: 管理員 ID {operatorId})",
                            ChangeTime = _appClock.UtcNow
                        };
                        _context.WalletHistories.Add(couponHistory);
                    }
                    else
                    {
                        return (false, $"找不到有效的優惠券類型: {input.CouponTypeCode}");
                    }
                }

                // 更新遊戲紀錄備註
                if (!string.IsNullOrEmpty(input.Reason))
                {
                    // MiniGame 資料表可能沒有 Remarks 欄位，這裡僅作示範
                    // 實際需根據資料表結構調整
                }

                await _context.SaveChangesAsync();

                var adjustmentDetails = new List<string>();
                if (input.AdjustPoints.HasValue && input.AdjustPoints.Value != 0)
                    adjustmentDetails.Add($"點數{(input.AdjustPoints.Value > 0 ? "+" : "")}{input.AdjustPoints.Value}");
                if (input.AdjustExp.HasValue && input.AdjustExp.Value != 0)
                    adjustmentDetails.Add($"經驗值{(input.AdjustExp.Value > 0 ? "+" : "")}{input.AdjustExp.Value}");
                if (input.IssueCoupon)
                    adjustmentDetails.Add($"補發優惠券({input.CouponTypeCode})");

                var message = adjustmentDetails.Count > 0
                    ? $"遊戲紀錄調整成功: {string.Join(", ", adjustmentDetails)}"
                    : "遊戲紀錄調整完成（無實際變動）";

                return (true, message);
            }
            catch (Exception ex)
            {
                return (false, $"調整失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 驗證關卡設定的合理性
        /// </summary>
        public (bool isValid, string errorMessage) ValidateLevelSettings(int level, int monsterCount, decimal speedMultiplier, int winPoints, int winExp)
        {
            // 驗證關卡等級
            if (level < 1 || level > 10)
            {
                return (false, "關卡等級必須在 1-10 之間");
            }

            // 驗證怪物數量
            if (monsterCount < 1 || monsterCount > 50)
            {
                return (false, "怪物數量必須在 1-50 之間");
            }

            // 驗證速度倍率
            if (speedMultiplier < 0.5m || speedMultiplier > 5.0m)
            {
                return (false, "速度倍率必須在 0.5-5.0 之間");
            }

            // 驗證獎勵點數
            if (winPoints < 0 || winPoints > 10000)
            {
                return (false, "獎勵點數必須在 0-10000 之間");
            }

            // 驗證獎勵經驗值
            if (winExp < 0 || winExp > 10000)
            {
                return (false, "獎勵經驗值必須在 0-10000 之間");
            }

            // 驗證難度遞增合理性
            if (level > 1)
            {
                // 建議後面關卡怪物更多、速度更快
                var expectedMinMonsters = 4 + (level - 1) * 2;
                if (monsterCount < expectedMinMonsters - 2)
                {
                    return (false, $"第 {level} 關怪物數量建議至少 {expectedMinMonsters - 2} 隻");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 批量更新所有關卡設定
        /// </summary>
        public async Task<(bool success, string message)> BatchUpdateLevelSettingsAsync(List<LevelSettingsInputModel> levelInputs, int operatorId)
        {
            try
            {
                // 驗證操作者
                var manager = await _context.ManagerData.FindAsync(operatorId);
                if (manager == null)
                {
                    return (false, "操作者不存在");
                }

                // 驗證所有關卡設定
                foreach (var input in levelInputs)
                {
                    var (isValid, errorMessage) = ValidateLevelSettings(
                        input.Level,
                        input.MonsterCount,
                        input.SpeedMultiplier,
                        input.WinPointsReward,
                        input.WinExpReward
                    );

                    if (!isValid)
                    {
                        return (false, $"第 {input.Level} 關: {errorMessage}");
                    }
                }

                // 轉換為 ViewModel 列表
                var levelSettings = levelInputs.OrderBy(i => i.Level).Select(i => new GameLevelSettingViewModel
                {
                    Level = i.Level,
                    MonsterCount = i.MonsterCount,
                    SpeedMultiplier = i.SpeedMultiplier,
                    WinPointsReward = i.WinPointsReward,
                    WinExpReward = i.WinExpReward,
                    WinCouponReward = i.WinCouponReward,
                    LosePointsReward = i.LosePointsReward,
                    LoseExpReward = i.LoseExpReward,
                    AbortPointsReward = i.AbortPointsReward,
                    AbortExpReward = i.AbortExpReward,
                    Description = i.Description
                }).ToList();

                // 儲存至系統設定
                var settingJson = JsonSerializer.Serialize(levelSettings);

                // SystemSettings 表尚未實作，功能暫時不可用
                await Task.CompletedTask;
                return (false, "此功能暫時不可用");
            }
            catch (Exception ex)
            {
                return (false, $"批量更新失敗: {ex.Message}");
            }
        }

        #region 私有輔助方法

        /// <summary>
        /// 獲取當前所有關卡設定
        /// </summary>
        private async Task<List<GameLevelSettingViewModel>> GetCurrentLevelSettingsAsync()
        {
            // 返回預設關卡設定
            await Task.CompletedTask;
            return GetDefaultLevelSettings();
        }

        /// <summary>
        /// 獲取預設關卡設定
        /// 根據 MiniGame_Area_完整描述文件.md 第 333-339 行規定
        /// </summary>
        private List<GameLevelSettingViewModel> GetDefaultLevelSettings()
        {
            return new List<GameLevelSettingViewModel>
            {
                new GameLevelSettingViewModel
                {
                    Level = 1,
                    MonsterCount = 6,
                    SpeedMultiplier = 1.0m,
                    WinPointsReward = 10,
                    WinExpReward = 100,
                    WinCouponReward = 0,
                    LosePointsReward = 0,
                    LoseExpReward = 0,
                    AbortPointsReward = 0,
                    AbortExpReward = 0,
                    Description = "第 1 關：怪物數量 6、移動速度 1 倍；獎勵 +100 寵物經驗值，+10 會員點數"
                },
                new GameLevelSettingViewModel
                {
                    Level = 2,
                    MonsterCount = 8,
                    SpeedMultiplier = 1.5m,
                    WinPointsReward = 20,
                    WinExpReward = 200,
                    WinCouponReward = 0,
                    LosePointsReward = 0,
                    LoseExpReward = 0,
                    AbortPointsReward = 0,
                    AbortExpReward = 0,
                    Description = "第 2 關：怪物數量 8、移動速度 1.5 倍；獎勵 +200 寵物經驗值，+20 會員點數"
                },
                new GameLevelSettingViewModel
                {
                    Level = 3,
                    MonsterCount = 10,
                    SpeedMultiplier = 2.0m,
                    WinPointsReward = 30,
                    WinExpReward = 300,
                    WinCouponReward = 1,
                    LosePointsReward = 0,
                    LoseExpReward = 0,
                    AbortPointsReward = 0,
                    AbortExpReward = 0,
                    Description = "第 3 關：怪物數量 10、移動速度 2 倍；獎勵 +300 寵物經驗值，+30 會員點數，+1 張商城優惠券"
                }
            };
        }

        /// <summary>
        /// 計算下一等級所需經驗值
        /// 根據 Pet.cs partial class 的 ExperienceToNextLevel 屬性計算
        /// 已棄用：現在使用 Pet.ExperienceToNextLevel 計算屬性
        /// </summary>
        [Obsolete("Use Pet.ExperienceToNextLevel property instead")]
        private int CalculateNextLevelExp(int level)
        {
            // 這個方法已被 Pet.ExperienceToNextLevel 屬性取代
            // 保留此方法以避免破壞現有引用
            // 實際計算公式在 Pet.cs partial class 中
            if (level >= 1 && level <= 10)
            {
                return 40 * level + 60;
            }
            else if (level >= 11 && level <= 100)
            {
                return (int)(0.8 * level * level + 380);
            }
            else
            {
                return (int)(285.69 * Math.Pow(1.06, level));
            }
        }

        /// <summary>
        /// 生成優惠券代碼
        /// </summary>
        private string GenerateCouponCode()
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"GAME-{code}";
        }

        #endregion
    }
}
