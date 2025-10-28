using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class Post
{
    public int PostId { get; set; }

    public string? Type { get; set; }

    public int? GameId { get; set; }

    public string? Title { get; set; }

    public string? Tldr { get; set; }

    public string? BodyMd { get; set; }

    public string? Status { get; set; }

    public bool? Pinned { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Game? Game { get; set; }

    public virtual PostMetricSnapshot? PostMetricSnapshot { get; set; }

    public virtual ICollection<PostSource> PostSources { get; set; } = new List<PostSource>();
}
