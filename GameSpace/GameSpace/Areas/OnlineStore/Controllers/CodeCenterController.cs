using GameSpace.Areas.OnlineStore.ViewModels;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class CodeCenterController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public CodeCenterController(GameSpacedatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ================= Dashboard =================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Summary tiles
            int prefixKinds = await _context.SProductCodeRules.AsNoTracking().CountAsync();
            int platforms = await _context.SPlatforms.AsNoTracking().CountAsync();
            int genres = await _context.SGameGenres.AsNoTracking().CountAsync();
            int merchTypes = await _context.SMerchTypes.AsNoTracking().CountAsync();

            // Prefix 分布（依 product_code 前兩碼）
            var prefixCount = await _context.SProductCodes
                .AsNoTracking()
                .Where(pc => pc.ProductCode != null && pc.ProductCode.Length >= 2)
                .GroupBy(pc => pc.ProductCode!.Substring(0, 2))
                .Select(g => new CodeCountItemVM { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // 平台 Top 10
            var platformTop = await (
                from d in _context.SGameProductDetails.AsNoTracking()
                join p in _context.SPlatforms.AsNoTracking() on d.PlatformId equals p.PlatformId
                group d by p.PlatformName into g
                select new CodeCountItemVM { Key = g.Key!, Count = g.Count() }
            )
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

            // 類型 Top 10（多對多：Product ↔ Genre）
            var genreTop = await _context.SProductInfos
                .AsNoTracking()
                .SelectMany(pi => pi.Genres)                 // 導覽屬性（多對多）
                .GroupBy(gn => gn.GenreName)
                .Select(g => new CodeCountItemVM { Key = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // 周邊分類 Top 10
            var merchTypeTop = await (
                from d in _context.SOtherProductDetails.AsNoTracking()
                join m in _context.SMerchTypes.AsNoTracking() on d.MerchTypeId equals m.MerchTypeId
                group d by m.MerchTypeName into g
                select new CodeCountItemVM { Key = g.Key!, Count = g.Count() }
            )
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

            ViewBag.Dashboard = new CodeCenterDashboardVM
            {
                PrefixKinds = prefixKinds,
                Platforms = platforms,
                Genres = genres,       // ← 對齊 VM
                MerchTypes = merchTypes,
                PrefixDistribution = prefixCount,
                PlatformTop = platformTop,
                GenreTop = genreTop,     // ← 對齊 VM
                MerchTypeTop = merchTypeTop
            };

            return View();
        }

        // ================= Prefix Catalog =================
        [HttpGet]
        public async Task<IActionResult> PrefixList(string? q = null)
        {
            q = (q ?? "").Trim();

            var query = _context.SProductCodeRules.AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(q))
                query = query.Where(x => x.Prefix.Contains(q) || x.ProductType.Contains(q));

            var list = await query
                .OrderBy(x => x.Prefix)
                .ThenBy(x => x.ProductType)
                .Select(x => new CodePrefixVM
                {
                    Id = x.RuleId,
                    Prefix = x.Prefix,
                    ProductType = x.ProductType,
                    PadLength = (int)x.PadLength
                })
                .ToListAsync();

            // 依 product_code 前兩碼統計數量
            var counts = await _context.SProductCodes
                .AsNoTracking()
                .Where(pc => pc.ProductCode != null && pc.ProductCode.Length >= 2)
                .GroupBy(pc => pc.ProductCode!.Substring(0, 2))
                .Select(g => new { Key = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Cnt);

            foreach (var r in list)
            {
                var key = (r.Prefix?.Length ?? 0) >= 2 ? r.Prefix!.Substring(0, 2) : (r.Prefix ?? "");
                r.ProductCount = counts.TryGetValue(key, out var c) ? c : 0;
            }

            return PartialView("Partials/_PrefixList", list);
        }

        [HttpGet]
        public IActionResult PrefixCreateModal()
            => PartialView("Partials/_PrefixForm", new CodePrefixVM { PadLength = 10 });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrefixCreate(CodePrefixVM vm)
        {
            if (!ModelState.IsValid)
                return Json(new { ok = false, message = "資料驗證失敗" });

            if (string.IsNullOrWhiteSpace(vm.Prefix))
                return Json(new { ok = false, message = "前綴代碼必填" });

            var prefix = vm.Prefix.Trim().ToUpperInvariant();
            var productType = (vm.ProductType ?? "").Trim();

            bool exists = await _context.SProductCodeRules
                .AnyAsync(x => x.Prefix == prefix && x.ProductType == productType);
            if (exists)
                return Json(new { ok = false, message = $"已存在相同規則：{prefix} / {productType}" });

            byte pad = (byte)Math.Clamp(vm.PadLength, 1, 30);
            var e = new SProductCodeRule { Prefix = prefix, ProductType = productType, PadLength = pad };
            _context.Add(e);
            await _context.SaveChangesAsync();

            return Json(new { ok = true, message = "已新增" });
        }

        [HttpGet]
        public async Task<IActionResult> PrefixEditModal(int id)
        {
            var e = await _context.SProductCodeRules.FindAsync(id);
            if (e == null) return NotFound();

            var vm = new CodePrefixVM
            {
                Id = e.RuleId,
                Prefix = e.Prefix,
                ProductType = e.ProductType,
                PadLength = e.PadLength
            };
            return PartialView("Partials/_PrefixForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrefixEdit(CodePrefixVM vm)
        {
            if (!ModelState.IsValid)
                return Json(new { ok = false, message = "資料驗證失敗" });

            var e = await _context.SProductCodeRules.FindAsync(vm.Id);
            if (e == null) return Json(new { ok = false, message = "資料不存在" });

            if (string.IsNullOrWhiteSpace(vm.Prefix))
                return Json(new { ok = false, message = "前綴代碼必填" });

            var prefix = vm.Prefix.Trim().ToUpperInvariant();
            var productType = (vm.ProductType ?? "").Trim();

            bool dup = await _context.SProductCodeRules
                .AnyAsync(x => x.RuleId != vm.Id && x.Prefix == prefix && x.ProductType == productType);
            if (dup)
                return Json(new { ok = false, message = $"已存在相同規則：{prefix} / {productType}" });

            e.Prefix = prefix;
            e.ProductType = productType;
            e.PadLength = (byte)Math.Clamp(vm.PadLength, 1, 30);

            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = "已修改" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrefixDelete(int id)
        {
            var e = await _context.SProductCodeRules.FindAsync(id);
            if (e == null) return Json(new { ok = false, message = "不存在" });

            _context.Remove(e);
            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = "已刪除" });
        }

        // ================= Platforms =================
        [HttpGet]
        public async Task<IActionResult> PlatformList(string? q = null)
        {
            q = (q ?? "").Trim();

            var query = _context.SPlatforms.AsNoTracking();
            if (!string.IsNullOrEmpty(q))
                query = query.Where(x => x.PlatformName.Contains(q));

            var countByPlatformId = await _context.SGameProductDetails
                .AsNoTracking()
                .Where(d => d.PlatformId != null)
                .GroupBy(d => d.PlatformId!.Value)
                .Select(g => new { PlatformId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.PlatformId, x => x.Cnt);

            var list = await query
                .OrderBy(x => x.PlatformName)
                .Select(x => new PlatformVM
                {
                    PlatformId = x.PlatformId,
                    PlatformName = x.PlatformName,
                    IsActive = true,  // 畫面用
                    SortOrder = 0,     // 畫面用
                    ProductCount = 0
                })
                .ToListAsync();

            foreach (var r in list)
                r.ProductCount = countByPlatformId.TryGetValue(r.PlatformId, out var c) ? c : 0;

            return PartialView("Partials/_PlatformList", list);
        }

        [HttpGet]
        public IActionResult PlatformCreateModal()
            => PartialView("Partials/_PlatformForm", new PlatformVM { IsActive = true });

        [HttpGet]
        public async Task<IActionResult> PlatformEditModal(int id)
        {
            var e = await _context.SPlatforms.FindAsync(id);
            if (e == null) return NotFound();

            return PartialView("Partials/_PlatformForm", new PlatformVM
            {
                PlatformId = e.PlatformId,
                PlatformName = e.PlatformName,
                IsActive = true,
                SortOrder = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlatformCreate(PlatformVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.PlatformName))
                return Json(new { ok = false, message = "平台名稱必填" });

            var name = vm.PlatformName.Trim();
            bool dup = await _context.SPlatforms.AnyAsync(x => x.PlatformName == name);
            if (dup) return Json(new { ok = false, message = $"「{name}」已存在" });

            _context.SPlatforms.Add(new SPlatform { PlatformName = name });
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlatformEdit(PlatformVM vm)
        {
            var e = await _context.SPlatforms.FindAsync(vm.PlatformId);
            if (e == null) return Json(new { ok = false, message = "不存在" });

            if (string.IsNullOrWhiteSpace(vm.PlatformName))
                return Json(new { ok = false, message = "平台名稱必填" });

            var name = vm.PlatformName.Trim();
            bool dup = await _context.SPlatforms.AnyAsync(x => x.PlatformId != vm.PlatformId && x.PlatformName == name);
            if (dup) return Json(new { ok = false, message = $"「{name}」已存在" });

            e.PlatformName = name;
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlatformDelete(int id)
        {
            var e = await _context.SPlatforms.FindAsync(id);
            if (e == null) return Json(new { ok = false, message = "不存在" });

            _context.SPlatforms.Remove(e);
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // ================= Genres =================
        [HttpGet]
        public async Task<IActionResult> GenreList(string? q = null)
        {
            q = (q ?? "").Trim();

            var query = _context.SGameGenres.AsNoTracking();
            if (!string.IsNullOrEmpty(q))
                query = query.Where(x => x.GenreName.Contains(q));

            // 依多對多導覽統計（ProductInfos.SelectMany(Genres)）
            var counts = await _context.SProductInfos
                .AsNoTracking()
                .SelectMany(p => p.Genres)
                .GroupBy(g => g.GenreId)
                .Select(g => new { GenreId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.GenreId, x => x.Cnt);

            var list = await query
                .OrderBy(x => x.GenreName)
                .Select(x => new GenreVM
                {
                    GenreId = x.GenreId,
                    GenreName = x.GenreName,
                    IsActive = true,   // 畫面用
                    SortOrder = 0,      // 畫面用
                    ProductCount = 0
                })
                .ToListAsync();

            foreach (var r in list)
                r.ProductCount = counts.TryGetValue(r.GenreId, out var c) ? c : 0;

            return PartialView("Partials/_GenreList", list);
        }

        [HttpGet]
        public IActionResult GenreCreateModal()
            => PartialView("Partials/_GenreForm", new GenreVM { IsActive = true, SortOrder = 0 });

        [HttpGet]
        public async Task<IActionResult> GenreEditModal(int id)
        {
            var e = await _context.SGameGenres.FindAsync(id);
            if (e == null) return NotFound();

            return PartialView("Partials/_GenreForm", new GenreVM
            {
                GenreId = e.GenreId,
                GenreName = e.GenreName,
                IsActive = true,
                SortOrder = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenreCreate(GenreVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.GenreName))
                return Json(new { ok = false, message = "類型名稱必填" });

            var name = vm.GenreName.Trim();
            bool dup = await _context.SGameGenres.AnyAsync(x => x.GenreName == name);
            if (dup) return Json(new { ok = false, message = $"「{name}」已存在" });

            _context.SGameGenres.Add(new SGameGenre { GenreName = name });
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenreEdit(GenreVM vm)
        {
            var e = await _context.SGameGenres.FindAsync(vm.GenreId);
            if (e == null) return Json(new { ok = false, message = "不存在" });

            if (string.IsNullOrWhiteSpace(vm.GenreName))
                return Json(new { ok = false, message = "類型名稱必填" });

            var name = vm.GenreName.Trim();
            bool dup = await _context.SGameGenres.AnyAsync(x => x.GenreId != vm.GenreId && x.GenreName == name);
            if (dup) return Json(new { ok = false, message = $"「{name}」已存在" });

            e.GenreName = name;
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenreDelete(int id)
        {
            var e = await _context.SGameGenres.FindAsync(id);
            if (e == null) return Json(new { ok = false, message = "不存在" });

            _context.SGameGenres.Remove(e);
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        // ================= Merch Types =================
        [HttpGet]
        public async Task<IActionResult> MerchTypeList(string? q = null)
        {
            q = (q ?? "").Trim();

            var query = _context.SMerchTypes.AsNoTracking();
            if (!string.IsNullOrEmpty(q))
                query = query.Where(x => x.MerchTypeName.Contains(q));

            var counts = await _context.SOtherProductDetails.AsNoTracking()
                .Where(d => d.MerchTypeId != null)
                .GroupBy(d => d.MerchTypeId!.Value)
                .Select(g => new { MerchTypeId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.MerchTypeId, x => x.Cnt);

            var list = await query
                .OrderBy(x => x.MerchTypeName)
                .Select(x => new MerchTypeVM
                {
                    MerchTypeId = x.MerchTypeId,
                    MerchTypeName = x.MerchTypeName,
                    IsActive = true, // 畫面用
                    SortOrder = 0,    // 畫面用
                    ProductCount = 0
                })
                .ToListAsync();

            foreach (var r in list)
                r.ProductCount = counts.TryGetValue(r.MerchTypeId, out var c) ? c : 0;

            return PartialView("Partials/_MerchTypeList", list);
        }

        [HttpGet]
        public IActionResult MerchTypeCreateModal()
            => PartialView("Partials/_MerchTypeForm", new MerchTypeVM { IsActive = true, SortOrder = 0 });

        [HttpGet]
        public async Task<IActionResult> MerchTypeEditModal(int id)
        {
            var e = await _context.SMerchTypes.FindAsync(id);
            if (e == null) return NotFound();

            return PartialView("Partials/_MerchTypeForm", new MerchTypeVM
            {
                MerchTypeId = e.MerchTypeId,
                MerchTypeName = e.MerchTypeName,
                IsActive = true,
                SortOrder = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MerchTypeCreate(MerchTypeVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.MerchTypeName))
                return Json(new { ok = false, message = "類別名稱必填" });

            var name = vm.MerchTypeName.Trim();
            bool dup = await _context.SMerchTypes.AnyAsync(x => x.MerchTypeName == name);
            if (dup) return Json(new { ok = false, message = $"「{name}」已存在" });

            _context.SMerchTypes.Add(new SMerchType { MerchTypeName = name });
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MerchTypeEdit(MerchTypeVM vm)
        {
            var e = await _context.SMerchTypes.FindAsync(vm.MerchTypeId);
            if (e == null) return Json(new { ok = false, message = "不存在" });

            if (string.IsNullOrWhiteSpace(vm.MerchTypeName))
                return Json(new { ok = false, message = "類別名稱必填" });

            var name = vm.MerchTypeName.Trim();
            bool dup = await _context.SMerchTypes.AnyAsync(x => x.MerchTypeId != vm.MerchTypeId && x.MerchTypeName == name);
            if (dup) return Json(new { ok = false, message = $"「{name}」已存在" });

            e.MerchTypeName = name;
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MerchTypeDelete(int id)
        {
            var e = await _context.SMerchTypes.FindAsync(id);
            if (e == null) return Json(new { ok = false, message = "不存在" });

            _context.SMerchTypes.Remove(e);
            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }
    }
}
