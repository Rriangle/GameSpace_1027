using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class Pet
{
    public int PetId { get; set; }

    public int UserId { get; set; }

    public string PetName { get; set; } = null!;

    public int Level { get; set; }

    public DateTime LevelUpTime { get; set; }

    public int Experience { get; set; }

    public int Hunger { get; set; }

    public int Mood { get; set; }

    public int Stamina { get; set; }

    public int Cleanliness { get; set; }

    public int Health { get; set; }

    public string SkinColor { get; set; } = null!;

    public DateTime SkinColorChangedTime { get; set; }

    public string BackgroundColor { get; set; } = null!;

    public DateTime BackgroundColorChangedTime { get; set; }

    public int PointsChangedSkinColor { get; set; }

    public int PointsChangedBackgroundColor { get; set; }

    public int PointsGainedLevelUp { get; set; }

    public DateTime PointsGainedTimeLevelUp { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public int CurrentExperience { get; set; }

    public int? ExperienceToNextLevel { get; set; }

    public int? TotalPointsGainedLevelUp { get; set; }

    public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();

    public virtual User User { get; set; } = null!;
}
