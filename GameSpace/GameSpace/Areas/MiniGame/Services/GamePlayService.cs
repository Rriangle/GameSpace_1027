using GameSpace.Infrastructure.Time;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 小遊戲玩法服務實作
    /// 實作遊戲核心邏輯: 健康檢查、難度進程、結果影響
    /// </summary>
    public class GamePlayService : IGamePlayService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IAppClock _appClock;
        private readonly ILogger<GamePlayService> _logger;

        public GamePlayService(
            GameSpacedatabaseContext context,
            IAppClock appClock,
            ILogger<GamePlayService> logger)
        {
            _context = context;
            _appClock = appClock;
            _logger = logger;
        }

        /// <summary>
        /// 檢查寵物健康狀態是否允許開始冒險
        /// 規格來源: 專案規格敘述1.txt 第 531 行
        /// 規則: 飢餓、心情、體力、清潔、健康任一屬性值為 0 則無法開始冒險
        /// </summary>
        public async Task<(bool canStart, string message)> CheckPetHealthForAdventureAsync(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);

            if (pet == null)
            {
                return (false, "找不到寵物資料");
            }

            // 檢查五大屬性是否任一為 0
            if (pet.Hunger == 0)
            {
                return (false, "寵物太餓了! 請先餵食寵物再開始冒險 (飢餓值為 0)");
            }

            if (pet.Mood == 0)
            {
                return (false, "寵物心情太差了! 請先哄睡寵物再開始冒險 (心情值為 0)");
            }

            if (pet.Stamina == 0)
            {
                return (false, "寵物太累了! 請先讓寵物休息再開始冒險 (體力值為 0)");
            }

            if (pet.Cleanliness == 0)
            {
                return (false, "寵物太髒了! 請先幫寵物洗澡再開始冒險 (清潔值為 0)");
            }

            if (pet.Health == 0)
            {
                return (false, "寵物生病了! 請先改善寵物健康狀態再開始冒險 (健康值為 0)");
            }

            return (true, "寵物狀態良好，可以開始冒險");
        }

        /// <summary>
        /// 獲取寵物當前應該挑戰的關卡等級
        /// 規格來源: 專案規格敘述1.txt 第 529 行
        /// 規則: 首次從第 1 關開始；勝利升級、失敗保持、最高第 3 關
        /// </summary>
        public async Task<int> GetCurrentLevelForPetAsync(int userId, int petId)
        {
            // 查詢該寵物最後一次遊戲記錄
            var lastGame = await _context.MiniGames
                .AsNoTracking()
                .Where(g => g.UserId == userId && g.PetId == petId && g.EndTime != null && !g.Aborted)
                .OrderByDescending(g => g.EndTime)
                .FirstOrDefaultAsync();

            // 首次遊戲: 從第 1 關開始
            if (lastGame == null)
            {
                return 1;
            }

            // 根據上次結果決定關卡
            // 勝利: 升級 (最高第 3 關)
            // 失敗: 保持原關卡
            if (lastGame.Result == "Win" || lastGame.Result == "勝利")
            {
                int nextLevel = lastGame.Level + 1;
                return Math.Min(nextLevel, 3); // 最高第 3 關
            }
            else
            {
                return lastGame.Level; // 失敗則留在同一關卡
            }
        }

        /// <summary>
        /// 開始遊戲並記錄遊戲狀態
        /// </summary>
        public async Task<(bool success, string message, int? playId, int level)> StartAdventureAsync(int userId, int petId)
        {
            try
            {
                // 1. 檢查寵物健康狀態
                var (canStart, healthMessage) = await CheckPetHealthForAdventureAsync(petId);
                if (!canStart)
                {
                    return (false, healthMessage, null, 0);
                }

                // 2. 獲取當前應挑戰的關卡
                int currentLevel = await GetCurrentLevelForPetAsync(userId, petId);

                // 3. 根據關卡獲取遊戲設定
                var (monsterCount, speedMultiplier) = GetLevelSettings(currentLevel);

                // 4. 建立遊戲記錄
                var game = new GameSpace.Models.MiniGame
                {
                    UserId = userId,
                    PetId = petId,
                    Level = currentLevel,
                    MonsterCount = monsterCount,
                    SpeedMultiplier = speedMultiplier,
                    Result = "進行中",
                    ExpGained = 0,
                    ExpGainedTime = _appClock.UtcNow,
                    PointsGained = 0,
                    PointsGainedTime = _appClock.UtcNow,
                    CouponGained = "",
                    CouponGainedTime = _appClock.UtcNow,
                    HungerDelta = 0,
                    MoodDelta = 0,
                    StaminaDelta = 0,
                    CleanlinessDelta = 0,
                    StartTime = _appClock.UtcNow,
                    EndTime = null,
                    Aborted = false
                };

                _context.MiniGames.Add(game);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "用戶 {UserId} 的寵物 {PetId} 開始第 {Level} 關冒險 (PlayId: {PlayId})",
                    userId, petId, currentLevel, game.PlayId);

                return (true, $"成功開始第 {currentLevel} 關冒險", game.PlayId, currentLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "開始冒險時發生錯誤 (UserId: {UserId}, PetId: {PetId})", userId, petId);
                return (false, $"開始冒險失敗: {ex.Message}", null, 0);
            }
        }

        /// <summary>
        /// 結束遊戲並處理結果影響
        /// 規格來源: 專案規格敘述1.txt 第 530 行
        /// 規則: 勝利時飢餓-20、心情+30、體力-20、清潔-20
        ///      失敗時飢餓-20、心情-30、體力-20、清潔-20
        /// </summary>
        public async Task<(bool success, string message)> EndAdventureAsync(
            int playId,
            bool isWin,
            int pointsEarned,
            int expEarned,
            string? couponEarned = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. 獲取遊戲記錄
                var game = await _context.MiniGames.FindAsync(playId);
                if (game == null)
                {
                    return (false, "找不到遊戲記錄");
                }

                if (game.EndTime != null)
                {
                    return (false, "此遊戲已經結束");
                }

                // 2. 更新遊戲記錄
                game.EndTime = _appClock.UtcNow;
                game.Result = isWin ? "Win" : "Lose";
                game.PointsGained = pointsEarned;
                game.PointsGainedTime = _appClock.UtcNow;
                game.ExpGained = expEarned;
                game.ExpGainedTime = _appClock.UtcNow;
                game.CouponGained = couponEarned ?? "";
                game.CouponGainedTime = string.IsNullOrEmpty(couponEarned) ? game.CouponGainedTime : _appClock.UtcNow;

                // 3. 應用遊戲結果對寵物屬性的影響
                var (statsSuccess, statsMessage) = await ApplyGameResultToPetStatsAsync(game.PetId, isWin);
                if (!statsSuccess)
                {
                    await transaction.RollbackAsync();
                    return (false, $"更新寵物狀態失敗: {statsMessage}");
                }

                // 4. 更新用戶點數
                if (pointsEarned > 0)
                {
                    var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == game.UserId);
                    if (wallet != null)
                    {
                        wallet.UserPoint += pointsEarned;

                        // 記錄錢包歷史
                        _context.WalletHistories.Add(new WalletHistory
                        {
                            UserId = game.UserId,
                            ChangeType = "遊戲獎勵",
                            PointsChanged = pointsEarned,
                            Description = $"第 {game.Level} 關冒險{(isWin ? "勝利" : "失敗")}獎勵",
                            ChangeTime = _appClock.UtcNow
                        });
                    }
                }

                // 5. 更新寵物經驗值
                if (expEarned > 0)
                {
                    var pet = await _context.Pets.FindAsync(game.PetId);
                    if (pet != null)
                    {
                        pet.Experience += expEarned;

                        // 檢查是否升級 (簡化版，實際應該使用寵物服務)
                        // 這裡僅更新經驗值，升級邏輯應由專門的寵物服務處理
                    }
                }

                // 6. 發放優惠券 (如果有)
                if (!string.IsNullOrEmpty(couponEarned))
                {
                    _context.WalletHistories.Add(new WalletHistory
                    {
                        UserId = game.UserId,
                        ChangeType = "優惠券獲得",
                        PointsChanged = 0,
                        ItemCode = couponEarned,
                        Description = $"第 {game.Level} 關冒險{(isWin ? "勝利" : "失敗")}獲得優惠券",
                        ChangeTime = _appClock.UtcNow
                    });
                }

                // 7. 更新難度等級 (僅在勝利時升級)
                await UpdatePetDifficultyLevelAsync(game.UserId, game.PetId, game.Level, isWin);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                string resultText = isWin ? "勝利" : "失敗";
                _logger.LogInformation(
                    "遊戲 {PlayId} 結束: {Result}, 獲得 {Points} 點數, {Exp} 經驗值",
                    playId, resultText, pointsEarned, expEarned);

                return (true, $"遊戲{resultText}! 獲得 {pointsEarned} 點數、{expEarned} 經驗值");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "結束遊戲時發生錯誤 (PlayId: {PlayId})", playId);
                return (false, $"結束遊戲失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 中止遊戲
        /// </summary>
        public async Task<(bool success, string message)> AbortAdventureAsync(int playId)
        {
            try
            {
                var game = await _context.MiniGames.FindAsync(playId);
                if (game == null)
                {
                    return (false, "找不到遊戲記錄");
                }

                if (game.EndTime != null)
                {
                    return (false, "此遊戲已經結束");
                }

                game.EndTime = _appClock.UtcNow;
                game.Aborted = true;
                game.Result = "Abort";

                await _context.SaveChangesAsync();

                _logger.LogInformation("遊戲 {PlayId} 被中止", playId);

                return (true, "遊戲已中止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "中止遊戲時發生錯誤 (PlayId: {PlayId})", playId);
                return (false, $"中止遊戲失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新寵物難度等級(基於遊戲結果)
        /// 規格來源: 專案規格敘述1.txt 第 529 行
        /// 規則: 勝利時升級(最高第3關)、失敗時保持原等級
        /// </summary>
        public Task<int> UpdatePetDifficultyLevelAsync(int userId, int petId, int currentLevel, bool isWin)
        {
            // 注意: 難度等級存儲在最後一次遊戲記錄中
            // 下次遊戲會讀取最後一次記錄來決定難度
            // 因此這裡不需要額外的存儲，邏輯在 GetCurrentLevelForPetAsync 中已實現

            if (isWin)
            {
                int nextLevel = Math.Min(currentLevel + 1, 3);
                _logger.LogInformation(
                    "寵物 {PetId} 勝利! 下次挑戰關卡: {NextLevel} (當前: {CurrentLevel})",
                    petId, nextLevel, currentLevel);
                return Task.FromResult(nextLevel);
            }
            else
            {
                _logger.LogInformation(
                    "寵物 {PetId} 失敗，下次繼續挑戰關卡: {CurrentLevel}",
                    petId, currentLevel);
                return Task.FromResult(currentLevel);
            }
        }

        /// <summary>
        /// 應用遊戲結果對寵物屬性的影響
        /// 規格來源: 專案規格敘述1.txt 第 530 行
        /// 勝利: 飢餓-20、心情+30、體力-20、清潔-20
        /// 失敗: 飢餓-20、心情-30、體力-20、清潔-20
        /// </summary>
        public async Task<(bool success, string message)> ApplyGameResultToPetStatsAsync(int petId, bool isWin)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null)
                {
                    return (false, "找不到寵物資料");
                }

                // 記錄變化量 (用於遊戲記錄)
                int hungerDelta = -20;
                int moodDelta = isWin ? 30 : -30;
                int staminaDelta = -20;
                int cleanlinessDelta = -20;

                // 應用變化 (屬性值範圍 0-100，自動鉗位)
                pet.Hunger = Math.Clamp(pet.Hunger + hungerDelta, 0, 100);
                pet.Mood = Math.Clamp(pet.Mood + moodDelta, 0, 100);
                pet.Stamina = Math.Clamp(pet.Stamina + staminaDelta, 0, 100);
                pet.Cleanliness = Math.Clamp(pet.Cleanliness + cleanlinessDelta, 0, 100);

                // 更新健康值 (如果四項屬性均達到 100，健康值恢復至 100)
                if (pet.Hunger == 100 && pet.Mood == 100 && pet.Stamina == 100 && pet.Cleanliness == 100)
                {
                    pet.Health = 100;
                }

                // 注意: 這裡不調用 SaveChangesAsync，由呼叫者統一處理交易

                _logger.LogInformation(
                    "寵物 {PetId} 狀態更新: 飢餓{Hunger:+0;-#}, 心情{Mood:+0;-#}, 體力{Stamina:+0;-#}, 清潔{Cleanliness:+0;-#}",
                    petId, hungerDelta, moodDelta, staminaDelta, cleanlinessDelta);

                string resultText = isWin ? "勝利" : "失敗";
                return (true, $"寵物狀態已更新 ({resultText})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物狀態時發生錯誤 (PetId: {PetId})", petId);
                return (false, $"更新寵物狀態失敗: {ex.Message}");
            }
        }

        #region 私有輔助方法

        /// <summary>
        /// 根據關卡獲取遊戲設定
        /// 規格來源: 專案規格敘述1.txt 第 526-528 行
        /// </summary>
        private (int monsterCount, decimal speedMultiplier) GetLevelSettings(int level)
        {
            return level switch
            {
                1 => (6, 1.0m),    // 第 1 關: 怪物 6 隻, 速度 1 倍
                2 => (8, 1.5m),    // 第 2 關: 怪物 8 隻, 速度 1.5 倍
                3 => (10, 2.0m),   // 第 3 關: 怪物 10 隻, 速度 2 倍
                _ => (6, 1.0m)     // 預設第 1 關
            };
        }

        #endregion
    }
}
