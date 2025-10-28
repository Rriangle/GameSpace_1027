using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminManagerController : MiniGameBaseController
    {
        public AdminManagerController(GameSpacedatabaseContext context)
            : base(context)
        {
        }

        // GET: AdminManager
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string role = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Max(pageSize, 1);

            var query = _context.ManagerData
                .Include(m => m.ManagerRoles)
                .AsQueryable();

            int? selectedRoleId = null;

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(m => (m.ManagerName != null && m.ManagerName.Contains(searchTerm)) ||
                                       (m.ManagerAccount != null && m.ManagerAccount.Contains(searchTerm)) ||
                                       m.ManagerEmail.Contains(searchTerm));
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active")
                    query = query.Where(m => !m.ManagerLockoutEnabled || !m.ManagerLockoutEnd.HasValue || m.ManagerLockoutEnd <= DateTime.Now);
                else if (status == "locked")
                    query = query.Where(m => m.ManagerLockoutEnabled && m.ManagerLockoutEnd.HasValue && m.ManagerLockoutEnd > DateTime.Now);
            }

            // 角色篩選
            if (!string.IsNullOrEmpty(role) && int.TryParse(role, out int roleId))
            {
                selectedRoleId = roleId;
                query = query.Where(m => m.ManagerRoles.Any(mr => mr.ManagerRoleId == roleId));
            }

            // 排序
            query = sortBy switch
            {
                "account" => query.OrderBy(m => m.ManagerAccount),
                "email" => query.OrderBy(m => m.ManagerEmail),
                "created" => query.OrderBy(m => m.AdministratorRegistrationDate),
                _ => query.OrderBy(m => m.ManagerName)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var managers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var activeManagers = await _context.ManagerData.CountAsync(m => !m.ManagerLockoutEnabled || !m.ManagerLockoutEnd.HasValue || m.ManagerLockoutEnd <= DateTime.Now);
            var roles = await _context.ManagerRolePermissions
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            var viewModel = new Models.ViewModels.AdminManagerIndexViewModel
            {
                Managers = managers,
                Roles = roles,
                SearchTerm = searchTerm,
                Status = status,
                SelectedRoleId = selectedRoleId,
                SortBy = sortBy,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                ActiveManagers = activeManagers,
                TotalRoles = roles.Count,
                TodayLogins = 0
            };

            return View(viewModel);
        }

        // GET: AdminManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == id);

            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        // GET: AdminManager/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _context.ManagerRolePermissions
                .OrderBy(r => r.RoleName)
                .ToListAsync();
            return View();
        }

        // POST: AdminManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string managerName, string managerAccount, string managerEmail, string password, List<int> roleIds)
        {
            if (ModelState.IsValid)
            {
                // 檢查帳號是否已存在
                if (await _context.ManagerData.AnyAsync(m => m.ManagerAccount == managerAccount))
                {
                    ModelState.AddModelError("managerAccount", "此帳號已存在");
                    ViewBag.Roles = await _context.ManagerRolePermissions
                        .OrderBy(r => r.RoleName)
                        .ToListAsync();
                    return View();
                }

                // 檢查電子郵件是否已存在
                if (await _context.ManagerData.AnyAsync(m => m.ManagerEmail == managerEmail))
                {
                    ModelState.AddModelError("managerEmail", "此電子郵件已存在");
                    ViewBag.Roles = await _context.ManagerRolePermissions
                        .OrderBy(r => r.RoleName)
                        .ToListAsync();
                    return View();
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var manager = new ManagerDatum
                    {
                        ManagerName = managerName,
                        ManagerAccount = managerAccount,
                        ManagerPassword = HashPassword(password),
                        ManagerEmail = managerEmail,
                        AdministratorRegistrationDate = DateTime.Now,
                        ManagerLockoutEnabled = false,
                        ManagerLockoutEnd = null,
                        ManagerEmailConfirmed = false,
                        ManagerAccessFailedCount = 0
                    };

                    _context.Add(manager);
                    await _context.SaveChangesAsync();

                    // 分配角色 - 需要使用正確的關聯表
                    if (roleIds != null && roleIds.Any())
                    {
                        foreach (var roleId in roleIds)
                        {
                            var rolePermission = await _context.ManagerRolePermissions.FindAsync(roleId);
                            if (rolePermission != null)
                            {
                                manager.ManagerRoles.Add(rolePermission);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "管理者建立成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"建立管理者失敗：{ex.Message}";
                }
            }

            ViewBag.Roles = await _context.ManagerRolePermissions
                .OrderBy(r => r.RoleName)
                .ToListAsync();
            return View();
        }

        // GET: AdminManager/CreateRole
        public IActionResult CreateRole()
        {
            // ManagerRolePermission 表已經包含所有角色和權限，無需額外的 ManagerPermission 表
            return View();
        }

        // POST: AdminManager/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName, bool? administratorPrivilegesManagement, bool? userStatusManagement,
            bool? shoppingPermissionManagement, bool? messagePermissionManagement, bool? petRightsManagement, bool? customerService)
        {
            if (ModelState.IsValid)
            {
                var role = new ManagerRolePermission
                {
                    RoleName = roleName,
                    AdministratorPrivilegesManagement = administratorPrivilegesManagement,
                    UserStatusManagement = userStatusManagement,
                    ShoppingPermissionManagement = shoppingPermissionManagement,
                    MessagePermissionManagement = messagePermissionManagement,
                    PetRightsManagement = petRightsManagement,
                    CustomerService = customerService
                };

                _context.Add(role);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "角色建立成功";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // GET: AdminManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == id);

            if (manager == null)
            {
                return NotFound();
            }

            ViewBag.Manager = manager;
            ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
            return View();
        }

        // POST: AdminManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string managerName, string managerAccount, string managerEmail,
            string? newPassword, string? confirmNewPassword, List<int> roleIds)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var manager = await _context.ManagerData
                        .Include(m => m.ManagerRoles)
                        .FirstOrDefaultAsync(m => m.ManagerId == id);

                    if (manager == null)
                    {
                        return NotFound();
                    }

                    // 檢查帳號是否已被其他管理者使用
                    if (await _context.ManagerData.AnyAsync(m => m.ManagerAccount == managerAccount && m.ManagerId != id))
                    {
                        ModelState.AddModelError("managerAccount", "此帳號已被其他管理者使用");
                        ViewBag.Manager = manager;
                        ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
                        return View();
                    }

                    // 檢查電子郵件是否已被其他管理者使用
                    if (await _context.ManagerData.AnyAsync(m => m.ManagerEmail == managerEmail && m.ManagerId != id))
                    {
                        ModelState.AddModelError("managerEmail", "此電子郵件已被其他管理者使用");
                        ViewBag.Manager = manager;
                        ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
                        return View();
                    }

                    manager.ManagerName = managerName;
                    manager.ManagerAccount = managerAccount;
                    manager.ManagerEmail = managerEmail;

                    // 如果提供了新密碼，則更新密碼
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        if (newPassword != confirmNewPassword)
                        {
                            ModelState.AddModelError("confirmNewPassword", "新密碼和確認密碼不匹配");
                            ViewBag.Manager = manager;
                            ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
                            return View();
                        }
                        manager.ManagerPassword = HashPassword(newPassword);
                    }

                    // 更新角色分配 - 清除現有並重新建立
                    manager.ManagerRoles.Clear();
                    if (roleIds != null && roleIds.Any())
                    {
                        var roles = await _context.ManagerRolePermissions
                            .Where(r => roleIds.Contains(r.ManagerRoleId))
                            .ToListAsync();

                        foreach (var role in roles)
                        {
                            manager.ManagerRoles.Add(role);
                        }
                    }

                    _context.Update(manager);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "管理者更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManagerExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var mgr = await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == id);
            ViewBag.Manager = mgr;
            ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
            return View();
        }

        // GET: AdminManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == id);

            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        // POST: AdminManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manager = await _context.ManagerData.FindAsync(id);
            if (manager != null)
            {
                _context.ManagerData.Remove(manager);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "管理者刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換管理者狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var manager = await _context.ManagerData.FindAsync(id);
            if (manager != null)
            {
                if (manager.ManagerLockoutEnabled && manager.ManagerLockoutEnd.HasValue && manager.ManagerLockoutEnd > DateTime.Now)
                {
                    // 解鎖
                    manager.ManagerLockoutEnabled = false;
                    manager.ManagerLockoutEnd = null;
                }
                else
                {
                    // 鎖定
                    manager.ManagerLockoutEnabled = true;
                    manager.ManagerLockoutEnd = DateTime.Now.AddDays(30);
                }

                _context.Update(manager);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isLocked = manager.ManagerLockoutEnabled });
            }

            return Json(new { success = false });
        }

        // 重置管理者密碼
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            var manager = await _context.ManagerData.FindAsync(id);
            if (manager != null)
            {
                manager.ManagerPassword = HashPassword(newPassword);
                _context.Update(manager);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // 獲取管理者統計數據
        [HttpGet]
        public async Task<IActionResult> GetManagerStats()
        {
            var now = DateTime.Now;
            var stats = new
            {
                total = await _context.ManagerData.CountAsync(),
                active = await _context.ManagerData.CountAsync(m => !m.ManagerLockoutEnabled || !m.ManagerLockoutEnd.HasValue || m.ManagerLockoutEnd <= now),
                locked = await _context.ManagerData.CountAsync(m => m.ManagerLockoutEnabled && m.ManagerLockoutEnd.HasValue && m.ManagerLockoutEnd > now),
                roles = await _context.ManagerRolePermissions.CountAsync()
            };

            return Json(stats);
        }

        // 獲取角色分佈
        [HttpGet]
        public async Task<IActionResult> GetRoleDistribution()
        {
            var distribution = await _context.ManagerData
                .SelectMany(m => m.ManagerRoles)
                .GroupBy(r => r.RoleName)
                .Select(g => new
                {
                    role = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToListAsync();

            return Json(distribution);
        }

        private bool ManagerExists(int id)
        {
            return _context.ManagerData.Any(e => e.ManagerId == id);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}



