using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Game
{
    public int GameId { get; set; }

    public string? Name { get; set; }

    public string? Genre { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? NameZh { get; set; }

    public virtual Forum? Forum { get; set; }

    public virtual ICollection<GameMetricDaily> GameMetricDailies { get; set; } = new List<GameMetricDaily>();

    public virtual ICollection<GameSourceMap> GameSourceMaps { get; set; } = new List<GameSourceMap>();

    public virtual ICollection<LeaderboardSnapshot> LeaderboardSnapshots { get; set; } = new List<LeaderboardSnapshot>();

    public virtual ICollection<PopularityIndexDaily> PopularityIndexDailies { get; set; } = new List<PopularityIndexDaily>();

    public virtual ICollection<PostMetricSnapshot> PostMetricSnapshots { get; set; } = new List<PostMetricSnapshot>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
