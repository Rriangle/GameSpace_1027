using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物顏色選項服務實作
    /// </summary>
    public class PetColorOptionService : IPetColorOptionService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetColorOptionService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 基本 CRUD
        public async Task<IEnumerable<PetColorOption>> GetAllAsync()
        {
            return await _context.Set<PetColorOption>()
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.ColorName)
                .ToListAsync();
        }

        public async Task<PetColorOption?> GetByIdAsync(int id)
        {
            return await _context.Set<PetColorOption>()
                .FirstOrDefaultAsync(o => o.ColorOptionId == id);
        }

        public async Task<bool> CreateAsync(PetColorOption option)
        {
            try
            {
                // 檢查顏色代碼是否已存在
                if (await ExistsByColorCodeAsync(option.ColorCode))
                {
                    return false;
                }

                option.CreatedAt = DateTime.UtcNow;
                option.IsActive = true;

                _context.Set<PetColorOption>().Add(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PetColorOption option)
        {
            try
            {
                var existing = await GetByIdAsync(option.ColorOptionId);
                if (existing == null) return false;

                // 檢查新顏色代碼是否與其他選項衝突
                if (existing.ColorCode != option.ColorCode &&
                    await ExistsByColorCodeAsync(option.ColorCode))
                {
                    return false;
                }

                existing.ColorName = option.ColorName;
                existing.ColorCode = option.ColorCode;
                existing.IsActive = option.IsActive;
                existing.SortOrder = option.SortOrder;
                existing.Remarks = option.Remarks;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = option.UpdatedBy;

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

                _context.Set<PetColorOption>().Remove(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 查詢功能
        public async Task<IEnumerable<PetColorOption>> GetActiveOptionsAsync()
        {
            return await _context.Set<PetColorOption>()
                .Where(o => o.IsActive)
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.ColorName)
                .ToListAsync();
        }

        public async Task<IEnumerable<PetColorOption>> GetByColorCodeAsync(string colorCode)
        {
            return await _context.Set<PetColorOption>()
                .Where(o => o.ColorCode == colorCode)
                .ToListAsync();
        }

        public async Task<PetColorOption?> GetByColorNameAsync(string colorName)
        {
            return await _context.Set<PetColorOption>()
                .FirstOrDefaultAsync(o => o.ColorName == colorName);
        }

        public async Task<bool> ExistsByColorCodeAsync(string colorCode)
        {
            return await _context.Set<PetColorOption>()
                .AnyAsync(o => o.ColorCode == colorCode);
        }

        public async Task<bool> ExistsByColorNameAsync(string colorName)
        {
            return await _context.Set<PetColorOption>()
                .AnyAsync(o => o.ColorName == colorName);
        }

        // 排序與分頁
        public async Task<IEnumerable<PetColorOption>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.Set<PetColorOption>()
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.ColorName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Set<PetColorOption>().CountAsync();
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
        public async Task<IEnumerable<PetColorOptionViewModel>> GetAllColorOptionsAsync()
        {
            var options = await GetAllAsync();
            return options.Select(MapToViewModel);
        }

        public async Task<IEnumerable<PetColorOptionViewModel>> GetActiveColorOptionsAsync()
        {
            var options = await GetActiveOptionsAsync();
            return options.Select(MapToViewModel);
        }

        public async Task<PetColorOptionViewModel?> GetColorOptionByIdAsync(int id)
        {
            var option = await GetByIdAsync(id);
            return option != null ? MapToViewModel(option) : null;
        }

        public async Task<bool> CreateColorOptionAsync(PetColorOptionViewModel viewModel, int managerId)
        {
            try
            {
                // 檢查顏色代碼是否已存在
                if (await ExistsByColorCodeAsync(viewModel.ColorCode))
                {
                    return false;
                }

                var entity = new PetColorOption
                {
                    ColorName = viewModel.ColorName,
                    ColorCode = viewModel.ColorCode,
                    IsActive = viewModel.IsActive,
                    SortOrder = viewModel.SortOrder,
                    Remarks = viewModel.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = managerId
                };

                _context.Set<PetColorOption>().Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateColorOptionAsync(PetColorOptionViewModel viewModel, int managerId)
        {
            try
            {
                var existing = await GetByIdAsync(viewModel.ColorOptionId);
                if (existing == null) return false;

                // 檢查新顏色代碼是否與其他選項衝突
                if (existing.ColorCode != viewModel.ColorCode &&
                    await ExistsByColorCodeAsync(viewModel.ColorCode))
                {
                    return false;
                }

                existing.ColorName = viewModel.ColorName;
                existing.ColorCode = viewModel.ColorCode;
                existing.IsActive = viewModel.IsActive;
                existing.SortOrder = viewModel.SortOrder;
                existing.Remarks = viewModel.Description;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = managerId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteColorOptionAsync(int id)
        {
            return await DeleteAsync(id);
        }

        public async Task<bool> ToggleColorOptionStatusAsync(int id, int managerId)
        {
            try
            {
                var option = await GetByIdAsync(id);
                if (option == null) return false;

                option.IsActive = !option.IsActive;
                option.UpdatedAt = DateTime.UtcNow;
                option.UpdatedBy = managerId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<PetColorOptionViewModel>> SearchColorOptionsAsync(string keyword, bool includeInactive)
        {
            var query = _context.Set<PetColorOption>().AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(o => o.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(o =>
                    o.ColorName.Contains(keyword) ||
                    o.ColorCode.Contains(keyword) ||
                    (o.Remarks != null && o.Remarks.Contains(keyword)));
            }

            var options = await query
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.ColorName)
                .ToListAsync();

            return options.Select(MapToViewModel);
        }

        public async Task<object> GetColorOptionStatisticsAsync()
        {
            var totalCount = await _context.Set<PetColorOption>().CountAsync();
            var activeCount = await _context.Set<PetColorOption>().CountAsync(o => o.IsActive);
            var inactiveCount = totalCount - activeCount;

            // 統計使用此顏色的寵物數量
            var usageStats = await _context.Set<PetColorOption>()
                .Select(o => new
                {
                    o.ColorOptionId,
                    o.ColorName,
                    o.ColorCode,
                    UsageCount = _context.Pets.Count(p => p.SkinColor == o.ColorCode)
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
                TopUsedColors = usageStats,
                LastUpdated = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 將 Entity 映射為 ViewModel
        /// </summary>
        private PetColorOptionViewModel MapToViewModel(PetColorOption entity)
        {
            // 計算使用此顏色的寵物數量
            var usageCount = _context.Pets.Count(p => p.SkinColor == entity.ColorCode);

            return new PetColorOptionViewModel
            {
                ColorOptionId = entity.ColorOptionId,
                ColorName = entity.ColorName,
                ColorCode = entity.ColorCode,
                ColorType = "Skin",
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder,
                Description = entity.Remarks,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                UsageCount = usageCount
            };
        }
    }
}
