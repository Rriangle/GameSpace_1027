using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class UserRight
{
    public int UserId { get; set; }

    public bool? UserStatus { get; set; }

    public bool? ShoppingPermission { get; set; }

    public bool? MessagePermission { get; set; }

    public bool? SalesAuthority { get; set; }

    public virtual User User { get; set; } = null!;
}
