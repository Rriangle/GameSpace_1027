using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// EF 實體 / DbContext
using GameSpace.Models;

// ViewModels
using GameSpace.Areas.Forum.Models;       // ThreadRowVm, ThreadsListVm
using GameSpace.Areas.Forum.Models.Posts; // ThreadPostRowVm, PostsListVm

namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class ThreadsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public ThreadsController(GameSpacedatabaseContext db) => _db = db;

        // 1) 論壇清單
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string sort = "created", string dir = "desc")
        {
            var query = _db.Forums.AsNoTracking();

            // 搜尋（名稱包含）
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(f => f.Name.Contains(q));

            // 排序（id / name / created）
            bool desc = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
            query = sort switch
            {
                "id" => desc ? query.OrderByDescending(f => f.ForumId)
                             : query.OrderBy(f => f.ForumId),

                "name" => desc
                    ? query.OrderByDescending(f => EF.Functions.Collate(f.Name, "Chinese_Taiwan_Stroke_CI_AS"))
                           .ThenBy(f => f.ForumId)
                    : query.OrderBy(f => EF.Functions.Collate(f.Name, "Chinese_Taiwan_Stroke_CI_AS"))
                           .ThenBy(f => f.ForumId),

                _ /*created*/ => desc ? query.OrderByDescending(f => f.CreatedAt).ThenBy(f => f.ForumId)
                                      : query.OrderBy(f => f.CreatedAt).ThenBy(f => f.ForumId)
            };

            ViewBag.Q = q;
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            var list = await query.ToListAsync();
            return View(list); // 型別仍是 IEnumerable<GameSpace.Models.Forum>
        }


        // 2) 主題清單（搜尋/篩選/分頁）
        [HttpGet]
        public async Task<IActionResult> List(int forumId, string? q, string? status, int page = 1, int size = 20)
        {
            if (page < 1) page = 1;
            if (size < 1) size = 20;

            var query = _db.Threads.AsNoTracking().Where(t => t.ForumId == forumId);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => t.Title.Contains(q));

            if (!string.IsNullOrWhiteSpace(status) &&
                !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                query = query.Where(t => t.Status == status);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(t => new ThreadRowVm
                {
                    ThreadId = t.ThreadId,
                    ForumId = t.ForumId,
                    Title = t.Title,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ReplyCount = _db.ThreadPosts.Count(p => p.ThreadId == t.ThreadId && p.Status == "normal"),
                    LikeCount = _db.Reactions.Count(r => r.TargetType == "thread" && r.TargetId == t.ThreadId && r.Kind == "like"),
                    BookmarkCount = _db.Bookmarks.Count(b => b.TargetType == "thread" && b.TargetId == t.ThreadId)
                })
                .ToListAsync();

            var vm = new ThreadsListVm
            {
                ForumId = forumId,
                Q = q,
                Status = string.IsNullOrWhiteSpace(status) ? "ALL" : status,
                Page = page,
                Size = size,
                Total = total,
                Items = items
            };

            return View(vm);
        }

        // 3) 修改主題狀態（white-list）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long id, string status, int? forumId)
        {
            var t = await _db.Threads.FirstOrDefaultAsync(x => x.ThreadId == id);
            if (t == null) return NotFound();

            status = (status ?? "").ToLowerInvariant();
            if (status is not ("normal" or "hidden" or "archived" or "deleted"))
                status = t.Status; // 無效值直接忽略

            if (t.Status != status)
            {
                t.Status = status;
                t.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(List), new { forumId = forumId ?? t.ForumId });
        }

        // 4) 回覆清單（帶 forumId 回去、帶作者顯示名）
        [HttpGet]
        public async Task<IActionResult> Posts(long threadId)
        {
            // 先取 forumId 讓「回主題列表」有得帶
            var t = await _db.Threads
                .AsNoTracking()
                .Where(x => x.ThreadId == threadId)
                .Select(x => new { x.ForumId })
                .FirstOrDefaultAsync();
            if (t == null) return NotFound();

            var rows = await (
                from p in _db.ThreadPosts.AsNoTracking().Where(p => p.ThreadId == threadId)
                join u in _db.Users.AsNoTracking() on p.AuthorUserId equals u.UserId into uu
                from u in uu.DefaultIfEmpty() // 作者被刪也不炸
                orderby p.CreatedAt
                select new ThreadPostRowVm
                {
                    Id = p.Id,
                    ThreadId = p.ThreadId,
                    AuthorUserId = p.AuthorUserId,
                    AuthorName = u.UserName, // ★ 只用現有欄位
                    ContentMd = p.ContentMd,
                    ParentPostId = p.ParentPostId,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    LikeCount = _db.Reactions.Count(r => r.TargetType == "post" && r.TargetId == p.Id && r.Kind == "like"),
                    BookmarkCount = _db.Bookmarks.Count(b => b.TargetType == "post" && b.TargetId == p.Id)
                })
                .ToListAsync();

            var vm = new PostsListVm { ThreadId = threadId, Total = rows.Count, Items = rows };
            ViewBag.ForumId = t.ForumId; // ★ 給「← 回主題列表」
            return View(vm);
        }


        // 5) 修改回覆狀態（white-list）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePostStatus(long id, string status, long threadId)
        {
            var p = await _db.ThreadPosts.FirstOrDefaultAsync(x => x.Id == id && x.ThreadId == threadId);
            if (p == null) return NotFound();

            status = (status ?? "").ToLowerInvariant();
            if (status is not ("normal" or "hidden" or "deleted"))
                status = p.Status;

            if (p.Status != status)
            {
                p.Status = status;
                p.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Posts), new { threadId });
        }






    }
}
