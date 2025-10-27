using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class Bookmark
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string? TargetType { get; set; }

    public long? TargetId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
