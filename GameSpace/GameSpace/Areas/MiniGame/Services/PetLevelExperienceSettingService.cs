using GameSpace.Areas.MiniGame.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IPetLevelExperienceSettingService
    {
        Task<List<PetLevelExperienceSetting>> GetAllAsync();
        Task<PetLevelExperienceSetting?> GetByIdAsync(int id);
        Task<PetLevelExperienceSetting?> GetByLevelAsync(int level);
        Task<PetLevelExperienceSetting> CreateAsync(PetLevelExperienceSetting setting);
        Task<PetLevelExperienceSetting> UpdateAsync(PetLevelExperienceSetting setting);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int level);
        Task<List<PetLevelExperienceSetting>> GetPagedAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
    }
    
    public class PetLevelExperienceSettingService : IPetLevelExperienceSettingService
    {
        private readonly List<PetLevelExperienceSetting> _settings;
        private readonly string _dataFilePath;
        
        public PetLevelExperienceSettingService()
        {
            _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Areas", "MiniGame", "Data", "pet_level_experience_settings.json");
            _settings = LoadSettings();
        }
        
        private List<PetLevelExperienceSetting> LoadSettings()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    return System.Text.Json.JsonSerializer.Deserialize<List<PetLevelExperienceSetting>>(json) ?? new();
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading pet level experience settings: {ex.Message}");
            }
            return new();
        }
        
        private async Task SaveSettingsAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }
                
                var json = System.Text.Json.JsonSerializer.Serialize(_settings, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await File.WriteAllTextAsync(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving pet level experience settings: {ex.Message}");
                throw;
            }
        }
        
        public async Task<List<PetLevelExperienceSetting>> GetAllAsync()
        {
            return await Task.FromResult(_settings.OrderBy(s => s.Level).ToList());
        }
        
        public async Task<PetLevelExperienceSetting?> GetByIdAsync(int id)
        {
            return await Task.FromResult(_settings.FirstOrDefault(s => s.Id == id));
        }
        
        public async Task<PetLevelExperienceSetting?> GetByLevelAsync(int level)
        {
            return await Task.FromResult(_settings.FirstOrDefault(s => s.Level == level));
        }
        
        public async Task<PetLevelExperienceSetting> CreateAsync(PetLevelExperienceSetting setting)
        {
            if (await ExistsAsync(setting.Level))
            {
                throw new InvalidOperationException($"Level {setting.Level} already exists");
            }
            
            setting.Id = _settings.Count > 0 ? _settings.Max(s => s.Id) + 1 : 1;
            setting.CreatedAt = DateTime.UtcNow;
            setting.UpdatedAt = DateTime.UtcNow;
            
            _settings.Add(setting);
            await SaveSettingsAsync();
            
            return setting;
        }
        
        public async Task<PetLevelExperienceSetting> UpdateAsync(PetLevelExperienceSetting setting)
        {
            var existing = await GetByIdAsync(setting.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Setting with ID {setting.Id} not found");
            }
            
            // Check if level is being changed and if the new level already exists
            if (existing.Level != setting.Level && await ExistsAsync(setting.Level))
            {
                throw new InvalidOperationException($"Level {setting.Level} already exists");
            }
            
            existing.Level = setting.Level;
            existing.RequiredExperience = setting.RequiredExperience;
            existing.Description = setting.Description;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await SaveSettingsAsync();
            
            return existing;
        }
        
        public async Task<bool> DeleteAsync(int id)
        {
            var setting = await GetByIdAsync(id);
            if (setting == null)
            {
                return false;
            }
            
            _settings.Remove(setting);
            await SaveSettingsAsync();
            
            return true;
        }
        
        public async Task<bool> ExistsAsync(int level)
        {
            return await Task.FromResult(_settings.Any(s => s.Level == level));
        }
        
        public async Task<List<PetLevelExperienceSetting>> GetPagedAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return await Task.FromResult(_settings
                .OrderBy(s => s.Level)
                .Skip(skip)
                .Take(pageSize)
                .ToList());
        }
        
        public async Task<int> GetTotalCountAsync()
        {
            return await Task.FromResult(_settings.Count);
        }
    }
}

