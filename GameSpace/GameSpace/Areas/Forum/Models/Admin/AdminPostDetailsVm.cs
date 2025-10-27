namespace GameSpace.Areas.Forum.Models.Admin
{
    public class AdminPostDetailsVm
    {
        public int PostId { get; set; }
        public string Title { get; set; } = "";
        public string? Tldr { get; set; }
        public string? BodyMd { get; set; }
        public string Status { get; set; } = "draft";
        public bool Pinned { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
