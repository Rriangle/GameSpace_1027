using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 錢包查詢服務
    /// </summary>
    public class WalletQueryService : IWalletQueryService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<WalletQueryService> _logger;

        public WalletQueryService(GameSpacedatabaseContext context, ILogger<WalletQueryService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 查詢積分
        /// </summary>
        public async Task<PagedResult<WalletPointRecord>> QueryUserPointsAsync(WalletQueryModel query)
        {
            try
            {
                var page = Math.Max(1, query.PageNumber);
                var pageSize = Math.Clamp(query.PageSize, 10, 200);

                var source = _context.UserWallets
                    .AsNoTracking()
                    .Include(w => w.User)
                    .AsQueryable();

                // 篩選條件
                if (query.UserId.HasValue)
                {
                    source = source.Where(w => w.UserId == query.UserId.Value);
                }

                if (query.MinAmount.HasValue)
                {
                    source = source.Where(w => w.UserPoint >= query.MinAmount.Value);
                }

                if (query.MaxAmount.HasValue)
                {
                    source = source.Where(w => w.UserPoint <= query.MaxAmount.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var term = query.SearchTerm.Trim();
                    source = source.Where(w =>
                        (w.User != null && (w.User.UserAccount.Contains(term) ||
                                           w.User.UserName.Contains(term))));
                }

                // 排序
                source = query.SortBy?.ToLowerInvariant() switch
                {
                    "points_asc" => source.OrderBy(w => w.UserPoint),
                    "userid_desc" => source.OrderByDescending(w => w.UserId),
                    "userid_asc" => source.OrderBy(w => w.UserId),
                    _ => source.OrderByDescending(w => w.UserPoint)
                };

                var totalCount = await source.CountAsync();
                var items = await source
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var records = items.Select(w => new WalletPointRecord
                {
                    UserId = w.UserId,
                    UserAccount = w.User?.UserAccount ?? string.Empty,
                    UserName = w.User?.UserName ?? string.Empty,
                    Email = w.User?.UserIntroduce?.Email ?? string.Empty,
                    Points = w.UserPoint
                }).ToList();

                _logger.LogInformation("查詢用戶積分成功: 頁碼={PageNumber}, 總數={TotalCount}", page, totalCount);

                return new PagedResult<WalletPointRecord>
                {
                    Items = records,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢用戶積分失敗: UserId={UserId}, PageNumber={PageNumber}",
                    query.UserId, query.PageNumber);
                throw;
            }
        }

        /// <summary>
        /// 查詢優惠券紀錄
        /// </summary>
        public async Task<PagedResult<UserCouponReadModel>> QueryUserCouponsAsync(CouponQueryModel query)
        {
            try
            {
                var page = Math.Max(1, query.PageNumber);
                var pageSize = Math.Clamp(query.PageSize, 10, 200);

                var source = _context.Coupons
                    .AsNoTracking()
                    .Include(c => c.User)
                    .Include(c => c.CouponType)
                    .AsQueryable();

                // 篩選條件
                if (query.UserId.HasValue)
                {
                    source = source.Where(c => c.UserId == query.UserId.Value);
                }

                if (query.CouponTypeId.HasValue)
                {
                    source = source.Where(c => c.CouponTypeId == query.CouponTypeId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.Status))
                {
                    source = query.Status.ToLowerInvariant() switch
                    {
                        "used" => source.Where(c => c.IsUsed),
                        "unused" => source.Where(c => !c.IsUsed && c.CouponType != null && (!c.CouponType.ValidTo.Equals(default) ? c.CouponType.ValidTo >= DateTime.UtcNow : true)),
                        "expired" => source.Where(c => !c.IsUsed && c.CouponType != null && c.CouponType.ValidTo < DateTime.UtcNow),
                        _ => source
                    };
                }

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var term = query.SearchTerm.Trim();
                    source = source.Where(c =>
                        c.CouponCode.Contains(term) ||
                        (c.User != null && (c.User.UserAccount.Contains(term) || c.User.UserName.Contains(term))));
                }

                // 排序
                source = query.SortBy?.ToLowerInvariant() switch
                {
                    "acquiredtime" => query.Descending ? source.OrderByDescending(c => c.AcquiredTime) : source.OrderBy(c => c.AcquiredTime),
                    "usetime" => query.Descending ? source.OrderByDescending(c => c.UsedTime) : source.OrderBy(c => c.UsedTime),
                    _ => source.OrderByDescending(c => c.AcquiredTime)
                };

                var totalCount = await source.CountAsync();
                var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                var records = items.Select(c => new UserCouponReadModel
                {
                    CouponId = c.CouponId,
                    CouponCode = c.CouponCode,
                    UserId = c.UserId,
                    UserName = c.User?.UserName ?? string.Empty,
                    Email = c.User?.UserIntroduce?.Email ?? string.Empty,
                    CouponTypeId = c.CouponTypeId,
                    CouponTypeName = c.CouponType?.Name ?? string.Empty,
                    DiscountAmount = c.CouponType?.DiscountValue ?? 0,
                    DiscountPercentage = c.CouponType?.DiscountType == "Percentage" ? c.CouponType?.DiscountValue : null,
                    MinimumPurchase = c.CouponType?.MinSpend,
                    AcquiredTime = c.AcquiredTime,
                    UsedTime = c.UsedTime,
                    ExpiryDate = c.CouponType?.ValidTo,
                    IsUsed = c.IsUsed
                }).ToList();

                _logger.LogInformation("查詢用戶優惠券成功: 頁碼={PageNumber}, 總數={TotalCount}", page, totalCount);

                return new PagedResult<UserCouponReadModel>
                {
                    Items = records,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢用戶優惠券失敗: UserId={UserId}, Status={Status}, PageNumber={PageNumber}",
                    query.UserId, query.Status, query.PageNumber);
                throw;
            }
        }

        /// <summary>
        /// 查詢電子票券
        /// </summary>
        public async Task<PagedResult<Models.ViewModels.EVoucherReadModel>> QueryUserEVouchersAsync(EVoucherQueryModel query)
        {
            try
            {
                var page = Math.Max(1, query.PageNumber);
                var pageSize = Math.Clamp(query.PageSize, 10, 200);

                var source = _context.Evouchers
                    .AsNoTracking()
                    .Include(e => e.User)
                    .Include(e => e.EvoucherType)
                    .AsQueryable();

                // 篩選條件
                if (query.UserId.HasValue)
                {
                    source = source.Where(e => e.UserId == query.UserId.Value);
                }

                if (query.EVoucherTypeId.HasValue)
                {
                    source = source.Where(e => e.EvoucherTypeId == query.EVoucherTypeId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.TypeCode))
                {
                    source = source.Where(e => e.EvoucherType != null && e.EvoucherType.Name.Contains(query.TypeCode));
                }

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var term = query.SearchTerm.Trim();
                    source = source.Where(e =>
                        e.EvoucherCode.Contains(term) ||
                        (e.User != null && (e.User.UserAccount.Contains(term) || e.User.UserName.Contains(term))));
                }

                if (!string.IsNullOrWhiteSpace(query.Status))
                {
                    source = query.Status.ToLowerInvariant() switch
                    {
                        "used" => source.Where(e => e.IsUsed),
                        "unused" => source.Where(e => !e.IsUsed && e.EvoucherType != null && e.EvoucherType.ValidTo >= DateTime.UtcNow),
                        "expired" => source.Where(e => !e.IsUsed && e.EvoucherType != null && e.EvoucherType.ValidTo < DateTime.UtcNow),
                        _ => source
                    };
                }

                // 排序
                source = query.SortBy?.ToLowerInvariant() switch
                {
                    "acquiredtime" => query.Descending ? source.OrderByDescending(e => e.AcquiredTime) : source.OrderBy(e => e.AcquiredTime),
                    "validto" => query.Descending ? source.OrderByDescending(e => e.EvoucherType != null ? e.EvoucherType.ValidTo : DateTime.MinValue) : source.OrderBy(e => e.EvoucherType != null ? e.EvoucherType.ValidTo : DateTime.MinValue),
                    _ => source.OrderByDescending(e => e.AcquiredTime)
                };

                var totalCount = await source.CountAsync();
                var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                var records = items.Select(e => new Models.ViewModels.EVoucherReadModel
                {
                    EVoucherId = e.EvoucherId,
                    EVoucherCode = e.EvoucherCode,
                    UserId = e.UserId,
                    UserName = e.User?.UserName ?? string.Empty,
                    UserEmail = e.User?.UserIntroduce?.Email ?? string.Empty,
                    EVoucherTypeId = e.EvoucherTypeId,
                    EVoucherTypeName = e.EvoucherType?.Name ?? string.Empty,
                    VoucherValue = e.EvoucherType?.ValueAmount ?? 0,
                    MerchantName = e.EvoucherType?.Description,
                    IsUsed = e.IsUsed,
                    AcquiredTime = e.AcquiredTime,
                    UsedTime = e.UsedTime,
                    ValidFrom = e.EvoucherType?.ValidFrom ?? DateTime.MinValue,
                    ValidTo = e.EvoucherType?.ValidTo ?? DateTime.MinValue,
                    UsedLocation = null
                }).ToList();

                _logger.LogInformation("查詢電子票券成功: 頁碼={PageNumber}, 總數={TotalCount}", page, totalCount);

                return new PagedResult<Models.ViewModels.EVoucherReadModel>
                {
                    Items = records,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢電子票券失敗: UserId={UserId}, Status={Status}, PageNumber={PageNumber}",
                    query.UserId, query.Status, query.PageNumber);
                throw;
            }
        }

        /// <summary>
        /// 查詢錢包異動紀錄
        /// </summary>
        public async Task<PagedResult<WalletHistoryRecord>> QueryWalletHistoryAsync(WalletHistoryQueryModel query)
        {
            try
            {
                var page = Math.Max(1, query.PageNumber);
                var pageSize = Math.Clamp(query.PageSize, 10, 200);

                var source = _context.WalletHistories
                    .AsNoTracking()
                    .Include(h => h.User)
                    .AsQueryable();

                // 篩選條件
                if (query.UserId.HasValue)
                {
                    source = source.Where(h => h.UserId == query.UserId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.ChangeType))
                {
                    source = source.Where(h => h.ChangeType == query.ChangeType);
                }

                if (query.StartDate.HasValue)
                {
                    source = source.Where(h => h.ChangeTime >= query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    source = source.Where(h => h.ChangeTime <= query.EndDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var term = query.SearchTerm.Trim();
                    source = source.Where(h =>
                        h.Description != null && h.Description.Contains(term) ||
                        (h.User != null && (h.User.UserAccount.Contains(term) || h.User.UserName.Contains(term))));
                }

                // 排序
                source = source.OrderByDescending(h => h.ChangeTime);

                var totalCount = await source.CountAsync();
                var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                // 查詢當前餘額
                var lookupIds = items.Select(h => h.UserId).Distinct().ToList();
                var walletLookup = await _context.UserWallets
                    .AsNoTracking()
                    .Where(w => lookupIds.Contains(w.UserId))
                    .ToDictionaryAsync(w => w.UserId, w => w.UserPoint);

                var records = items.Select(h => new WalletHistoryRecord
                {
                    LogId = h.LogId,
                    UserId = h.UserId,
                    UserAccount = h.User?.UserAccount ?? string.Empty,
                    UserName = h.User?.UserName ?? string.Empty,
                    ChangeType = h.ChangeType,
                    PointsChanged = h.PointsChanged,
                    BalanceAfter = walletLookup.TryGetValue(h.UserId, out var balance) ? balance : 0,
                    Description = h.Description,
                    ChangeTime = h.ChangeTime
                }).ToList();

                _logger.LogInformation("查詢錢包異動紀錄成功: 頁碼={PageNumber}, 總數={TotalCount}", page, totalCount);

                return new PagedResult<WalletHistoryRecord>
                {
                    Items = records,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢錢包異動紀錄失敗: UserId={UserId}, ChangeType={ChangeType}, PageNumber={PageNumber}",
                    query.UserId, query.ChangeType, query.PageNumber);
                throw;
            }
        }

        /// <summary>
        /// 取得單一用戶積分
        /// </summary>
        public async Task<int> GetUserPointsAsync(int userId)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.UserId == userId);
                return wallet?.UserPoint ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得用戶積分失敗: UserId={UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// 取得可用優惠券數量
        /// </summary>
        public async Task<int> GetUserAvailableCouponsCountAsync(int userId)
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _context.Coupons
                    .AsNoTracking()
                    .Include(c => c.CouponType)
                    .Where(c => c.UserId == userId && !c.IsUsed && c.CouponType != null && c.CouponType.ValidTo >= now)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得可用優惠券數量失敗: UserId={UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// 取得可用電子票券數量
        /// </summary>
        public async Task<int> GetUserAvailableEVouchersCountAsync(int userId)
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _context.Evouchers
                    .AsNoTracking()
                    .Include(e => e.EvoucherType)
                    .Where(e => e.UserId == userId && !e.IsUsed && e.EvoucherType != null && e.EvoucherType.ValidTo >= now)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得可用電子票券數量失敗: UserId={UserId}", userId);
                return 0;
            }
        }
    }
}
