//using System.Collections.Generic;

//namespace GameSpace.Areas.Forum.Models.Posts
//{
//    public class PostsListVm
//    {
//        public long ThreadId { get; set; }

//        // 篩選 / 查詢
//        public string? Q { get; set; }             // 內容關鍵字
//        public string? Status { get; set; }        // normal/hidden/deleted/ALL(null)

//        // 分頁
//        public int Page { get; set; } = 1;
//        public int Size { get; set; } = 20;
//        public int Total { get; set; }

//        public List<ThreadPostRowVm> Items { get; set; } = new();
//    }
//}

using GameSpace.Areas.Forum.Models.Posts;
using System.Collections.Generic;

namespace GameSpace.Areas.Forum.Models
{
    //public class ThreadsListVm
    //{
    //    public int ForumId { get; set; }

    //    // 篩選 / 查詢
    //    public string? Q { get; set; }
    //    public string? Status { get; set; }

    //    // 分頁
    //    public int Page { get; set; } = 1;
    //    public int Size { get; set; } = 20;
    //    public int Total { get; set; }

    //    // 主題列
    //    public List<ThreadRowVm> Items { get; set; } = new();
    //}

    public class PostsListVm
    {
        public long ThreadId { get; set; }

        // 篩選 / 查詢
        public string? Q { get; set; }
        public string? Status { get; set; }

        // 分頁
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public int Total { get; set; }

        public List<ThreadPostRowVm> Items { get; set; } = new();
    }
}
