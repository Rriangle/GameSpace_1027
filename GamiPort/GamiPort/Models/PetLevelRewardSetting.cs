using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class PetLevelRewardSetting
{
    public int SettingId { get; set; }

    public int LevelRangeStart { get; set; }

    public int LevelRangeEnd { get; set; }

    public int PointsReward { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
