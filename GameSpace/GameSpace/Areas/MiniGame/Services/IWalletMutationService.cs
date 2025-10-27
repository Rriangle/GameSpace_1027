using System;
using System.Threading.Tasks;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 錢包異動服務介面 - 處理會員點數、優惠券、電子禮券的發放與調整
    /// </summary>
    public interface IWalletMutationService
    {
        /// <summary>
        /// 發放會員點數（支援增減）
        /// </summary>
        /// <param name="userId">會員ID</param>
        /// <param name="points">點數（正數為增加，負數為減少）</param>
        /// <param name="reason">發放原因</param>
        /// <param name="operatorId">操作者ID（管理員）</param>
        /// <returns>操作結果</returns>
        Task<WalletMutationResult> IssuePointsAsync(int userId, int points, string reason, int operatorId);

        /// <summary>
        /// 發放商城優惠券
        /// </summary>
        /// <param name="userId">會員ID</param>
        /// <param name="couponTypeId">優惠券類型ID</param>
        /// <param name="operatorId">操作者ID（管理員）</param>
        /// <param name="quantity">發放數量（預設1）</param>
        /// <returns>操作結果（包含生成的優惠券序號列表）</returns>
        Task<WalletMutationResult> IssueCouponAsync(int userId, int couponTypeId, int operatorId, int quantity = 1);

        /// <summary>
        /// 發放電子禮券
        /// </summary>
        /// <param name="userId">會員ID</param>
        /// <param name="evoucherTypeId">電子禮券類型ID</param>
        /// <param name="operatorId">操作者ID（管理員）</param>
        /// <param name="quantity">發放數量（預設1）</param>
        /// <returns>操作結果（包含生成的電子禮券序號列表）</returns>
        Task<WalletMutationResult> IssueEVoucherAsync(int userId, int evoucherTypeId, int operatorId, int quantity = 1);

        /// <summary>
        /// 調整電子禮券（撤銷/恢復/作廢）
        /// </summary>
        /// <param name="evoucherId">電子禮券ID</param>
        /// <param name="action">操作類型（Revoke=撤銷刪除, MarkUsed=標記已使用, Restore=恢復）</param>
        /// <param name="operatorId">操作者ID（管理員）</param>
        /// <returns>操作結果</returns>
        Task<WalletMutationResult> AdjustEVoucherAsync(int evoucherId, string action, int operatorId);

        /// <summary>
        /// 批量發放點數給多個會員
        /// </summary>
        /// <param name="userIds">會員ID列表</param>
        /// <param name="points">點數</param>
        /// <param name="reason">發放原因</param>
        /// <param name="operatorId">操作者ID</param>
        /// <returns>批量操作結果</returns>
        Task<BatchWalletMutationResult> IssuePointsBatchAsync(int[] userIds, int points, string reason, int operatorId);
    }

    /// <summary>
    /// 錢包異動操作結果
    /// </summary>
    public class WalletMutationResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 結果訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 錯誤訊息（如果失敗）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 受影響的會員ID
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 操作前餘額
        /// </summary>
        public int? BalanceBefore { get; set; }

        /// <summary>
        /// 操作後餘額
        /// </summary>
        public int? BalanceAfter { get; set; }

        /// <summary>
        /// 生成的序號（優惠券或電子禮券）
        /// </summary>
        public string? GeneratedCode { get; set; }

        /// <summary>
        /// 生成的序號列表（批量發放時）
        /// </summary>
        public List<string>? GeneratedCodes { get; set; }

        /// <summary>
        /// 交易歷史記錄ID
        /// </summary>
        public int? HistoryLogId { get; set; }

        /// <summary>
        /// 操作時間
        /// </summary>
        public DateTime OperationTime { get; set; }

        /// <summary>
        /// 建立成功結果
        /// </summary>
        public static WalletMutationResult CreateSuccess(string message, int? userId = null)
        {
            return new WalletMutationResult
            {
                Success = true,
                Message = message,
                UserId = userId,
                OperationTime = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        public static WalletMutationResult CreateFailure(string errorMessage, int? userId = null)
        {
            return new WalletMutationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                Message = "操作失敗",
                UserId = userId,
                OperationTime = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// 批量錢包異動操作結果
    /// </summary>
    public class BatchWalletMutationResult
    {
        /// <summary>
        /// 總數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 成功數
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失敗數
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 個別操作結果
        /// </summary>
        public List<WalletMutationResult> Results { get; set; } = new List<WalletMutationResult>();

        /// <summary>
        /// 整體操作是否成功（所有子操作都成功才算成功）
        /// </summary>
        public bool Success => FailureCount == 0 && SuccessCount == TotalCount;

        /// <summary>
        /// 結果訊息
        /// </summary>
        public string Message => $"批量操作完成：成功 {SuccessCount} 筆，失敗 {FailureCount} 筆，共 {TotalCount} 筆";
    }
}
