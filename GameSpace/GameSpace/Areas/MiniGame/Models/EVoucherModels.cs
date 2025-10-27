using System;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    // Form models for EVoucher create/edit pages
    public class EVoucherCreateModel
    {
        [Required]
        [Display(Name = "EVoucher Code")]
        public string EvoucherCode { get; set; } = string.Empty;
        public string EVoucherCode { get => EvoucherCode; set => EvoucherCode = value; }

        [Required]
        [Display(Name = "User Id")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "EVoucher Type")]
        public int EvoucherTypeId { get; set; }
        public int EVoucherTypeID { get => EvoucherTypeId; set => EvoucherTypeId = value; }

        [Display(Name = "Acquired Time")]
        public DateTime? AcquiredTime { get; set; }

        // Optional fields referenced by views
        [Display(Name = "Value")]
        public decimal? Value { get; set; }

        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Is Used")]
        public bool IsUsed { get; set; }

        [Display(Name = "Used Date")]
        public DateTime? UsedDate { get; set; }

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "描述長度不可超過 1000 字元")]
        public string? Description { get; set; }
    }
}

