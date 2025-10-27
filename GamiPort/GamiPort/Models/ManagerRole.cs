using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class ManagerRole
{
    public int ManagerId { get; set; }

    public int ManagerRoleId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual ManagerDatum Manager { get; set; } = null!;

    public virtual ManagerRolePermission ManagerRoleNavigation { get; set; } = null!;
}
