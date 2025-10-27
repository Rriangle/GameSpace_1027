using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class GroupReadState
{
    public int StateId { get; set; }

    public int GroupId { get; set; }

    public int UserId { get; set; }

    public DateTime? LastReadAt { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
