namespace GameSpace.Areas.Forum.Models
{
    public class ThreadsListVm
    {
        public int ForumId { get; set; }
        public string? Q { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public int Total { get; set; }
        public List<ThreadRowVm> Items { get; set; } = new();
    }
}
