using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PostSource
{
    public int Id { get; set; }

    public int? PostId { get; set; }

    public string? SourceName { get; set; }

    public string? Url { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Post? Post { get; set; }
}
