using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    public class MiniGameAdminService : IMiniGameAdminService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<MiniGameAdminService> _logger;
        private static readonly TimeZoneInfo TaipeiTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

        public MiniGameAdminService(GameSpacedatabaseContext context, ILogger<MiniGameAdminService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private (DateTime startUtc, DateTime endUtc) GetTaipeiDateRange(DateTime taipeiDate)
        {
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(taipeiDate.Date, TaipeiTimeZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(taipeiDate.Date.AddDays(1), TaipeiTimeZone);
            return (startUtc, endUtc);
        }

        // 會員點數系統
        public async Task<UserWallet?> GetUserPointsAsync(int userId)
        {
            return await _context.UserWallets
                .AsNoTracking()
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<List<UserWallet>> GetAllUserPointsAsync()
        {
            return await _context.UserWallets
                .AsNoTracking()
                .Include(u => u.User)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserPointsAsync(int userId, int points)
        {
            var wallet = await _context.UserWallets.FirstOrDefaultAsync(u => u.UserId == userId);

            if (wallet != null)
            {
                wallet.UserPoint = points;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PagedResult<UserWallet>> QueryUserPointsAsync(CouponQueryModel query)
        {
            var queryable = _context.UserWallets
                .AsNoTracking()
                .Include(u => u.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(u => u.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<UserWallet>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<GameSpace.Models.User>> GetUsersAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<bool> AdjustUserPointsAsync(int userId, int points, string reason)
        {
            var wallet = await _context.UserWallets.FirstOrDefaultAsync(u => u.UserId == userId);

            if (wallet == null)
            {
                _logger.LogWarning("找不到用戶錢包: UserId={UserId}", userId);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 驗證點數不會變負
                var newBalance = wallet.UserPoint + points;
                if (newBalance < 0)
                {
                    _logger.LogWarning("調整後點數會變負: UserId={UserId}, 當前={Current}, 調整={Adjust}",
                        userId, wallet.UserPoint, points);
                    return false;
                }

                // 記錄變更前後的餘額
                var balanceBefore = wallet.UserPoint;

                // 更新錢包
                wallet.UserPoint = newBalance;

                // 記錄 WalletHistory（在 Description 中記錄餘額變化）
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = points > 0 ? "Admin_Add" : "Admin_Deduct",
                    PointsChanged = points,
                    Description = $"{reason} (餘額：{balanceBefore} → {newBalance})",
                    ItemCode = "ADMIN_ADJUST",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("管理員調整用戶點數成功: UserId={UserId}, 調整={Points}, 原因={Reason}",
                    userId, points, reason);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "調整用戶點數失敗: UserId={UserId}, Points={Points}", userId, points);
                return false;
            }
        }

        // 商城優惠券系統
        public async Task<List<UserCouponReadModel>> GetUserCouponsAsync(int userId)
        {
            return await _context.Coupons
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .Select(c => new UserCouponReadModel
                {
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    CouponTypeId = c.CouponTypeId,
                    CouponTypeName = c.CouponType.Name,
                    Quantity = 1,
                    LastUpdated = c.AcquiredTime
                })
                .ToListAsync();
        }

        public async Task<bool> AddUserCouponAsync(int userId, int couponTypeId, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                var coupon = new GameSpace.Models.Coupon
                {
                    UserId = userId,
                    CouponTypeId = couponTypeId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow,
                    CouponCode = $"CPN-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
                };
                _context.Coupons.Add(coupon);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveUserCouponAsync(int couponId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<GameSpace.Models.CouponType>> GetCouponTypesAsync()
        {
            return await _context.CouponTypes.AsNoTracking().ToListAsync();
        }

        public async Task<bool> IssueCouponToUserAsync(int userId, int couponTypeId, int quantity)
        {
            return await AddUserCouponAsync(userId, couponTypeId, quantity);
        }

        public async Task<bool> RemoveCouponFromUserAsync(int userId, int couponTypeId)
        {
            var coupons = await _context.Coupons
                .Where(c => c.UserId == userId && c.CouponTypeId == couponTypeId && !c.IsUsed)
                .ToListAsync();

            if (coupons.Any())
            {
                _context.Coupons.RemoveRange(coupons);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PagedResult<UserCouponReadModel>> QueryUserCouponsAsync(CouponQueryModel query)
        {
            var queryable = _context.Coupons
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(c => c.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new UserCouponReadModel
                {
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    CouponTypeId = c.CouponTypeId,
                    CouponTypeName = c.CouponType.Name,
                    Quantity = 1,
                    LastUpdated = c.AcquiredTime
                })
                .ToListAsync();

            return new PagedResult<UserCouponReadModel>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // 電子優惠券系統
        public async Task<List<Evoucher>> GetUserEVouchersAsync(int userId)
        {
            return await _context.Evouchers
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> AddUserEVoucherAsync(int userId, int EvoucherTypeId, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                var evoucher = new Evoucher
                {
                    UserId = userId,
                    EvoucherTypeId = EvoucherTypeId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow,
                    EvoucherCode = $"EV-{EvoucherTypeId}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
                };
                _context.Evouchers.Add(evoucher);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveUserEVoucherAsync(int evoucherId)
        {
            var evoucher = await _context.Evouchers.FindAsync(evoucherId);
            if (evoucher != null)
            {
                _context.Evouchers.Remove(evoucher);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<EvoucherType>> GetEVoucherTypesAsync()
        {
            return await _context.EvoucherTypes.AsNoTracking().ToListAsync();
        }

        public async Task<bool> IssueEVoucherToUserAsync(int userId, int EvoucherTypeId, int quantity)
        {
            return await AddUserEVoucherAsync(userId, EvoucherTypeId, quantity);
        }

        public async Task<bool> RemoveEVoucherFromUserAsync(int userId, int EvoucherTypeId)
        {
            var evouchers = await _context.Evouchers
                .Where(e => e.UserId == userId && e.EvoucherTypeId == EvoucherTypeId && !e.IsUsed)
                .ToListAsync();

            if (evouchers.Any())
            {
                _context.Evouchers.RemoveRange(evouchers);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PagedResult<Evoucher>> QueryUserEVouchersAsync(EVoucherQueryModel query)
        {
            var queryable = _context.Evouchers
                .AsNoTracking()
                .Include(e => e.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(e => e.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Evoucher>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // 簽到系統
        public async Task<List<UserSignInStat>> GetUserSignInRecordsAsync(int userId)
        {
            return await _context.UserSignInStats
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();
        }

        public async Task<bool> AddSignInRecordAsync(int userId, DateTime signInDate)
        {
            var signIn = new UserSignInStat
            {
                UserId = userId,
                SignTime = signInDate
            };
            _context.UserSignInStats.Add(signIn);
            return await _context.SaveChangesAsync() > 0;
        }


        public async Task<bool> RemoveSignInRecordAsync(int signInId)
        {
            var signIn = await _context.UserSignInStats.FirstOrDefaultAsync(s => s.LogId == signInId);
            if (signIn != null)
            {
                _context.UserSignInStats.Remove(signIn);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> RemoveSignInRecordAsync(int userId, DateTime signInDate)
        {
            var (dayStartUtc, dayEndUtc) = GetTaipeiDateRange(signInDate);
            var signIn = await _context.UserSignInStats
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SignTime >= dayStartUtc && s.SignTime < dayEndUtc);
            if (signIn != null)
            {
                _context.UserSignInStats.Remove(signIn);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PagedResult<UserSignInStat>> GetSignInStatsAsync()
        {
            var query = _context.UserSignInStats
                .AsNoTracking()
                .Include(s => s.User)
                .OrderByDescending(s => s.SignTime);

            var totalCount = await query.CountAsync();
            var items = await query.ToListAsync();

            return new PagedResult<UserSignInStat>
            {
                Items = items,
                Page = 1,
                PageSize = totalCount, // 返回所有記錄
                TotalCount = totalCount
            };
        }

        public Task<SignInRuleReadModel> GetSignInRuleAsync()
        {
            return Task.FromResult(new SignInRuleReadModel());
        }

        public async Task<bool> AddUserSignInRecordAsync(int userId, DateTime signInDate)
        {
            return await AddSignInRecordAsync(userId, signInDate);
        }

        public async Task<bool> RemoveUserSignInRecordAsync(int userId, DateTime signInDate)
        {
            return await RemoveSignInRecordAsync(userId, signInDate);
        }

        public async Task<GameSpace.Models.User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<List<SignInRecordReadModel>> GetUserSignInHistoryAsync(int userId)
        {
            var stats = await GetUserSignInRecordsAsync(userId);
            return stats.Select(s => new SignInRecordReadModel
            {
                RecordId = s.LogId,
                UserId = s.UserId,
                UserName = s.User.UserName,
                Email = s.User.UserIntroduce?.Email ?? string.Empty,
                SignInDate = s.SignTime,
                ConsecutiveDays = 0, // This would need to be calculated
                PointsGained = s.PointsGained,
                ExpGained = s.ExpGained,
                CouponGained = null, // CouponGained in UserSignInStat is string, needs conversion
                CouponCode = s.CouponGained,
                RewardDescription = $"點數: {s.PointsGained}, 經驗: {s.ExpGained}"
            }).ToList();
        }

        // 寵物系統
        public async Task<GameSpace.Models.Pet?> GetUserPetAsync(int userId)
        {
            return await _context.Pets
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<List<GameSpace.Models.Pet>> GetAllPetsAsync()
        {
            return await _context.Pets
                .AsNoTracking()
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<bool> UpdatePetAsync(int userId, string petName, string color, string background, int experience, int level, int hunger, int happiness, int health, int energy, int cleanliness)
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);
            if (pet != null)
            {
                pet.PetName = petName;
                pet.Experience = experience;
                pet.Level = level;
                pet.Hunger = hunger;
                pet.Health = health;
                pet.Cleanliness = cleanliness;

                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<GameSpace.Models.Pet>> GetPetsAsync(PetQueryModel query)
        {
            return await GetAllPetsAsync();
        }

        public async Task<PetSummary> GetPetSummaryAsync()
        {
            var totalPets = await _context.Pets.AsNoTracking().CountAsync();
            var averageLevel = await _context.Pets.AsNoTracking().AverageAsync(p => p.Level);

            return new PetSummary
            {
                TotalPets = totalPets,
                AverageLevel = (int)averageLevel
            };
        }

        public Task<PetRuleReadModel> GetPetRuleAsync()
        {
            return Task.FromResult(new PetRuleReadModel());
        }

        public async Task<GameSpace.Models.Pet?> GetPetDetailAsync(int petId)
        {
            return await _context.Pets
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PetId == petId);
        }

        public async Task<bool> UpdatePetDetailsAsync(int petId, PetUpdateModel model)
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == petId);
            if (pet != null)
            {
                // 更新寵物屬性
                if (!string.IsNullOrEmpty(model.PetName))
                    pet.PetName = model.PetName;
                if (model.Experience.HasValue)
                    pet.Experience = model.Experience.Value;
                if (model.Level.HasValue)
                    pet.Level = model.Level.Value;
                if (model.Hunger.HasValue)
                    pet.Hunger = model.Hunger.Value;
                if (model.Mood.HasValue)
                    pet.Mood = model.Mood.Value;
                if (model.Health.HasValue)
                    pet.Health = model.Health.Value;
                if (model.Stamina.HasValue)
                    pet.Stamina = model.Stamina.Value;
                if (model.Cleanliness.HasValue)
                    pet.Cleanliness = model.Cleanliness.Value;
                
                _context.Pets.Update(pet);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public Task<List<PetSkinColorChangeLog>> GetPetSkinColorChangeLogsAsync(PetQueryModel query)
        {
            return Task.FromResult(new List<PetSkinColorChangeLog>());
        }

        public Task<List<PetBackgroundColorChangeLog>> GetPetBackgroundColorChangeLogsAsync(PetQueryModel query)
        {
            return Task.FromResult(new List<PetBackgroundColorChangeLog>());
        }

        // 小遊戲系統
        public async Task<List<GameSpace.Models.MiniGame>> GetUserGameRecordsAsync(int userId)
        {
            return await _context.MiniGames
                .AsNoTracking()
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.StartTime)
                .ToListAsync();
        }

        public async Task<bool> AddGameRecordAsync(int userId, DateTime startTime, DateTime? endTime, string result, int pointsReward, int expReward, int couponReward)
        {
            var game = new GameSpace.Models.MiniGame
            {
                UserId = userId,
                StartTime = startTime,
                EndTime = endTime,
                Result = result
            };
            _context.MiniGames.Add(game);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<GameSpace.Models.MiniGame>> GetGameRecordsAsync(GameSpace.Areas.MiniGame.Models.ViewModels.GameQueryModel query)
        {
            return await _context.MiniGames
                .AsNoTracking()
                .Include(g => g.User)
                .OrderByDescending(g => g.StartTime)
                .ToListAsync();
        }

        public async Task<GameSummary> GetGameSummaryAsync()
        {
            var totalGames = await _context.MiniGames.AsNoTracking().CountAsync();

            return new GameSummary
            {
                TotalGames = totalGames
            };
        }

        public Task<GameRuleReadModel> GetGameRuleAsync()
        {
            return Task.FromResult(new GameRuleReadModel());
        }

        public async Task<GameSpace.Models.MiniGame?> GetGameDetailAsync(int playId)
        {
            return await _context.MiniGames
                .AsNoTracking()
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.PlayId == playId);
        }

        // 總覽系統
        public async Task<WalletSummary> GetWalletSummaryAsync()
        {
            var totalUsers = await _context.Users.AsNoTracking().CountAsync();
            var totalPoints = await _context.UserWallets.AsNoTracking().SumAsync(w => w.UserPoint);

            return new WalletSummary
            {
                TotalUsers = totalUsers,
                TotalPoints = totalPoints
            };
        }

        // 交易記錄
        public Task<PagedResult<WalletTransaction>> QueryWalletTransactionsAsync(CouponQueryModel query)
        {
            return Task.FromResult(new PagedResult<WalletTransaction>
            {
                Items = new List<WalletTransaction>(),
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = 0
            });
        }

        // 規則設定
        public SignInRuleReadModel GetSignInRule()
        {
            return new SignInRuleReadModel();
        }

        public Task<bool> UpdateSignInRuleAsync(SignInRuleUpdateModel model)
        {
            return Task.FromResult(true);
        }

        public PetRuleReadModel GetPetRule()
        {
            return new PetRuleReadModel();
        }

        public Task<bool> UpdatePetRuleAsync(PetRuleUpdateModel model)
        {
            return Task.FromResult(true);
        }

        public GameRuleReadModel GetGameRule()
        {
            return new GameRuleReadModel();
        }

        public Task<bool> UpdateGameRuleAsync(GameRuleUpdateModel model)
        {
            return Task.FromResult(true);
        }
    }
}





