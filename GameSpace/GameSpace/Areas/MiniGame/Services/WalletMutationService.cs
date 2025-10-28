using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 錢包異動服務實作 - 處理會員點數、優惠券、電子禮券的發放與調整
    /// </summary>
    public class WalletMutationService : IWalletMutationService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<WalletMutationService> _logger;

        public WalletMutationService(
            GameSpacedatabaseContext context,
            ILogger<WalletMutationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 發放會員點數（支援增減）
        /// </summary>
        public async Task<WalletMutationResult> IssuePointsAsync(int userId, int points, string reason, int operatorId)
        {
            if (points == 0)
            {
                return WalletMutationResult.CreateFailure("點數不能為 0", userId);
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return WalletMutationResult.CreateFailure("必須提供發放原因", userId);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 驗證會員存在
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return WalletMutationResult.CreateFailure($"找不到會員 ID: {userId}", userId);
                }

                // 查詢或建立會員錢包
                var wallet = await _context.UserWallets.FindAsync(userId);
                if (wallet == null)
                {
                    wallet = new UserWallet
                    {
                        UserId = userId,
                        UserPoint = 0
                    };
                    _context.UserWallets.Add(wallet);
                    await _context.SaveChangesAsync();
                }

                var balanceBefore = wallet.UserPoint;

                // 檢查餘額（如果是扣除點數）
                if (points < 0 && wallet.UserPoint < Math.Abs(points))
                {
                    return WalletMutationResult.CreateFailure(
                        $"點數不足，目前餘額：{wallet.UserPoint}，需要扣除：{Math.Abs(points)}",
                        userId);
                }

                // 更新點數
                wallet.UserPoint += points;
                var balanceAfter = wallet.UserPoint;

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = points > 0 ? "管理員發放點數" : "管理員扣除點數",
                    PointsChanged = points,
                    Description = $"{reason} (操作者: 管理員 ID {operatorId})",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "管理員 {OperatorId} 對會員 {UserId} 發放點數 {Points}，原因：{Reason}",
                    operatorId, userId, points, reason);

                return new WalletMutationResult
                {
                    Success = true,
                    Message = points > 0
                        ? $"成功發放 {points} 點數給會員 {user.UserName} (ID: {userId})"
                        : $"成功扣除會員 {user.UserName} (ID: {userId}) {Math.Abs(points)} 點數",
                    UserId = userId,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    HistoryLogId = history.LogId,
                    OperationTime = history.ChangeTime
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "發放點數失敗，會員ID: {UserId}, 點數: {Points}", userId, points);
                return WalletMutationResult.CreateFailure($"發放點數失敗：{ex.Message}", userId);
            }
        }

        /// <summary>
        /// 發放商城優惠券
        /// </summary>
        public async Task<WalletMutationResult> IssueCouponAsync(int userId, int couponTypeId, int operatorId, int quantity = 1)
        {
            if (quantity <= 0 || quantity > 100)
            {
                return WalletMutationResult.CreateFailure("發放數量必須介於 1 到 100 之間", userId);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 驗證會員存在
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return WalletMutationResult.CreateFailure($"找不到會員 ID: {userId}", userId);
                }

                // 驗證優惠券類型存在
                var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
                if (couponType == null)
                {
                    return WalletMutationResult.CreateFailure($"找不到優惠券類型 ID: {couponTypeId}", userId);
                }

                // 檢查優惠券是否有效
                var now = DateTime.UtcNow;
                if (couponType.ValidTo < now)
                {
                    return WalletMutationResult.CreateFailure(
                        $"優惠券類型「{couponType.Name}」已過期（到期日：{couponType.ValidTo:yyyy-MM-dd}）",
                        userId);
                }

                var generatedCodes = new List<string>();

                // 批量發放優惠券
                for (int i = 0; i < quantity; i++)
                {
                    // 生成唯一優惠券序號 CPN-{年月}-{6位隨機碼}
                    var couponCode = GenerateUniqueCouponCode();

                    var coupon = new Coupon
                    {
                        CouponCode = couponCode,
                        CouponTypeId = couponTypeId,
                        UserId = userId,
                        IsUsed = false,
                        AcquiredTime = now,
                        UsedTime = null,
                        UsedInOrderId = null
                    };
                    _context.Coupons.Add(coupon);
                    generatedCodes.Add(couponCode);
                }

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "管理員發放優惠券",
                    PointsChanged = 0,
                    ItemCode = string.Join(", ", generatedCodes),
                    Description = $"發放優惠券「{couponType.Name}」x{quantity} (操作者: 管理員 ID {operatorId})",
                    ChangeTime = now
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "管理員 {OperatorId} 對會員 {UserId} 發放優惠券「{CouponType}」x{Quantity}",
                    operatorId, userId, couponType.Name, quantity);

                return new WalletMutationResult
                {
                    Success = true,
                    Message = $"成功發放 {quantity} 張優惠券「{couponType.Name}」給會員 {user.UserName} (ID: {userId})",
                    UserId = userId,
                    GeneratedCode = generatedCodes.FirstOrDefault(),
                    GeneratedCodes = generatedCodes,
                    HistoryLogId = history.LogId,
                    OperationTime = now
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "發放優惠券失敗，會員ID: {UserId}, 優惠券類型ID: {CouponTypeId}", userId, couponTypeId);
                return WalletMutationResult.CreateFailure($"發放優惠券失敗：{ex.Message}", userId);
            }
        }

        /// <summary>
        /// 發放電子禮券
        /// </summary>
        public async Task<WalletMutationResult> IssueEVoucherAsync(int userId, int evoucherTypeId, int operatorId, int quantity = 1)
        {
            if (quantity <= 0 || quantity > 100)
            {
                return WalletMutationResult.CreateFailure("發放數量必須介於 1 到 100 之間", userId);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 驗證會員存在
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return WalletMutationResult.CreateFailure($"找不到會員 ID: {userId}", userId);
                }

                // 驗證電子禮券類型存在
                var evoucherType = await _context.EvoucherTypes.FindAsync(evoucherTypeId);
                if (evoucherType == null)
                {
                    return WalletMutationResult.CreateFailure($"找不到電子禮券類型 ID: {evoucherTypeId}", userId);
                }

                // 檢查電子禮券是否有效
                var now = DateTime.UtcNow;
                if (evoucherType.ValidTo < now)
                {
                    return WalletMutationResult.CreateFailure(
                        $"電子禮券類型「{evoucherType.Name}」已過期（到期日：{evoucherType.ValidTo:yyyy-MM-dd}）",
                        userId);
                }

                // 檢查庫存
                var issuedCount = await _context.Evouchers
                    .CountAsync(e => e.EvoucherTypeId == evoucherTypeId);

                if (evoucherType.TotalAvailable > 0 && issuedCount + quantity > evoucherType.TotalAvailable)
                {
                    return WalletMutationResult.CreateFailure(
                        $"電子禮券「{evoucherType.Name}」庫存不足，可用數量：{evoucherType.TotalAvailable - issuedCount}",
                        userId);
                }

                var generatedCodes = new List<string>();

                // 批量發放電子禮券
                for (int i = 0; i < quantity; i++)
                {
                    // 生成唯一電子禮券序號 EV-{類型代碼}-{4位隨機碼}-{6位數字}
                    var evoucherCode = GenerateUniqueEVoucherCode(evoucherType.Name);

                    var evoucher = new Evoucher
                    {
                        EvoucherCode = evoucherCode,
                        EvoucherTypeId = evoucherTypeId,
                        UserId = userId,
                        IsUsed = false,
                        AcquiredTime = now,
                        UsedTime = null
                    };
                    _context.Evouchers.Add(evoucher);
                    generatedCodes.Add(evoucherCode);
                }

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "管理員發放電子禮券",
                    PointsChanged = 0,
                    ItemCode = string.Join(", ", generatedCodes),
                    Description = $"發放電子禮券「{evoucherType.Name}」x{quantity} (操作者: 管理員 ID {operatorId})",
                    ChangeTime = now
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "管理員 {OperatorId} 對會員 {UserId} 發放電子禮券「{EVoucherType}」x{Quantity}",
                    operatorId, userId, evoucherType.Name, quantity);

                return new WalletMutationResult
                {
                    Success = true,
                    Message = $"成功發放 {quantity} 張電子禮券「{evoucherType.Name}」給會員 {user.UserName} (ID: {userId})",
                    UserId = userId,
                    GeneratedCode = generatedCodes.FirstOrDefault(),
                    GeneratedCodes = generatedCodes,
                    HistoryLogId = history.LogId,
                    OperationTime = now
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "發放電子禮券失敗，會員ID: {UserId}, 電子禮券類型ID: {EVoucherTypeId}", userId, evoucherTypeId);
                return WalletMutationResult.CreateFailure($"發放電子禮券失敗：{ex.Message}", userId);
            }
        }

        /// <summary>
        /// 調整電子禮券（撤銷/標記已使用/恢復）
        /// </summary>
        public async Task<WalletMutationResult> AdjustEVoucherAsync(int evoucherId, string action, int operatorId)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return WalletMutationResult.CreateFailure("必須指定操作類型");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 查詢電子禮券
                var evoucher = await _context.Evouchers
                    .Include(e => e.EvoucherType)
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EvoucherId == evoucherId);

                if (evoucher == null)
                {
                    return WalletMutationResult.CreateFailure($"找不到電子禮券 ID: {evoucherId}");
                }

                var now = DateTime.UtcNow;
                string operationDesc = string.Empty;

                switch (action.ToLowerInvariant())
                {
                    case "revoke":
                        // 撤銷（刪除）電子禮券
                        if (evoucher.IsUsed)
                        {
                            return WalletMutationResult.CreateFailure(
                                $"電子禮券「{evoucher.EvoucherCode}」已被使用，無法撤銷",
                                evoucher.UserId);
                        }

                        _context.Evouchers.Remove(evoucher);
                        operationDesc = $"撤銷電子禮券「{evoucher.EvoucherType?.Name ?? "未知"}」，序號：{evoucher.EvoucherCode}";
                        break;

                    case "markused":
                        // 標記為已使用
                        if (evoucher.IsUsed)
                        {
                            return WalletMutationResult.CreateFailure(
                                $"電子禮券「{evoucher.EvoucherCode}」已被標記為已使用",
                                evoucher.UserId);
                        }

                        evoucher.IsUsed = true;
                        evoucher.UsedTime = now;
                        operationDesc = $"標記電子禮券「{evoucher.EvoucherType?.Name ?? "未知"}」為已使用，序號：{evoucher.EvoucherCode}";
                        break;

                    case "restore":
                        // 恢復為未使用
                        if (!evoucher.IsUsed)
                        {
                            return WalletMutationResult.CreateFailure(
                                $"電子禮券「{evoucher.EvoucherCode}」未被使用，無需恢復",
                                evoucher.UserId);
                        }

                        evoucher.IsUsed = false;
                        evoucher.UsedTime = null;
                        operationDesc = $"恢復電子禮券「{evoucher.EvoucherType?.Name ?? "未知"}」為未使用狀態，序號：{evoucher.EvoucherCode}";
                        break;

                    default:
                        return WalletMutationResult.CreateFailure($"不支援的操作類型：{action}");
                }

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = evoucher.UserId,
                    ChangeType = "管理員調整電子禮券",
                    PointsChanged = 0,
                    ItemCode = evoucher.EvoucherCode,
                    Description = $"{operationDesc} (操作者: 管理員 ID {operatorId})",
                    ChangeTime = now
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "管理員 {OperatorId} 調整電子禮券 {EVoucherId}，操作：{Action}",
                    operatorId, evoucherId, action);

                return new WalletMutationResult
                {
                    Success = true,
                    Message = operationDesc,
                    UserId = evoucher.UserId,
                    GeneratedCode = evoucher.EvoucherCode,
                    HistoryLogId = history.LogId,
                    OperationTime = now
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "調整電子禮券失敗，電子禮券ID: {EVoucherId}, 操作: {Action}", evoucherId, action);
                return WalletMutationResult.CreateFailure($"調整電子禮券失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量發放點數給多個會員
        /// </summary>
        public async Task<BatchWalletMutationResult> IssuePointsBatchAsync(int[] userIds, int points, string reason, int operatorId)
        {
            var result = new BatchWalletMutationResult
            {
                TotalCount = userIds.Length
            };

            foreach (var userId in userIds)
            {
                var mutationResult = await IssuePointsAsync(userId, points, reason, operatorId);
                result.Results.Add(mutationResult);

                if (mutationResult.Success)
                {
                    result.SuccessCount++;
                }
                else
                {
                    result.FailureCount++;
                }
            }

            return result;
        }

        #region 私有輔助方法

        /// <summary>
        /// 生成唯一優惠券序號 CPN-{年月}-{6位隨機碼}
        /// </summary>
        private string GenerateUniqueCouponCode()
        {
            var now = DateTime.UtcNow;
            var yearMonth = now.ToString("yyMM");
            var random = new Random();
            string code;
            int attempts = 0;

            do
            {
                var randomCode = new string(Enumerable.Range(0, 6)
                    .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[random.Next(36)])
                    .ToArray());
                code = $"CPN-{yearMonth}-{randomCode}";
                attempts++;

                // 防止無限迴圈
                if (attempts > 100)
                {
                    code = $"CPN-{yearMonth}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
                    break;
                }
            }
            while (_context.Coupons.Any(c => c.CouponCode == code));

            return code;
        }

        /// <summary>
        /// 生成唯一電子禮券序號 EV-{類型代碼}-{4位隨機碼}-{6位數字}
        /// </summary>
        private string GenerateUniqueEVoucherCode(string typeName)
        {
            // 從類型名稱提取類型代碼
            string typeCode = ExtractTypeCode(typeName);
            var random = new Random();
            string code;
            int attempts = 0;

            do
            {
                var randomCode = new string(Enumerable.Range(0, 4)
                    .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)])
                    .ToArray());
                var randomDigits = random.Next(0, 1000000).ToString("D6");
                code = $"EV-{typeCode}-{randomCode}-{randomDigits}";
                attempts++;

                // 防止無限迴圈
                if (attempts > 100)
                {
                    code = $"EV-{typeCode}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}-{random.Next(0, 1000000):D6}";
                    break;
                }
            }
            while (_context.Evouchers.Any(e => e.EvoucherCode == code));

            return code;
        }

        /// <summary>
        /// 從類型名稱提取類型代碼
        /// </summary>
        private string ExtractTypeCode(string typeName)
        {
            if (typeName.Contains("現金")) return "CASH";
            if (typeName.Contains("電影")) return "MOVIE";
            if (typeName.Contains("餐")) return "FOOD";
            if (typeName.Contains("加油")) return "GAS";
            if (typeName.Contains("咖啡")) return "COFFEE";
            if (typeName.Contains("購物")) return "SHOP";
            if (typeName.Contains("書店")) return "BOOK";
            if (typeName.Contains("超商")) return "STORE";
            return "GIFT";
        }

        #endregion
    }
}
