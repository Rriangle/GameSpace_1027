//namespace GameSpace.Areas.Forum.view_Models
//{

//    /// <summary>
//    /// 回復PO文 ViewModel
//    /// </summary>
//    public class ThreadPostRowVm
//    {
//        public int PostId { get; set; }
//        public int ThreadId { get; set; }
//        public string AuthorName { get; set; } = string.Empty; // 來源：Users.DisplayName 或 Nickname
//        public string ContentMd { get; set; } = string.Empty;
//        public string Status { get; set; } = "normal"; // normal / hidden / deleted
//        public int LikeCount { get; set; }
//        public int ReplyCount { get; set; } // 若有二層回覆，沒有就留著0
//        public DateTime CreatedAt { get; set; }
//        public DateTime? UpdatedAt { get; set; }

//    }
//}

namespace GameSpace.Areas.Forum.Models.Posts
{
    public class ThreadPostRowVm
    {
        public long Id { get; set; }
        public long? ThreadId { get; set; }
        public int? AuthorUserId { get; set; }
        public string ContentMd { get; set; } = "";
        public long? ParentPostId { get; set; }     // 二層回覆用，允許 NULL
        public string Status { get; set; } = "normal";
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // 額外顯示用
        public string? AuthorName { get; set; }
        public int LikeCount { get; set; }       // ← 新增
        public int BookmarkCount { get; set; }   // ← 新增

    }
}

