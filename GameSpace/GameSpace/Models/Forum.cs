using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Forum
{
    public int ForumId { get; set; }

    public int? GameId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Game? Game { get; set; }

    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();
}
