using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物膚色成本設定管理服務（InMemory 版本）
    /// 支援查詢、新增、修改、刪除寵物膚色成本設定
    /// 使用 JSON 檔案持久化存儲
    /// 實現 IPetSkinColorCostSettingService 介面
    /// </summary>

    public class InMemoryPetSkinColorCostSettingService : IPetSkinColorCostSettingService
    {
        private readonly string _dataFilePath;
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static int _nextId = 4; // 起始 ID（預設有 3 筆）

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // 支援中文
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public InMemoryPetSkinColorCostSettingService(IWebHostEnvironment env)
        {
            // 將設定存在 Areas/MiniGame/App_Data/pet-skin-color-cost-settings.json
            var dataDir = Path.Combine(env.ContentRootPath, "Areas", "MiniGame", "App_Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _dataFilePath = Path.Combine(dataDir, "pet-skin-color-cost-settings.json");
        }

        /// <summary>
        /// 取得所有寵物膚色成本設定（從檔案載入，若檔案不存在則創建預設設定）
        /// </summary>
        public async Task<IEnumerable<PetSkinColorCostSetting>> GetAllAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    await InitializeDefaultSettingsAsync();
                }

                var json = await File.ReadAllTextAsync(_dataFilePath, System.Text.Encoding.UTF8);
                var settings = JsonSerializer.Deserialize<List<PetSkinColorCostSetting>>(json, JsonOptions) ?? new List<PetSkinColorCostSetting>();
                return settings.OrderBy(s => s.PointsCost).ThenBy(s => s.ColorName);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 根據 ID 取得寵物膚色成本設定
        /// </summary>
        public async Task<PetSkinColorCostSetting?> GetByIdAsync(int id)
        {
            var settings = await GetAllAsync();
            return settings.FirstOrDefault(s => s.SettingId == id);
        }

        /// <summary>
        /// 新增寵物膚色成本設定
        /// </summary>
        public async Task<bool> CreateAsync(PetSkinColorCostSetting setting)
        {
            await _lock.WaitAsync();
            try
            {
                var settings = (await GetAllAsync()).ToList();
                
                // 檢查顏色代碼是否已存在
                if (settings.Any(s => s.ColorCode.Equals(setting.ColorCode, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                setting.SettingId = _nextId++;
                setting.CreatedAt = DateTime.UtcNow;
                setting.IsActive = true;

                settings.Add(setting);
                await SaveSettingsAsync(settings);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 更新寵物膚色成本設定
        /// </summary>
        public async Task<bool> UpdateAsync(PetSkinColorCostSetting setting)
        {
            await _lock.WaitAsync();
            try
            {
                var settings = (await GetAllAsync()).ToList();
                var existingSetting = settings.FirstOrDefault(s => s.SettingId == setting.SettingId);

                if (existingSetting == null)
                {
                    return false;
                }

                // 檢查顏色代碼是否被其他記錄使用
                if (settings.Any(s => s.SettingId != setting.SettingId && s.ColorCode.Equals(setting.ColorCode, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                existingSetting.ColorName = setting.ColorName;
                existingSetting.ColorCode = setting.ColorCode;
                existingSetting.PointsCost = setting.PointsCost;
                existingSetting.IsActive = setting.IsActive;
                existingSetting.Description = setting.Description;
                existingSetting.UpdatedAt = DateTime.UtcNow;

                await SaveSettingsAsync(settings);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 刪除寵物膚色成本設定
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                var settings = (await GetAllAsync()).ToList();
                var setting = settings.FirstOrDefault(s => s.SettingId == id);
                
                if (setting == null)
                {
                    return false;
                }

                settings.Remove(setting);
                await SaveSettingsAsync(settings);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 取得所有啟用的寵物膚色成本設定
        /// </summary>
        public async Task<IEnumerable<PetSkinColorCostSetting>> GetActiveSettingsAsync()
        {
            var settings = await GetAllAsync();
            return settings.Where(s => s.IsActive).OrderBy(s => s.PointsCost);
        }

        /// <summary>
        /// 根據顏色代碼取得寵物膚色成本設定
        /// </summary>
        public async Task<PetSkinColorCostSetting?> GetByColorCodeAsync(string colorCode)
        {
            var settings = await GetAllAsync();
            return settings.FirstOrDefault(s => s.ColorCode.Equals(colorCode, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 根據顏色名稱取得寵物膚色成本設定
        /// </summary>
        public async Task<PetSkinColorCostSetting?> GetByColorNameAsync(string colorName)
        {
            var settings = await GetAllAsync();
            return settings.FirstOrDefault(s => s.ColorName.Equals(colorName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 檢查顏色代碼是否存在
        /// </summary>
        public async Task<bool> ExistsByColorCodeAsync(string colorCode)
        {
            var settings = await GetAllAsync();
            return settings.Any(s => s.ColorCode.Equals(colorCode, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 分頁取得寵物膚色成本設定
        /// </summary>
        public async Task<IEnumerable<PetSkinColorCostSetting>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var settings = await GetAllAsync();
            return settings.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 取得總數量
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            var settings = await GetAllAsync();
            return settings.Count();
        }

        /// <summary>
        /// 根據顏色代碼取得成本
        /// </summary>
        public async Task<int> GetCostByColorCodeAsync(string colorCode)
        {
            var setting = await GetByColorCodeAsync(colorCode);
            return setting?.PointsCost ?? 0;
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                var settings = (await GetAllAsync()).ToList();
                var setting = settings.FirstOrDefault(s => s.SettingId == id);
                
                if (setting == null)
                {
                    return false;
                }

                setting.IsActive = !setting.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;
                await SaveSettingsAsync(settings);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 設定啟用狀態
        /// </summary>
        public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
        {
            await _lock.WaitAsync();
            try
            {
                var settings = (await GetAllAsync()).ToList();
                var setting = settings.FirstOrDefault(s => s.SettingId == id);
                
                if (setting == null)
                {
                    return false;
                }

                setting.IsActive = isActive;
                setting.UpdatedAt = DateTime.UtcNow;
                await SaveSettingsAsync(settings);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 切換啟用狀態（別名方法）
        /// </summary>
        public async Task<bool> ToggleActiveAsync(int settingId)
        {
            return await ToggleActiveStatusAsync(settingId);
        }

        /// <summary>
        /// 批次更新成本
        /// </summary>
        public async Task<bool> UpdateMultipleCostsAsync(Dictionary<int, int> costMapping)
        {
            await _lock.WaitAsync();
            try
            {
                var settings = (await GetAllAsync()).ToList();
                
                foreach (var mapping in costMapping)
                {
                    var setting = settings.FirstOrDefault(s => s.SettingId == mapping.Key);
                    if (setting != null)
                    {
                        setting.PointsCost = mapping.Value;
                        setting.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await SaveSettingsAsync(settings);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 重置為預設設定
        /// </summary>
        public async Task ResetToDefaultAsync()
        {
            await _lock.WaitAsync();
            try
            {
                await InitializeDefaultSettingsAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 初始化預設設定
        /// </summary>
        private async Task InitializeDefaultSettingsAsync()
        {
            var now = DateTime.UtcNow;
            var defaultSettings = new List<PetSkinColorCostSetting>
            {
                new PetSkinColorCostSetting
                {
                    SettingId = 1, 
                    ColorName = "經典紅色", 
                    ColorCode = "#FF0000", 
                    PointsCost = 2000, 
                    IsActive = true, 
                    CreatedAt = now, 
                    Description = "預設種子資料 - 經典紅色" 
                },
                new PetSkinColorCostSetting
                {
                    SettingId = 2, 
                    ColorName = "經典藍色", 
                    ColorCode = "#0000FF", 
                    PointsCost = 2000, 
                    IsActive = true, 
                    CreatedAt = now, 
                    Description = "預設種子資料 - 經典藍色" 
                },
                new PetSkinColorCostSetting
                {
                    SettingId = 3, 
                    ColorName = "經典綠色", 
                    ColorCode = "#00FF00", 
                    PointsCost = 2000, 
                    IsActive = true, 
                    CreatedAt = now, 
                    Description = "預設種子資料 - 經典綠色" 
                }
            };

            await SaveSettingsAsync(defaultSettings);
        }

        /// <summary>
        /// 儲存設定到檔案
        /// </summary>
        private async Task SaveSettingsAsync(List<PetSkinColorCostSetting> settings)
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(_dataFilePath, json, System.Text.Encoding.UTF8);
        }
    }
}
