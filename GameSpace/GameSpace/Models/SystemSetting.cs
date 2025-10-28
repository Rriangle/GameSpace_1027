using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SystemSetting
{
    public int SettingId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string? Description { get; set; }

    public string Category { get; set; } = null!;

    public string SettingType { get; set; } = null!;

    public bool IsReadOnly { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ManagerDatum? UpdatedByNavigation { get; set; }
}
