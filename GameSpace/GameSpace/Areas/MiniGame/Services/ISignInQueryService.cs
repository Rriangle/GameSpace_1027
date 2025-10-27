using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// Service interface for querying sign-in rules and records
    /// </summary>
    public interface ISignInQueryService
    {
        /// <summary>
        /// Get paginated list of sign-in rules
        /// </summary>
        Task<PagedResult<SignInRuleViewModel>> GetSignInRulesAsync(int page = 1, int pageSize = 20);

        /// <summary>
        /// Get sign-in rule by ID
        /// </summary>
        Task<SignInRuleViewModel?> GetSignInRuleByIdAsync(int id);

        /// <summary>
        /// Query sign-in records with filters
        /// </summary>
        Task<PagedResult<SignInRecordViewModel>> QuerySignInRecordsAsync(
            int? userId = null,
            string? userAccount = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? minPoints = null,
            int? maxPoints = null,
            string sortBy = "SignInDate",
            string sortOrder = "desc",
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Get sign-in record by ID
        /// </summary>
        Task<SignInRecordViewModel?> GetSignInRecordByIdAsync(int logId);

        /// <summary>
        /// Get sign-in statistics summary
        /// </summary>
        Task<SignInStatsViewModel> GetSignInStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get active sign-in rules
        /// </summary>
        Task<List<SignInRuleViewModel>> GetActiveSignInRulesAsync();

        /// <summary>
        /// Get user's consecutive sign-in days
        /// </summary>
        Task<int> GetUserConsecutiveDaysAsync(int userId);
    }
}
