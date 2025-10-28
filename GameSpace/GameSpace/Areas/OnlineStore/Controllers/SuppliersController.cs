using GameSpace.Areas.OnlineStore.ViewModels;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class SuppliersController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public SuppliersController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // =====================================================
        // Index：搜尋 / 分頁 / 儀表板
        // q: 關鍵字, status: all|active|inactive, from/to: 建立日 (暫留)
        // page/pageSize: 分頁
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? q, string status = "all",
            DateTime? from = null, DateTime? to = null,
            int page = 1, int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 200);

            var supplierQ = _context.SSuppliers.AsNoTracking().AsQueryable();

            // 關鍵字（名稱）
            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                supplierQ = supplierQ.Where(s => s.SupplierName.Contains(keyword));
            }

            // 建立日期：若你的 Supplier 有對應欄位可在此啟用
            // if (from.HasValue) supplierQ = supplierQ.Where(s => s.CreatedAt >= from.Value);
            // if (to.HasValue)   supplierQ = supplierQ.Where(s => s.CreatedAt <  to.Value.AddDays(1));

            // 狀態（用 StatusCode：1=ACTIVE）
            if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                bool expectActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
                supplierQ = supplierQ.Where(s => (s.StatusCode == 1) == expectActive);
            }

            var total = await supplierQ.CountAsync();

            // 列表資料
            var pageData = await supplierQ
                .OrderBy(s => s.SupplierName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierIndexRowVM
                {
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierName,
                    IsActive = (s.StatusCode == 1),
                    GameProductCount = _context.SGameProductDetails.Count(g => g.SupplierId == s.SupplierId),
                    OtherProductCount = _context.SOtherProductDetails.Count(o => o.SupplierId == s.SupplierId)
                })
                .ToListAsync();

            // 儀表板
            // 1) 合作最多商品的供應商 前五（Game+Other 合計）
            var top5 = await _context.SSuppliers
                .Select(s => new SupplierTopVM
                {
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierName,
                    TotalProducts =
                        _context.SGameProductDetails.Count(g => g.SupplierId == s.SupplierId) +
                        _context.SOtherProductDetails.Count(o => o.SupplierId == s.SupplierId)
                })
                .OrderByDescending(x => x.TotalProducts)
                .ThenBy(x => x.SupplierName)
                .Take(5)
                .ToListAsync();

            // 2) 遊戲類供應商總數：曾在 SGameProductDetails 出現過的供應商
            var gameSupplierCount = await _context.SGameProductDetails
                .Select(g => g.SupplierId)
                .Distinct()
                .CountAsync();

            // 3) 周邊供應商總數
            var otherSupplierCount = await _context.SOtherProductDetails
                .Select(o => o.SupplierId)
                .Distinct()
                .CountAsync();

            // 4) 停用的供應商（StatusCode != 1）
            var inactiveCount = await _context.SSuppliers.CountAsync(s => s.StatusCode != 1);

            var vm = new SupplierIndexPageVM
            {
                Query = new SupplierIndexQueryVM
                {
                    Q = q,
                    Status = status,
                    From = from?.ToString("yyyy-MM-dd"),
                    To = to?.ToString("yyyy-MM-dd"),
                    Page = page,
                    PageSize = pageSize,
                    Total = total
                },
                Rows = pageData,
                Dashboard = new SupplierDashboardVM
                {
                    TopSuppliers = top5,
                    GameSupplierCount = gameSupplierCount,
                    OtherSupplierCount = otherSupplierCount,
                    InactiveSupplierCount = inactiveCount
                }
            };

            return View(vm);
        }

        // =====================================================
        // Create / Edit：右側 Offcanvas
        // =====================================================
        [HttpGet]
        public IActionResult CreatePanel()
        {
            return PartialView("_SupplierForm", new SupplierVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierVM vm)
        {
            if (!ModelState.IsValid)
                return Json(new { ok = false, message = "❌ 驗證失敗" });

            var name = (vm.SupplierName ?? "").Trim();

            // 防重複（全字比對）
            bool duplicated = await _context.SSuppliers.AnyAsync(s => s.SupplierName == name);
            if (duplicated)
                return Json(new { ok = false, message = "⚠️ 已存在相同供應商名稱" });

            // 預設啟用：StatusCode = 1 (ACTIVE)
            var entity = new SSupplier
            {
                SupplierName = name,
                StatusCode = 1,
                IsDeleted = false
            };

            _context.SSuppliers.Add(entity);
            await _context.SaveChangesAsync();

            return Json(new { ok = true, message = "✅ 新增成功" });
        }

        [HttpGet]
        public async Task<IActionResult> EditPanel(int id)
        {
            var s = await _context.SSuppliers.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierId == id);
            if (s == null) return NotFound();

            var vm = new SupplierVM
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                IsActive = (s.StatusCode == 1)
            };
            return PartialView("_SupplierForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SupplierVM vm)
        {
            if (!ModelState.IsValid)
                return Json(new { ok = false, message = "❌ 驗證失敗" });

            var s = await _context.SSuppliers.FindAsync(vm.SupplierId);
            if (s == null)
                return Json(new { ok = false, message = "資料不存在" });

            var name = (vm.SupplierName ?? "").Trim();

            // 防重複（排除自己）
            bool duplicated = await _context.SSuppliers
                .AnyAsync(x => x.SupplierName == name && x.SupplierId != vm.SupplierId);
            if (duplicated)
                return Json(new { ok = false, message = "⚠️ 名稱重複" });

            s.SupplierName = name;

            // 允許切換啟用狀態（VM 勾選）
            if (vm.IsActive.HasValue)
                s.StatusCode = (byte)(vm.IsActive.Value ? 1 : 2); // 1=ACTIVE, 2=SUSPENDED

            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = "✏️ 修改成功" });
        }

        // =====================================================
        // 停用 / 啟用（改寫 StatusCode）
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var s = await _context.SSuppliers.FindAsync(id);
            if (s == null) return Json(new { ok = false, message = "不存在" });

            bool nowActive = (s.StatusCode == 1);
            s.StatusCode = (byte)(nowActive ? 2 : 1); // 1=ACTIVE, 2=SUSPENDED
            await _context.SaveChangesAsync();

            return Json(new { ok = true, message = nowActive ? "⛔ 已停用" : "✅ 已啟用", active = !nowActive });
        }

        // =====================================================
        // 刪除（硬刪；若要改軟刪可改寫 is_deleted）
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _context.SSuppliers.FindAsync(id);
            if (s == null) return Json(new { ok = false, message = "不存在" });

            _context.SSuppliers.Remove(s);
            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = "🗑️ 已刪除" });
        }

        // =====================================================
        // 詳細（右側 Offcanvas）
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> DetailPanel(int id)
        {
            var s = await _context.SSuppliers.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierId == id);
            if (s == null) return NotFound();

            var detail = new SupplierDetailVM
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                IsActive = (s.StatusCode == 1),
                GameProductCount = await _context.SGameProductDetails.CountAsync(g => g.SupplierId == id),
                OtherProductCount = await _context.SOtherProductDetails.CountAsync(o => o.SupplierId == id)
            };
            return PartialView("_SupplierDetails", detail);
        }

        // =====================================================
        // 查詢所屬商品（多選供應商）
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductsBySuppliers([FromBody] int[] supplierIds)
        {
            if (supplierIds == null || supplierIds.Length == 0)
                return Content("<div class='text-muted'>未選取任何供應商。</div>", "text/html; charset=utf-8");

            var gameQuery =
                from sup in _context.SSuppliers.AsNoTracking()
                join det in _context.SGameProductDetails.AsNoTracking()
                    on sup.SupplierId equals det.SupplierId
                join info in _context.SProductInfos.AsNoTracking()
                    on det.ProductId equals info.ProductId
                where supplierIds.Contains(sup.SupplierId)
                select new SupplierProductVM
                {
                    SupplierId = sup.SupplierId,
                    SupplierName = sup.SupplierName,
                    ProductId = info.ProductId,
                    ProductName = info.ProductName,
                    ProductCode = _context.SProductCodes
                        .AsNoTracking()
                        .Where(pc => pc.ProductId == info.ProductId)
                        .OrderBy(pc => pc.ProductCode)
                        .Select(pc => pc.ProductCode)
                        .FirstOrDefault(),
                    Category = "遊戲商品"
                };

            var otherQuery =
                from sup in _context.SSuppliers.AsNoTracking()
                join det in _context.SOtherProductDetails.AsNoTracking()
                    on sup.SupplierId equals det.SupplierId
                join info in _context.SProductInfos.AsNoTracking()
                    on det.ProductId equals info.ProductId
                where supplierIds.Contains(sup.SupplierId)
                select new SupplierProductVM
                {
                    SupplierId = sup.SupplierId,
                    SupplierName = sup.SupplierName,
                    ProductId = info.ProductId,
                    ProductName = info.ProductName,
                    ProductCode = _context.SProductCodes
                        .AsNoTracking()
                        .Where(pc => pc.ProductId == info.ProductId)
                        .OrderBy(pc => pc.ProductCode)
                        .Select(pc => pc.ProductCode)
                        .FirstOrDefault(),
                    Category = "周邊商品"
                };

            var list = await gameQuery
                .Concat(otherQuery)
                .OrderBy(x => x.SupplierId)
                .ThenBy(x => x.Category)
                .ThenBy(x => x.ProductId)
                .ToListAsync();

            return PartialView("_SupplierProducts", list);
        }

        // =====================================================
        // CSV 匯出
        // =====================================================
        [HttpGet]
        public async Task<FileResult> ExportCsv(string? q = null)
        {
            var query = _context.SSuppliers.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(s => s.SupplierName.Contains(q.Trim()));

            var rows = await query
                .OrderBy(s => s.SupplierName)
                .Select(s => new
                {
                    s.SupplierId,
                    s.SupplierName,
                    IsActive = (s.StatusCode == 1),
                    GameCount = _context.SGameProductDetails.Count(g => g.SupplierId == s.SupplierId),
                    OtherCount = _context.SOtherProductDetails.Count(o => o.SupplierId == s.SupplierId)
                })
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("SupplierId,SupplierName,IsActive,GameProductCount,OtherProductCount,TotalProducts");
            foreach (var r in rows)
            {
                var total = r.GameCount + r.OtherCount;
                var activeStr = r.IsActive ? "1" : "0";
                sb.AppendLine($"{r.SupplierId},\"{r.SupplierName.Replace("\"", "\"\"")}\",{activeStr},{r.GameCount},{r.OtherCount},{total}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            var fileName = $"suppliers_{DateTime.Now:yyyyMMddHHmm}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // =====================================================
        // CSV 匯入（只新增新供應商名）
        // 模板：SupplierName
        // =====================================================
        [HttpGet]
        public IActionResult ImportPanel()
        {
            return PartialView("_SupplierImport");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { ok = false, message = "請選擇 CSV 檔案" });

            var names = new List<string>();
            using (var sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
            {
                string? line;
                bool first = true;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    if (first) { first = false; continue; } // 跳過標題
                    var cells = line.Split(',');
                    var name = cells.FirstOrDefault()?.Trim().Trim('"') ?? "";
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name);
                }
            }

            int created = 0, skipped = 0;
            foreach (var name in names.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                bool existed = await _context.SSuppliers.AnyAsync(s => s.SupplierName == name);
                if (existed) { skipped++; continue; }

                _context.SSuppliers.Add(new SSupplier
                {
                    SupplierName = name,
                    StatusCode = 1,   // 預設啟用
                    IsDeleted = false
                });
                created++;
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = $"✅ 匯入完成：新增 {created} 筆，跳過 {skipped} 筆（重複）" });
        }
    }
}
