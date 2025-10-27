namespace GameSpace.Areas.Forum.Models
{
    public class PostEditVm
    {
        public int? PostId { get; set; }              // Edit 用得到
        public string Title { get; set; } = "";
        public string? Tldr { get; set; }
        public string? BodyMd { get; set; }
        public bool Pinned { get; set; }

        // 可留著未來擴充
        public string? Type { get; set; } = "insight";
        public int? GameId { get; set; }

        public string? SourceName { get; set; }
        public string? SourceUrl { get; set; }
    }
}
