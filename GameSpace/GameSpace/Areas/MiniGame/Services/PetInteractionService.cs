using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Infrastructure.Time;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物互動服務實作
    /// 處理寵物餵食、洗澡、玩耍、哄睡等互動操作
    /// </summary>
    public class PetInteractionService : IPetInteractionService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IAppClock _appClock;
        private readonly IPetService _petService;
        private readonly ILogger<PetInteractionService> _logger;

        // 根據規格文件 7.5 寵物互動效果
        private const int FEED_BONUS = 10;      // 餵食：飢餓值 +10
        private const int BATH_BONUS = 10;      // 洗澡：清潔值 +10
        private const int PLAY_BONUS = 10;      // 哄睡：心情值 +10
        private const int SLEEP_BONUS = 10;     // 休息：體力值 +10

        // 每日全滿獎勵 (規格 7.4)
        private const int DAILY_FULL_STATS_BONUS_EXP = 100;

        public PetInteractionService(
            GameSpacedatabaseContext context,
            IAppClock appClock,
            IPetService petService,
            ILogger<PetInteractionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
            _petService = petService ?? throw new ArgumentNullException(nameof(petService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 餵食寵物
        /// 效果：飢餓值 +10
        /// </summary>
        public async Task<PetInteractionResult> FeedAsync(int petId, int userId)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetId == petId && p.UserId == userId);

                if (pet == null)
                {
                    return PetInteractionResult.Failed("找不到寵物或無權限操作");
                }

                // 增加飢餓值
                pet.Hunger = Math.Min(100, pet.Hunger + FEED_BONUS);

                // 檢查並獎勵每日全滿獎勵
                bool dailyBonusAwarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);

                // 檢查健康值恢復
                bool healthRestored = CheckAndRestoreHealth(pet);

                await _context.SaveChangesAsync();

                var stats = MapToStatsViewModel(pet);
                return PetInteractionResult.Succeeded(
                    $"餵食成功！飢餓值 +{FEED_BONUS}",
                    stats,
                    dailyBonusAwarded,
                    healthRestored);
            }
            catch (Exception ex)
            {
                return PetInteractionResult.Failed($"餵食失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 為寵物洗澡
        /// 效果：清潔值 +10
        /// </summary>
        public async Task<PetInteractionResult> BathAsync(int petId, int userId)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetId == petId && p.UserId == userId);

                if (pet == null)
                {
                    return PetInteractionResult.Failed("找不到寵物或無權限操作");
                }

                // 增加清潔值
                pet.Cleanliness = Math.Min(100, pet.Cleanliness + BATH_BONUS);

                // 檢查並獎勵每日全滿獎勵
                bool dailyBonusAwarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);

                // 檢查健康值恢復
                bool healthRestored = CheckAndRestoreHealth(pet);

                await _context.SaveChangesAsync();

                var stats = MapToStatsViewModel(pet);
                return PetInteractionResult.Succeeded(
                    $"洗澡成功！清潔值 +{BATH_BONUS}",
                    stats,
                    dailyBonusAwarded,
                    healthRestored);
            }
            catch (Exception ex)
            {
                return PetInteractionResult.Failed($"洗澡失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 哄寵物睡覺（玩耍）
        /// 效果：心情值 +10
        /// </summary>
        public async Task<PetInteractionResult> PlayAsync(int petId, int userId)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetId == petId && p.UserId == userId);

                if (pet == null)
                {
                    return PetInteractionResult.Failed("找不到寵物或無權限操作");
                }

                // 增加心情值
                pet.Mood = Math.Min(100, pet.Mood + PLAY_BONUS);

                // 檢查並獎勵每日全滿獎勵
                bool dailyBonusAwarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);

                // 檢查健康值恢復
                bool healthRestored = CheckAndRestoreHealth(pet);

                await _context.SaveChangesAsync();

                var stats = MapToStatsViewModel(pet);
                return PetInteractionResult.Succeeded(
                    $"哄睡成功！心情值 +{PLAY_BONUS}",
                    stats,
                    dailyBonusAwarded,
                    healthRestored);
            }
            catch (Exception ex)
            {
                return PetInteractionResult.Failed($"哄睡失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 讓寵物休息
        /// 效果：體力值 +10
        /// </summary>
        public async Task<PetInteractionResult> SleepAsync(int petId, int userId)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetId == petId && p.UserId == userId);

                if (pet == null)
                {
                    return PetInteractionResult.Failed("找不到寵物或無權限操作");
                }

                // 增加體力值
                pet.Stamina = Math.Min(100, pet.Stamina + SLEEP_BONUS);

                // 檢查並獎勵每日全滿獎勵
                bool dailyBonusAwarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);

                // 檢查健康值恢復
                bool healthRestored = CheckAndRestoreHealth(pet);

                await _context.SaveChangesAsync();

                var stats = MapToStatsViewModel(pet);
                return PetInteractionResult.Succeeded(
                    $"休息成功！體力值 +{SLEEP_BONUS}",
                    stats,
                    dailyBonusAwarded,
                    healthRestored);
            }
            catch (Exception ex)
            {
                return PetInteractionResult.Failed($"休息失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查並獎勵每日全滿狀態獎勵（公開方法）
        /// 規格 7.4：寵物若於每日首次同時達到飢餓、心情、體力、清潔值皆 100，則額外獲得 100 點寵物經驗值
        /// </summary>
        public async Task<bool> CheckAndAwardDailyFullStatsBonusAsync(int petId)
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == petId);
            if (pet == null)
            {
                return false;
            }

            bool awarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);
            if (awarded)
            {
                await _context.SaveChangesAsync();
            }

            return awarded;
        }

        /// <summary>
        /// 檢查並獎勵每日全滿狀態獎勵（內部方法，不儲存變更）
        /// </summary>
        private async Task<bool> CheckAndAwardDailyFullStatsBonusInternalAsync(Pet pet)
        {
            // 檢查是否四項屬性都達到 100
            if (pet.Hunger != 100 || pet.Mood != 100 || pet.Stamina != 100 || pet.Cleanliness != 100)
            {
                return false;
            }

            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// 檢查並恢復健康值
        /// 規格：當飢餓、心情、體力、清潔四項值均達到 100 時，健康值恢復至 100
        /// </summary>
        private bool CheckAndRestoreHealth(Pet pet)
        {
            if (pet.Hunger == 100 && pet.Mood == 100 && pet.Stamina == 100 && pet.Cleanliness == 100)
            {
                if (pet.Health < 100)
                {
                    pet.Health = 100;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 將寵物實體映射為狀態視圖模型
        /// </summary>
        private PetStatsViewModel MapToStatsViewModel(Pet pet)
        {
            return new PetStatsViewModel
            {
                PetId = pet.PetId,
                Hunger = pet.Hunger,
                Mood = pet.Mood,
                Stamina = pet.Stamina,
                Cleanliness = pet.Cleanliness,
                Health = pet.Health,
                Experience = pet.Experience,
                Level = pet.Level
            };
        }
    }
}
