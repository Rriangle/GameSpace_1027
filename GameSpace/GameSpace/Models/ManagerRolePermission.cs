using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ManagerRolePermission
{
    public int ManagerRoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public bool? AdministratorPrivilegesManagement { get; set; }

    public bool? UserStatusManagement { get; set; }

    public bool? ShoppingPermissionManagement { get; set; }

    public bool? MessagePermissionManagement { get; set; }

    public bool? PetRightsManagement { get; set; }

    public bool? CustomerService { get; set; }

    public virtual ICollection<ManagerDatum> Managers { get; set; } = new List<ManagerDatum>();
}
