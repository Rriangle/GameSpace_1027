// Areas/Admin/Controllers/MetricsController.cs
using GameSpace.Areas.Forum.Models.Metric; // MetricListItemVm / MetricEditVm
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GameSpace.Areas.Admin.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class MetricsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public MetricsController(GameSpacedatabaseContext db) => _db = db;

        // =============== Index（首次載入頁面） ===============
        [HttpGet]
        public async Task<IActionResult> Index(int? sourceId, string? q, DateOnly? date)
        {
            var target = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // 上半部清單
            var baseQuery =
                from m in _db.Metrics.AsNoTracking()
                join s in _db.MetricSources.AsNoTracking() on m.SourceId equals s.SourceId
                select new { m, s };

            if (sourceId.HasValue) baseQuery = baseQuery.Where(x => x.s.SourceId == sourceId.Value);
            if (!string.IsNullOrWhiteSpace(q))
                baseQuery = baseQuery.Where(x => x.m.Code.Contains(q) || (x.m.Description ?? "").Contains(q));

            var list = await baseQuery
                .OrderBy(x => x.s.Name).ThenBy(x => x.m.Code)
                .Select(x => new MetricListItemVm
                {
                    MetricId = x.m.MetricId,
                    SourceId = x.s.SourceId,
                    SourceName = x.s.Name,
                    Code = x.m.Code,
                    Unit = x.m.Unit,
                    Description = x.m.Description,
                    IsActive = x.m.IsActive ?? true,
                    CreatedAt = x.m.CreatedAt
                })
                .ToListAsync();

            var sources = await _db.MetricSources.AsNoTracking()
                .OrderBy(s => s.Name).ToListAsync();
            ViewBag.Sources = new SelectList(sources, "SourceId", "Name", sourceId);
            ViewBag.SourceId = sourceId;
            ViewBag.Q = q;

            // 下半部 Top10（等權重、僅啟用指標）
            var top10 = await BuildLeaderboardAsync(target, sourceId, q);

            var vm = new MetricsIndexVm
            {
                TargetDate = target,
                List = list,
                Top10 = top10
            };
            return View(vm);
        }

        // =============== AJAX：不重整預覽資料（表格＋圖） ===============
        [HttpGet]
        public async Task<IActionResult> PreviewJson(DateTime? date, int? sourceId, string? q)
        {
            // input type="date" 送來的是 yyyy-MM-dd（本地），取 Date 部分即可
            var target = DateOnly.FromDateTime((date ?? DateTime.UtcNow).Date);
            
            var top10 = await BuildLeaderboardAsync(target, sourceId, q);
            var ranks = top10.Select(x => x.Rank).ToArray();
            var labelsDisplay = top10.Select(x => x.DisplayName).ToArray(); // 中文優先
            var labelsEn = top10.Select(x => x.GameName).ToArray();    // 英文
            var labels = top10.Select(x => x.GameName).ToArray();
            var values = top10.Select(x => Math.Round(x.Score, 4)).ToArray(); // 前端會顯示四位小數

            // 百分比在前端算也行，這裡也一起給
            var total = values.Sum();
            var shares = total == 0
                ? values.Select(_ => 0d).ToArray()
                : values.Select(v => Math.Round(v / (total == 0 ? 1 : total) * 100, 1)).ToArray();

            return Json(new
            {
                date = target.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ranks,
                labelsDisplay,
                labelsEn,
                values,
                shares
            });
        }

        // =============== CRUD（你原本的） ===============
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await FillSources(null);
            return View(new MetricEditVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MetricEditVm vm)
        {
            await ValidateMetric(vm);
            if (!ModelState.IsValid)
            {
                await FillSources(vm.SourceId);
                return View(vm);
            }

            var e = new Metric
            {
                SourceId = vm.SourceId,
                Code = vm.Code,
                Unit = vm.Unit,
                Description = vm.Description,
                IsActive = vm.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            _db.Metrics.Add(e);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.Metrics.FindAsync(id);
            if (m == null) return NotFound();

            var vm = new MetricEditVm
            {
                MetricId = m.MetricId,
                SourceId = m.SourceId ?? 0,
                Code = m.Code,
                Unit = m.Unit,
                Description = m.Description,
                IsActive = m.IsActive ?? true
            };
            await FillSources(m.SourceId);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MetricEditVm vm)
        {
            if (!vm.MetricId.HasValue) return BadRequest();
            await ValidateMetric(vm);
            if (!ModelState.IsValid)
            {
                await FillSources(vm.SourceId);
                return View(vm);
            }

            var m = await _db.Metrics.FindAsync(vm.MetricId.Value);
            if (m == null) return NotFound();

            m.SourceId = vm.SourceId;
            m.Code = vm.Code;
            m.Unit = vm.Unit;
            m.Description = vm.Description;
            m.IsActive = vm.IsActive;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var m = await _db.Metrics.FindAsync(id);
            if (m == null) return NotFound();

            m.IsActive = !(m.IsActive ?? true);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =============== Private helpers ===============
        private async Task FillSources(int? selected)
        {
            var sources = await _db.MetricSources.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.SourceId, s.Name })
                .ToListAsync();
            ViewBag.Sources = new SelectList(sources, "SourceId", "Name", selected);
        }

        private async Task ValidateMetric(MetricEditVm vm)
        {
            var dup = await _db.Metrics
                .AnyAsync(x => x.SourceId == vm.SourceId && x.Code == vm.Code && x.MetricId != vm.MetricId);
            if (dup) ModelState.AddModelError("", "同一來源下的代碼必須唯一。");
        }

        // =============== Page VMs ===============
        public class MetricsIndexVm
        {
            public DateOnly TargetDate { get; set; }
            public List<MetricListItemVm> List { get; set; } = new();
            public List<LeaderboardRowVm> Top10 { get; set; } = new();
        }

        public class LeaderboardRowVm
        {
            public int Rank { get; set; }
            public int GameId { get; set; }
            public string GameName { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public double Score { get; set; } // 0~1（等權重平均後）
        }

        // =============== 核心：等權重排行榜（支援 sourceId / q 過濾） ===============
        private async Task<List<LeaderboardRowVm>> BuildLeaderboardAsync(
            DateOnly date, int? sourceId = null, string? q = null)
        {
            // 1) 活動中的指標（可選來源/關鍵字過濾）
            var activeMetricIdsQ = _db.Metrics.AsNoTracking()
                .Where(m => (m.IsActive ?? true));

            if (sourceId.HasValue) activeMetricIdsQ = activeMetricIdsQ.Where(m => m.SourceId == sourceId);
            if (!string.IsNullOrWhiteSpace(q))
                activeMetricIdsQ = activeMetricIdsQ.Where(m => m.Code.Contains(q) || (m.Description ?? "").Contains(q));

            var activeMetricIds = await activeMetricIdsQ
                .Select(m => m.MetricId).ToListAsync();

            if (activeMetricIds.Count == 0) return new();

            // 2) 當日數據（你的欄位是 DateOnly 的 d.Date）
            //    ⚠️ 如果你的欄位其實是 MetricDate，就把 d.Date 換成 d.MetricDate
            var rows = await _db.GameMetricDailies
                .Where(d => d.Date == date
                            && d.GameId.HasValue
                            && d.MetricId.HasValue
                            && activeMetricIds.Contains(d.MetricId.Value))
                .Select(d => new
                {
                    GameId = d.GameId.Value,
                    MetricId = d.MetricId.Value,
                    d.Value
                })
                .ToListAsync();

            if (rows.Count == 0) return new();

            // 3) 每指標的當日最大值（避免全 0 導致除以 0）
            var maxByMetric = rows
                .GroupBy(r => r.MetricId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(x => x.Value == 0 ? 1 : x.Value)
                );

            // 4) 0~1 標準化後等權重平均
            var scoreByGame = rows
                .GroupBy(r => r.GameId)
                .Select(g => new
                {
                    GameId = g.Key,
                    Score = g.Sum(x =>
                    {
                        var max = maxByMetric[x.MetricId];
                        return max == 0 ? 0 : (double)(x.Value / max);
                    }) / activeMetricIds.Count
                })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .ToList();

            // 5) 遊戲名稱（同時取中/英）
            var gameIds = scoreByGame.Select(x => x.GameId).ToList();
            var nameMap = await _db.Games.AsNoTracking()
                .Where(g => gameIds.Contains(g.GameId))
                .Select(g => new
                {
                    g.GameId,
                    En = g.Name,          // 如果你的欄位叫別的，這裡改成正確的屬性
                    Zh = g.NameZh         // 如果是 Name_zh → 改成 g.Name_zh
                })
                .ToDictionaryAsync(x => x.GameId, x => new { x.En, x.Zh });

            return scoreByGame.Select((x, i) =>
            {
                var hit = nameMap.TryGetValue(x.GameId, out var nm);
                var en = hit ? (string.IsNullOrWhiteSpace(nm!.En) ? $"#{x.GameId}" : nm.En) : $"#{x.GameId}";
                var zh = hit ? (nm!.Zh ?? "") : "";
                var display = string.IsNullOrWhiteSpace(zh) ? en : zh;

                return new LeaderboardRowVm
                {
                    Rank = i + 1,
                    GameId = x.GameId,
                    GameName = en,          // 英文
                    DisplayName = display,  // 中文優先
                    Score = Math.Round(x.Score, 4)
                };
            }).ToList();


        }
    }
}
