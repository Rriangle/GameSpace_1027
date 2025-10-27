using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IPetService
    {
        // Pet 基本 CRUD
        Task<IEnumerable<Pet>> GetAllPetsAsync();
        Task<Pet?> GetPetByIdAsync(int petId);
        Task<Pet?> GetPetByUserIdAsync(int userId);
        Task<bool> CreatePetAsync(Pet pet);
        Task<bool> UpdatePetAsync(Pet pet);
        Task<bool> DeletePetAsync(int petId);

        // Pet 狀態管理
        Task<bool> UpdatePetStatsAsync(int petId, int hunger, int mood, int stamina, int cleanliness, int health);
        Task<bool> FeedPetAsync(int petId, int hungerIncrease);
        Task<bool> PlayWithPetAsync(int petId, int moodIncrease);
        Task<bool> CleanPetAsync(int petId, int cleanlinessIncrease);
        Task<bool> RestPetAsync(int petId, int staminaIncrease);

        // Pet 升級系統
        Task<bool> AddExperienceAsync(int petId, int exp);
        Task<bool> LevelUpPetAsync(int petId);
        Task<int> GetRequiredExpForLevelAsync(int level);

        // Pet 外觀管理
        Task<bool> ChangeSkinColorAsync(int petId, string colorCode, int pointsCost);
        Task<bool> ChangeBackgroundColorAsync(int petId, string colorCode, int pointsCost);
        Task<IEnumerable<PetColorOption>> GetAvailableColorsAsync();
        Task<IEnumerable<GameSpace.Areas.MiniGame.Models.ViewModels.PetBackgroundOption>> GetAvailableBackgroundsAsync();

        // Pet 統計
        Task<Dictionary<string, int>> GetPetStatsSummaryAsync(int petId);
        Task<IEnumerable<Pet>> GetTopLevelPetsAsync(int count = 10);
    }
}


