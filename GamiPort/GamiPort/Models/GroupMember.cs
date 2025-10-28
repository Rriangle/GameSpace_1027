using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class GroupMember
{
    public int GroupMemberId { get; set; }

    public int GroupId { get; set; }

    public int UserId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
