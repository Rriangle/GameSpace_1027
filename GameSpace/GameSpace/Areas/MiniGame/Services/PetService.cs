using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    public class PetService : IPetService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetService> _logger;

        public PetService(GameSpacedatabaseContext context, ILogger<PetService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Pet 基本 CRUD
        public async Task<IEnumerable<Pet>> GetAllPetsAsync()
        {
            return await _context.Pets
                .AsNoTracking()
                .Include(p => p.User)
                .OrderByDescending(p => p.Level)
                .ToListAsync();
        }

        public async Task<Pet?> GetPetByIdAsync(int petId)
        {
            return await _context.Pets
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PetId == petId);
        }

        public async Task<Pet?> GetPetByUserIdAsync(int userId)
        {
            return await _context.Pets
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<bool> CreatePetAsync(Pet pet)
        {
            try
            {
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建寵物失敗");
                return false;
            }
        }

        public async Task<bool> UpdatePetAsync(Pet pet)
        {
            try
            {
                _context.Pets.Update(pet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物失敗: PetId={PetId}", pet.PetId);
                return false;
            }
        }

        public async Task<bool> DeletePetAsync(int petId)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物失敗: PetId={PetId}", petId);
                return false;
            }
        }

        // Pet 狀態管理
        public async Task<bool> UpdatePetStatsAsync(int petId, int hunger, int mood, int stamina, int cleanliness, int health)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Hunger = Math.Clamp(hunger, 0, 100);
                pet.Mood = Math.Clamp(mood, 0, 100);
                pet.Stamina = Math.Clamp(stamina, 0, 100);
                pet.Cleanliness = Math.Clamp(cleanliness, 0, 100);
                pet.Health = Math.Clamp(health, 0, 100);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物狀態失敗: PetId={PetId}", petId);
                return false;
            }
        }

        public async Task<bool> FeedPetAsync(int petId, int hungerIncrease)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Hunger = Math.Min(100, pet.Hunger + hungerIncrease);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "餵食寵物失敗: PetId={PetId}", petId);
                return false;
            }
        }

        public async Task<bool> PlayWithPetAsync(int petId, int moodIncrease)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Mood = Math.Min(100, pet.Mood + moodIncrease);
                pet.Stamina = Math.Max(0, pet.Stamina - 10); // 玩耍消耗體力
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "與寵物玩耍失敗: PetId={PetId}", petId);
                return false;
            }
        }

        public async Task<bool> CleanPetAsync(int petId, int cleanlinessIncrease)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Cleanliness = Math.Min(100, pet.Cleanliness + cleanlinessIncrease);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清潔寵物失敗: PetId={PetId}", petId);
                return false;
            }
        }

        public async Task<bool> RestPetAsync(int petId, int staminaIncrease)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Stamina = Math.Min(100, pet.Stamina + staminaIncrease);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "寵物休息失敗: PetId={PetId}", petId);
                return false;
            }
        }

        // Pet 升級系統
        public async Task<bool> AddExperienceAsync(int petId, int exp)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Experience += exp;

                // 檢查是否可以升級
                var requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
                while (pet.Experience >= requiredExp && requiredExp > 0)
                {
                    await LevelUpPetAsync(petId);
                    pet = await GetPetByIdAsync(petId);
                    if (pet == null) break;
                    requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加經驗值失敗: PetId={PetId}, Exp={Exp}", petId, exp);
                return false;
            }
        }

        public async Task<bool> LevelUpPetAsync(int petId)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                var requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
                if (pet.Experience < requiredExp) return false;

                pet.Level++;
                pet.LevelUpTime = DateTime.UtcNow;
                pet.Experience -= requiredExp;

                // 獎勵點數 - 使用分段公式
                // Level 1-10: +10 點, Level 11-20: +20 點, Level 21-30: +30 點, ...以此類推
                // Level 241-250: +250 點（上限）
                var pointsReward = CalculateLevelUpReward(pet.Level);
                pet.PointsGainedLevelUp += pointsReward;
                pet.PointsGainedTimeLevelUp = DateTime.UtcNow;

                // 更新使用者錢包
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == pet.UserId);
                if (wallet != null)
                {
                    wallet.UserPoint += pointsReward;
                }

                // 記錄錢包歷史
                var history = new WalletHistory
                {
                    UserId = pet.UserId,
                    ChangeType = "Pet",
                    PointsChanged = pointsReward,
                    ItemCode = $"PET_LEVELUP_{pet.Level}",
                    Description = $"寵物升級至 Level {pet.Level}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "寵物升級失敗: PetId={PetId}", petId);
                return false;
            }
        }

        /// <summary>
        /// 計算寵物升級點數獎勵
        /// Level 1-10: +10 點, Level 11-20: +20 點, Level 21-30: +30 點, ...
        /// Level 241-250: +250 點（上限）
        /// </summary>
        /// <param name="level">寵物當前等級</param>
        /// <returns>獎勵點數</returns>
        private int CalculateLevelUpReward(int level)
        {
            if (level < 1) return 0;
            if (level > 250) return 250;

            // 公式: Math.Min((level - 1) / 10 + 1, 25) * 10
            // Level 1-10: tier = 1, reward = 10
            // Level 11-20: tier = 2, reward = 20
            // Level 21-30: tier = 3, reward = 30
            // ...
            // Level 241-250: tier = 25 (capped), reward = 250
            int tier = Math.Min((level - 1) / 10 + 1, 25);
            return tier * 10;
        }

        public Task<int> GetRequiredExpForLevelAsync(int level)
        {
            // 正確的寵物升級經驗值公式（分段式）:
            // Level 1-10: EXP = 40 × level + 60
            // Level 11-100: EXP = 0.8 × level² + 380
            // Level ≥101: EXP = 285.69 × (1.06^level)

            if (level <= 1) return Task.FromResult(0);

            if (level <= 10)
            {
                // Level 1-10: 線性成長
                return Task.FromResult(40 * level + 60);
            }
            else if (level <= 100)
            {
                // Level 11-100: 二次方成長
                return Task.FromResult((int)(0.8 * level * level + 380));
            }
            else
            {
                // Level ≥101: 指數成長
                return Task.FromResult((int)(285.69 * Math.Pow(1.06, level)));
            }
        }

        // Pet 外觀管理
        public async Task<bool> ChangeSkinColorAsync(int petId, string colorCode, int pointsCost)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                // 檢查使用者點數
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == pet.UserId);
                if (wallet == null || wallet.UserPoint < pointsCost) return false;

                // 扣除點數
                wallet.UserPoint -= pointsCost;

                // 更新寵物
                pet.SkinColor = colorCode;
                pet.SkinColorChangedTime = DateTime.UtcNow;
                pet.PointsChangedSkinColor += pointsCost;

                // 記錄歷史
                var history = new WalletHistory
                {
                    UserId = pet.UserId,
                    ChangeType = "Pet",
                    PointsChanged = -pointsCost,
                    ItemCode = $"SKIN_{colorCode}",
                    Description = $"寵物換膚色: {colorCode}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "寵物換膚色失敗: PetId={PetId}, ColorCode={ColorCode}", petId, colorCode);
                return false;
            }
        }

        public async Task<bool> ChangeBackgroundColorAsync(int petId, string colorCode, int pointsCost)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                // 檢查使用者點數
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == pet.UserId);
                if (wallet == null || wallet.UserPoint < pointsCost) return false;

                // 扣除點數
                wallet.UserPoint -= pointsCost;

                // 更新寵物
                pet.BackgroundColor = colorCode;
                pet.BackgroundColorChangedTime = DateTime.UtcNow;
                pet.PointsChangedBackgroundColor += pointsCost;

                // 記錄歷史
                var history = new WalletHistory
                {
                    UserId = pet.UserId,
                    ChangeType = "Pet",
                    PointsChanged = -pointsCost,
                    ItemCode = $"BG_{colorCode}",
                    Description = $"寵物換背景: {colorCode}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "寵物換背景失敗: PetId={PetId}, ColorCode={ColorCode}", petId, colorCode);
                return false;
            }
        }

        public async Task<IEnumerable<PetColorOption>> GetAvailableColorsAsync()
        {
            return await _context.Set<PetColorOption>()
                .AsNoTracking()
                .Where(o => o.IsActive)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameSpace.Areas.MiniGame.Models.ViewModels.PetBackgroundOption>> GetAvailableBackgroundsAsync()
        {
            var entities = await _context.Set<PetBackgroundOption>()
                .AsNoTracking()
                .Where(o => o.IsActive)
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            // 將 Entity 映射為 ViewModel
            return entities.Select(e => new GameSpace.Areas.MiniGame.Models.ViewModels.PetBackgroundOption
            {
                BackgroundId = e.BackgroundOptionId,
                BackgroundName = e.BackgroundName,
                ImageUrl = null!, // Entity 中沒有 ImageUrl，可根據 BackgroundCode 生成或留空
                RequiredPoints = 0, // Entity 中沒有此欄位，預設為 0（免費）
                IsDefault = e.SortOrder == 0, // 假設 SortOrder 為 0 的是預設背景
                IsUnlocked = true, // 所有啟用的背景視為已解鎖
                Description = null! // Entity 中沒有描述欄位
            }).ToList();
        }

        // Pet 統計
        public async Task<Dictionary<string, int>> GetPetStatsSummaryAsync(int petId)
        {
            var pet = await GetPetByIdAsync(petId);
            if (pet == null) return new Dictionary<string, int>();

            return new Dictionary<string, int>
            {
                { "Level", pet.Level },
                { "Experience", pet.Experience },
                { "Hunger", pet.Hunger },
                { "Mood", pet.Mood },
                { "Stamina", pet.Stamina },
                { "Cleanliness", pet.Cleanliness },
                { "Health", pet.Health },
                { "PointsSpentOnSkin", pet.PointsChangedSkinColor },
                { "PointsSpentOnBackground", pet.PointsChangedBackgroundColor },
                { "PointsEarnedFromLevelUp", pet.PointsGainedLevelUp }
            };
        }

        public async Task<IEnumerable<Pet>> GetTopLevelPetsAsync(int count = 10)
        {
            return await _context.Pets
                .AsNoTracking()
                .Include(p => p.User)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Take(count)
                .ToListAsync();
        }
    }
}

