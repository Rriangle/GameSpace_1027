using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IUserService
    {
        // 使用者基本 CRUD
        Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 50);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByAccountAsync(string account);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);

        // 使用者狀態管理
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> LockUserAsync(int userId, DateTime? lockoutEnd = null);
        Task<bool> UnlockUserAsync(int userId);

        // 使用者搜尋與篩選
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetInactiveUsersAsync();
        Task<IEnumerable<User>> GetLockedUsersAsync();

        // 使用者統計
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetNewUsersCountAsync(DateTime since);
        Task<Dictionary<string, int>> GetUserStatsByDateAsync(int days = 30);

        // 使用者權限
        Task<bool> GrantRightAsync(int userId, string rightName);
        Task<bool> RevokeRightAsync(int userId, string rightName);
        Task<IEnumerable<string>> GetUserRightsAsync(int userId);
        Task<bool> HasRightAsync(int userId, string rightName);
    }
}

