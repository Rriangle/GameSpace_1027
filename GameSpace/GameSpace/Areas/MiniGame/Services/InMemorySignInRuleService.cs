using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 簽到規則管理服務
    /// 支援查詢、新增、修改、刪除簽到規則
    /// ⚠️ 已改為直接使用 SQL Server SignInRule 表（不再使用 JSON 檔案）
    /// </summary>
    public interface IInMemorySignInRuleService
    {
        Task<List<SignInRuleDisplay>> GetAllRulesAsync();
        Task<SignInRuleDisplay?> GetRuleByIdAsync(int id);
        Task<bool> UpdateRuleAsync(int id, int points, int experience, bool hasCoupon, string? couponTypeCode, bool isActive, string? description);
        Task<bool> CreateRuleAsync(int signInDay, int points, int experience, bool hasCoupon, string? couponTypeCode, string? description);
        Task<bool> DeleteRuleAsync(int id);
        Task ResetToDefaultAsync();
    }

    public class InMemorySignInRuleService : IInMemorySignInRuleService
    {
        private readonly GameSpacedatabaseContext _context;

        public InMemorySignInRuleService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有簽到規則（從 SQL Server SignInRule 表讀取）
        /// </summary>
        public async Task<List<SignInRuleDisplay>> GetAllRulesAsync()
        {
            var rules = await _context.Set<SignInRule>()
                .FromSqlRaw("SELECT Id, SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive, CreatedAt, UpdatedAt, Description, IsDeleted, DeletedAt, DeletedBy, DeleteReason FROM SignInRule WHERE IsDeleted = 0")
                .AsNoTracking()
                .OrderBy(r => r.SignInDay)
                .ToListAsync();

            return rules.Select(r => new SignInRuleDisplay
            {
                Id = r.Id,
                DayNumber = r.SignInDay,
                Points = r.Points,
                Experience = r.Experience,
                HasCoupon = r.HasCoupon,
                CouponTypeCode = r.CouponTypeCode,
                Description = r.Description,
                IsActive = r.IsActive
            }).ToList();
        }

        /// <summary>
        /// 根據 ID 取得單一規則
        /// </summary>
        public async Task<SignInRuleDisplay?> GetRuleByIdAsync(int id)
        {
            var rule = await _context.Set<SignInRule>()
                .FromSqlRaw("SELECT Id, SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive, CreatedAt, UpdatedAt, Description, IsDeleted, DeletedAt, DeletedBy, DeleteReason FROM SignInRule WHERE IsDeleted = 0")
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null)
                return null;

            return new SignInRuleDisplay
            {
                Id = rule.Id,
                DayNumber = rule.SignInDay,
                Points = rule.Points,
                Experience = rule.Experience,
                HasCoupon = rule.HasCoupon,
                CouponTypeCode = rule.CouponTypeCode,
                Description = rule.Description,
                IsActive = rule.IsActive
            };
        }

        /// <summary>
        /// 更新規則（持久化到 SQL Server）
        /// </summary>
        public async Task<bool> UpdateRuleAsync(int id, int points, int experience, bool hasCoupon, string? couponTypeCode, bool isActive, string? description)
        {
            var rule = await _context.Set<SignInRule>()
                .FromSqlRaw("SELECT Id, SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive, CreatedAt, UpdatedAt, Description, IsDeleted, DeletedAt, DeletedBy, DeleteReason FROM SignInRule WHERE IsDeleted = 0")
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null)
                return false;

            rule.Points = points;
            rule.Experience = experience;
            rule.HasCoupon = hasCoupon;
            rule.CouponTypeCode = hasCoupon ? couponTypeCode : null;
            rule.IsActive = isActive;
            rule.Description = description;
            rule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 創建新規則（持久化到 SQL Server）
        /// </summary>
        public async Task<bool> CreateRuleAsync(int signInDay, int points, int experience, bool hasCoupon, string? couponTypeCode, string? description)
        {
            // 檢查是否已有相同天數的規則
            var exists = await _context.Set<SignInRule>()
                .FromSqlRaw("SELECT Id, SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive, CreatedAt, UpdatedAt, Description, IsDeleted, DeletedAt, DeletedBy, DeleteReason FROM SignInRule WHERE IsDeleted = 0")
                .AnyAsync(r => r.SignInDay == signInDay);

            if (exists)
                return false;

            var newRule = new SignInRule
            {
                SignInDay = signInDay,
                Points = points,
                Experience = experience,
                HasCoupon = hasCoupon,
                CouponTypeCode = hasCoupon ? couponTypeCode : null,
                IsActive = true,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<SignInRule>().Add(newRule);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 刪除規則（從 SQL Server 刪除）
        /// </summary>
        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _context.Set<SignInRule>()
                .FromSqlRaw("SELECT Id, SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive, CreatedAt, UpdatedAt, Description, IsDeleted, DeletedAt, DeletedBy, DeleteReason FROM SignInRule WHERE IsDeleted = 0")
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null)
                return false;

            _context.Set<SignInRule>().Remove(rule);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 重置為預設規則（清空現有資料並插入預設規則）
        /// </summary>
        public async Task ResetToDefaultAsync()
        {
            // 清空現有規則
            var existingRules = await _context.Set<SignInRule>()
                .FromSqlRaw("SELECT Id, SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive, CreatedAt, UpdatedAt, Description, IsDeleted, DeletedAt, DeletedBy, DeleteReason FROM SignInRule WHERE IsDeleted = 0")
                .ToListAsync();
            _context.Set<SignInRule>().RemoveRange(existingRules);

            // 插入預設規則
            var defaultRules = GetDefaultRules();
            foreach (var displayRule in defaultRules)
            {
                var rule = new SignInRule
                {
                    SignInDay = displayRule.DayNumber,
                    Points = displayRule.Points,
                    Experience = displayRule.Experience,
                    HasCoupon = displayRule.HasCoupon,
                    CouponTypeCode = displayRule.CouponTypeCode,
                    IsActive = displayRule.IsActive,
                    Description = displayRule.Description,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Set<SignInRule>().Add(rule);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 取得預設簽到規則（參考資料）
        /// </summary>
        private static List<SignInRuleDisplay> GetDefaultRules()
        {
            return new List<SignInRuleDisplay>
            {
                new() { Id = 1, DayNumber = 1, Points = 10, Experience = 5, HasCoupon = false, CouponTypeCode = null, IsActive = true, Description = "第一天簽到獎勵" },
                new() { Id = 2, DayNumber = 2, Points = 10, Experience = 5, HasCoupon = false, CouponTypeCode = null, IsActive = true, Description = "第二天簽到獎勵" },
                new() { Id = 3, DayNumber = 3, Points = 15, Experience = 8, HasCoupon = false, CouponTypeCode = null, IsActive = true, Description = "第三天簽到獎勵" },
                new() { Id = 4, DayNumber = 4, Points = 15, Experience = 8, HasCoupon = false, CouponTypeCode = null, IsActive = true, Description = "第四天簽到獎勵" },
                new() { Id = 5, DayNumber = 5, Points = 20, Experience = 10, HasCoupon = false, CouponTypeCode = null, IsActive = true, Description = "第五天簽到獎勵" },
                new() { Id = 6, DayNumber = 6, Points = 20, Experience = 10, HasCoupon = false, CouponTypeCode = null, IsActive = true, Description = "第六天簽到獎勵" },
                new() { Id = 7, DayNumber = 7, Points = 30, Experience = 15, HasCoupon = true, CouponTypeCode = "WEEK_BONUS", IsActive = true, Description = "第七天簽到獎勵 + 週獎勵優惠券" },
                new() { Id = 8, DayNumber = 14, Points = 50, Experience = 25, HasCoupon = true, CouponTypeCode = "TWO_WEEK_BONUS", IsActive = true, Description = "連續簽到 14 天獎勵" },
                new() { Id = 9, DayNumber = 21, Points = 80, Experience = 40, HasCoupon = true, CouponTypeCode = "THREE_WEEK_BONUS", IsActive = true, Description = "連續簽到 21 天獎勵" },
                new() { Id = 10, DayNumber = 30, Points = 150, Experience = 75, HasCoupon = true, CouponTypeCode = "MONTH_BONUS", IsActive = true, Description = "連續簽到 30 天獎勵（滿月獎勵）" }
            };
        }
    }
}
