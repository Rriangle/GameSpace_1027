using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class Reaction
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string? TargetType { get; set; }

    public long? TargetId { get; set; }

    public string? Kind { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
