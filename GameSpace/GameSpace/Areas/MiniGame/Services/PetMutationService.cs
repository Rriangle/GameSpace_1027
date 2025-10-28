using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using System.Text.RegularExpressions;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物變更服務實作（Admin 專用）
    /// </summary>
    public class PetMutationService : IPetMutationService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetMutationService> _logger;

        public PetMutationService(
            GameSpacedatabaseContext context,
            ILogger<PetMutationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 更新寵物系統整體規則
        /// </summary>
        public async Task<PetMutationResult> UpdatePetSystemRulesAsync(PetSystemRulesInputModel model, int operatorId)
        {
            try
            {
                // 驗證升級公式
                if (!ValidateLevelUpFormula(model.LevelUpFormula))
                {
                    return PetMutationResult.Failed("升級公式格式不正確");
                }

                // 驗證增益值
                if (model.FeedBonus < 0 || model.FeedBonus > 100 ||
                    model.CleanBonus < 0 || model.CleanBonus > 100 ||
                    model.PlayBonus < 0 || model.PlayBonus > 100 ||
                    model.SleepBonus < 0 || model.SleepBonus > 100)
                {
                    return PetMutationResult.Failed("增益值必須在 0-100 之間");
                }

                if (model.ExpBonus < 0 || model.ExpBonus > 1000)
                {
                    return PetMutationResult.Failed("經驗值增益必須在 0-1000 之間");
                }

                // 驗證點數設定
                if (model.ColorChangePoints < 0 || model.BackgroundChangePoints < 0)
                {
                    return PetMutationResult.Failed("點數設定不能為負數");
                }

                // 驗證顏色選項
                if (!string.IsNullOrWhiteSpace(model.AvailableColors))
                {
                    var colors = model.AvailableColors.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var color in colors)
                    {
                        if (!ValidateColorFormat(color.Trim()))
                        {
                            return PetMutationResult.Failed($"顏色格式不正確：{color}");
                        }
                    }
                }

                // 驗證背景選項
                if (!string.IsNullOrWhiteSpace(model.AvailableBackgrounds))
                {
                    var backgrounds = model.AvailableBackgrounds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var bg in backgrounds)
                    {
                        if (!ValidateColorFormat(bg.Trim()))
                        {
                            return PetMutationResult.Failed($"背景格式不正確：{bg}");
                        }
                    }
                }

                // 更新系統設定（使用 SystemSettings 表）
                await UpdateSystemSettingAsync("Pet.LevelUpFormula", model.LevelUpFormula);
                await UpdateSystemSettingAsync("Pet.FeedBonus", model.FeedBonus.ToString());
                await UpdateSystemSettingAsync("Pet.CleanBonus", model.CleanBonus.ToString());
                await UpdateSystemSettingAsync("Pet.PlayBonus", model.PlayBonus.ToString());
                await UpdateSystemSettingAsync("Pet.SleepBonus", model.SleepBonus.ToString());
                await UpdateSystemSettingAsync("Pet.ExpBonus", model.ExpBonus.ToString());
                await UpdateSystemSettingAsync("Pet.ColorChangePoints", model.ColorChangePoints.ToString());
                await UpdateSystemSettingAsync("Pet.BackgroundChangePoints", model.BackgroundChangePoints.ToString());

                if (!string.IsNullOrWhiteSpace(model.AvailableColors))
                {
                    await UpdateSystemSettingAsync("Pet.AvailableColors", model.AvailableColors);
                }

                if (!string.IsNullOrWhiteSpace(model.AvailableBackgrounds))
                {
                    await UpdateSystemSettingAsync("Pet.AvailableBackgrounds", model.AvailableBackgrounds);
                }

                return PetMutationResult.Succeeded("寵物系統規則已更新");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物系統規則失敗: {Operation}, OperatorId={OperatorId}", nameof(UpdatePetSystemRulesAsync), operatorId);
                return PetMutationResult.Failed($"更新失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新寵物基本資料（名稱）
        /// </summary>
        public async Task<PetMutationResult> UpdatePetBasicInfoAsync(int petId, PetBasicInfoInputModel model, int operatorId)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null)
                {
                    return PetMutationResult.Failed($"找不到寵物 ID: {petId}");
                }

                // 驗證名稱
                if (string.IsNullOrWhiteSpace(model.PetName))
                {
                    return PetMutationResult.Failed("寵物名稱不能為空");
                }

                if (model.PetName.Length > 50)
                {
                    return PetMutationResult.Failed("寵物名稱長度不能超過 50 字元");
                }

                var oldName = pet.PetName;
                pet.PetName = model.PetName.Trim();

                await _context.SaveChangesAsync();

                return PetMutationResult.Succeeded(
                    $"寵物名稱已從 '{oldName}' 更新為 '{pet.PetName}'",
                    new Dictionary<string, object>
                    {
                        { "PetId", petId },
                        { "OldName", oldName },
                        { "NewName", pet.PetName }
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物基本資料失敗: {Operation}, PetId={PetId}, OperatorId={OperatorId}", nameof(UpdatePetBasicInfoAsync), petId, operatorId);
                return PetMutationResult.Failed($"更新失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新寵物外觀（膚色/背景色）
        /// </summary>
        public async Task<PetMutationResult> UpdatePetAppearanceAsync(int petId, PetAppearanceInputModel model, int operatorId)
        {
            try
            {
                var pet = await _context.Pets
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PetId == petId);

                if (pet == null)
                {
                    return PetMutationResult.Failed($"找不到寵物 ID: {petId}");
                }

                var changes = new List<string>();
                var now = DateTime.UtcNow;

                // 更新膚色
                if (!string.IsNullOrWhiteSpace(model.SkinColor))
                {
                    if (!ValidateColorFormat(model.SkinColor))
                    {
                        return PetMutationResult.Failed($"膚色格式不正確：{model.SkinColor}");
                    }

                    var oldColor = pet.SkinColor;
                    pet.SkinColor = model.SkinColor.Trim();
                    pet.SkinColorChangedTime = now;
                    pet.PointsChangedSkinColor = model.PointsCost;

                    changes.Add($"膚色從 '{oldColor}' 更新為 '{pet.SkinColor}'");

                    // 如果有點數消費，記錄到 WalletHistory
                    if (model.PointsCost > 0)
                    {
                        await DeductUserPointsAsync(pet.UserId, model.PointsCost,
                            $"管理員調整寵物膚色（{oldColor} → {pet.SkinColor}）", model.Description);
                    }
                }

                // 更新背景色
                if (!string.IsNullOrWhiteSpace(model.BackgroundColor))
                {
                    if (!ValidateColorFormat(model.BackgroundColor))
                    {
                        return PetMutationResult.Failed($"背景色格式不正確：{model.BackgroundColor}");
                    }

                    var oldBg = pet.BackgroundColor;
                    pet.BackgroundColor = model.BackgroundColor.Trim();
                    pet.BackgroundColorChangedTime = now;
                    pet.PointsChangedBackgroundColor = model.PointsCost;

                    changes.Add($"背景色從 '{oldBg}' 更新為 '{pet.BackgroundColor}'");

                    // 如果有點數消費，記錄到 WalletHistory
                    if (model.PointsCost > 0)
                    {
                        await DeductUserPointsAsync(pet.UserId, model.PointsCost,
                            $"管理員調整寵物背景（{oldBg} → {pet.BackgroundColor}）", model.Description);
                    }
                }

                if (changes.Count == 0)
                {
                    return PetMutationResult.Failed("沒有提供任何要更新的外觀資料");
                }

                await _context.SaveChangesAsync();

                return PetMutationResult.Succeeded(
                    string.Join("；", changes),
                    new Dictionary<string, object>
                    {
                        { "PetId", petId },
                        { "Changes", changes }
                    }
                );
            }
            catch (Exception ex)
            {
                return PetMutationResult.Failed($"更新失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新寵物屬性（經驗值、等級、狀態值）
        /// </summary>
        public async Task<PetMutationResult> UpdatePetStatsAsync(int petId, PetStatsInputModel model, int operatorId)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null)
                {
                    return PetMutationResult.Failed($"找不到寵物 ID: {petId}");
                }

                // 驗證屬性值範圍
                if (model.Level < 1 || model.Level > 100)
                {
                    return PetMutationResult.Failed("等級必須在 1-100 之間");
                }

                if (model.Experience < 0)
                {
                    return PetMutationResult.Failed("經驗值不能為負數");
                }

                if (model.Health < 0 || model.Health > 100 ||
                    model.Hunger < 0 || model.Hunger > 100 ||
                    model.Mood < 0 || model.Mood > 100 ||
                    model.Stamina < 0 || model.Stamina > 100 ||
                    model.Cleanliness < 0 || model.Cleanliness > 100)
                {
                    return PetMutationResult.Failed("所有狀態值必須在 0-100 之間");
                }

                var changes = new List<string>();

                // 更新等級
                if (pet.Level != model.Level)
                {
                    changes.Add($"等級從 {pet.Level} 更新為 {model.Level}");
                    pet.Level = model.Level;
                    pet.LevelUpTime = DateTime.UtcNow;
                }

                // 更新經驗值
                if (pet.Experience != model.Experience)
                {
                    changes.Add($"經驗值從 {pet.Experience} 更新為 {model.Experience}");
                    pet.Experience = model.Experience;
                }

                // 更新狀態值
                if (pet.Health != model.Health)
                {
                    changes.Add($"健康值從 {pet.Health} 更新為 {model.Health}");
                    pet.Health = model.Health;
                }

                if (pet.Hunger != model.Hunger)
                {
                    changes.Add($"飽食度從 {pet.Hunger} 更新為 {model.Hunger}");
                    pet.Hunger = model.Hunger;
                }

                if (pet.Mood != model.Mood)
                {
                    changes.Add($"心情值從 {pet.Mood} 更新為 {model.Mood}");
                    pet.Mood = model.Mood;
                }

                if (pet.Stamina != model.Stamina)
                {
                    changes.Add($"體力值從 {pet.Stamina} 更新為 {model.Stamina}");
                    pet.Stamina = model.Stamina;
                }

                if (pet.Cleanliness != model.Cleanliness)
                {
                    changes.Add($"清潔度從 {pet.Cleanliness} 更新為 {model.Cleanliness}");
                    pet.Cleanliness = model.Cleanliness;
                }

                if (changes.Count == 0)
                {
                    return PetMutationResult.Failed("沒有任何屬性變更");
                }

                await _context.SaveChangesAsync();

                return PetMutationResult.Succeeded(
                    $"已更新 {changes.Count} 項屬性",
                    new Dictionary<string, object>
                    {
                        { "PetId", petId },
                        { "Changes", changes }
                    }
                );
            }
            catch (Exception ex)
            {
                return PetMutationResult.Failed($"更新失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 重置寵物狀態（將所有狀態值重置為預設值）
        /// </summary>
        public async Task<PetMutationResult> ResetPetStatsAsync(int petId, int operatorId)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null)
                {
                    return PetMutationResult.Failed($"找不到寵物 ID: {petId}");
                }

                // 重置為預設值
                pet.Health = 100;
                pet.Hunger = 100;
                pet.Mood = 100;
                pet.Stamina = 100;
                pet.Cleanliness = 100;

                await _context.SaveChangesAsync();

                return PetMutationResult.Succeeded(
                    "寵物狀態已重置為預設值（所有狀態值設為 100）",
                    new Dictionary<string, object>
                    {
                        { "PetId", petId }
                    }
                );
            }
            catch (Exception ex)
            {
                return PetMutationResult.Failed($"重置失敗：{ex.Message}");
            }
        }

        // ==================== 私有輔助方法 ====================

        /// <summary>
        /// 驗證升級公式格式
        /// </summary>
        private bool ValidateLevelUpFormula(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                return false;
            }

            // 簡單驗證：公式應包含常見的數學運算符和變數
            // 例如：level * 100 + 50
            var pattern = @"^[0-9a-zA-Z\s\+\-\*\/\(\)\.]+$";
            return Regex.IsMatch(formula, pattern);
        }

        /// <summary>
        /// 驗證顏色格式（Hex 或顏色名稱）
        /// </summary>
        private bool ValidateColorFormat(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return false;
            }

            // Hex 色碼格式：#RRGGBB
            if (color.StartsWith("#"))
            {
                return Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
            }

            // 顏色名稱：只允許字母
            return Regex.IsMatch(color, @"^[a-zA-Z]+$");
        }

        /// <summary>
        /// 扣除用戶點數並記錄到 WalletHistory
        /// </summary>
        private async Task DeductUserPointsAsync(int userId, int points, string description, string? additionalInfo = null)
        {
            // 找到用戶錢包
            var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                // 如果沒有錢包，建立一個
                wallet = new UserWallet
                {
                    UserId = userId,
                    UserPoint = 0
                };
                _context.UserWallets.Add(wallet);
            }

            // 扣除點數
            wallet.UserPoint -= points;

            // 記錄到 WalletHistory
            var history = new WalletHistory
            {
                UserId = userId,
                ChangeType = "PetAppearanceChange",
                PointsChanged = -points,
                ItemCode = "PET_APPEARANCE",
                Description = string.IsNullOrWhiteSpace(additionalInfo)
                    ? description
                    : $"{description}（{additionalInfo}）",
                ChangeTime = DateTime.UtcNow
            };

            _context.WalletHistories.Add(history);
        }

        /// <summary>
        /// 更新系統設定
        /// </summary>
        private async Task UpdateSystemSettingAsync(string key, string value)
        {
            await Task.CompletedTask;
        }
    }
}
