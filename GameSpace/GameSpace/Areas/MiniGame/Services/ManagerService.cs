using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Areas.MiniGame.Services
{
    public class ManagerService : IManagerService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<ManagerService> _logger;

        public ManagerService(GameSpacedatabaseContext context, ILogger<ManagerService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Manager 基本 CRUD

        /// <summary>
        /// 取得所有管理員列表（分頁）
        /// </summary>
        public async Task<IEnumerable<ManagerDatum>> GetAllManagersAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.ManagerData
                    .AsNoTracking()
                    .Include(m => m.ManagerRoles)
                    .OrderByDescending(m => m.AdministratorRegistrationDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得管理員列表時發生錯誤，頁碼: {PageNumber}, 頁面大小: {PageSize}", pageNumber, pageSize);
                return new List<ManagerDatum>();
            }
        }

        /// <summary>
        /// 根據管理員 ID 取得管理員資料
        /// </summary>
        public async Task<ManagerDatum?> GetManagerByIdAsync(int managerId)
        {
            try
            {
                return await _context.ManagerData
                    .AsNoTracking()
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得管理員資料時發生錯誤，管理員ID: {ManagerId}", managerId);
                return null;
            }
        }

        /// <summary>
        /// 根據帳號取得管理員資料
        /// </summary>
        public async Task<ManagerDatum?> GetManagerByAccountAsync(string account)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(account))
                    return null;

                return await _context.ManagerData
                    .AsNoTracking()
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerAccount == account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據帳號取得管理員資料時發生錯誤，帳號: {Account}", account);
                return null;
            }
        }

        /// <summary>
        /// 根據電子郵件取得管理員資料
        /// </summary>
        public async Task<ManagerDatum?> GetManagerByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                return await _context.ManagerData
                    .AsNoTracking()
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerEmail == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據電子郵件取得管理員資料時發生錯誤，電子郵件: {Email}", email);
                return null;
            }
        }

        /// <summary>
        /// 建立新管理員
        /// </summary>
        public async Task<bool> CreateManagerAsync(ManagerDatum manager, string password)
        {
            try
            {
                // 驗證必填欄位
                if (string.IsNullOrWhiteSpace(manager.ManagerAccount))
                    return false;

                if (string.IsNullOrWhiteSpace(password))
                    return false;

                // 檢查帳號是否已存在
                var existingManager = await GetManagerByAccountAsync(manager.ManagerAccount);
                if (existingManager != null)
                    return false;

                // 檢查電子郵件是否已存在
                if (!string.IsNullOrWhiteSpace(manager.ManagerEmail))
                {
                    var existingEmail = await GetManagerByEmailAsync(manager.ManagerEmail);
                    if (existingEmail != null)
                        return false;
                }

                // 設定密碼雜湊
                manager.ManagerPassword = HashPassword(password);

                // 設定預設值
                manager.AdministratorRegistrationDate = DateTime.UtcNow;
                manager.ManagerEmailConfirmed = false;
                manager.ManagerAccessFailedCount = 0;
                manager.ManagerLockoutEnabled = true;
                manager.ManagerLockoutEnd = null;

                _context.ManagerData.Add(manager);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立管理員時發生錯誤，帳號: {Account}", manager.ManagerAccount);
                return false;
            }
        }

        /// <summary>
        /// 更新管理員資料
        /// </summary>
        public async Task<bool> UpdateManagerAsync(ManagerDatum manager)
        {
            try
            {
                if (manager == null || manager.ManagerId <= 0)
                    return false;

                // 確認管理員存在
                var existingManager = await GetManagerByIdAsync(manager.ManagerId);
                if (existingManager == null)
                    return false;

                // 如果更改帳號，檢查新帳號是否已被使用
                if (existingManager.ManagerAccount != manager.ManagerAccount)
                {
                    var accountExists = await _context.ManagerData
                        .AnyAsync(m => m.ManagerAccount == manager.ManagerAccount && m.ManagerId != manager.ManagerId);
                    if (accountExists)
                        return false;
                }

                // 如果更改電子郵件，檢查新電子郵件是否已被使用
                if (existingManager.ManagerEmail != manager.ManagerEmail)
                {
                    var emailExists = await _context.ManagerData
                        .AnyAsync(m => m.ManagerEmail == manager.ManagerEmail && m.ManagerId != manager.ManagerId);
                    if (emailExists)
                        return false;
                }

                _context.Entry(existingManager).CurrentValues.SetValues(manager);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新管理員資料時發生錯誤，管理員ID: {ManagerId}", manager?.ManagerId);
                return false;
            }
        }

        /// <summary>
        /// 刪除管理員
        /// </summary>
        public async Task<bool> DeleteManagerAsync(int managerId)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null)
                    return false;

                _context.ManagerData.Remove(manager);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除管理員時發生錯誤，管理員ID: {ManagerId}", managerId);
                return false;
            }
        }

        #endregion

        #region Manager 認證

        /// <summary>
        /// 管理員登入驗證
        /// </summary>
        public async Task<ManagerDatum?> AuthenticateAsync(string account, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
                    return null;

                var manager = await GetManagerByAccountAsync(account);
                if (manager == null)
                    return null;

                // 檢查帳號是否被鎖定
                if (manager.ManagerLockoutEnabled &&
                    manager.ManagerLockoutEnd.HasValue &&
                    manager.ManagerLockoutEnd.Value > DateTime.UtcNow)
                {
                    return null;
                }

                // 驗證密碼
                if (string.IsNullOrEmpty(manager.ManagerPassword) ||
                    !VerifyPassword(password, manager.ManagerPassword))
                {
                    // 記錄失敗次數
                    manager.ManagerAccessFailedCount++;

                    // 連續失敗 5 次則鎖定帳號 30 分鐘
                    if (manager.ManagerAccessFailedCount >= 5)
                    {
                        manager.ManagerLockoutEnd = DateTime.UtcNow.AddMinutes(30);
                    }

                    await _context.SaveChangesAsync();
                    return null;
                }

                // 登入成功，重置失敗次數
                manager.ManagerAccessFailedCount = 0;
                manager.ManagerLockoutEnd = null;
                await _context.SaveChangesAsync();

                return manager;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "管理員認證時發生錯誤，帳號: {Account}", account);
                return null;
            }
        }

        /// <summary>
        /// 變更管理員密碼（需要提供舊密碼）
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int managerId, string oldPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                    return false;

                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null || string.IsNullOrEmpty(manager.ManagerPassword))
                    return false;

                // 驗證舊密碼
                if (!VerifyPassword(oldPassword, manager.ManagerPassword))
                    return false;

                // 設定新密碼
                manager.ManagerPassword = HashPassword(newPassword);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "變更密碼時發生錯誤，管理員ID: {ManagerId}", managerId);
                return false;
            }
        }

        /// <summary>
        /// 重設管理員密碼（不需要舊密碼，由超級管理員執行）
        /// </summary>
        public async Task<bool> ResetPasswordAsync(int managerId, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                    return false;

                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null)
                    return false;

                // 設定新密碼並重置相關狀態
                manager.ManagerPassword = HashPassword(newPassword);
                manager.ManagerAccessFailedCount = 0;
                manager.ManagerLockoutEnd = null;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重設密碼時發生錯誤，管理員ID: {ManagerId}", managerId);
                return false;
            }
        }

        #endregion

        #region Manager 狀態管理

        /// <summary>
        /// 鎖定管理員帳號
        /// </summary>
        public async Task<bool> LockManagerAsync(int managerId, DateTime? lockoutEnd = null)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null)
                    return false;

                manager.ManagerLockoutEnabled = true;
                manager.ManagerLockoutEnd = lockoutEnd ?? DateTime.UtcNow.AddDays(30);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "鎖定管理員帳號時發生錯誤，管理員ID: {ManagerId}", managerId);
                return false;
            }
        }

        /// <summary>
        /// 解除管理員帳號鎖定
        /// </summary>
        public async Task<bool> UnlockManagerAsync(int managerId)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null)
                    return false;

                manager.ManagerLockoutEnabled = false;
                manager.ManagerLockoutEnd = null;
                manager.ManagerAccessFailedCount = 0;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解除管理員帳號鎖定時發生錯誤，管理員ID: {ManagerId}", managerId);
                return false;
            }
        }

        /// <summary>
        /// 確認管理員電子郵件
        /// </summary>
        public async Task<bool> ConfirmEmailAsync(int managerId)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null)
                    return false;

                manager.ManagerEmailConfirmed = true;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "確認電子郵件時發生錯誤，管理員ID: {ManagerId}", managerId);
                return false;
            }
        }

        #endregion

        #region Manager 角色管理

        /// <summary>
        /// 取得管理員的角色權限
        /// </summary>
        public async Task<ManagerRolePermission?> GetManagerRoleAsync(int managerId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .AsNoTracking()
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null)
                    return null;

                // 返回第一個角色的權限（假設一個管理員只有一個角色）
                return manager.ManagerRoles.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得管理員角色時發生錯誤，管理員ID: {ManagerId}", managerId);
                return null;
            }
        }

        /// <summary>
        /// 指派角色給管理員
        /// </summary>
        public async Task<bool> AssignRoleAsync(int managerId, int roleId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null)
                    return false;

                // 檢查角色是否存在
                var role = await _context.ManagerRolePermissions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ManagerRoleId == roleId);

                if (role == null)
                    return false;

                // 檢查是否已經有此角色
                if (manager.ManagerRoles.Any(r => r.ManagerRoleId == roleId))
                    return true;

                // 清除現有角色（假設一個管理員只能有一個角色）
                manager.ManagerRoles.Clear();

                // 直接加入角色權限（因為是多對多關係）
                manager.ManagerRoles.Add(role);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "指派角色時發生錯誤，管理員ID: {ManagerId}, 角色ID: {RoleId}", managerId, roleId);
                return false;
            }
        }

        /// <summary>
        /// 移除管理員的角色
        /// </summary>
        public async Task<bool> RemoveRoleAsync(int managerId, int roleId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null)
                    return false;

                var roleToRemove = manager.ManagerRoles.FirstOrDefault(r => r.ManagerRoleId == roleId);
                if (roleToRemove == null)
                    return false;

                manager.ManagerRoles.Remove(roleToRemove);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除角色時發生錯誤，管理員ID: {ManagerId}, 角色ID: {RoleId}", managerId, roleId);
                return false;
            }
        }

        /// <summary>
        /// 取得管理員的所有權限列表
        /// </summary>
        public async Task<IEnumerable<string>> GetManagerPermissionsAsync(int managerId)
        {
            try
            {
                var role = await GetManagerRoleAsync(managerId);
                if (role == null)
                    return new List<string>();

                var permissions = new List<string>();

                // 根據角色的布林欄位轉換為權限字串
                if (role.AdministratorPrivilegesManagement == true)
                {
                    // 超級管理員擁有所有權限
                    permissions.Add("Administrator.Full");
                    permissions.Add("User.View");
                    permissions.Add("User.Edit");
                    permissions.Add("User.Delete");
                    permissions.Add("Wallet.View");
                    permissions.Add("Wallet.Edit");
                    permissions.Add("Coupon.View");
                    permissions.Add("Coupon.Edit");
                    permissions.Add("EVoucher.View");
                    permissions.Add("EVoucher.Edit");
                    permissions.Add("Pet.View");
                    permissions.Add("Pet.Edit");
                    permissions.Add("Message.View");
                    permissions.Add("Message.Edit");
                    permissions.Add("CustomerService");
                    permissions.Add("Manager.View");
                    permissions.Add("Manager.Edit");
                }
                else
                {
                    // 根據各個權限欄位添加對應權限
                    if (role.UserStatusManagement == true)
                    {
                        permissions.Add("User.View");
                        permissions.Add("User.Edit");
                    }

                    if (role.ShoppingPermissionManagement == true)
                    {
                        permissions.Add("Wallet.View");
                        permissions.Add("Wallet.Edit");
                        permissions.Add("Coupon.View");
                        permissions.Add("Coupon.Edit");
                        permissions.Add("EVoucher.View");
                        permissions.Add("EVoucher.Edit");
                    }

                    if (role.PetRightsManagement == true)
                    {
                        permissions.Add("Pet.View");
                        permissions.Add("Pet.Edit");
                    }

                    if (role.MessagePermissionManagement == true)
                    {
                        permissions.Add("Message.View");
                        permissions.Add("Message.Edit");
                    }

                    if (role.CustomerService == true)
                    {
                        permissions.Add("CustomerService");
                    }
                }

                return permissions.Distinct();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得管理員權限時發生錯誤，管理員ID: {ManagerId}", managerId);
                return new List<string>();
            }
        }

        /// <summary>
        /// 檢查管理員是否擁有特定權限
        /// </summary>
        public async Task<bool> HasPermissionAsync(int managerId, string permission)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permission))
                    return false;

                var permissions = await GetManagerPermissionsAsync(managerId);

                // 檢查是否有完整權限或超級管理員權限
                return permissions.Contains(permission) ||
                       permissions.Contains("Administrator.Full");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查管理員權限時發生錯誤，管理員ID: {ManagerId}, 權限: {Permission}", managerId, permission);
                return false;
            }
        }

        #endregion

        #region Manager 活動日誌

        /// <summary>
        /// 取得管理員的活動日誌（分頁）
        /// </summary>
        public async Task<IEnumerable<ManagerActivityLog>> GetManagerActivitiesAsync(int managerId, int pageNumber = 1, int pageSize = 20)
        {
            // 目前資料庫尚未建立活動日誌表，返回空列表
            // 未來實作時需要建立 ManagerActivityLog 資料表並實作此功能
            await Task.CompletedTask;
            return new List<ManagerActivityLog>();
        }

        /// <summary>
        /// 記錄管理員活動
        /// </summary>
        public async Task<bool> LogActivityAsync(int managerId, string action, string details)
        {
            try
            {
                // 目前資料庫尚未建立活動日誌表
                // 未來實作時需要建立 ManagerActivityLog 資料表並實作此功能
                //
                // 範例實作：
                // var log = new ManagerActivityLog
                // {
                //     ManagerId = managerId,
                //     Action = action,
                //     Details = details,
                //     IpAddress = "0.0.0.0", // 需要從 HttpContext 取得
                //     CreatedAt = DateTime.Now
                // };
                // _context.ManagerActivityLogs.Add(log);
                // await _context.SaveChangesAsync();

                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄管理員活動時發生錯誤，管理員ID: {ManagerId}, 動作: {Action}", managerId, action);
                return false;
            }
        }

        #endregion

        #region Manager 統計

        /// <summary>
        /// 取得管理員總數
        /// </summary>
        public async Task<int> GetTotalManagersCountAsync()
        {
            try
            {
                return await _context.ManagerData.AsNoTracking().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得管理員總數時發生錯誤");
                return 0;
            }
        }

        /// <summary>
        /// 取得活躍管理員數量（未被鎖定或鎖定已過期）
        /// </summary>
        public async Task<int> GetActiveManagersCountAsync()
        {
            try
            {
                return await _context.ManagerData
                    .AsNoTracking()
                    .CountAsync(m =>
                        !m.ManagerLockoutEnabled ||
                        !m.ManagerLockoutEnd.HasValue ||
                        m.ManagerLockoutEnd.Value <= DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得活躍管理員數量時發生錯誤");
                return 0;
            }
        }

        /// <summary>
        /// 取得各角色的管理員統計
        /// </summary>
        public async Task<Dictionary<string, int>> GetManagerStatsByRoleAsync()
        {
            try
            {
                var stats = new Dictionary<string, int>();

                // 取得所有角色
                var roles = await _context.ManagerRolePermissions.AsNoTracking().ToListAsync();

                foreach (var role in roles)
                {
                    // 計算擁有此角色的管理員數量
                    var count = await _context.ManagerData
                        .AsNoTracking()
                        .Include(m => m.ManagerRoles)
                        .CountAsync(m => m.ManagerRoles.Any(r => r.ManagerRoleId == role.ManagerRoleId));

                    stats[role.RoleName] = count;
                }

                // 計算沒有角色的管理員數量
                var noRoleCount = await _context.ManagerData
                    .AsNoTracking()
                    .Include(m => m.ManagerRoles)
                    .CountAsync(m => !m.ManagerRoles.Any());

                if (noRoleCount > 0)
                {
                    stats["無角色"] = noRoleCount;
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得管理員角色統計時發生錯誤");
                return new Dictionary<string, int>();
            }
        }

        #endregion

        #region 密碼處理輔助方法

        /// <summary>
        /// 使用 SHA256 雜湊密碼
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// 驗證密碼是否正確
        /// </summary>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        #endregion
    }
}

