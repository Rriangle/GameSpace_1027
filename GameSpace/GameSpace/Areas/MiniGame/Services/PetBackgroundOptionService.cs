using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景選項服務實作
    /// </summary>
    public class PetBackgroundOptionService : IPetBackgroundOptionService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetBackgroundOptionService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 基本 CRUD
        public async Task<IEnumerable<PetBackgroundOptionEntity>> GetAllAsync()
        {
            return await _context.Set<PetBackgroundOptionEntity>()
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.BackgroundName)
                .ToListAsync();
        }

        public async Task<PetBackgroundOptionEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<PetBackgroundOptionEntity>()
                .FirstOrDefaultAsync(o => o.BackgroundOptionId == id);
        }

        public async Task<bool> CreateAsync(PetBackgroundOptionEntity option)
        {
            try
            {
                // 檢查背景代碼是否已存在
                if (await ExistsByBackgroundCodeAsync(option.BackgroundCode))
                {
                    return false;
                }

                option.CreatedAt = DateTime.UtcNow;
                option.IsActive = true;

                _context.Set<PetBackgroundOptionEntity>().Add(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PetBackgroundOptionEntity option)
        {
            try
            {
                var existing = await GetByIdAsync(option.BackgroundOptionId);
                if (existing == null) return false;

                // 檢查新背景代碼是否與其他選項衝突
                if (existing.BackgroundCode != option.BackgroundCode &&
                    await ExistsByBackgroundCodeAsync(option.BackgroundCode))
                {
                    return false;
                }

                existing.BackgroundName = option.BackgroundName;
                existing.BackgroundCode = option.BackgroundCode;
                existing.IsActive = option.IsActive;
                existing.SortOrder = option.SortOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var option = await GetByIdAsync(id);
                if (option == null) return false;

                _context.Set<PetBackgroundOptionEntity>().Remove(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 查詢功能
        public async Task<IEnumerable<PetBackgroundOptionEntity>> GetActiveOptionsAsync()
        {
            return await _context.Set<PetBackgroundOptionEntity>()
                .Where(o => o.IsActive)
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.BackgroundName)
                .ToListAsync();
        }

        public async Task<IEnumerable<PetBackgroundOptionEntity>> GetByBackgroundCodeAsync(string backgroundCode)
        {
            return await _context.Set<PetBackgroundOptionEntity>()
                .Where(o => o.BackgroundCode == backgroundCode)
                .ToListAsync();
        }

        public async Task<PetBackgroundOptionEntity?> GetByBackgroundNameAsync(string backgroundName)
        {
            return await _context.Set<PetBackgroundOptionEntity>()
                .FirstOrDefaultAsync(o => o.BackgroundName == backgroundName);
        }

        public async Task<bool> ExistsByBackgroundCodeAsync(string backgroundCode)
        {
            return await _context.Set<PetBackgroundOptionEntity>()
                .AnyAsync(o => o.BackgroundCode == backgroundCode);
        }

        public async Task<bool> ExistsByBackgroundNameAsync(string backgroundName)
        {
            return await _context.Set<PetBackgroundOptionEntity>()
                .AnyAsync(o => o.BackgroundName == backgroundName);
        }

        // 排序與分頁
        public async Task<IEnumerable<PetBackgroundOptionEntity>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.Set<PetBackgroundOptionEntity>()
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.BackgroundName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Set<PetBackgroundOptionEntity>().CountAsync();
        }

        // 狀態管理
        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            try
            {
                var option = await GetByIdAsync(id);
                if (option == null) return false;

                option.IsActive = !option.IsActive;
                option.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
        {
            try
            {
                var option = await GetByIdAsync(id);
                if (option == null) return false;

                option.IsActive = isActive;
                option.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 排序管理
        public async Task<bool> UpdateSortOrderAsync(int id, int newSortOrder)
        {
            try
            {
                var option = await GetByIdAsync(id);
                if (option == null) return false;

                option.SortOrder = newSortOrder;
                option.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReorderOptionsAsync(Dictionary<int, int> orderMapping)
        {
            try
            {
                foreach (var kvp in orderMapping)
                {
                    var option = await GetByIdAsync(kvp.Key);
                    if (option != null)
                    {
                        option.SortOrder = kvp.Value;
                        option.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ViewModel-based 方法
        public async Task<IEnumerable<PetBackgroundOptionViewModel>> GetAllBackgroundOptionsAsync()
        {
            var options = await GetAllAsync();
            return await MapToViewModelsAsync(options);
        }

        public async Task<IEnumerable<PetBackgroundOptionViewModel>> GetActiveBackgroundOptionsAsync()
        {
            var options = await GetActiveOptionsAsync();
            return await MapToViewModelsAsync(options);
        }

        public async Task<PetBackgroundOptionViewModel?> GetBackgroundOptionByIdAsync(int id)
        {
            var option = await GetByIdAsync(id);
            if (option == null) return null;

            var usageCount = await _context.Pets.CountAsync(p => p.BackgroundColor == option.BackgroundCode);
            return MapToViewModel(option, usageCount);
        }

        public async Task<bool> CreateBackgroundOptionAsync(PetBackgroundOptionViewModel viewModel, int managerId)
        {
            try
            {
                // 檢查背景代碼是否已存在
                if (await ExistsByBackgroundCodeAsync(viewModel.BackgroundCode))
                {
                    return false;
                }

                var entity = new PetBackgroundOptionEntity
                {
                    BackgroundName = viewModel.BackgroundName,
                    BackgroundCode = viewModel.BackgroundCode,
                    IsActive = viewModel.IsActive,
                    SortOrder = viewModel.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Set<PetBackgroundOptionEntity>().Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBackgroundOptionAsync(PetBackgroundOptionViewModel viewModel, int managerId)
        {
            try
            {
                var existing = await GetByIdAsync(viewModel.BackgroundOptionId);
                if (existing == null) return false;

                // 檢查新背景代碼是否與其他選項衝突
                if (existing.BackgroundCode != viewModel.BackgroundCode &&
                    await ExistsByBackgroundCodeAsync(viewModel.BackgroundCode))
                {
                    return false;
                }

                existing.BackgroundName = viewModel.BackgroundName;
                existing.BackgroundCode = viewModel.BackgroundCode;
                existing.IsActive = viewModel.IsActive;
                existing.SortOrder = viewModel.DisplayOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBackgroundOptionAsync(int id)
        {
            return await DeleteAsync(id);
        }

        public async Task<bool> ToggleBackgroundOptionStatusAsync(int id, int managerId)
        {
            try
            {
                var option = await GetByIdAsync(id);
                if (option == null) return false;

                option.IsActive = !option.IsActive;
                option.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<PetBackgroundOptionViewModel>> SearchBackgroundOptionsAsync(string keyword, bool includeInactive)
        {
            var query = _context.Set<PetBackgroundOptionEntity>().AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(o => o.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(o =>
                    o.BackgroundName.Contains(keyword) ||
                    o.BackgroundCode.Contains(keyword));
            }

            var options = await query
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.BackgroundName)
                .ToListAsync();

            return await MapToViewModelsAsync(options);
        }

        public async Task<object> GetBackgroundOptionStatisticsAsync()
        {
            var totalCount = await _context.Set<PetBackgroundOptionEntity>().CountAsync();
            var activeCount = await _context.Set<PetBackgroundOptionEntity>().CountAsync(o => o.IsActive);
            var inactiveCount = totalCount - activeCount;

            // 統計使用此背景的寵物數量
            var usageStats = await _context.Set<PetBackgroundOptionEntity>()
                .Select(o => new
                {
                    o.BackgroundOptionId,
                    o.BackgroundName,
                    o.BackgroundCode,
                    UsageCount = _context.Pets.Count(p => p.BackgroundColor == o.BackgroundCode)
                })
                .Where(x => x.UsageCount > 0)
                .OrderByDescending(x => x.UsageCount)
                .Take(10)
                .ToListAsync();

            return new
            {
                TotalCount = totalCount,
                ActiveCount = activeCount,
                InactiveCount = inactiveCount,
                TopUsedBackgrounds = usageStats,
                LastUpdated = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 將 Entity 映射為 ViewModel (單個對象，需要提供使用數量)
        /// </summary>
        private PetBackgroundOptionViewModel MapToViewModel(PetBackgroundOptionEntity entity, int usageCount)
        {
            return new PetBackgroundOptionViewModel
            {
                BackgroundOptionId = entity.BackgroundOptionId,
                BackgroundName = entity.BackgroundName,
                BackgroundCode = entity.BackgroundCode,
                BackgroundType = "Color",
                IsActive = entity.IsActive,
                DisplayOrder = entity.SortOrder,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                UsageCount = usageCount
            };
        }

        /// <summary>
        /// 批量將 Entity 集合映射為 ViewModel 集合 (優化查詢性能，避免 N+1 問題)
        /// </summary>
        private async Task<List<PetBackgroundOptionViewModel>> MapToViewModelsAsync(IEnumerable<PetBackgroundOptionEntity> entities)
        {
            var entityList = entities.ToList();
            if (!entityList.Any())
            {
                return new List<PetBackgroundOptionViewModel>();
            }

            // 收集所有背景代碼
            var backgroundCodes = entityList.Select(e => e.BackgroundCode).Distinct().ToList();

            // 一次性查詢所有使用數量
            var usageCounts = await _context.Pets
                .Where(p => backgroundCodes.Contains(p.BackgroundColor))
                .GroupBy(p => p.BackgroundColor)
                .Select(g => new { BackgroundCode = g.Key, Count = g.Count() })
                .ToListAsync();

            var usageDict = usageCounts.ToDictionary(x => x.BackgroundCode, x => x.Count);

            // 映射為 ViewModel
            return entityList.Select(entity => new PetBackgroundOptionViewModel
            {
                BackgroundOptionId = entity.BackgroundOptionId,
                BackgroundName = entity.BackgroundName,
                BackgroundCode = entity.BackgroundCode,
                BackgroundType = "Color",
                IsActive = entity.IsActive,
                DisplayOrder = entity.SortOrder,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                UsageCount = usageDict.ContainsKey(entity.BackgroundCode) ? usageDict[entity.BackgroundCode] : 0
            }).ToList();
        }
    }
}
