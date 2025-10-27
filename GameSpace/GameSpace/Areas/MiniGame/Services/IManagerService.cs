using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IManagerService
    {
        // Manager 基本 CRUD
        Task<IEnumerable<ManagerDatum>> GetAllManagersAsync(int pageNumber = 1, int pageSize = 50);
        Task<ManagerDatum?> GetManagerByIdAsync(int managerId);
        Task<ManagerDatum?> GetManagerByAccountAsync(string account);
        Task<ManagerDatum?> GetManagerByEmailAsync(string email);
        Task<bool> CreateManagerAsync(ManagerDatum manager, string password);
        Task<bool> UpdateManagerAsync(ManagerDatum manager);
        Task<bool> DeleteManagerAsync(int managerId);

        // Manager 認證
        Task<ManagerDatum?> AuthenticateAsync(string account, string password);
        Task<bool> ChangePasswordAsync(int managerId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int managerId, string newPassword);

        // Manager 狀態管理
        Task<bool> LockManagerAsync(int managerId, DateTime? lockoutEnd = null);
        Task<bool> UnlockManagerAsync(int managerId);
        Task<bool> ConfirmEmailAsync(int managerId);

        // Manager 角色管理
        Task<ManagerRolePermission?> GetManagerRoleAsync(int managerId);
        Task<bool> AssignRoleAsync(int managerId, int roleId);
        Task<bool> RemoveRoleAsync(int managerId, int roleId);
        Task<IEnumerable<string>> GetManagerPermissionsAsync(int managerId);
        Task<bool> HasPermissionAsync(int managerId, string permission);

        // Manager 活動日誌
        Task<IEnumerable<ManagerActivityLog>> GetManagerActivitiesAsync(int managerId, int pageNumber = 1, int pageSize = 20);
        Task<bool> LogActivityAsync(int managerId, string action, string details);

        // Manager 統計
        Task<int> GetTotalManagersCountAsync();
        Task<int> GetActiveManagersCountAsync();
        Task<Dictionary<string, int>> GetManagerStatsByRoleAsync();
    }

    public class ManagerActivityLog
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

