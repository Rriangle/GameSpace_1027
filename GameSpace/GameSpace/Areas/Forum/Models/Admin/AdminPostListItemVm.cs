namespace GameSpace.Areas.Forum.Models.Admin
{
    public class AdminPostListItemVm
    {
        public int AdminPostId { get; set; }   // ← 映射自 Post.PostId（讓你的 asp-route-id 不用改）
        public string Title { get; set; } = "";
        public string Status { get; set; } = "";
        public bool Pinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }  // 如果之後要顯示
    }
}
