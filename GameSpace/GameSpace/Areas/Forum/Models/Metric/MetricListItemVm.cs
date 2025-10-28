namespace GameSpace.Areas.Forum.Models.Metric
{
    public class MetricListItemVm
    {
        public int MetricId { get; set; }
        public int SourceId { get; set; }
        public string SourceName { get; set; } = "";
        public string Code { get; set; } = "";
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
