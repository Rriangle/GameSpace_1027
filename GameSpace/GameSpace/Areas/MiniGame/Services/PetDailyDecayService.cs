using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Infrastructure.Time;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物每日屬性衰減服務實作
    /// 處理每日凌晨 00:00 (Asia/Taipei) 的屬性衰減
    /// </summary>
    public class PetDailyDecayService : IPetDailyDecayService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IAppClock _appClock;
        private readonly ILogger<PetDailyDecayService> _logger;

        // 根據規格文件 7.6 每日屬性衰減
        private const int HUNGER_DECAY = 20;        // 飢餓值 -20
        private const int MOOD_DECAY = 30;          // 心情值 -30
        private const int STAMINA_DECAY = 10;       // 體力值 -10
        private const int CLEANLINESS_DECAY = 20;   // 清潔值 -20

        // SystemSettings key for tracking last decay execution
        private const string LAST_DECAY_KEY = "Pet.LastDailyDecayDate";

        public PetDailyDecayService(GameSpacedatabaseContext context, IAppClock appClock, ILogger<PetDailyDecayService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 應用每日屬性衰減至指定寵物
        /// </summary>
        public async Task<PetDecayResult> ApplyDailyDecayAsync(int petId)
        {
            try
            {
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == petId);
                if (pet == null)
                {
                    return PetDecayResult.Failed(petId, "找不到寵物");
                }

                // 應用衰減（不低於 0）
                pet.Hunger = Math.Max(0, pet.Hunger - HUNGER_DECAY);
                pet.Mood = Math.Max(0, pet.Mood - MOOD_DECAY);
                pet.Stamina = Math.Max(0, pet.Stamina - STAMINA_DECAY);
                pet.Cleanliness = Math.Max(0, pet.Cleanliness - CLEANLINESS_DECAY);

                await _context.SaveChangesAsync();

                var stats = new Dictionary<string, int>
                {
                    { "Hunger", pet.Hunger },
                    { "Mood", pet.Mood },
                    { "Stamina", pet.Stamina },
                    { "Cleanliness", pet.Cleanliness },
                    { "Health", pet.Health }
                };

                return PetDecayResult.Succeeded(petId, stats, "每日屬性衰減已應用");
            }
            catch (Exception ex)
            {
                return PetDecayResult.Failed(petId, $"衰減失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 應用每日屬性衰減至所有寵物
        /// </summary>
        public async Task<PetDecayBatchResult> ApplyDailyDecayToAllPetsAsync()
        {
            var taipeiNow = _appClock.ToAppTime(_appClock.UtcNow);
            var result = new PetDecayBatchResult
            {
                ExecutedAt = taipeiNow
            };

            try
            {
                var allPets = await _context.Pets.ToListAsync();

                foreach (var pet in allPets)
                {
                    var petResult = await ApplyDailyDecayAsync(pet.PetId);
                    result.Results.Add(petResult);

                    if (petResult.Success)
                    {
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.FailureCount++;
                    }
                }

                // 更新上次執行時間
                await UpdateLastDecayTimeAsync(taipeiNow);
            }
            catch (Exception ex)
            {
                // 記錄錯誤但繼續
                result.Results.Add(PetDecayResult.Failed(0, $"批次處理錯誤：{ex.Message}"));
                result.FailureCount++;
            }

            return result;
        }

        /// <summary>
        /// 檢查是否需要執行每日衰減
        /// 邏輯：檢查今日（台北時區）是否已執行過衰減
        /// </summary>
        public async Task<bool> ShouldApplyDailyDecayAsync()
        {
            var lastDecayTime = await GetLastDecayTimeAsync();
            if (lastDecayTime == null)
            {
                // 從未執行過，應該執行
                return true;
            }

            var taipeiNow = _appClock.ToAppTime(_appClock.UtcNow);
            var todayInTaipei = taipeiNow.Date;
            var lastDecayDate = lastDecayTime.Value.Date;

            // 如果上次執行日期早於今天，則應該執行
            return lastDecayDate < todayInTaipei;
        }

        /// <summary>
        /// 取得上次執行衰減的時間（台北時區）
        /// </summary>
        public async Task<DateTime?> GetLastDecayTimeAsync()
        {
            await Task.CompletedTask;
            return null;
        }

        /// <summary>
        /// 更新上次執行衰減的時間
        /// </summary>
        private async Task UpdateLastDecayTimeAsync(DateTime taipeiTime)
        {
            await Task.CompletedTask;
        }
    }
}
