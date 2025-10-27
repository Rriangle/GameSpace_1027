using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminCouponController : MiniGameBaseController
    {
        public AdminCouponController(GameSpacedatabaseContext context) : base(context)
        {
        }

        // GET: AdminCoupon
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.CouponCode.Contains(searchTerm) || c.User.UserName.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "used")
                    query = query.Where(c => c.IsUsed);
                else if (status == "unused")
                    query = query.Where(c => !c.IsUsed);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminWalletIndexViewModel
            {
                Coupons = new PagedResult<UserCouponModel>
                {
                    Items = items.Select(c => new UserCouponModel
                    {
                        CouponID = c.CouponId,
                        CouponCode = c.CouponCode,
                        UserID = c.UserId,
                        UserName = c.User?.UserName ?? "Unknown",
                        CouponTypeID = c.CouponTypeId,
                        CouponTypeName = c.CouponType?.Name ?? "Unknown",
                        IsUsed = c.IsUsed,
                        AcquiredTime = c.AcquiredTime,
                        UsedTime = c.UsedTime,
                        UsedInOrderID = c.UsedInOrderId
                    }).ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                },
                CouponQuery = new CouponQueryModel
                {
                    SearchTerm = searchTerm,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                }
            };

            return View(viewModel);
        }

        // GET: AdminCoupon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CouponId == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        // GET: AdminCoupon/Create
        public IActionResult Create()
        {
            return View(new AdminCouponCreateViewModel());
        }

        // POST: AdminCoupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,CouponTypeId,CouponCode,CouponName,DiscountType,DiscountValue,MinOrderAmount,MaxDiscountAmount,StartDate,EndDate,UsageLimit,IsActive,Description")] AdminCouponCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Coupons.AnyAsync(c => c.CouponCode == model.CouponCode))
                {
                    ModelState.AddModelError("CouponCode", "此優惠券代碼已存在");
                    return View(model);
                }

                var coupon = new Coupon
                {
                    CouponCode = model.CouponCode,
                    UserId = model.UserId,
                    CouponTypeId = model.CouponTypeId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now,
                    UsedTime = null,
                    UsedInOrderId = null
                };

                _context.Add(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "優惠券建立成功";
                return RedirectToAction(nameof(Index));
            }
            
            return View(model);
        }

        // GET: AdminCoupon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }
            
            // 轉換為 SelectListItem 格式
            ViewData["CouponTypeId"] = _context.CouponTypes.Select(ct => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = ct.CouponTypeId.ToString(),
                Text = ct.Name
            }).ToList();
            
            ViewData["UserId"] = _context.Users.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = u.UserName
            }).ToList();
            
            return View(coupon);
        }

        // POST: AdminCoupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CouponId,CouponCode,UserId,CouponTypeId,IsUsed,AcquiredTime,UsedTime,UsedInOrderId")] Coupon coupon)
        {
            if (id != coupon.CouponId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Coupons.AnyAsync(c => c.CouponCode == coupon.CouponCode && c.CouponId != id))
                    {
                        ModelState.AddModelError("CouponCode", "此優惠券代碼已被其他優惠券使用");
                        return View(coupon);
                    }

                    _context.Update(coupon);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "優惠券更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponExists(coupon.CouponId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            // 轉換為 SelectListItem 格式
            ViewData["CouponTypeId"] = _context.CouponTypes.Select(ct => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = ct.CouponTypeId.ToString(),
                Text = ct.Name
            }).ToList();
            
            ViewData["UserId"] = _context.Users.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = u.UserName
            }).ToList();
            
            return View(coupon);
        }

        // GET: AdminCoupon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CouponId == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        // POST: AdminCoupon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "優惠券刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.CouponId == id);
        }

        #region CouponType Management

        // GET: AdminCoupon/CouponTypes
        public async Task<IActionResult> CouponTypes(string searchTerm = "", string discountType = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.CouponTypes.AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(ct => ct.Name.Contains(searchTerm) || (ct.Description != null && ct.Description.Contains(searchTerm)));
            }

            // 折扣類型篩選
            if (!string.IsNullOrEmpty(discountType))
            {
                query = query.Where(ct => ct.DiscountType == discountType);
            }

            // 排序
            query = sortBy switch
            {
                "points" => query.OrderBy(ct => ct.PointsCost),
                "discount" => query.OrderByDescending(ct => ct.DiscountValue),
                "validfrom" => query.OrderBy(ct => ct.ValidFrom),
                _ => query.OrderBy(ct => ct.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new
            {
                CouponTypes = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                DiscountType = discountType,
                SortBy = sortBy
            };

            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(viewModel);
        }

        // GET: AdminCoupon/CreateCouponType
        public IActionResult CreateCouponType()
        {
            ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
            return View();
        }

        // POST: AdminCoupon/CreateCouponType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCouponType([Bind("CouponTypeId,Name,DiscountType,DiscountValue,MinSpend,ValidFrom,ValidTo,PointsCost,Description")] CouponType couponType)
        {
            if (ModelState.IsValid)
            {
                // 驗證邏輯
                if (couponType.ValidFrom >= couponType.ValidTo)
                {
                    ModelState.AddModelError("ValidFrom", "開始日期必須早於結束日期");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.DiscountType == "Percentage" && (couponType.DiscountValue < 0 || couponType.DiscountValue > 100))
                {
                    ModelState.AddModelError("DiscountValue", "百分比折扣必須在 0-100 之間");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.DiscountValue < 0)
                {
                    ModelState.AddModelError("DiscountValue", "折扣值不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.MinSpend < 0)
                {
                    ModelState.AddModelError("MinSpend", "最低消費金額不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.PointsCost < 0)
                {
                    ModelState.AddModelError("PointsCost", "所需點數不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                _context.Add(couponType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "優惠券類型建立成功";
                return RedirectToAction(nameof(CouponTypes));
            }

            ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
            return View(couponType);
        }

        // GET: AdminCoupon/EditCouponType/5
        public async Task<IActionResult> EditCouponType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var couponType = await _context.CouponTypes.FindAsync(id);
            if (couponType == null)
            {
                return NotFound();
            }

            ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
            return View(couponType);
        }

        // POST: AdminCoupon/EditCouponType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCouponType(int id, [Bind("CouponTypeId,Name,DiscountType,DiscountValue,MinSpend,ValidFrom,ValidTo,PointsCost,Description")] CouponType couponType)
        {
            if (id != couponType.CouponTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 驗證邏輯
                if (couponType.ValidFrom >= couponType.ValidTo)
                {
                    ModelState.AddModelError("ValidFrom", "開始日期必須早於結束日期");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.DiscountType == "Percentage" && (couponType.DiscountValue < 0 || couponType.DiscountValue > 100))
                {
                    ModelState.AddModelError("DiscountValue", "百分比折扣必須在 0-100 之間");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.DiscountValue < 0)
                {
                    ModelState.AddModelError("DiscountValue", "折扣值不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.MinSpend < 0)
                {
                    ModelState.AddModelError("MinSpend", "最低消費金額不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.PointsCost < 0)
                {
                    ModelState.AddModelError("PointsCost", "所需點數不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                try
                {
                    _context.Update(couponType);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "優惠券類型更新成功";
                    return RedirectToAction(nameof(CouponTypes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponTypeExists(couponType.CouponTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
            return View(couponType);
        }

        // GET: AdminCoupon/DeleteCouponType/5
        public async Task<IActionResult> DeleteCouponType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var couponType = await _context.CouponTypes
                .FirstOrDefaultAsync(m => m.CouponTypeId == id);

            if (couponType == null)
            {
                return NotFound();
            }

            // 檢查是否有相關的優惠券
            var relatedCouponsCount = await _context.Coupons.CountAsync(c => c.CouponTypeId == id);
            ViewBag.RelatedCouponsCount = relatedCouponsCount;

            return View(couponType);
        }

        // POST: AdminCoupon/DeleteCouponType/5
        [HttpPost, ActionName("DeleteCouponType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCouponTypeConfirmed(int id)
        {
            try
            {
                // 檢查是否有相關的優惠券
                var relatedCouponsCount = await _context.Coupons.CountAsync(c => c.CouponTypeId == id);
                
                var couponType = await _context.CouponTypes.FindAsync(id);
                if (couponType == null)
                {
                    TempData["ErrorMessage"] = "找不到要刪除的優惠券類型";
                    return RedirectToAction(nameof(CouponTypes));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 如果有相關優惠券，先刪除相關的優惠券
                    if (relatedCouponsCount > 0)
                    {
                        var relatedCoupons = await _context.Coupons.Where(c => c.CouponTypeId == id).ToListAsync();
                        _context.Coupons.RemoveRange(relatedCoupons);
                        
                        TempData["WarningMessage"] = $"已刪除 {relatedCouponsCount} 個相關優惠券";
                    }

                    // 刪除優惠券類型
                    _context.CouponTypes.Remove(couponType);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "優惠券類型刪除成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(CouponTypes));
        }

        // GET: AdminCoupon/CouponTypeDetails/5
        public async Task<IActionResult> CouponTypeDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var couponType = await _context.CouponTypes
                .FirstOrDefaultAsync(m => m.CouponTypeId == id);

            if (couponType == null)
            {
                return NotFound();
            }

            // 取得使用此類型的優惠券統計
            var relatedCoupons = await _context.Coupons
                .Where(c => c.CouponTypeId == id)
                .ToListAsync();

            ViewBag.TotalCoupons = relatedCoupons.Count;
            ViewBag.UsedCoupons = relatedCoupons.Count(c => c.IsUsed);
            ViewBag.UnusedCoupons = relatedCoupons.Count(c => !c.IsUsed);

            return View(couponType);
        }

        private bool CouponTypeExists(int id)
        {
            return _context.CouponTypes.Any(e => e.CouponTypeId == id);
        }

        #endregion
    }
}

