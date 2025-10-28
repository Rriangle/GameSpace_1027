using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    public class MiniGameAdminAuthService : IMiniGameAdminAuthService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<MiniGameAdminAuthService> _logger;

        public MiniGameAdminAuthService(GameSpacedatabaseContext context, ILogger<MiniGameAdminAuthService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> HasPermissionAsync(int managerId, string permission)
        {
            var manager = await _context.ManagerData
                .AsNoTracking()
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            if (manager?.ManagerRoles == null)
                return false;

            return permission switch
            {
                "AdministratorPrivilegesManagement" => manager.ManagerRoles.Any(r => r?.AdministratorPrivilegesManagement == true),
                "UserStatusManagement" => manager.ManagerRoles.Any(r => r?.UserStatusManagement == true),
                "ShoppingPermissionManagement" => manager.ManagerRoles.Any(r => r?.ShoppingPermissionManagement == true),
                "MessagePermissionManagement" => manager.ManagerRoles.Any(r => r?.MessagePermissionManagement == true),
                "PetRightsManagement" => manager.ManagerRoles.Any(r => r?.PetRightsManagement == true),
                "CustomerService" => manager.ManagerRoles.Any(r => r?.CustomerService == true),
                _ => false
            };
        }

        public async Task<List<ManagerRolePermission>> GetManagerPermissionsAsync(int managerId)
        {
            var manager = await _context.ManagerData
                .AsNoTracking()
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            return manager?.ManagerRoles?
                .Where(r => r != null)
                .ToList() ?? new List<ManagerRolePermission>();
        }

        public async Task<ManagerDatum?> GetManagerDatumAsync(int managerId)
        {
            return await _context.ManagerData
                .AsNoTracking()
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);
        }
    }
}

