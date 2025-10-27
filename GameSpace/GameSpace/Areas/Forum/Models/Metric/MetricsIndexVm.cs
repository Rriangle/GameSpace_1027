namespace GameSpace.Areas.Forum.Models.Metric
{
    public class MetricsIndexVm
    {
        public DateOnly TargetDate { get; set; }
        public List<MetricListItemVm> List { get; set; } = new();
        public List<LeaderboardRowVm> Top10 { get; set; } = new();
    }

    public class LeaderboardRowVm
    {
        public int Rank { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public double Score { get; set; }
    }
}
