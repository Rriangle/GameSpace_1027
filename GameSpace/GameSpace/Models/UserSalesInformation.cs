using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class UserSalesInformation
{
    public int UserId { get; set; }

    public int? UserSalesWallet { get; set; }

    public virtual User User { get; set; } = null!;
}
