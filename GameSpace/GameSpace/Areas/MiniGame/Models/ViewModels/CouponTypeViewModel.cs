using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 優惠券類型 ViewModel
    /// </summary>
    public class CouponTypeViewModel
    {
        /// <summary>
        /// 優惠券類型ID
        /// </summary>
        public int CouponTypeId { get; set; }

        /// <summary>
        /// 類型名稱
        /// </summary>
        [Required(ErrorMessage = "類型名稱為必填")]
        [StringLength(100, ErrorMessage = "類型名稱長度不可超過 100 字元")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 折扣類型 (Amount 或 Percent)
        /// </summary>
        [Required(ErrorMessage = "折扣類型為必填")]
        [StringLength(20)]
        public string DiscountType { get; set; } = "Amount";

        /// <summary>
        /// 折扣值（金額或百分比）
        /// </summary>
        [Required(ErrorMessage = "折扣值為必填")]
        [Range(0.01, 100000, ErrorMessage = "折扣值必須在 0.01-100000 之間")]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// 最低消費金額
        /// </summary>
        [Range(0, 1000000, ErrorMessage = "最低消費金額必須在 0-1000000 之間")]
        public decimal MinSpend { get; set; } = 0;

        /// <summary>
        /// 有效期限起始
        /// </summary>
        [Required(ErrorMessage = "有效期限起始為必填")]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// 有效期限結束
        /// </summary>
        [Required(ErrorMessage = "有效期限結束為必填")]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// 兌換所需點數
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "兌換所需點數必須大於等於 0")]
        public int PointsCost { get; set; } = 0;

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 最大發行數量（-1 表示無限制）
        /// </summary>
        public int MaxIssueCount { get; set; } = -1;

        /// <summary>
        /// 已發行數量
        /// </summary>
        public int IssuedCount { get; set; } = 0;

        /// <summary>
        /// 使用條款
        /// </summary>
        [StringLength(1000)]
        public string? TermsOfUse { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 是否為有效期間內
        /// </summary>
        public bool IsValidPeriod => DateTime.Now >= ValidFrom && DateTime.Now <= ValidTo;

        /// <summary>
        /// 是否還有庫存
        /// </summary>
        public bool HasStock => MaxIssueCount == -1 || IssuedCount < MaxIssueCount;

        /// <summary>
        /// 剩餘數量
        /// </summary>
        public int? RemainingStock => MaxIssueCount == -1 ? null : MaxIssueCount - IssuedCount;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 優惠券類型建立請求
    /// </summary>
    public class CreateCouponTypeRequest
    {
        /// <summary>
        /// 類型名稱
        /// </summary>
        [Required(ErrorMessage = "類型名稱為必填")]
        [StringLength(100, ErrorMessage = "類型名稱長度不可超過 100 字元")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 折扣類型 (Amount 或 Percent)
        /// </summary>
        [Required(ErrorMessage = "折扣類型為必填")]
        [StringLength(20)]
        public string DiscountType { get; set; } = "Amount";

        /// <summary>
        /// 折扣值
        /// </summary>
        [Required(ErrorMessage = "折扣值為必填")]
        [Range(0.01, 100000, ErrorMessage = "折扣值必須在 0.01-100000 之間")]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// 最低消費金額
        /// </summary>
        [Range(0, 1000000)]
        public decimal MinSpend { get; set; } = 0;

        /// <summary>
        /// 有效期限起始
        /// </summary>
        [Required(ErrorMessage = "有效期限起始為必填")]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// 有效期限結束
        /// </summary>
        [Required(ErrorMessage = "有效期限結束為必填")]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// 兌換所需點數
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PointsCost { get; set; } = 0;

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 最大發行數量
        /// </summary>
        public int MaxIssueCount { get; set; } = -1;

        /// <summary>
        /// 使用條款
        /// </summary>
        [StringLength(1000)]
        public string? TermsOfUse { get; set; }
    }

    /// <summary>
    /// 優惠券類型更新請求
    /// </summary>
    public class UpdateCouponTypeRequest
    {
        /// <summary>
        /// 優惠券類型ID
        /// </summary>
        [Required(ErrorMessage = "優惠券類型ID為必填")]
        public int CouponTypeId { get; set; }

        /// <summary>
        /// 類型名稱
        /// </summary>
        [Required(ErrorMessage = "類型名稱為必填")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 折扣類型
        /// </summary>
        [Required(ErrorMessage = "折扣類型為必填")]
        [StringLength(20)]
        public string DiscountType { get; set; } = "Amount";

        /// <summary>
        /// 折扣值
        /// </summary>
        [Required(ErrorMessage = "折扣值為必填")]
        [Range(0.01, 100000)]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// 最低消費金額
        /// </summary>
        [Range(0, 1000000)]
        public decimal MinSpend { get; set; }

        /// <summary>
        /// 有效期限起始
        /// </summary>
        [Required(ErrorMessage = "有效期限起始為必填")]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// 有效期限結束
        /// </summary>
        [Required(ErrorMessage = "有效期限結束為必填")]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// 兌換所需點數
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PointsCost { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 最大發行數量
        /// </summary>
        public int MaxIssueCount { get; set; }

        /// <summary>
        /// 使用條款
        /// </summary>
        [StringLength(1000)]
        public string? TermsOfUse { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }
    }
}
