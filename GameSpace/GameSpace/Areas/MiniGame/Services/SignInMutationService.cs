using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// Service implementation for sign-in mutation operations (Create, Update, Delete)
    /// </summary>
    public class SignInMutationService : ISignInMutationService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<SignInMutationService> _logger;
        private static readonly TimeZoneInfo TaipeiTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        private const string SignInRuleSelectSql = "SELECT Id, SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive, CreatedAt, UpdatedAt, Description, IsDeleted, DeletedAt, DeletedBy, DeleteReason FROM SignInRule WHERE IsDeleted = 0";

        public SignInMutationService(GameSpacedatabaseContext context, ILogger<SignInMutationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private (DateTime startUtc, DateTime endUtc) GetTaipeiDateRange(DateTime taipeiDate)
        {
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(taipeiDate.Date, TaipeiTimeZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(taipeiDate.Date.AddDays(1), TaipeiTimeZone);
            return (startUtc, endUtc);
        }

        /// <summary>
        /// Create a new sign-in rule
        /// </summary>
        public async Task<(bool success, int? ruleId, string errorMessage)> CreateSignInRuleAsync(SignInRuleInputModel model, int operatorId)
        {
            try
            {
                // Validate the rule
                var (isValid, errorMessage) = await ValidateSignInRuleAsync(model);
                if (!isValid)
                {
                    return (false, null, errorMessage);
                }

                // Create the rule entity
                var rule = new SignInRule
                {
                    SignInDay = model.SignInDay,
                    Points = model.Points,
                    Experience = model.Experience ?? 0,
                    HasCoupon = !string.IsNullOrWhiteSpace(model.CouponTypeCode),
                    CouponTypeCode = model.CouponTypeCode,
                    IsActive = model.IsActive,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Set<SignInRule>().Add(rule);
                await _context.SaveChangesAsync();

                // Log the operation
                await LogOperationAsync(operatorId, "CreateSignInRule", $"Created sign-in rule for day {rule.SignInDay}");

                return (true, rule.Id, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, null, $"建立簽到規則時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing sign-in rule
        /// </summary>
        public async Task<(bool success, string errorMessage)> UpdateSignInRuleAsync(int ruleId, SignInRuleInputModel model, int operatorId)
        {
            try
            {
                // Find the existing rule
                var rule = await _context.Set<SignInRule>()
                    .FromSqlRaw(SignInRuleSelectSql)
                    .FirstOrDefaultAsync(r => r.Id == ruleId);
                if (rule == null)
                {
                    return (false, "找不到指定的簽到規則");
                }

                // Validate the rule (excluding current rule from duplicate check)
                var (isValid, errorMessage) = await ValidateSignInRuleAsync(model, ruleId);
                if (!isValid)
                {
                    return (false, errorMessage);
                }

                // Update the rule
                rule.SignInDay = model.SignInDay;
                rule.Points = model.Points;
                rule.Experience = model.Experience ?? 0;
                rule.HasCoupon = !string.IsNullOrWhiteSpace(model.CouponTypeCode);
                rule.CouponTypeCode = model.CouponTypeCode;
                rule.IsActive = model.IsActive;
                rule.Description = model.Description;
                rule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log the operation
                await LogOperationAsync(operatorId, "UpdateSignInRule", $"Updated sign-in rule {ruleId} for day {rule.SignInDay}");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"更新簽到規則時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// Delete a sign-in rule (soft delete by setting IsActive to false)
        /// </summary>
        public async Task<(bool success, string errorMessage)> DeleteSignInRuleAsync(int ruleId, int operatorId)
        {
            try
            {
                var rule = await _context.Set<SignInRule>()
                    .FromSqlRaw(SignInRuleSelectSql)
                    .FirstOrDefaultAsync(r => r.Id == ruleId);
                if (rule == null)
                {
                    return (false, "找不到指定的簽到規則");
                }

                // Check if this rule is currently being used
                var isUsed = await _context.UserSignInStats
                    .AnyAsync(s => s.PointsGained == rule.Points &&
                                   s.ExpGained == rule.Experience);

                if (isUsed)
                {
                    // Soft delete - set IsActive to false
                    rule.IsActive = false;
                    rule.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Hard delete if not used
                    _context.Set<SignInRule>().Remove(rule);
                }

                await _context.SaveChangesAsync();

                // Log the operation
                await LogOperationAsync(operatorId, "DeleteSignInRule", $"Deleted sign-in rule {ruleId} for day {rule.SignInDay}");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"刪除簽到規則時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// Manually add a sign-in record for a user (補簽)
        /// </summary>
        public async Task<(bool success, string errorMessage)> ManualSignInAsync(ManualSignInInputModel model, int operatorId)
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return (false, "找不到指定的使用者");
            }

            // Check if user already signed in on this date
            var signDate = model.SignInDate.Date;
            var (dayStartUtc, dayEndUtc) = GetTaipeiDateRange(signDate);
            var existingRecord = await _context.UserSignInStats
                .FirstOrDefaultAsync(s => s.UserId == model.UserId &&
                                         s.SignTime >= dayStartUtc && s.SignTime < dayEndUtc);

            if (existingRecord != null)
            {
                return (false, "該使用者在此日期已有簽到記錄");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the appropriate sign-in rule based on consecutive days
                var consecutiveDays = await GetUserConsecutiveDaysAsync(model.UserId, signDate);
                var rule = await _context.Set<SignInRule>()
                    .FromSqlRaw(SignInRuleSelectSql)
                    .Where(r => r.SignInDay == consecutiveDays && r.IsActive)
                    .FirstOrDefaultAsync();

                // Use default values if no rule found
                int points = rule?.Points ?? 10;
                int experience = rule?.Experience ?? 5;
                string? coupon = rule?.HasCoupon == true ? rule.CouponTypeCode : null;

                // Create the sign-in record
                var now = DateTime.UtcNow;
                var record = new UserSignInStat
                {
                    UserId = model.UserId,
                    SignTime = model.SignInDate,
                    PointsGained = points,
                    PointsGainedTime = now,
                    ExpGained = experience,
                    ExpGainedTime = now,
                    CouponGained = coupon ?? string.Empty,
                    CouponGainedTime = now
                };

                _context.UserSignInStats.Add(record);

                // Update user wallet if points > 0
                if (points > 0)
                {
                    var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == model.UserId);
                    if (wallet != null)
                    {
                        wallet.UserPoint += points;
                    }
                }

                // Update pet experience if user has a pet and experience > 0
                if (experience > 0)
                {
                    var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == model.UserId);
                    if (pet != null)
                    {
                        pet.Experience += experience;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Log the operation
                await LogOperationAsync(operatorId, "ManualSignIn",
                    $"Manual sign-in for user {model.UserId} on {signDate:yyyy-MM-dd}. Reason: {model.Reason}");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"手動補簽時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// Delete a sign-in record
        /// </summary>
        public async Task<(bool success, string errorMessage)> DeleteSignInRecordAsync(int logId, int operatorId)
        {
            var record = await _context.UserSignInStats.FindAsync(logId);
            if (record == null)
            {
                return (false, "找不到指定的簽到記錄");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Revert points from user wallet
                if (record.PointsGained > 0)
                {
                    var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == record.UserId);
                    if (wallet != null)
                    {
                        wallet.UserPoint = Math.Max(0, wallet.UserPoint - record.PointsGained);
                    }
                }

                // Revert experience from pet
                if (record.ExpGained > 0)
                {
                    var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == record.UserId);
                    if (pet != null)
                    {
                        pet.Experience = Math.Max(0, pet.Experience - record.ExpGained);
                    }
                }

                // Remove the record
                _context.UserSignInStats.Remove(record);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Log the operation
                await LogOperationAsync(operatorId, "DeleteSignInRecord",
                    $"Deleted sign-in record {logId} for user {record.UserId}");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"刪除簽到記錄時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// Validate sign-in rule data
        /// </summary>
        public async Task<(bool isValid, string errorMessage)> ValidateSignInRuleAsync(SignInRuleInputModel model, int? excludeRuleId = null)
        {
            // Check if SignInDay is valid
            if (model.SignInDay < 1 || model.SignInDay > 365)
            {
                return (false, "簽到天數必須在 1 到 365 之間");
            }

            // Check if Points is valid
            if (model.Points < 0)
            {
                return (false, "點數不可為負數");
            }

            // Check if Experience is valid
            if (model.Experience < 0)
            {
                return (false, "經驗值不可為負數");
            }

            // Check for duplicate SignInDay
            var duplicateQuery = _context.Set<SignInRule>()
                .FromSqlRaw(SignInRuleSelectSql)
                .Where(r => r.SignInDay == model.SignInDay && r.IsActive);

            if (excludeRuleId.HasValue)
            {
                duplicateQuery = duplicateQuery.Where(r => r.Id != excludeRuleId.Value);
            }

            var hasDuplicate = await duplicateQuery.AnyAsync();
            if (hasDuplicate)
            {
                return (false, $"已存在第 {model.SignInDay} 天的有效簽到規則");
            }

            // Validate coupon type if specified
            if (!string.IsNullOrWhiteSpace(model.CouponTypeCode))
            {
                var couponTypeExists = await _context.CouponTypes
                    .AnyAsync(ct => ct.Name == model.CouponTypeCode);

                if (!couponTypeExists)
                {
                    return (false, "指定的優惠券類型不存在");
                }
            }

            // Validate reward reasonability
            if (model.Points > 1000)
            {
                return (false, "單次簽到點數不應超過 1000 點");
            }

            if (model.Experience > 500)
            {
                return (false, "單次簽到經驗值不應超過 500 點");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Get user's consecutive sign-in days up to a specific date
        /// </summary>
        private async Task<int> GetUserConsecutiveDaysAsync(int userId, DateTime asOfDate)
        {
            // Convert asOfDate to Taipei timezone and get its date part
            var asOfDateTaiwan = TimeZoneInfo.ConvertTimeFromUtc(asOfDate, TaipeiTimeZone).Date;
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(asOfDateTaiwan.AddDays(1), TaipeiTimeZone);

            var records = await _context.UserSignInStats
                .Where(s => s.UserId == userId && s.SignTime < endUtc)
                .OrderByDescending(s => s.SignTime)
                .Select(s => s.SignTime.Date)
                .ToListAsync();

            if (!records.Any())
            {
                return 1; // First sign-in
            }

            int consecutiveDays = 1;
            var currentDate = asOfDateTaiwan;

            foreach (var signDate in records)
            {
                var expectedDate = currentDate.AddDays(-consecutiveDays + 1);
                if (signDate == expectedDate)
                {
                    consecutiveDays++;
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        /// <summary>
        /// Log admin operation (已移除 - 不需要 AdminOperationLog 表)
        /// </summary>
        private Task LogOperationAsync(int managerId, string operationType, string details)
        {
            // 已決定不使用 AdminOperationLog 表，此方法保留為空實現以避免破壞現有程式碼
            return Task.CompletedTask;
        }
    }
}
