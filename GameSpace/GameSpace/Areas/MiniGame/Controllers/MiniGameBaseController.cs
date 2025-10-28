using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public abstract class MiniGameBaseController : Controller
    {
        protected readonly GameSpacedatabaseContext _context;
        protected readonly IMiniGameAdminService _adminService;

        protected MiniGameBaseController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        protected MiniGameBaseController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : this(context)
        {
            _adminService = adminService;
        }

        // 獲取當前管理員ID
        protected int? GetCurrentManagerId()
        {
            var managerIdClaim = User.FindFirst("ManagerId");
            if (managerIdClaim != null && int.TryParse(managerIdClaim.Value, out int managerId))
            {
                return managerId;
            }
            return null;
        }

        // 獲取當前管理員信息
        protected async Task<ManagerDatum> GetCurrentManagerAsync()
        {
            var managerId = GetCurrentManagerId();
            if (managerId.HasValue)
            {
                return await _context.ManagerData.FindAsync(managerId.Value);
            }
            return null;
        }

        // 檢查管理員權限
        protected async Task<bool> HasPermissionAsync(string permission)
        {
            // MiniGame 權限：只要通過 AdminCookie 認證就允許（不需要檢查 ManagerId 或角色）
            if (permission == "MiniGame.View" || permission == "MiniGame.Edit")
            {
                // 只要用戶已認證就允許
                return User?.Identity?.IsAuthenticated == true;
            }

            // 其他權限需要檢查 ManagerId 和角色
            var managerId = GetCurrentManagerId();
            if (!managerId.HasValue) return false;

            // 檢查管理員角色權限
            var manager = await GetCurrentManagerAsync();
            if (manager == null) return false;

            // 載入管理員的角色權限
            var managerWithRoles = await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == manager.ManagerId);

            if (managerWithRoles == null || !managerWithRoles.ManagerRoles.Any())
                return false;

            // 檢查是否有任何角色具有管理員權限（最高權限）
            var hasAdminPrivilege = managerWithRoles.ManagerRoles
                .Any(r => r?.AdministratorPrivilegesManagement == true);

            if (hasAdminPrivilege) return true;

            // 根據權限類型檢查特定權限
            return permission switch
            {
                "MiniGame.Delete" => hasAdminPrivilege,
                "User.View" or "User.Edit" => managerWithRoles.ManagerRoles.Any(r => r?.UserStatusManagement == true) || hasAdminPrivilege,
                "Wallet.View" or "Wallet.Edit" => managerWithRoles.ManagerRoles.Any(r => r?.ShoppingPermissionManagement == true) || hasAdminPrivilege,
                "Pet.View" or "Pet.Edit" => managerWithRoles.ManagerRoles.Any(r => r?.PetRightsManagement == true) || hasAdminPrivilege,
                "Coupon.View" or "Coupon.Edit" or "EVoucher.View" or "EVoucher.Edit" => managerWithRoles.ManagerRoles.Any(r => r?.ShoppingPermissionManagement == true) || hasAdminPrivilege,
                "Message.View" or "Message.Edit" => managerWithRoles.ManagerRoles.Any(r => r?.MessagePermissionManagement == true) || hasAdminPrivilege,
                "CustomerService" => managerWithRoles.ManagerRoles.Any(r => r?.CustomerService == true) || hasAdminPrivilege,
                _ => false
            };
        }

        // 檢查用戶權限
        protected async Task<bool> HasUserRightAsync(int userId, string rightName)
        {
            return true; // 默認允許所有用戶權限
        }

        // 記錄操作日誌（已移除 - 不需要 AdminOperationLog 表）
        protected Task LogOperationAsync(string operation, string details, int? targetUserId = null)
        {
            // 已決定不使用 AdminOperationLog 表，此方法保留為空實現以避免破壞現有程式碼
            return Task.CompletedTask;
        }

        // 驗證用戶權限
        protected async Task<bool> ValidateUserPermissionAsync(int userId, string permission)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRight)
                    .FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null) return false;

                // 檢查用戶狀態
                if (user.UserRight?.UserStatus != true) return false;

                // 檢查管理員權限
                var hasPermission = await HasPermissionAsync(permission);
                if (!hasPermission) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // 獲取分頁結果
        protected PagedResult<T> CreatePagedResult<T>(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // 計算分頁信息
        protected (int totalPages, int startPage, int endPage) CalculatePagination(int totalCount, int pageNumber, int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var startPage = Math.Max(1, pageNumber - 2);
            var endPage = Math.Min(totalPages, pageNumber + 2);
            
            return (totalPages, startPage, endPage);
        }

        // 生成隨機代碼
        protected string GenerateRandomCode(string prefix, int length = 8)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"{prefix}{code}";
        }

        // 驗證日期範圍
        protected bool IsValidDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue) return true;
            return startDate.Value <= endDate.Value;
        }

        // 格式化日期顯示
        protected string FormatDate(DateTime? date)
        {
            return date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未設定";
        }

        // 檢查字符串是否為空或空白
        protected bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        // 安全地轉換字符串為整數
        protected int? SafeParseInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            return null;
        }

        // 安全地轉換字符串為布爾值
        protected bool? SafeParseBool(string value)
        {
            if (bool.TryParse(value, out bool result))
                return result;
            return null;
        }

        // 獲取客戶端IP地址
        protected string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    ipAddress = forwardedFor.Split(',')[0].Trim();
                }
            }
            return ipAddress ?? "Unknown";
        }

        // 驗證文件上傳
        protected bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;
            
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            return allowedExtensions.Contains(extension);
        }

        // 獲取文件大小描述
        protected string GetFileSizeDescription(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        // 驗證電子郵件格式
        protected bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // 驗證手機號碼格式
        protected bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
            
            // 簡單的手機號碼驗證（台灣格式）
            var cleanNumber = phoneNumber.Replace("-", "").Replace(" ", "");
            return System.Text.RegularExpressions.Regex.IsMatch(cleanNumber, @"^09\d{8}$");
        }

        // 生成QR碼內容
        protected string GenerateQRCodeContent(string type, string code)
        {
            return $"{type}:{code}:{DateTime.Now:yyyyMMddHHmmss}";
        }

        // 驗證QR碼內容
        protected (bool isValid, string type, string code, DateTime? timestamp) ValidateQRCodeContent(string content)
        {
            try
            {
                var parts = content.Split(':');
                if (parts.Length != 3) return (false, null, null, null);
                
                var type = parts[0];
                var code = parts[1];
                var timestampStr = parts[2];
                
                if (DateTime.TryParseExact(timestampStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime timestamp))
                {
                    return (true, type, code, timestamp);
                }
                
                return (false, null, null, null);
            }
            catch
            {
                return (false, null, null, null);
            }
        }

        // 檢查操作是否在允許的時間範圍內
        protected bool IsOperationAllowed(DateTime? operationTime, int maxHours = 24)
        {
            if (!operationTime.HasValue) return false;
            return DateTime.Now.Subtract(operationTime.Value).TotalHours <= maxHours;
        }

        // 獲取系統設定值 (暫時返回預設值，等待 SystemSettings 表實作)
        protected async Task<string> GetSystemSettingAsync(string key, string defaultValue = "")
        {
            return await Task.FromResult(defaultValue);
        }

        // 設定系統設定值
        protected async Task<bool> SetSystemSettingAsync(string key, string value)
        {
            return await Task.FromResult(false);
        }
    }

}

