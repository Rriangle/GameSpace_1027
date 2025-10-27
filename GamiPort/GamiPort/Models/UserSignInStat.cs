using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class UserSignInStat
{
    public int LogId { get; set; }

    public DateTime SignTime { get; set; }

    public int UserId { get; set; }

    public int PointsGained { get; set; }

    public DateTime PointsGainedTime { get; set; }

    public int ExpGained { get; set; }

    public DateTime ExpGainedTime { get; set; }

    public string CouponGained { get; set; } = null!;

    public DateTime CouponGainedTime { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual User User { get; set; } = null!;
}
