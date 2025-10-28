using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ThreadPost
{
    public long Id { get; set; }

    public long? ThreadId { get; set; }

    public int? AuthorUserId { get; set; }

    public string? ContentMd { get; set; }

    public long? ParentPostId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? AuthorUser { get; set; }

    public virtual ICollection<ThreadPost> InverseParentPost { get; set; } = new List<ThreadPost>();

    public virtual ThreadPost? ParentPost { get; set; }

    public virtual Thread? Thread { get; set; }
}
