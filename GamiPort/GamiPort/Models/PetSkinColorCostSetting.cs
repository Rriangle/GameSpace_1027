using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class PetSkinColorCostSetting
{
    public int SettingId { get; set; }

    public string ColorCode { get; set; } = null!;

    public string ColorName { get; set; } = null!;

    public int PointsCost { get; set; }

    public string Rarity { get; set; } = null!;

    public string? Description { get; set; }

    public string? PreviewImagePath { get; set; }

    public string? ColorHex { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFree { get; set; }

    public bool IsLimitedEdition { get; set; }

    public DateTime? AvailableFrom { get; set; }

    public DateTime? AvailableUntil { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
