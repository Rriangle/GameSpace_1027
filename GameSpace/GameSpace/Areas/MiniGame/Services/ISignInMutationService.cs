using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// Service interface for sign-in mutation operations (Create, Update, Delete)
    /// </summary>
    public interface ISignInMutationService
    {
        /// <summary>
        /// Create a new sign-in rule
        /// </summary>
        /// <param name="model">Sign-in rule input model</param>
        /// <param name="operatorId">ID of the manager performing the operation</param>
        /// <returns>Created rule ID if successful, null otherwise</returns>
        Task<(bool success, int? ruleId, string errorMessage)> CreateSignInRuleAsync(SignInRuleInputModel model, int operatorId);

        /// <summary>
        /// Update an existing sign-in rule
        /// </summary>
        /// <param name="ruleId">ID of the rule to update</param>
        /// <param name="model">Updated rule data</param>
        /// <param name="operatorId">ID of the manager performing the operation</param>
        /// <returns>True if update succeeded, false otherwise</returns>
        Task<(bool success, string errorMessage)> UpdateSignInRuleAsync(int ruleId, SignInRuleInputModel model, int operatorId);

        /// <summary>
        /// Delete a sign-in rule (soft delete by setting IsActive to false)
        /// </summary>
        /// <param name="ruleId">ID of the rule to delete</param>
        /// <param name="operatorId">ID of the manager performing the operation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        Task<(bool success, string errorMessage)> DeleteSignInRuleAsync(int ruleId, int operatorId);

        /// <summary>
        /// Manually add a sign-in record for a user (補簽)
        /// </summary>
        /// <param name="model">Manual sign-in input model</param>
        /// <param name="operatorId">ID of the manager performing the operation</param>
        /// <returns>True if sign-in record created successfully, false otherwise</returns>
        Task<(bool success, string errorMessage)> ManualSignInAsync(ManualSignInInputModel model, int operatorId);

        /// <summary>
        /// Delete a sign-in record
        /// </summary>
        /// <param name="logId">ID of the sign-in record to delete</param>
        /// <param name="operatorId">ID of the manager performing the operation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        Task<(bool success, string errorMessage)> DeleteSignInRecordAsync(int logId, int operatorId);

        /// <summary>
        /// Validate sign-in rule data
        /// </summary>
        /// <param name="model">Rule data to validate</param>
        /// <param name="excludeRuleId">Rule ID to exclude from duplicate check (for updates)</param>
        /// <returns>Validation result</returns>
        Task<(bool isValid, string errorMessage)> ValidateSignInRuleAsync(SignInRuleInputModel model, int? excludeRuleId = null);
    }
}
