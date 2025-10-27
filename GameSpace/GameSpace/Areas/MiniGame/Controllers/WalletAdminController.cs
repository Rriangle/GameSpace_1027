using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Infrastructure.Time;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class WalletAdminController : MiniGameBaseController
    {
        private readonly IAppClock _appClock;

        public WalletAdminController(GameSpacedatabaseContext context, IAppClock appClock)
            : base(context)
        {
            _appClock = appClock;
        }

        [HttpGet]
        public async Task<IActionResult> PointsQuery([FromQuery] WalletQueryModel query)
        {
            query ??= new WalletQueryModel();
            // 伺服器端保護：避免負數參數造成不合理查詢
            if (query.UserId.HasValue && query.UserId.Value < 0) query.UserId = 0;
            if (query.MinAmount.HasValue && query.MinAmount.Value < 0) query.MinAmount = 0;
            if (query.MaxAmount.HasValue && query.MaxAmount.Value < 0) query.MaxAmount = 0;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 10, 200);

            var source = _context.UserWallets
                .AsNoTracking()
                .AsQueryable();

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
                var matchedUserIds = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserAccount.Contains(term) || u.UserName.Contains(term))
                    .Select(u => u.UserId)
                    .ToListAsync();
                source = source.Where(w => matchedUserIds.Contains(w.UserId));
            }

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

            var userIds = items.Select(i => i.UserId).Distinct().ToList();
            var userLookup = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.UserAccount, u.UserName });
            var introduceLookup = await _context.UserIntroduces
                .AsNoTracking()
                .Where(ui => userIds.Contains(ui.UserId))
                .ToDictionaryAsync(ui => ui.UserId, ui => ui.Email);

            var records = items.Select(w =>
            {
                userLookup.TryGetValue(w.UserId, out var u);
                introduceLookup.TryGetValue(w.UserId, out var email);
                return new WalletPointRecord
                {
                    UserId = w.UserId,
                    UserAccount = u?.UserAccount ?? string.Empty,
                    UserName = u?.UserName ?? string.Empty,
                    Email = email ?? string.Empty,
                    Points = w.UserPoint
                };
            }).ToList();

            var model = new WalletPointsQueryViewModel
            {
                Query = query,
                Results = new PagedResult<WalletPointRecord>
                {
                    Items = records,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CouponsQuery([FromQuery] CouponQueryModel query)
        {
            query ??= new CouponQueryModel();
            // 伺服器端保護：避免負數參數
            if (query.UserId.HasValue && query.UserId.Value < 0) query.UserId = 0;
            if (query.CouponTypeId.HasValue && query.CouponTypeId.Value < 0) query.CouponTypeId = 0;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 10, 200);

            var source = from c in _context.Coupons.AsNoTracking()
                         join u in _context.Users.AsNoTracking() on c.UserId equals u.UserId into uj
                         from u in uj.DefaultIfEmpty()
                         join ct in _context.CouponTypes.AsNoTracking() on c.CouponTypeId equals ct.CouponTypeId into ctj
                         from ct in ctj.DefaultIfEmpty()
                         select new { c, u, ct };

            if (query.UserId.HasValue)
            {
                source = source.Where(x => x.c.UserId == query.UserId.Value);
            }

            if (query.CouponTypeId.HasValue)
            {
                source = source.Where(x => x.c.CouponTypeId == query.CouponTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var now = _appClock.UtcNow;
                source = query.Status.ToLowerInvariant() switch
                {
                    "used" => source.Where(x => x.c.IsUsed),
                    "unused" => source.Where(x => !x.c.IsUsed && (x.ct == null || x.ct.ValidTo >= now)),
                    "expired" => source.Where(x => !x.c.IsUsed && x.ct != null && x.ct.ValidTo < now),
                    _ => source
                };
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim();
                source = source.Where(x =>
                    x.c.CouponCode.Contains(term) ||
                    (x.u != null && (x.u.UserAccount.Contains(term) || x.u.UserName.Contains(term))));
            }

            source = query.SortBy?.ToLowerInvariant() switch
            {
                "acquiredtime" => query.Descending ? source.OrderByDescending(x => x.c.AcquiredTime) : source.OrderBy(x => x.c.AcquiredTime),
                "usetime" => query.Descending ? source.OrderByDescending(x => x.c.UsedTime) : source.OrderBy(x => x.c.UsedTime),
                _ => source.OrderByDescending(x => x.c.AcquiredTime)
            };

            var totalCount = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var records = items.Select(x => new UserCouponReadModel
            {
                CouponId = x.c.CouponId,
                CouponCode = x.c.CouponCode,
                UserId = x.c.UserId,
                UserName = x.u?.UserName ?? string.Empty,
                Email = string.Empty, // 若需 Email 可額外查 UserIntroduces
                CouponTypeId = x.c.CouponTypeId,
                CouponTypeName = x.ct?.Name ?? string.Empty,
                DiscountAmount = x.ct?.DiscountValue ?? 0,
                DiscountPercentage = x.ct?.DiscountType == "Percentage" ? x.ct?.DiscountValue : null,
                MinimumPurchase = x.ct?.MinSpend,
                AcquiredTime = x.c.AcquiredTime,
                UsedTime = x.c.UsedTime,
                ExpiryDate = x.ct?.ValidTo,
                IsUsed = x.c.IsUsed
            }).ToList();

            var model = new WalletCouponsQueryViewModel
            {
                Query = query,
                Results = new PagedResult<UserCouponReadModel>
                {
                    Items = records,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EVouchersQuery([FromQuery] EVoucherQueryModel query)
        {
            query ??= new EVoucherQueryModel();
            // 伺服器端保護：避免負數參數
            if (query.UserId.HasValue && query.UserId.Value < 0) query.UserId = 0;
            if (query.EVoucherTypeId.HasValue && query.EVoucherTypeId.Value < 0) query.EVoucherTypeId = 0;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 10, 200);

            var source = from e in _context.Evouchers.AsNoTracking()
                         join u in _context.Users.AsNoTracking() on e.UserId equals u.UserId into uj
                         from u in uj.DefaultIfEmpty()
                         join et in _context.EvoucherTypes.AsNoTracking() on e.EvoucherTypeId equals et.EvoucherTypeId into etj
                         from et in etj.DefaultIfEmpty()
                         select new { e, u, et };

            if (query.UserId.HasValue)
            {
                source = source.Where(x => x.e.UserId == query.UserId.Value);
            }

            if (query.EVoucherTypeId.HasValue)
            {
                source = source.Where(x => x.e.EvoucherTypeId == query.EVoucherTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.TypeCode))
            {
                source = source.Where(x => x.et != null && x.et.Name.Contains(query.TypeCode));
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim();
                source = source.Where(x =>
                    x.e.EvoucherCode.Contains(term) ||
                    (x.u != null && (x.u.UserAccount.Contains(term) || x.u.UserName.Contains(term))));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var now2 = _appClock.UtcNow;
                source = query.Status.ToLowerInvariant() switch
                {
                    "used" => source.Where(x => x.e.IsUsed),
                    "unused" => source.Where(x => !x.e.IsUsed && x.et != null && x.et.ValidTo >= now2),
                    "expired" => source.Where(x => !x.e.IsUsed && x.et != null && x.et.ValidTo < now2),
                    _ => source
                };
            }

            source = query.SortBy?.ToLowerInvariant() switch
            {
                "acquiredtime" => query.Descending ? source.OrderByDescending(x => x.e.AcquiredTime) : source.OrderBy(x => x.e.AcquiredTime),
                "validto" => query.Descending ? source.OrderByDescending(x => x.et != null ? x.et.ValidTo : DateTime.MinValue) : source.OrderBy(x => x.et != null ? x.et.ValidTo : DateTime.MinValue),
                _ => source.OrderByDescending(x => x.e.AcquiredTime)
            };

            var totalCount = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var records = items.Select(x => new Models.ViewModels.EVoucherReadModel
            {
                EVoucherId = x.e.EvoucherId,
                EVoucherCode = x.e.EvoucherCode,
                UserId = x.e.UserId,
                UserName = x.u?.UserName ?? string.Empty,
                UserEmail = string.Empty,
                EVoucherTypeId = x.e.EvoucherTypeId,
                EVoucherTypeName = x.et?.Name ?? string.Empty,
                VoucherValue = x.et?.ValueAmount ?? 0,
                MerchantName = x.et?.Description,
                IsUsed = x.e.IsUsed,
                AcquiredTime = x.e.AcquiredTime,
                UsedTime = x.e.UsedTime,
                ValidFrom = x.et?.ValidFrom ?? DateTime.MinValue,
                ValidTo = x.et?.ValidTo ?? DateTime.MinValue,
                UsedLocation = null
            }).ToList();

            var model = new WalletEVouchersQueryViewModel
            {
                Query = query,
                Results = new PagedResult<Models.ViewModels.EVoucherReadModel>
                {
                    Items = records,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> WalletHistory([FromQuery] WalletHistoryQueryModel query)
        {
            query ??= new WalletHistoryQueryModel();
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 10, 200);

            var source = _context.WalletHistories
                .AsNoTracking()
                .AsQueryable();

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
                var userIds = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserAccount.Contains(term) || u.UserName.Contains(term))
                    .Select(u => u.UserId)
                    .ToListAsync();
                
                source = source.Where(h =>
                    (h.Description != null && h.Description.Contains(term)) ||
                    userIds.Contains(h.UserId));
            }

            source = source.OrderByDescending(h => h.ChangeTime);

            var totalCount = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 手動載入用戶資料
            var lookupIds = items.Select(h => h.UserId).Distinct().ToList();
            var userLookup = await _context.Users
                .AsNoTracking()
                .Where(u => lookupIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.UserAccount, u.UserName });
            
            var walletLookup = await _context.UserWallets
                .AsNoTracking()
                .Where(w => lookupIds.Contains(w.UserId))
                .ToDictionaryAsync(w => w.UserId, w => w.UserPoint);

            var records = items.Select(h =>
            {
                userLookup.TryGetValue(h.UserId, out var userData);
                walletLookup.TryGetValue(h.UserId, out var balance);
                
                return new WalletHistoryRecord
                {
                    LogId = h.LogId,
                    UserId = h.UserId,
                    UserAccount = userData?.UserAccount ?? string.Empty,
                    UserName = userData?.UserName ?? string.Empty,
                    ChangeType = h.ChangeType,
                    PointsChanged = h.PointsChanged,
                    BalanceAfter = balance,
                    Description = h.Description,
                    ChangeTime = h.ChangeTime
                };
            }).ToList();

            var model = new WalletHistoryViewModel
            {
                Query = query,
                Results = new PagedResult<WalletHistoryRecord>
                {
                    Items = records,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GrantPoints()
        {
            var candidates = await _context.UserWallets
                .AsNoTracking()
                .OrderByDescending(w => w.UserPoint)
                .Take(20)
                .ToListAsync();

            var ids = candidates.Select(c => c.UserId).Distinct().ToList();
            var userLookup = await _context.Users
                .AsNoTracking()
                .Where(u => ids.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.UserAccount, u.UserName });
            var introduceLookup = await _context.UserIntroduces
                .AsNoTracking()
                .Where(ui => ids.Contains(ui.UserId))
                .ToDictionaryAsync(ui => ui.UserId, ui => ui.Email);

            var model = new WalletGrantPointsViewModel
            {
                Candidates = candidates.Select(w =>
                {
                    userLookup.TryGetValue(w.UserId, out var u);
                    introduceLookup.TryGetValue(w.UserId, out var email);
                    return new WalletPointRecord
                    {
                        UserId = w.UserId,
                        UserAccount = u?.UserAccount ?? string.Empty,
                        UserName = u?.UserName ?? string.Empty,
                        Email = email ?? string.Empty,
                        Points = w.UserPoint
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantPoints(int userId, int pointsToGrant, string reason)
        {
            if (userId < 0)
            {
                TempData["Error"] = "會員 ID 不可為負數";
                return RedirectToAction(nameof(GrantPoints));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "請填寫發放原因";
                return RedirectToAction(nameof(GrantPoints));
            }

            var wallet = await _context.UserWallets.FindAsync(userId);
            if (wallet == null)
            {
                TempData["Error"] = "找不到該會員的錢包";
                return RedirectToAction(nameof(GrantPoints));
            }

            if (pointsToGrant < 0 && wallet.UserPoint + pointsToGrant < 0)
            {
                TempData["Error"] = $"會員點數不足。目前點數：{wallet.UserPoint}，欲扣除：{-pointsToGrant}";
                return RedirectToAction(nameof(GrantPoints));
            }

            if (pointsToGrant == 0)
            {
                TempData["Success"] = "未變更點數（輸入為 0）";
                return RedirectToAction(nameof(GrantPoints));
            }

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 更新會員點數（可正負）
                wallet.UserPoint += pointsToGrant;

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = pointsToGrant,
                    Description = reason,
                    ChangeTime = _appClock.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (pointsToGrant > 0)
                {
                    TempData["Success"] = $"成功發放 {pointsToGrant} 點數給會員 ID {userId}";
                }
                else
                {
                    TempData["Success"] = $"成功扣除會員 ID {userId} 的 {-pointsToGrant} 點數。剩餘點數：{wallet.UserPoint}";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"調整點數失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(GrantPoints));
        }

        [HttpGet]
        public async Task<IActionResult> GrantCoupon()
        {
            var summaries = await _context.CouponTypes
                .AsNoTracking()
                .Select(ct => new CouponTypeSummary
                {
                    CouponTypeId = ct.CouponTypeId,
                    TypeName = ct.Name,
                    Description = ct.Description,
                    TotalIssued = ct.Coupons.Count,
                    UnusedCount = ct.Coupons.Count(c => !c.IsUsed)
                })
                .OrderBy(ct => ct.TypeName)
                .ToListAsync();

            var model = new WalletGrantCouponViewModel
            {
                CouponTypes = summaries
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantCoupon(int userId, int couponTypeId)
        {
            // 驗證會員存在
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                TempData["Error"] = $"找不到會員 ID {userId}";
                return RedirectToAction(nameof(GrantCoupon));
            }

            // 驗證優惠券類型存在
            var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
            if (couponType == null)
            {
                TempData["Error"] = "找不到該優惠券類型";
                return RedirectToAction(nameof(GrantCoupon));
            }

            // 生成優惠券序號 CPN-YYYYMM-XXXXXX
            var now = _appClock.UtcNow;
            var random = new Random();
            var randomCode = new string(Enumerable.Range(0, 6)
                .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[random.Next(36)])
                .ToArray());
            var couponCode = $"CPN-{now:yyMM}-{randomCode}";

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 創建優惠券
                var coupon = new Coupon
                {
                    CouponCode = couponCode,
                    CouponTypeId = couponTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = now
                };
                _context.Coupons.Add(coupon);

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon",
                    PointsChanged = 0,
                    ItemCode = couponCode,
                    Description = $"發放商城優惠券：{couponType.Name}",
                    ChangeTime = now
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"成功發放優惠券「{couponType.Name}」給會員 ID {userId}，序號：{couponCode}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"發放優惠券失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(GrantCoupon));
        }

        [HttpGet]
        public async Task<IActionResult> AdjustEVoucher()
        {
            var summaries = await _context.EvoucherTypes
                .AsNoTracking()
                .Select(et => new EVoucherTypeSummary
                {
                    EVoucherTypeId = et.EvoucherTypeId,
                    TypeName = et.Name,
                    MerchantName = et.Description,
                    VoucherValue = et.ValueAmount,
                    TotalIssued = et.Evouchers.Count,
                    UnusedCount = et.Evouchers.Count(ev => !ev.IsUsed)
                })
                .OrderBy(et => et.TypeName)
                .ToListAsync();

            var model = new WalletAdjustEVoucherViewModel
            {
                EVoucherTypes = summaries
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustEVoucher(int userId, int evoucherTypeId, string action)
        {
            // 驗證會員存在
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                TempData["Error"] = $"找不到會員 ID {userId}";
                return RedirectToAction(nameof(AdjustEVoucher));
            }

            // 驗證電子禮券類型存在
            var evoucherType = await _context.EvoucherTypes.FindAsync(evoucherTypeId);
            if (evoucherType == null)
            {
                TempData["Error"] = "找不到該電子禮券類型";
                return RedirectToAction(nameof(AdjustEVoucher));
            }

            var now = _appClock.UtcNow;

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (action == "grant")
                {
                    // 發放電子禮券 - 生成序號 EV-{類型}-{4位隨機碼}-{6位數字}
                    var random = new Random();
                    var randomCode = new string(Enumerable.Range(0, 4)
                        .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)])
                        .ToArray());
                    var randomDigits = random.Next(0, 1000000).ToString("D6");

                    // 從類型名稱提取類型代碼
                    string typeCode = evoucherType.Name.Contains("現金") ? "CASH" :
                                      evoucherType.Name.Contains("電影") ? "MOVIE" :
                                      evoucherType.Name.Contains("餐") ? "FOOD" :
                                      evoucherType.Name.Contains("加油") ? "GAS" :
                                      evoucherType.Name.Contains("咖啡") ? "COFFEE" : "STORE";

                    var evoucherCode = $"EV-{typeCode}-{randomCode}-{randomDigits}";

                    // 創建電子禮券
                    var evoucher = new Evoucher
                    {
                        EvoucherCode = evoucherCode,
                        EvoucherTypeId = evoucherTypeId,
                        UserId = userId,
                        IsUsed = false,
                        AcquiredTime = now
                    };
                    _context.Evouchers.Add(evoucher);

                    // 記錄異動歷史
                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "EVoucher",
                        PointsChanged = 0,
                        ItemCode = evoucherCode,
                        Description = $"發放電子禮券：{evoucherType.Name}",
                        ChangeTime = now
                    };
                    _context.WalletHistories.Add(history);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"成功發放電子禮券「{evoucherType.Name}」給會員 ID {userId}，序號：{evoucherCode}";
                }
                else if (action == "revoke")
                {
                    // 撤銷電子禮券 - 找到該會員最近一張未使用的該類型禮券
                    var evoucherToRevoke = await _context.Evouchers
                        .Where(ev => ev.UserId == userId && ev.EvoucherTypeId == evoucherTypeId && !ev.IsUsed)
                        .OrderByDescending(ev => ev.AcquiredTime)
                        .FirstOrDefaultAsync();

                    if (evoucherToRevoke == null)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "找不到可撤銷的電子禮券";
                        return RedirectToAction(nameof(AdjustEVoucher));
                    }

                    _context.Evouchers.Remove(evoucherToRevoke);

                    // 記錄異動歷史
                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "EVoucher",
                        PointsChanged = 0,
                        ItemCode = evoucherToRevoke.EvoucherCode,
                        Description = $"撤銷電子禮券：{evoucherType.Name}",
                        ChangeTime = now
                    };
                    _context.WalletHistories.Add(history);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"成功撤銷會員 ID {userId} 的電子禮券「{evoucherType.Name}」，序號：{evoucherToRevoke.EvoucherCode}";
                }
                else
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "無效的操作";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"操作電子禮券失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(AdjustEVoucher));
        }

        [HttpGet]
        public async Task<IActionResult> DeductPoints()
        {
            var candidates = await _context.UserWallets
                .AsNoTracking()
                .Where(w => w.UserPoint > 0) // 只顯示有點數的會員
                .OrderByDescending(w => w.UserPoint)
                .Take(20)
                .ToListAsync();

            var ids2 = candidates.Select(c => c.UserId).Distinct().ToList();
            var userLookup2 = await _context.Users
                .AsNoTracking()
                .Where(u => ids2.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.UserAccount, u.UserName });
            var introduceLookup2 = await _context.UserIntroduces
                .AsNoTracking()
                .Where(ui => ids2.Contains(ui.UserId))
                .ToDictionaryAsync(ui => ui.UserId, ui => ui.Email);

            var model = new WalletDeductPointsViewModel
            {
                Candidates = candidates.Select(w =>
                {
                    userLookup2.TryGetValue(w.UserId, out var u);
                    introduceLookup2.TryGetValue(w.UserId, out var email);
                    return new WalletPointRecord
                    {
                        UserId = w.UserId,
                        UserAccount = u?.UserAccount ?? string.Empty,
                        UserName = u?.UserName ?? string.Empty,
                        Email = email ?? string.Empty,
                        Points = w.UserPoint
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeductPoints(int userId, int pointsToDeduct, string reason)
        {
            if (pointsToDeduct <= 0)
            {
                TempData["Error"] = "扣除點數必須大於 0";
                return RedirectToAction(nameof(DeductPoints));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "請填寫扣除原因";
                return RedirectToAction(nameof(DeductPoints));
            }

            var wallet = await _context.UserWallets.FindAsync(userId);
            if (wallet == null)
            {
                TempData["Error"] = "找不到該會員的錢包";
                return RedirectToAction(nameof(DeductPoints));
            }

            if (wallet.UserPoint < pointsToDeduct)
            {
                TempData["Error"] = $"會員點數不足。目前點數：{wallet.UserPoint}，欲扣除：{pointsToDeduct}";
                return RedirectToAction(nameof(DeductPoints));
            }

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 扣除會員點數
                wallet.UserPoint -= pointsToDeduct;

                // 記錄異動歷史（負值）
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = -pointsToDeduct, // 負值表示扣除
                    Description = reason,
                    ChangeTime = _appClock.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"成功扣除會員 ID {userId} 的 {pointsToDeduct} 點數。剩餘點數：{wallet.UserPoint}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"扣除點數失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(DeductPoints));
        }

        [HttpGet]
        public async Task<IActionResult> RevokeCoupon()
        {
            // 取得最近發放的優惠券（未使用）
            var recentCoupons = await _context.Coupons
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .Where(c => !c.IsUsed)
                .OrderByDescending(c => c.AcquiredTime)
                .Take(50)
                .ToListAsync();

            var records = recentCoupons.Select(c => new UserCouponReadModel
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

            var model = new WalletRevokeCouponViewModel
            {
                UnusedCoupons = records
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeCoupon(int couponId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "請填寫撤銷原因";
                return RedirectToAction(nameof(RevokeCoupon));
            }

            var coupon = await _context.Coupons
                .Include(c => c.CouponType)
                .FirstOrDefaultAsync(c => c.CouponId == couponId);

            if (coupon == null)
            {
                TempData["Error"] = "找不到該優惠券";
                return RedirectToAction(nameof(RevokeCoupon));
            }

            if (coupon.IsUsed)
            {
                TempData["Error"] = "該優惠券已被使用，無法撤銷";
                return RedirectToAction(nameof(RevokeCoupon));
            }

            var couponCode = coupon.CouponCode;
            var userId = coupon.UserId;
            var couponTypeName = coupon.CouponType?.Name ?? "未知類型";

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 刪除優惠券
                _context.Coupons.Remove(coupon);

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon",
                    PointsChanged = 0,
                    ItemCode = couponCode,
                    Description = $"撤銷商城優惠券：{couponTypeName}。原因：{reason}",
                    ChangeTime = _appClock.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"成功撤銷會員 ID {userId} 的優惠券「{couponTypeName}」，序號：{couponCode}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"撤銷優惠券失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(RevokeCoupon));
        }
    }
}
