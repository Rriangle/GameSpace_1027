using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class UserWallet
{
    public int UserId { get; set; }

    public int UserPoint { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual User User { get; set; } = null!;
}
