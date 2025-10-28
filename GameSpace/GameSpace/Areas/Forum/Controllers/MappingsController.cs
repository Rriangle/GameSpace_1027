// MappingsController.cs

using GameSpace.Areas.Forum.Models.Mapping;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.Admin.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class MappingsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public MappingsController(GameSpacedatabaseContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index(int? gameId, int? sourceId, string? q)
        {
            var baseQuery =
                from m in _db.GameSourceMaps.AsNoTracking()
                join g in _db.Games.AsNoTracking() on m.GameId equals g.GameId
                join s in _db.MetricSources.AsNoTracking() on m.SourceId equals s.SourceId
                select new { m, g, s };

            if (gameId.HasValue) baseQuery = baseQuery.Where(x => x.g.GameId == gameId.Value);
            if (sourceId.HasValue) baseQuery = baseQuery.Where(x => x.s.SourceId == sourceId.Value);
            if (!string.IsNullOrWhiteSpace(q)) baseQuery = baseQuery.Where(x => x.m.ExternalKey.Contains(q));

            var list = await baseQuery
                .OrderBy(x => x.g.Name)
                .ThenBy(x => x.s.Name)
                .Select(x => new MappingRowVm
                {
                    Id = x.m.Id,
                    GameId = x.g.GameId,
                    GameName = x.g.Name,
                    SourceId = x.s.SourceId,
                    SourceName = x.s.Name,
                    ExternalKey = x.m.ExternalKey,
                    CreatedAt = x.m.CreatedAt
                })
                .ToListAsync();

            await FillSelects(gameId, sourceId);
            ViewBag.Q = q;
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await FillSelects(null, null);
            return View(new MappingEditVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MappingEditVm vm)
        {
            await ValidateMapping(vm);
            if (!ModelState.IsValid)
            {
                await FillSelects(vm.GameId, vm.SourceId);
                return View(vm);
            }

            var e = new GameSourceMap
            {
                GameId = vm.GameId,
                SourceId = vm.SourceId,
                ExternalKey = vm.ExternalKey,
                //CreatedAt = DateTime.UtcNow
            };
            _db.GameSourceMaps.Add(e);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.GameSourceMaps.FindAsync(id);
            if (m == null) return NotFound();

            var vm = new MappingEditVm
            {
                Id = m.Id,
                GameId = m.GameId ?? 0,
                SourceId = m.SourceId ?? 0,
                ExternalKey = m.ExternalKey
            };
            await FillSelects(m.GameId, m.SourceId);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MappingEditVm vm)
        {
            if (!vm.Id.HasValue) return BadRequest();
            await ValidateMapping(vm);
            if (!ModelState.IsValid)
            {
                await FillSelects(vm.GameId, vm.SourceId);
                return View(vm);
            }

            var m = await _db.GameSourceMaps.FindAsync(vm.Id.Value);
            if (m == null) return NotFound();

            m.GameId = vm.GameId;
            m.SourceId = vm.SourceId;
            m.ExternalKey = vm.ExternalKey;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task FillSelects(int? gameId, int? sourceId)
        {
            var games = await _db.Games.AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => new { g.GameId, g.Name })
                .ToListAsync();
            var sources = await _db.MetricSources.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.SourceId, s.Name })
                .ToListAsync();

            ViewBag.Games = new SelectList(games, "GameId", "Name", gameId);
            ViewBag.Sources = new SelectList(sources, "SourceId", "Name", sourceId);
            ViewBag.GameId = gameId;       // 為了「全部遊戲」要不要選
            ViewBag.SourceId = sourceId;   // 為了「全部來源」要不要選
            
        }

        private async Task ValidateMapping(MappingEditVm vm)
        {
            // (game_id, source_id) 唯一
            var dupPair = await _db.GameSourceMaps
                .AnyAsync(x => x.GameId == vm.GameId && x.SourceId == vm.SourceId && x.Id != vm.Id);
            if (dupPair) ModelState.AddModelError("", "同一遊戲在同一來源只能對應一次。");

            // (source_id, external_key) 不能指到多個 game
            var dupKey = await _db.GameSourceMaps
                .Where(x => x.SourceId == vm.SourceId && x.ExternalKey == vm.ExternalKey)
                .Where(x => x.Id != vm.Id)
                .AnyAsync();
            if (dupKey) ModelState.AddModelError("", "此來源的 External Key 已對應到其他遊戲。");
        }
    }
}
