using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ClosedXML.Excel;
using System.Linq;

namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class ReportsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public ReportsController(GameSpacedatabaseContext db) => _db = db;

        /// <summary>
        /// 歷史指數查詢頁 + 查詢結果
        /// 功能：
        /// 1) 下拉顯示中文遊戲名（無中文→退回英文）
        /// 2) 日期區間查詢 → 表格 + 折線圖（View 端 Chart.js）
        /// 3) 顯示匯出按鈕（CSV / Excel / Excel含圖）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> History(int? gameId, DateOnly? from, DateOnly? to, [FromQuery] int[]? metricIds)
        {
            // 1) 下拉：遊戲（中/英名）
            ViewBag.Games = await _db.Games.AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => new { g.GameId, g.Name, g.NameZh })
                .ToListAsync();

            // 2) 多選：指標清單（你要只列啟用也可把 Where 換成 (m.IsActive ?? true)）
            ViewBag.Metrics = await _db.Metrics.AsNoTracking()
                .OrderBy(m => m.MetricId)
                .Select(m => new { m.MetricId, m.Code, m.Description, IsActive = (m.IsActive ?? true) })
                .ToListAsync();

            // 3) 回填查詢條件到 View
            ViewBag.GameId = gameId;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.SelectedMetrics = metricIds ?? Array.Empty<int>();

            // 4) 有條件才查
            if (gameId.HasValue && from.HasValue && to.HasValue)
            {
                // 傳入使用者勾的 metricIds；沒勾的話，內部會 fallback 用啟用中的全部
                var rows = await QueryDailyIndexAsync(gameId.Value, from.Value, to.Value, metricIds);
                return View(rows);
            }

            return View(new List<DailyIndexVm>());
        }

        /// <summary>
        /// 匯出 CSV（純資料，不含圖）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportCsv(int gameId, DateOnly from, DateOnly to, [FromQuery] int[]? metricIds)
        {
            var rows = await QueryDailyIndexAsync(gameId, from, to, metricIds);

            var sb = new StringBuilder();
            sb.AppendLine("date,game,Index");
            foreach (var r in rows)
                sb.AppendLine($"{r.Date:yyyy-MM-dd},{Quote(r.GameName)},{r.IndexValue:0.####}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"index_{gameId}_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");

            static string Quote(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
        }

        /// <summary>
        /// 匯出 Excel（純資料，不含圖）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(int gameId, DateOnly from, DateOnly to, [FromQuery] int[]? metricIds)
        {
            var rows = await QueryDailyIndexAsync(gameId, from, to, metricIds);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Index");
            ws.Cell(1, 1).Value = "Date";
            ws.Cell(1, 2).Value = "Game";
            ws.Cell(1, 3).Value = "Index";

            for (int i = 0; i < rows.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = rows[i].Date.ToDateTime(TimeOnly.MinValue);
                ws.Cell(i + 2, 1).Style.DateFormat.Format = "yyyy-mm-dd";
                ws.Cell(i + 2, 2).Value = rows[i].GameName;
                ws.Cell(i + 2, 3).Value = rows[i].IndexValue;
            }
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"index_{gameId}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }

        /// <summary>
        /// 匯出 Excel（含折線圖 PNG）
        /// 做法：前端把 Canvas 轉 dataURL 丟上來 → 後端解碼貼進 Excel（圖片）
        /// 優點：長相跟前端一致；缺點：圖不是 Excel「活圖表」。
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportExcelWithChartImage(
            int gameId, DateOnly from, DateOnly to, [FromForm] int[]? metricIds, string? chartDataUrl)
        {
            var rows = await QueryDailyIndexAsync(gameId, from, to, metricIds);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Index");
            ws.Cell(1, 1).Value = "Date";
            ws.Cell(1, 2).Value = "Game";
            ws.Cell(1, 3).Value = "Index";

            for (int i = 0; i < rows.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = rows[i].Date.ToDateTime(TimeOnly.MinValue);
                ws.Cell(i + 2, 1).Style.DateFormat.Format = "yyyy-mm-dd";
                ws.Cell(i + 2, 2).Value = rows[i].GameName;
                ws.Cell(i + 2, 3).Value = rows[i].IndexValue;
            }
            ws.Columns().AdjustToContents();

            // 解析 data:image/png;base64,...
            if (!string.IsNullOrWhiteSpace(chartDataUrl) && chartDataUrl.StartsWith("data:image"))
            {
                var comma = chartDataUrl.IndexOf(',');
                if (comma > 0)
                {
                    var b64 = chartDataUrl[(comma + 1)..];
                    var pngBytes = Convert.FromBase64String(b64);

                    using var imgStream = new MemoryStream(pngBytes);
                    var pic = ws.AddPicture(imgStream).MoveTo(ws.Cell(1, 5)); // E1
                    pic.Name = "PopularityChart"; // 不用 WithName()，避免路由擴充衝突
                    pic.Scale(0.9);
                }
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"index_{gameId}_{from:yyyyMMdd}_{to:yyyyMMdd}_chart.xlsx");
        }

        /// <summary>
        /// 舊簽名（3 參數）→ 為相容保留，內部轉呼叫新版（4 參數）
        /// </summary>
        private Task<List<DailyIndexVm>> QueryDailyIndexAsync(int gameId, DateOnly from, DateOnly to)
            => QueryDailyIndexAsync(gameId, from, to, null);

        /// <summary>
        /// 查遊戲在區間內的每日日指數（等權重平均）。
        /// 計算細節：
        /// - 若 metricIds 有值：只用這些；否則只取啟用中的 metrics（IsActive = true/NULL→當作 true）
        /// - 逐日：先抓該日每個指標的最大值，用來做 0~1 標準化
        /// - 某遊戲的日指數 = 各指標標準化後的均值（分母用「使用中的指標數」）
        /// ★ 重點：先整段撈回（by 日期），foreach 內在記憶體篩選 → 不會產生 OPENJSON
        /// </summary>
        private async Task<List<DailyIndexVm>> QueryDailyIndexAsync(int gameId, DateOnly from, DateOnly to, int[]? metricIds)
        {
            var dates = EachDate(from, to).ToList();

            var game = await _db.Games.AsNoTracking()
                                      .FirstOrDefaultAsync(g => g.GameId == gameId);
            var gameName = (game == null) ? $"#{gameId}"
                         : (string.IsNullOrWhiteSpace(game.NameZh) ? game.Name : game.NameZh);

            // 指標集合：使用者勾選優先；否則抓啟用中的
            List<int> usingMetricIds = (metricIds != null && metricIds.Length > 0)
                ? metricIds.Distinct().ToList()
                : await _db.Metrics.Where(m => (m.IsActive ?? true))
                                   .Select(m => m.MetricId)
                                   .ToListAsync();
            if (usingMetricIds.Count == 0) return new List<DailyIndexVm>();

            var usingMetricIdSet = new HashSet<int>(usingMetricIds);

            // ========= 一次把區間資料撈回（只靠日期 + NotNull 過濾） =========
            var allRows = await _db.GameMetricDailies
                .Where(x => x.Date >= from && x.Date <= to
                            && x.GameId.HasValue && x.MetricId.HasValue)
                .Select(x => new
                {
                    Date = x.Date,                               // 後續 per-day 會用到
                    GameId = x.GameId!.Value,
                    MetricId = x.MetricId!.Value,
                    x.Value
                })
                .AsNoTracking()
                .ToListAsync();

            var result = new List<DailyIndexVm>();

            foreach (var d in dates)
            {
                // 在記憶體裡篩出「當日 + 選定指標」
                var rows = allRows
                    .Where(r => r.Date == d && usingMetricIdSet.Contains(r.MetricId))
                    .ToList();

                if (rows.Count == 0)
                {
                    result.Add(new DailyIndexVm { Date = d, GameName = gameName, IndexValue = 0m });
                    continue;
                }

                // 各指標最大值（避免除 0，max=0 時當 1）
                var maxByMetric = rows
                    .GroupBy(r => r.MetricId)
                    .ToDictionary(g => g.Key, g => g.Max(x => x.Value == 0 ? 1 : x.Value));

                // 本遊戲的當日各指標值
                var mine = rows.Where(r => r.GameId == gameId).ToList();
                if (mine.Count == 0)
                {
                    result.Add(new DailyIndexVm { Date = d, GameName = gameName, IndexValue = 0m });
                    continue;
                }

                double sum = 0d; int used = 0;
                foreach (var r in mine)
                {
                    if (!maxByMetric.TryGetValue(r.MetricId, out var max)) continue;
                    var norm = max == 0 ? 0 : (double)(r.Value / max);
                    sum += norm; used++;
                }

                var index = (used == 0) ? 0d : sum / usingMetricIds.Count; // 等權重
                result.Add(new DailyIndexVm
                {
                    Date = d,
                    GameName = gameName,
                    IndexValue = (decimal)Math.Round(index, 4)
                });
            }

            return result;
        }

        private static IEnumerable<DateOnly> EachDate(DateOnly from, DateOnly to)
        {
            for (var d = from; d <= to; d = d.AddDays(1)) yield return d;
        }

        public class DailyIndexVm
        {
            public DateOnly Date { get; set; }
            public string GameName { get; set; } = "";
            public decimal IndexValue { get; set; }
        }
    }
}
