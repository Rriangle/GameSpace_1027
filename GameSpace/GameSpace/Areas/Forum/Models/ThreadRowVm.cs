namespace GameSpace.Areas.Forum.Models
{
    // 主題列表每一列
    public class ThreadRowVm
    {
        public long ThreadId { get; set; }
        public int? ForumId { get; set; }          // 對應 DB 可能為 NULL
        public string Title { get; set; } = "";
        public string Status { get; set; } = "normal";
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // 衍生/聚合欄位
        public string? AuthorName { get; set; }
        public int ReplyCount { get; set; }
        public int LikeCount { get; set; }
        public int BookmarkCount { get; set; }   // ← 新增
    }
}
