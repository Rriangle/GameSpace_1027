using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class MiniGame
{
    public int PlayId { get; set; }

    public int UserId { get; set; }

    public int PetId { get; set; }

    public int Level { get; set; }

    public int MonsterCount { get; set; }

    public decimal SpeedMultiplier { get; set; }

    public string Result { get; set; } = null!;

    public int ExpGained { get; set; }

    public DateTime ExpGainedTime { get; set; }

    public int PointsGained { get; set; }

    public DateTime PointsGainedTime { get; set; }

    public string CouponGained { get; set; } = null!;

    public DateTime CouponGainedTime { get; set; }

    public int HungerDelta { get; set; }

    public int MoodDelta { get; set; }

    public int StaminaDelta { get; set; }

    public int CleanlinessDelta { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool Aborted { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
