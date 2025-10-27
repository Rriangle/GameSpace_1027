using GameSpace.Areas.Forum.Models;
using GameSpace.Areas.Forum.Models.Admin;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class AdminPostsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public AdminPostsController(GameSpacedatabaseContext db) => _db = db;

        // ===== 列表 =====
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _db.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.Pinned)
                .ThenByDescending(p => p.PublishedAt)
                .ThenByDescending(p => p.CreatedAt)
                .Select(p => new AdminPostListItemVm
                {
                    AdminPostId = p.PostId,
                    Title = p.Title,
                    Status = p.Status ?? "draft",
                    Pinned = p.Pinned ?? false,
                    CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                    PublishedAt = p.PublishedAt
                })
                .ToListAsync();

            return View(list);
        }

        // ===== 建立 =====
        [HttpGet]
        public IActionResult Create() => View(new PostEditVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var entity = new Post
            {
                Title = vm.Title,
                Tldr = vm.Tldr,
                BodyMd = vm.BodyMd,
                Pinned = vm.Pinned,
                Status = "draft",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Type = vm.Type ?? "insight",
                GameId = vm.GameId
            };

            _db.Posts.Add(entity);
            await _db.SaveChangesAsync();
            TempData["ok"] = "已建立草稿。";
            return RedirectToAction(nameof(Index));
        }

        // ===== 編輯 =====
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            var vm = new PostEditVm
            {
                PostId = p.PostId,
                Title = p.Title,
                Tldr = p.Tldr,
                BodyMd = p.BodyMd,
                Pinned = p.Pinned ?? false,
                Type = p.Type,
                GameId = p.GameId
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditVm vm)
        {
            if (!vm.PostId.HasValue) return BadRequest();

            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == vm.PostId.Value);
            if (p == null) return NotFound();

            p.Title = vm.Title;
            p.Tldr = vm.Tldr;
            p.BodyMd = vm.BodyMd;
            p.Pinned = vm.Pinned;
            p.Type = vm.Type ?? p.Type;
            p.GameId = vm.GameId;
            p.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["ok"] = "已儲存變更。";
            return RedirectToAction(nameof(Index));
        }

        // ===== 發布（Index 按鈕）=====
        [HttpPost]
        [ValidateAntiForgeryToken] // ← 關鍵：配合 View 的 AntiForgeryToken
        public async Task<IActionResult> Publish(int id)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            if ((p.Status ?? "draft") != "published")
            {
                p.Status = "published";
                p.PublishedAt = DateTime.UtcNow;
                p.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                TempData["ok"] = "已發布。";
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== 隱藏（Index 按鈕）=====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            p.Status = "hidden";
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["ok"] = "已隱藏。";
            return RedirectToAction(nameof(Index));
        }

        // ===== 切換置頂（可選）=====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            p.Pinned = !(p.Pinned ?? false);
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["ok"] = (p.Pinned == true) ? "已置頂。" : "已取消置頂。";
            return RedirectToAction(nameof(Index));
        }

        // ===== 刪除（Index → 確認 Modal → POST）=====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            _db.Posts.Remove(p);
            await _db.SaveChangesAsync();
            TempData["ok"] = "已刪除。";
            return RedirectToAction(nameof(Index));
        }

        // ===== 詳細內容（給彈窗載入 Partial）=====
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var p = await _db.Posts.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PostId == id);
                if (p == null) return NotFound();

                var vm = new AdminPostDetailsVm
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Tldr = p.Tldr,
                    BodyMd = p.BodyMd,
                    Status = p.Status ?? "draft",
                    Pinned = p.Pinned ?? false,
                    CreatedAt = p.CreatedAt,
                    PublishedAt = p.PublishedAt,
                    UpdatedAt = p.UpdatedAt
                };

                return PartialView("_PostDetailsPartial", vm);
            }
            catch (Exception ex)
            {
                return Content("Details 錯誤: " + ex.Message);
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> Details(int id)
        //{
        //    var p = await _db.Posts
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(x => x.PostId == id);
        //    if (p == null) return NotFound();

        //    var vm = new AdminPostDetailsVm
        //    {
        //        PostId = p.PostId,
        //        Title = p.Title ?? "",
        //        Tldr = p.Tldr,
        //        BodyMd = p.BodyMd,
        //        Status = p.Status ?? "draft",
        //        Pinned = (p.Pinned ?? false),
        //        CreatedAt = p.CreatedAt,        // 注意大小寫
        //        PublishedAt = p.PublishedAt,
        //        UpdatedAt = p.UpdatedAt
        //    };

        //    // 回傳 Partial 給 Modal 用
        //    return PartialView(
        //        "~/Areas/Forum/Views/AdminPosts/_PostDetailsPartial.cshtml",
        //        vm
        //    );
        //}

    }
}
