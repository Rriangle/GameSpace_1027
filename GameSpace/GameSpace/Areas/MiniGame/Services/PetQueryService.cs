using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物查詢服務實作
    /// </summary>
    public class PetQueryService : IPetQueryService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetQueryService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得會員寵物清單（分頁）
        /// </summary>
        public async Task<PetAdminListPagedResult> GetPetListAsync(PetAdminListQueryModel query)
        {
            var dbQuery = _context.Pets
                .Include(p => p.User)
                .AsNoTracking()
                .AsQueryable();

            // 篩選條件：會員 ID
            if (query.UserId.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.UserId == query.UserId.Value);
            }

            // 篩選條件：寵物名稱
            if (!string.IsNullOrWhiteSpace(query.PetName))
            {
                var nameLower = query.PetName.ToLower().Trim();
                dbQuery = dbQuery.Where(p => p.PetName.ToLower().Contains(nameLower));
            }

            // 篩選條件：等級範圍
            if (query.MinLevel.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Level >= query.MinLevel.Value);
            }

            if (query.MaxLevel.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Level <= query.MaxLevel.Value);
            }

            // 篩選條件：經驗值範圍
            if (query.MinExperience.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Experience >= query.MinExperience.Value);
            }

            if (query.MaxExperience.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Experience <= query.MaxExperience.Value);
            }

            // 篩選條件：膚色
            if (!string.IsNullOrWhiteSpace(query.SkinColor))
            {
                dbQuery = dbQuery.Where(p => p.SkinColor == query.SkinColor);
            }

            // 篩選條件：背景
            if (!string.IsNullOrWhiteSpace(query.BackgroundColor))
            {
                dbQuery = dbQuery.Where(p => p.BackgroundColor == query.BackgroundColor);
            }

            // 排序
            dbQuery = (query.SortBy ?? "petid").ToLower() switch
            {
                "petname" => (query.SortOrder ?? "asc").ToLower() == "desc"
                    ? dbQuery.OrderByDescending(p => p.PetName)
                    : dbQuery.OrderBy(p => p.PetName),
                "level" => (query.SortOrder ?? "asc").ToLower() == "desc"
                    ? dbQuery.OrderByDescending(p => p.Level)
                    : dbQuery.OrderBy(p => p.Level),
                "experience" => (query.SortOrder ?? "asc").ToLower() == "desc"
                    ? dbQuery.OrderByDescending(p => p.Experience)
                    : dbQuery.OrderBy(p => p.Experience),
                "health" => (query.SortOrder ?? "asc").ToLower() == "desc"
                    ? dbQuery.OrderByDescending(p => p.Health)
                    : dbQuery.OrderBy(p => p.Health),
                "userid" => (query.SortOrder ?? "asc").ToLower() == "desc"
                    ? dbQuery.OrderByDescending(p => p.UserId)
                    : dbQuery.OrderBy(p => p.UserId),
                _ => (query.SortOrder ?? "asc").ToLower() == "desc"
                    ? dbQuery.OrderByDescending(p => p.PetId)
                    : dbQuery.OrderBy(p => p.PetId)
            };

            // 計算總數
            var totalCount = await dbQuery.CountAsync();

            // 分頁
            var items = await dbQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PetAdminListItemViewModel
                {
                    PetId = p.PetId,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    PetName = p.PetName,
                    Level = p.Level,
                    Experience = p.Experience,
                    SkinColor = p.SkinColor,
                    BackgroundColor = p.BackgroundColor,
                    Health = p.Health,
                    Hunger = p.Hunger,
                    Mood = p.Mood,
                    Stamina = p.Stamina,
                    Cleanliness = p.Cleanliness
                })
                .ToListAsync();

            return new PetAdminListPagedResult
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        /// <summary>
        /// 取得單一寵物詳細資料
        /// </summary>
        public async Task<PetAdminDetailViewModel?> GetPetDetailAsync(int petId)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                    .ThenInclude(u => u.UserIntroduce)
                .AsNoTracking()
                .Where(p => p.PetId == petId)
                .Select(p => new PetAdminDetailViewModel
                {
                    PetId = p.PetId,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    UserEmail = p.User.UserIntroduce != null ? p.User.UserIntroduce.Email : string.Empty,
                    PetName = p.PetName,
                    Level = p.Level,
                    LevelUpTime = p.LevelUpTime,
                    Experience = p.Experience,
                    Health = p.Health,
                    Hunger = p.Hunger,
                    Mood = p.Mood,
                    Stamina = p.Stamina,
                    Cleanliness = p.Cleanliness,
                    SkinColor = p.SkinColor,
                    SkinColorChangedTime = p.SkinColorChangedTime,
                    BackgroundColor = p.BackgroundColor,
                    BackgroundColorChangedTime = p.BackgroundColorChangedTime,
                    PointsChangedSkinColor = p.PointsChangedSkinColor,
                    PointsChangedBackgroundColor = p.PointsChangedBackgroundColor,
                    PointsGainedLevelUp = p.PointsGainedLevelUp,
                    PointsGainedTimeLevelUp = p.PointsGainedTimeLevelUp
                })
                .FirstOrDefaultAsync();

            return pet;
        }

        /// <summary>
        /// 取得寵物膚色更換歷史記錄（分頁）
        /// </summary>
        public async Task<PetColorChangeHistoryPagedResult> GetColorChangeHistoryAsync(PetColorChangeHistoryQueryModel query)
        {
            var dbQuery = _context.WalletHistories
                .Include(wh => wh.User)
                .AsNoTracking()
                .Where(wh => wh.ChangeType == "PetSkinColorChange")
                .AsQueryable();

            // 篩選條件：會員 ID
            if (query.UserId.HasValue)
            {
                dbQuery = dbQuery.Where(wh => wh.UserId == query.UserId.Value);
            }

            // 篩選條件：寵物 ID（從 ItemCode 中解析）
            if (query.PetId.HasValue)
            {
                dbQuery = dbQuery.Where(wh => wh.ItemCode != null && wh.ItemCode.Contains($"Pet:{query.PetId.Value}"));
            }

            // 篩選條件：日期範圍
            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(wh => wh.ChangeTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                var endDate = query.EndDate.Value.Date.AddDays(1);
                dbQuery = dbQuery.Where(wh => wh.ChangeTime < endDate);
            }

            // 排序（預設為時間倒序）
            dbQuery = query.SortOrder.ToLower() == "asc"
                ? dbQuery.OrderBy(wh => wh.ChangeTime)
                : dbQuery.OrderByDescending(wh => wh.ChangeTime);

            // 計算總數
            var totalCount = await dbQuery.CountAsync();

            // 分頁並轉換為 ViewModel
            var items = await dbQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(wh => new ColorChangeHistoryItemViewModel
                {
                    LogId = wh.LogId,
                    UserId = wh.UserId,
                    UserName = wh.User.UserName,
                    PetId = ExtractPetIdFromItemCode(wh.ItemCode),
                    OldColor = ExtractOldColorFromDescription(wh.Description),
                    NewColor = ExtractNewColorFromDescription(wh.Description),
                    PointsCost = Math.Abs(wh.PointsChanged),
                    ChangedAt = wh.ChangeTime,
                    Description = wh.Description
                })
                .ToListAsync();

            // 補充寵物名稱
            var petIds = items.Where(i => i.PetId.HasValue).Select(i => i.PetId!.Value).Distinct().ToList();
            if (petIds.Any())
            {
                var petNames = await _context.Pets
                    .AsNoTracking()
                    .Where(p => petIds.Contains(p.PetId))
                    .ToDictionaryAsync(p => p.PetId, p => p.PetName);

                foreach (var item in items)
                {
                    if (item.PetId.HasValue && petNames.ContainsKey(item.PetId.Value))
                    {
                        item.PetName = petNames[item.PetId.Value];
                    }
                }
            }

            return new PetColorChangeHistoryPagedResult
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        /// <summary>
        /// 取得寵物背景更換歷史記錄（分頁）
        /// </summary>
        public async Task<PetBackgroundChangeHistoryPagedResult> GetBackgroundChangeHistoryAsync(PetBackgroundChangeHistoryQueryModel query)
        {
            var dbQuery = _context.WalletHistories
                .Include(wh => wh.User)
                .AsNoTracking()
                .Where(wh => wh.ChangeType == "PetBackgroundChange")
                .AsQueryable();

            // 篩選條件：會員 ID
            if (query.UserId.HasValue)
            {
                dbQuery = dbQuery.Where(wh => wh.UserId == query.UserId.Value);
            }

            // 篩選條件：寵物 ID（從 ItemCode 中解析）
            if (query.PetId.HasValue)
            {
                dbQuery = dbQuery.Where(wh => wh.ItemCode != null && wh.ItemCode.Contains($"Pet:{query.PetId.Value}"));
            }

            // 篩選條件：日期範圍
            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(wh => wh.ChangeTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                var endDate = query.EndDate.Value.Date.AddDays(1);
                dbQuery = dbQuery.Where(wh => wh.ChangeTime < endDate);
            }

            // 排序（預設為時間倒序）
            dbQuery = query.SortOrder.ToLower() == "asc"
                ? dbQuery.OrderBy(wh => wh.ChangeTime)
                : dbQuery.OrderByDescending(wh => wh.ChangeTime);

            // 計算總數
            var totalCount = await dbQuery.CountAsync();

            // 分頁並轉換為 ViewModel
            var items = await dbQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(wh => new BackgroundChangeHistoryItemViewModel
                {
                    LogId = wh.LogId,
                    UserId = wh.UserId,
                    UserName = wh.User.UserName,
                    PetId = ExtractPetIdFromItemCode(wh.ItemCode),
                    OldBackground = ExtractOldBackgroundFromDescription(wh.Description),
                    NewBackground = ExtractNewBackgroundFromDescription(wh.Description),
                    PointsCost = Math.Abs(wh.PointsChanged),
                    ChangedAt = wh.ChangeTime,
                    Description = wh.Description
                })
                .ToListAsync();

            // 補充寵物名稱
            var petIds = items.Where(i => i.PetId.HasValue).Select(i => i.PetId!.Value).Distinct().ToList();
            if (petIds.Any())
            {
                var petNames = await _context.Pets
                    .AsNoTracking()
                    .Where(p => petIds.Contains(p.PetId))
                    .ToDictionaryAsync(p => p.PetId, p => p.PetName);

                foreach (var item in items)
                {
                    if (item.PetId.HasValue && petNames.ContainsKey(item.PetId.Value))
                    {
                        item.PetName = petNames[item.PetId.Value];
                    }
                }
            }

            return new PetBackgroundChangeHistoryPagedResult
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        #region 輔助方法

        /// <summary>
        /// 從 ItemCode 解析寵物 ID
        /// 格式範例："Pet:123" 或 "Pet:123:Color"
        /// </summary>
        private static int? ExtractPetIdFromItemCode(string? itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                return null;

            var parts = itemCode.Split(':');
            if (parts.Length >= 2 && parts[0] == "Pet" && int.TryParse(parts[1], out int petId))
            {
                return petId;
            }

            return null;
        }

        /// <summary>
        /// 從 Description 解析舊膚色
        /// 格式範例："膚色更換：#FFFFFF → #000000"
        /// </summary>
        private static string ExtractOldColorFromDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var parts = description.Split('→');
            if (parts.Length >= 2)
            {
                var oldPart = parts[0].Trim();
                var colonIndex = oldPart.IndexOf('：');
                if (colonIndex >= 0)
                {
                    return oldPart.Substring(colonIndex + 1).Trim();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 從 Description 解析新膚色
        /// 格式範例："膚色更換：#FFFFFF → #000000"
        /// </summary>
        private static string ExtractNewColorFromDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var parts = description.Split('→');
            if (parts.Length >= 2)
            {
                return parts[1].Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// 從 Description 解析舊背景
        /// 格式範例："背景更換：Forest → Ocean"
        /// </summary>
        private static string ExtractOldBackgroundFromDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var parts = description.Split('→');
            if (parts.Length >= 2)
            {
                var oldPart = parts[0].Trim();
                var colonIndex = oldPart.IndexOf('：');
                if (colonIndex >= 0)
                {
                    return oldPart.Substring(colonIndex + 1).Trim();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 從 Description 解析新背景
        /// 格式範例："背景更換：Forest → Ocean"
        /// </summary>
        private static string ExtractNewBackgroundFromDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var parts = description.Split('→');
            if (parts.Length >= 2)
            {
                return parts[1].Trim();
            }

            return string.Empty;
        }

        #endregion
    }
}
