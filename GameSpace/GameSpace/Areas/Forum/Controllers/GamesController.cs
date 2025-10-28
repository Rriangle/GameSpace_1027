// GamesController.cs
//using GameSpace.Areas.Admin.Models.ViewModels;
using GameSpace.Areas.Forum.Models.Game;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ForumEntity = GameSpace.Models.Forum;   // 把資料表實體 Forum 取別名

namespace GameSpace.Areas.Admin.Controllers
{
    
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class GamesController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public GamesController(GameSpacedatabaseContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? genre, string sort = "created", string dir = "desc")
        {
            var query = _db.Games.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(g => g.Name.Contains(q) || (g.NameZh ?? "").Contains(q));

            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(g => g.Genre == genre);

            bool desc = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
            query = sort switch
            {
                "id" => desc ? query.OrderByDescending(g => g.GameId) : query.OrderBy(g => g.GameId),
                "name" => desc ? query.OrderByDescending(g => g.Name).ThenBy(g => g.GameId)
                                  : query.OrderBy(g => g.Name).ThenBy(g => g.GameId),
                "namezh" => desc ? query.OrderByDescending(g => EF.Functions.Collate(g.NameZh ?? g.Name, "Chinese_Taiwan_Stroke_CI_AS")).ThenBy(g => g.GameId)
                                  : query.OrderBy(g => EF.Functions.Collate(g.NameZh ?? g.Name, "Chinese_Taiwan_Stroke_CI_AS")).ThenBy(g => g.GameId),
                _ /*created*/ => desc ? query.OrderByDescending(g => g.CreatedAt).ThenBy(g => g.GameId)
                                      : query.OrderBy(g => g.CreatedAt).ThenBy(g => g.GameId)
            };

            ViewBag.Q = q; ViewBag.Genre = genre; ViewBag.Sort = sort; ViewBag.Dir = dir;

            var list = await query.Select(g => new GameListItemVm
            {
                GameId = g.GameId,
                Name = g.Name,
                NameZh = g.NameZh,
                Genre = g.Genre,
                CreatedAt = g.CreatedAt
            }).ToListAsync();

            return View(list);
        }



        [HttpGet]
        public IActionResult Create() => View(new GameEditVm());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            await using var tx = await _db.Database.BeginTransactionAsync();

            // 1. 新增遊戲
            var game = new Game
            {
                Name = vm.Name,
                NameZh = vm.NameZh,
                Genre = vm.Genre,
                CreatedAt = DateTime.UtcNow
            };
            _db.Games.Add(game);
            await _db.SaveChangesAsync(); // ★ 這裡拿到 game.GameId

            // 2. 檢查 forums 是否已存在（防重複）
            var hasForum = await _db.Forums.AnyAsync(f => f.GameId == game.GameId);
            if (!hasForum)
            {
                var forum = new ForumEntity
                {
                    GameId = game.GameId,
                    Name = (vm.NameZh ?? vm.Name) + " 討論區",
                    Description = $"針對 {vm.NameZh ?? vm.Name} 的交流與討論",
                    CreatedAt = DateTime.UtcNow
                };
                _db.Forums.Add(forum);
                await _db.SaveChangesAsync();
            }

            await tx.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var g = await _db.Games.FindAsync(id);
            if (g == null) return NotFound();

            var vm = new GameEditVm
            {
                GameId = g.GameId,
                Name = g.Name,
                NameZh = g.NameZh,
                Genre = g.Genre
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameEditVm vm)
        {
            if (!vm.GameId.HasValue) return BadRequest();
            var g = await _db.Games.FindAsync(vm.GameId.Value);
            if (g == null) return NotFound();

            g.Name = vm.Name;
            g.NameZh = vm.NameZh;
            g.Genre = vm.Genre;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
