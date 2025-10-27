using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Mute
{
    public int MuteId { get; set; }

    public string Word { get; set; } = null!;

    public string? Replacement { get; set; }

    public bool IsActive { get; set; }

    public int? ManagerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ManagerDatum? Manager { get; set; }
}
