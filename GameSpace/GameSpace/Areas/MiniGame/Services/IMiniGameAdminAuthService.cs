using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IMiniGameAdminAuthService
    {
        Task<bool> HasPermissionAsync(int managerId, string permission);
        Task<List<ManagerRolePermission>> GetManagerPermissionsAsync(int managerId);
        Task<ManagerDatum?> GetManagerDatumAsync(int managerId);
    }
}

