using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class EVoucherService : IEVoucherService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<EVoucherService> _logger;

        public EVoucherService(GameSpacedatabaseContext context, ILogger<EVoucherService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Evoucher>> GetAllEVouchersAsync()
        {
            try
            {
                return await _context.Evouchers
                    .Include(e => e.EvoucherType)
                    .OrderByDescending(e => e.AcquiredTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有電子禮券時發生錯誤");
                return new List<Evoucher>();
            }
        }

        public async Task<IEnumerable<Evoucher>> GetEVouchersByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Evouchers
                    .Include(e => e.EvoucherType)
                    .Where(e => e.UserId == userId)
                    .OrderByDescending(e => e.AcquiredTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 的電子禮券時發生錯誤", userId);
                return new List<Evoucher>();
            }
        }

        public async Task<IEnumerable<Evoucher>> GetUnusedEVouchersByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Evouchers
                    .Include(e => e.EvoucherType)
                    .Where(e => e.UserId == userId && !e.IsUsed)
                    .OrderByDescending(e => e.AcquiredTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 未使用的電子禮券時發生錯誤", userId);
                return new List<Evoucher>();
            }
        }

        public async Task<Evoucher?> GetEVoucherByIdAsync(int eVoucherId)
        {
            try
            {
                return await _context.Evouchers
                    .Include(e => e.EvoucherType)
                    .FirstOrDefaultAsync(e => e.EvoucherId == eVoucherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得電子禮券 {EVoucherId} 時發生錯誤", eVoucherId);
                return null;
            }
        }

        public async Task<Evoucher?> GetEVoucherByCodeAsync(string eVoucherCode)
        {
            try
            {
                return await _context.Evouchers
                    .Include(e => e.EvoucherType)
                    .FirstOrDefaultAsync(e => e.EvoucherCode == eVoucherCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得電子禮券代碼 {EVoucherCode} 時發生錯誤", eVoucherCode);
                return null;
            }
        }

        public async Task<bool> CreateEVoucherAsync(Evoucher eVoucher)
        {
            try
            {
                // 驗證電子憑證代碼格式
                if (!ValidateEVoucherCodeFormat(eVoucher.EvoucherCode))
                {
                    _logger.LogWarning("電子憑證代碼格式無效: {EVoucherCode}", eVoucher.EvoucherCode);
                    return false;
                }

                _context.Evouchers.Add(eVoucher);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功建立電子禮券 {EVoucherCode}", eVoucher.EvoucherCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立電子禮券時發生錯誤");
                return false;
            }
        }

        public async Task<bool> UpdateEVoucherAsync(Evoucher eVoucher)
        {
            try
            {
                // 驗證電子憑證代碼格式
                if (!ValidateEVoucherCodeFormat(eVoucher.EvoucherCode))
                {
                    _logger.LogWarning("電子憑證代碼格式無效: {EVoucherCode}", eVoucher.EvoucherCode);
                    return false;
                }

                _context.Evouchers.Update(eVoucher);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功更新電子禮券 {EVoucherId}", eVoucher.EvoucherId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新電子禮券 {EVoucherId} 時發生錯誤", eVoucher.EvoucherId);
                return false;
            }
        }

        public async Task<bool> DeleteEVoucherAsync(int eVoucherId)
        {
            try
            {
                var eVoucher = await _context.Evouchers.FindAsync(eVoucherId);
                if (eVoucher == null) return false;

                _context.Evouchers.Remove(eVoucher);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功刪除電子禮券 {EVoucherId}", eVoucherId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除電子禮券 {EVoucherId} 時發生錯誤", eVoucherId);
                return false;
            }
        }

        public async Task<bool> UseEVoucherAsync(int eVoucherId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 載入 EvoucherType 並檢查有效期
                var eVoucher = await _context.Evouchers
                    .Include(e => e.EvoucherType)
                    .FirstOrDefaultAsync(e => e.EvoucherId == eVoucherId);

                if (eVoucher == null || eVoucher.IsUsed)
                {
                    _logger.LogWarning("無法使用電子憑證: {EVoucherId}, 不存在或已使用", eVoucherId);
                    return false;
                }

                // 檢查過期日期
                if (eVoucher.EvoucherType != null)
                {
                    var now = DateTime.UtcNow;
                    if (now < eVoucher.EvoucherType.ValidFrom || now > eVoucher.EvoucherType.ValidTo)
                    {
                        _logger.LogWarning("嘗試使用已過期電子憑證: {EVoucherId}", eVoucherId);
                        return false;
                    }
                }

                // 更新憑證狀態
                eVoucher.IsUsed = true;
                eVoucher.UsedTime = DateTime.UtcNow;

                // 記錄兌換日誌
                var log = new EvoucherRedeemLog
                {
                    EvoucherId = eVoucherId,
                    UserId = eVoucher.UserId,
                    ScannedAt = DateTime.UtcNow,
                    Status = "Redeemed"
                };
                _context.EvoucherRedeemLogs.Add(log);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("電子憑證使用成功: {EVoucherId}, 用戶={UserId}", eVoucherId, eVoucher.UserId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "使用電子憑證失敗: {EVoucherId}", eVoucherId);
                throw;
            }
        }

        public async Task<bool> GrantEVoucherToUserAsync(int userId, int eVoucherTypeId)
        {
            try
            {
                var eVoucherType = await _context.EvoucherTypes.FindAsync(eVoucherTypeId);
                if (eVoucherType == null) return false;

                var eVoucherCode = GenerateEVoucherCode(eVoucherType.Name);
                var eVoucher = new Evoucher
                {
                    EvoucherCode = eVoucherCode,
                    EvoucherTypeId = eVoucherTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                };

                _context.Evouchers.Add(eVoucher);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功發放電子禮券 {EVoucherCode} 給使用者 {UserId}", eVoucherCode, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放電子禮券給使用者 {UserId} 時發生錯誤", userId);
                return false;
            }
        }

        public async Task<int> GetEVoucherCountByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Evouchers
                    .Where(e => e.UserId == userId && !e.IsUsed)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 電子禮券數量時發生錯誤", userId);
                return 0;
            }
        }

        public async Task<IEnumerable<EvoucherType>> GetAllEVoucherTypesAsync()
        {
            try
            {
                return await _context.EvoucherTypes
                    .OrderBy(et => et.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有電子禮券類型時發生錯誤");
                return new List<EvoucherType>();
            }
        }

        public async Task<EvoucherType?> GetEVoucherTypeByIdAsync(int eVoucherTypeId)
        {
            try
            {
                return await _context.EvoucherTypes.FindAsync(eVoucherTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得電子禮券類型 {EVoucherTypeId} 時發生錯誤", eVoucherTypeId);
                return null;
            }
        }

        private string GenerateEVoucherCode(string typeName)
        {
            var typeCode = GetTypeCode(typeName);
            var random = new Random();
            var randomCode = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            var randomNumber = random.Next(100000, 999999);
            return $"EV-{typeCode}-{randomCode}-{randomNumber}";
        }

        private string GetTypeCode(string typeName)
        {
            if (typeName.Contains("現金")) return "CASH";
            if (typeName.Contains("咖啡")) return "COFFEE";
            if (typeName.Contains("影城")) return "MOVIE";
            if (typeName.Contains("餐飲")) return "FOOD";
            if (typeName.Contains("加油")) return "GAS";
            if (typeName.Contains("百貨")) return "STORE";
            return "OTHER";
        }

        /// <summary>
        /// 驗證電子憑證代碼格式
        /// </summary>
        /// <param name="code">電子憑證代碼</param>
        /// <returns>格式正確回傳 true，否則回傳 false</returns>
        /// <remarks>
        /// 格式: EV-TYPE-XXXX-XXXXXX
        /// 範例: EV-CASH-AB12-123456
        /// </remarks>
        private bool ValidateEVoucherCodeFormat(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var parts = code.Split('-');
            if (parts.Length != 4)
                return false;

            if (parts[0] != "EV")
                return false;

            // TYPE 部分：至少 2 個大寫字母
            if (parts[1].Length < 2 || !parts[1].All(char.IsUpper))
                return false;

            // XXXX 部分：4 個字母或數字
            if (parts[2].Length != 4 || !parts[2].All(c => char.IsLetterOrDigit(c)))
                return false;

            // XXXXXX 部分：6 個數字
            if (parts[3].Length != 6 || !parts[3].All(char.IsDigit))
                return false;

            return true;
        }
    }
}

