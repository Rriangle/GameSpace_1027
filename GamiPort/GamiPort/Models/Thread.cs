using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class Thread
{
    public long ThreadId { get; set; }

    public int? ForumId { get; set; }

    public int? AuthorUserId { get; set; }

    public string? Title { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? AuthorUser { get; set; }

    public virtual Forum? Forum { get; set; }

    public virtual ICollection<ThreadPost> ThreadPosts { get; set; } = new List<ThreadPost>();
}
