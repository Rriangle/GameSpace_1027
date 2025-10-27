
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2019.Excel.RichData2;
using GameSpace.Areas.Forum.Models.Imports;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class ImportsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public ImportsController(GameSpacedatabaseContext db) => _db = db;

        // ==== 1) 定義匯入（Excel：Metrics + Mappings 兩個工作表） ====

        [HttpGet]
        public IActionResult Defs() => View();

        /// <summary>
        /// Excel 需要兩個工作表：
        ///  - Metrics: [來源名稱, 代碼, 單位, 說明, 啟用]
        ///  - Mappings: [遊戲名稱, 來源名稱, external_key, external_url]
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Defs(IFormFile file)
        {

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "請選擇 Excel 檔。");
                return View();
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "只接受 .xlsx");
                return View();
            }

            int inserted = 0, updated = 0, skipped = 0, errors = 0;
            var msgs = new List<string>();
            try
            {
                using var wb = new XLWorkbook(file.OpenReadStream());
                var wsM = wb.Worksheets.FirstOrDefault(w => w.Name.Equals("Metrics", StringComparison.OrdinalIgnoreCase));
                var wsMp = wb.Worksheets.FirstOrDefault(w => w.Name.Equals("Mappings", StringComparison.OrdinalIgnoreCase));
                if (wsM is null || wsMp is null)
                    throw new InvalidOperationException("Excel 需包含工作表：Metrics 與 Mappings");

                // 字典（用「名稱」對：MetricSources.Name、Games.Name）
                var srcDict = await _db.MetricSources.AsNoTracking()
                                   .ToDictionaryAsync(x => x.Name.ToLower().Trim(), x => x.SourceId);
                var gameDict = await _db.Games.AsNoTracking()
                                   .ToDictionaryAsync(x => x.Name.ToLower().Trim(), x => x.GameId);

                // ---- Metrics ----
                // ---- Metrics ----
                foreach (var row in wsM.RowsUsed().Skip(1))
                {
                    var sourceName = row.Cell(1).GetString().Trim().ToLower();
                    var code = row.Cell(2).GetString().Trim();
                    var unit = row.Cell(3).GetString().Trim();
                    var desc = row.Cell(4).GetString().Trim();

                    // 寬鬆解析 isActive：接受 1/0/true/false/是/否，忽略前後空白
                    bool active = true; // 預設啟用
                    var raw = row.Cell(5).GetString().Trim().ToLower();
                    if (!string.IsNullOrEmpty(raw))
                    {
                        active = raw switch
                        {
                            "1" or "true" or "是" or "y" => true,
                            "0" or "false" or "否" or "n" => false,
                            _ => bool.TryParse(raw, out var b) ? b : true
                        };
                    }

                    if (string.IsNullOrWhiteSpace(sourceName) || string.IsNullOrWhiteSpace(code))
                    { skipped++; continue; }

                    if (!srcDict.TryGetValue(sourceName, out var sourceId))
                    { errors++; msgs.Add($"[Metrics] 未知來源: {sourceName}"); continue; }

                    var m = await _db.Metrics.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.Code == code);
                    if (m == null)
                    {
                        _db.Metrics.Add(new Metric
                        {
                            SourceId = sourceId,
                            Code = code,
                            Unit = string.IsNullOrWhiteSpace(unit) ? "count" : unit,
                            Description = desc,
                            IsActive = active,
                            CreatedAt = DateTime.UtcNow
                        });
                        inserted++;
                    }
                    else
                    {
                        m.Unit = string.IsNullOrWhiteSpace(unit) ? m.Unit : unit;
                        m.Description = desc;
                        m.IsActive = active;
                        updated++;
                    }
                }

                // ---- Mappings ----
                foreach (var row in wsMp.RowsUsed().Skip(1))
                {
                    var gameName = row.Cell(1).GetString().Trim().ToLower();
                    var sourceName = row.Cell(2).GetString().Trim().ToLower();
                    var key = row.Cell(3).GetString().Trim();
                    // var url     = row.Cell(4).GetString().Trim(); // 你目前不存就先略

                    if (!gameDict.TryGetValue(gameName, out var gameId))
                    { errors++; msgs.Add($"[Map] 未知遊戲: {gameName}"); continue; }

                    if (!srcDict.TryGetValue(sourceName, out var sourceId))
                    { errors++; msgs.Add($"[Map] 未知來源: {sourceName}"); continue; }

                    if (string.IsNullOrEmpty(key))
                    { skipped++; msgs.Add($"[Map] externalKey 空白: game={gameName}, source={sourceName}"); continue; }

                    var map = await _db.GameSourceMaps
                        .FirstOrDefaultAsync(x => x.GameId == gameId && x.SourceId == sourceId);

                    if (map == null)
                    {
                        _db.GameSourceMaps.Add(new GameSourceMap
                        {
                            GameId = gameId,
                            SourceId = sourceId,
                            ExternalKey = key,
                            CreatedAt = DateTime.UtcNow
                        });
                        inserted++;
                    }
                    else
                    {
                        if (!string.Equals(map.ExternalKey, key, StringComparison.Ordinal))
                        {
                            map.ExternalKey = key;
                            updated++;
                        }
                        else
                        {
                            skipped++;
                        }
                    }
                }


                await _db.SaveChangesAsync();
                TempData["msg"] = $"定義匯入完成：+{inserted} / ✎{updated} / ↷{skipped} / ⚠{errors}";
                TempData["detail"] = string.Join("<br/>", msgs);
            }
            catch (Exception ex)
            {
                TempData["msg"] = "定義匯入失敗";
                TempData["detail"] = ex.Message;
            }

            return RedirectToAction(nameof(Defs));
        }
        /// <summary>
        /// /補JSON 
        /// </summary>
        /// <returns></returns>

 

        [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DefsJsonText(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            TempData["msg"] = "定義匯入失敗";
            TempData["detail"] = "沒有收到 JSON 內容。";
            return RedirectToAction(nameof(Defs));
        }

        try
        {
            var payload = JsonSerializer.Deserialize<DefsJsonVm>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null)
            {
                TempData["msg"] = "定義匯入失敗";
                TempData["detail"] = "JSON 反序列化失敗。";
                return RedirectToAction(nameof(Defs));
            }

            var (inserted, updated, errors, msgs) = await DefsJsonCore(payload);
            TempData["msg"] = $"定義匯入完成：+{inserted}/✎{updated}/⚠{errors}";
            TempData["detail"] = string.Join("<br/>", msgs);
        }
        catch (Exception ex)
        {
            TempData["msg"] = "定義匯入失敗";
            TempData["detail"] = ex.Message;
        }

        return RedirectToAction(nameof(Defs));
    }



    // ==== 2) 每日數據匯入（JSON 陣列） ====

    [HttpGet]
        public IActionResult Daily() => View();

        public class DailyRowDto
        {
            public DateTime date { get; set; }               // 2025-09-01
            public string game_name { get; set; } = "";      // Elden Ring
            public string source { get; set; } = "";         // Steam
            public string metric { get; set; } = "";         // ccu
            public decimal value { get; set; }               // 123456
        }

        /// <summary>
        /// 接 JSON 陣列：
        /// [
        ///   { "date":"2025-09-01","game_name":"Elden Ring","source":"Steam","metric":"ccu","value":123456 }
        /// ]
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DailyJson([FromBody] List<DailyRowDto> rows)
        {
            if (rows == null || rows.Count == 0) return BadRequest("empty payload");

            int inserted = 0, updated = 0, skipped = 0, errors = 0;
            var msgs = new List<string>();

            // 快取字典
            var srcDict = await _db.MetricSources.AsNoTracking().ToDictionaryAsync(x => x.Name.ToLower(), x => x.SourceId);
            var gameDict = await _db.Games.AsNoTracking().ToDictionaryAsync(x => x.Name.ToLower(), x => x.GameId);

            foreach (var r in rows)
            {
                try
                {
                    var gk = (r.game_name ?? "").Trim().ToLower();
                    var sk = (r.source ?? "").Trim().ToLower();
                    var code = (r.metric ?? "").Trim();
                    var day = DateOnly.FromDateTime(r.date);

                    if (!gameDict.TryGetValue(gk, out var gameId)) { errors++; msgs.Add($"[Daily] 未知遊戲: {r.game_name}"); continue; }
                    if (!srcDict.TryGetValue(sk, out var sourceId)) { errors++; msgs.Add($"[Daily] 未知來源: {r.source}"); continue; }

                    var metricId = await _db.Metrics
                        .Where(x => x.SourceId == sourceId && x.Code == code)
                        .Select(x => (int?)x.MetricId).FirstOrDefaultAsync();

                    if (metricId is null) { errors++; msgs.Add($"[Daily] 未知指標: {r.source}.{code}"); continue; }

                    var row = await _db.GameMetricDailies
                        .FirstOrDefaultAsync(x => x.GameId == gameId && x.MetricId == metricId && x.Date == day);

                    if (row == null)
                    {
                        _db.GameMetricDailies.Add(new GameMetricDaily
                        {
                            GameId = gameId,
                            MetricId = metricId.Value,
                            Date = day,
                            Value = r.value,
                            AggMethod = "raw",
                            Quality = "real",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        inserted++;
                    }
                    else
                    {
                        row.Value = r.value;
                        row.UpdatedAt = DateTime.UtcNow;
                        updated++;
                    }
                }
                catch (Exception ex)
                {
                    errors++; msgs.Add("[Daily] 例外：" + ex.Message);
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { inserted, updated, skipped, errors, messages = msgs });
        }

        // ==== 3) （可選）每日數據匯入：Excel 檔 ====
        /// <summary>
        /// Excel 第一個工作表欄位（含標題列）：[date, game_name, source, metric, value]
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DailyExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "請選擇 Excel 檔。");
                return RedirectToAction(nameof(Daily));
            }
            // 副檔名檢查
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "只接受 .xlsx");
                return RedirectToAction(nameof(Daily));
            }


            int inserted = 0, updated = 0, skipped = 0, errors = 0;
            var msgs = new List<string>();

            try
            {
                using var wb = new XLWorkbook(file.OpenReadStream());
                var ws = wb.Worksheets.First();

                var srcDict = await _db.MetricSources.AsNoTracking().ToDictionaryAsync(x => x.Name.ToLower(), x => x.SourceId);
                var gameDict = await _db.Games.AsNoTracking().ToDictionaryAsync(x => x.Name.ToLower(), x => x.GameId);

                foreach (var row in ws.RowsUsed().Skip(1))
                {
                    try
                    {
                        // 日期安全讀取
                        if (!row.Cell(1).TryGetValue<DateTime>(out var dt))
                        {
                            errors++;
                            msgs.Add($"[ExcelDaily] 第 {row.RowNumber()} 列：日期不是日期型別");
                            continue;
                        }
                        var day = DateOnly.FromDateTime(dt);
                        var gname = row.Cell(2).GetString().Trim();
                        var sname = row.Cell(3).GetString().Trim();
                        var code = row.Cell(4).GetString().Trim();
                        // 數值安全讀取
                        if (!row.Cell(5).TryGetValue<decimal>(out var value))
                        {
                            errors++;
                            msgs.Add($"[ExcelDaily] 第 {row.RowNumber()} 列：value 不是數字");
                            continue;
                        }

                        if (!gameDict.TryGetValue(gname.ToLower(), out var gameId)) { errors++; msgs.Add($"[ExcelDaily] 未知遊戲: {gname}"); continue; }
                        if (!srcDict.TryGetValue(sname.ToLower(), out var sourceId)) { errors++; msgs.Add($"[ExcelDaily] 未知來源: {sname}"); continue; }

                        var metricId = await _db.Metrics
                            .Where(x => x.SourceId == sourceId && x.Code == code)
                            .Select(x => (int?)x.MetricId).FirstOrDefaultAsync();

                        if (metricId is null) { errors++; msgs.Add($"[ExcelDaily] 未知指標: {sname}.{code}"); continue; }

                        var d = await _db.GameMetricDailies
                            .FirstOrDefaultAsync(x => x.GameId == gameId && x.MetricId == metricId && x.Date == day);

                        if (d == null)
                        {
                            _db.GameMetricDailies.Add(new GameMetricDaily
                            {
                                GameId = gameId,
                                MetricId = metricId.Value,
                                Date = day,
                                Value = value,
                                AggMethod = "raw",
                                Quality = "real",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                            inserted++;
                        }
                        else
                        {
                            d.Value = value;
                            d.UpdatedAt = DateTime.UtcNow;
                            updated++;
                        }
                    }
                    catch (Exception exRow)
                    {
                        errors++; msgs.Add("[ExcelDaily] 例外：" + exRow.Message);
                    }
                }

                await _db.SaveChangesAsync();
                TempData["msg"] = $"每日數據（Excel）完成：+{inserted} / ✎{updated} / ↷{skipped} / ⚠{errors}";
                TempData["detail"] = string.Join("<br/>", msgs);
            }
            catch (Exception ex)
            {
                TempData["msg"] = "每日數據（Excel）匯入失敗";
                TempData["detail"] = ex.Message;
            }

            return RedirectToAction(nameof(Daily));
        }


        /// <summary>
        /// 用 JSON 匯入定義（metrics + mappings）
        /// POST /Forum/Imports/DefsJson
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DefsJson([FromBody] DefsJsonVm payload)
    
        {
            try
            {
                if (payload == null) return BadRequest("empty payload");

                int inserted = 0, updated = 0, errors = 0;
                var msgs = new List<string>();

                // 先把來源/遊戲字典撈起來（用小寫去比）
                var srcDict = await _db.MetricSources.AsNoTracking()
                                  .ToDictionaryAsync(x => x.Name.ToLower().Trim(), x => x.SourceId);
                var gameDict = await _db.Games.AsNoTracking()
                                  .ToDictionaryAsync(x => x.Name.ToLower().Trim(), x => x.GameId);

                // === Metrics ===
                foreach (var m in payload.Metrics ?? Enumerable.Empty<MetricJsonVm>())
                {
                    var srcKey = (m.SourceCode ?? "").Trim().ToLower();
                    if (!srcDict.TryGetValue(srcKey, out var sourceId))
                    {
                        errors++; msgs.Add($"[Metrics] 未知來源: {m.SourceCode}");
                        continue;
                    }

                    var metric = await _db.Metrics
                        .FirstOrDefaultAsync(x => x.SourceId == sourceId && x.Code == m.Code);

                    if (metric == null)
                    {
                        _db.Metrics.Add(new Metric
                        {
                            SourceId = sourceId,
                            Code = m.Code,
                            Unit = m.Unit,
                            Description = m.Description,
                            IsActive = m.IsActive,
                            CreatedAt = DateTime.UtcNow
                        });
                        inserted++;
                    }
                    else
                    {
                        metric.Unit = m.Unit;
                        metric.Description = m.Description;
                        metric.IsActive = m.IsActive;
                        updated++;
                    }
                }

                // === Mappings ===
                foreach (var mp in payload.Mappings ?? Enumerable.Empty<MappingJsonVm>())
                {
                    var gKey = (mp.GameName ?? "").Trim().ToLower();
                    var sKey = (mp.SourceCode ?? "").Trim().ToLower();

                    if (!gameDict.TryGetValue(gKey, out var gameId))
                    {
                        errors++; msgs.Add($"[Map] 未知遊戲: {mp.GameName}");
                        continue;
                    }
                    if (!srcDict.TryGetValue(sKey, out var sourceId))
                    {
                        errors++; msgs.Add($"[Map] 未知來源: {mp.SourceCode}");
                        continue;
                    }

                    var map = await _db.GameSourceMaps
                        .FirstOrDefaultAsync(x => x.GameId == gameId && x.SourceId == sourceId);

                    if (map == null)
                    {
                        _db.GameSourceMaps.Add(new GameSourceMap
                        {
                            GameId = gameId,
                            SourceId = sourceId,
                            ExternalKey = mp.ExternalKey,
                            CreatedAt = DateTime.UtcNow
                        });
                        inserted++;
                    }
                    else
                    {
                        map.ExternalKey = mp.ExternalKey;
                        updated++;
                    }
                }

                await _db.SaveChangesAsync();

                return Json(new
                {
                    inserted,
                    updated,
                    errors,
                    messages = msgs
                });
            }
            catch (Exception ex)
            {
                // 直接把詳細錯誤丟回給你看
                return Problem(detail: ex.ToString(), title: "DefsJson crashed", statusCode: 500);
            }

        }
        // 共用：把 JSON 物件寫進 DB（Metrics + Mappings）
        private async Task<(int inserted, int updated, int errors, List<string> msgs)> DefsJsonCore(GameSpace.Areas.Forum.Models.Imports.DefsJsonVm payload)
        {
            int inserted = 0, updated = 0, errors = 0;
            var msgs = new List<string>();

            var srcDict = await _db.MetricSources.AsNoTracking()
                .ToDictionaryAsync(x => x.Name.ToLower().Trim(), x => x.SourceId);
            var gameDict = await _db.Games.AsNoTracking()
                .ToDictionaryAsync(x => x.Name.ToLower().Trim(), x => x.GameId);

            // ===== Metrics =====
            foreach (var m in payload.Metrics ?? Enumerable.Empty<GameSpace.Areas.Forum.Models.Imports.MetricJsonVm>())
            {
                var skey = (m.SourceCode ?? "").Trim().ToLower();
                var code = (m.Code ?? "").Trim();
                if (string.IsNullOrWhiteSpace(skey) || string.IsNullOrWhiteSpace(code)) { errors++; msgs.Add("[Metrics] 缺欄位"); continue; }
                if (!srcDict.TryGetValue(skey, out var sourceId)) { errors++; msgs.Add($"[Metrics] 未知來源: {m.SourceCode}"); continue; }

                var row = await _db.Metrics.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.Code == code);
                if (row == null)
                {
                    _db.Metrics.Add(new Metric
                    {
                        SourceId = sourceId,
                        Code = code,
                        Unit = m.Unit?.Trim(),
                        Description = m.Description?.Trim(),
                        IsActive = m.IsActive,
                        CreatedAt = DateTime.UtcNow
                    });
                    inserted++;
                }
                else
                {
                    row.Unit = m.Unit?.Trim();
                    row.Description = m.Description?.Trim();
                    row.IsActive = m.IsActive;
                    updated++;
                }
            }

            // ===== Mappings =====
            foreach (var mp in payload.Mappings ?? Enumerable.Empty<GameSpace.Areas.Forum.Models.Imports.MappingJsonVm>())
            {
                var gkey = (mp.GameName ?? "").Trim().ToLower();
                var skey = (mp.SourceCode ?? "").Trim().ToLower();
                var ext = (mp.ExternalKey ?? "").Trim();
                if (!gameDict.TryGetValue(gkey, out var gameId)) { errors++; msgs.Add($"[Map] 未知遊戲: {mp.GameName}"); continue; }
                if (!srcDict.TryGetValue(skey, out var sourceId)) { errors++; msgs.Add($"[Map] 未知來源: {mp.SourceCode}"); continue; }
                if (string.IsNullOrWhiteSpace(ext)) { errors++; msgs.Add("[Map] external_key 空白"); continue; }

                var map = await _db.GameSourceMaps.FirstOrDefaultAsync(x => x.GameId == gameId && x.SourceId == sourceId);
                if (map == null)
                {
                    _db.GameSourceMaps.Add(new GameSourceMap
                    {
                        GameId = gameId,
                        SourceId = sourceId,
                        ExternalKey = ext,
                        CreatedAt = DateTime.UtcNow
                    });
                    inserted++;
                }
                else
                {
                    map.ExternalKey = ext;
                    updated++;
                }
            }

            await _db.SaveChangesAsync();
            return (inserted, updated, errors, msgs);
        }

        // 讓使用者不會工作表名稱打錯
        [HttpGet]
        public IActionResult ExcelTemplate()
        {
            using var wb = new ClosedXML.Excel.XLWorkbook();
            var m = wb.AddWorksheet("Metrics");
            m.Cell(1, 1).Value = "來源名稱";
            m.Cell(1, 2).Value = "代碼";
            m.Cell(1, 3).Value = "單位";
            m.Cell(1, 4).Value = "說明";
            m.Cell(1, 5).Value = "啟用";

            var mp = wb.AddWorksheet("Mappings");
            mp.Cell(1, 1).Value = "遊戲名稱";
            mp.Cell(1, 2).Value = "來源名稱";
            mp.Cell(1, 3).Value = "external_key";
            mp.Cell(1, 4).Value = "external_url";

            using var ms = new MemoryStream();
            wb.SaveAs(ms); ms.Position = 0;
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "defs_template.xlsx");
        }


    }
}

