using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class PetBackgroundCostSetting
{
    public int SettingId { get; set; }

    public string BackgroundCode { get; set; } = null!;

    public string BackgroundName { get; set; } = null!;

    public int PointsCost { get; set; }

    public string? Description { get; set; }

    public string? PreviewImagePath { get; set; }

    public bool IsActive { get; set; }

    public int? DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Rarity { get; set; }

    public virtual ManagerDatum? UpdatedByNavigation { get; set; }
}
