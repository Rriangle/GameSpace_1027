
using GameSpace.Areas.OnlineStore.Utilities;                     // ← ProductHelpers
using GameSpace.Areas.OnlineStore.ViewModels;
using GameSpace.Areas.OnlineStore.ViewModels.Common;            // ← PagingVM 在這裡
using GameSpace.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;


namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class ProductInfoesController : Controller
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IWebHostEnvironment _host;

        public ProductInfoesController(GameSpacedatabaseContext context, IWebHostEnvironment host)
        {
            _context = context;
            _host = host;
        }

   
        // ---------------- Shell ----------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 給前端篩選下拉
            await FillDropdownsAsync();
            return View();
        }
        // ---------------- Data: List Mode ----------------
        [HttpGet]
        public async Task<IActionResult> List(
            string? q = null, string? type = null,
            int? supplierId = null, int? platformId = null, int? merchTypeId = null,
            bool? hasImage = null, string status = "all",
            string sort = "code", int page = 1, int pageSize = 20)
        {
            var query = _context.Set<SProductInfo>().AsNoTracking().AsQueryable();

            // 關鍵字
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.ProductName.Contains(q!));

            // 類型（以對應 detail 是否存在判斷）— 改用 Utilities 版本
            var t = ProductHelpers.NormalizeType(type);
            if (!string.IsNullOrEmpty(t))
            {
                if (t == "game")
                    query = query.Where(p => _context.Set<SGameProductDetail>().Any(g => g.ProductId == p.ProductId));
                else // "notgame"
                    query = query.Where(p => _context.Set<SOtherProductDetail>().Any(o => o.ProductId == p.ProductId));
            }

            // 供應商 / 平台 / 周邊分類
            if (supplierId.HasValue)
                query = query.Where(p =>
                    _context.Set<SGameProductDetail>().Any(g => g.ProductId == p.ProductId && g.SupplierId == supplierId.Value) ||
                    _context.Set<SOtherProductDetail>().Any(o => o.ProductId == p.ProductId && o.SupplierId == supplierId.Value)
                );

            if (platformId.HasValue)
                query = query.Where(p =>
                    _context.Set<SGameProductDetail>().Any(g => g.ProductId == p.ProductId && g.PlatformId == platformId.Value)
                );

            if (merchTypeId.HasValue)
                query = query.Where(p =>
                    _context.Set<SOtherProductDetail>().Any(o => o.ProductId == p.ProductId && o.MerchTypeId == merchTypeId.Value)
                );

            // 有圖 / 無圖
            if (hasImage.HasValue)
            {
                if (hasImage.Value)
                    query = query.Where(p => _context.Set<SProductImage>().Any(i => i.ProductId == p.ProductId));
                else
                    query = query.Where(p => !_context.Set<SProductImage>().Any(i => i.ProductId == p.ProductId));
            }

            // 取基本欄位（新 DB 沒有 is_active/qty/created_at）
            var basics = await query
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.ProductType,
                    p.Price,
                    p.CurrencyCode
                })
                .ToListAsync();

            var ids = basics.Select(x => x.ProductId).ToList();

            // 代碼字典（新欄位 ProductCode）
            var codeMap = await _context.Set<SProductCode>().AsNoTracking()
                .Where(c => ids.Contains(c.ProductId))
                .GroupBy(c => c.ProductId)
                .Select(g => new { ProductId = g.Key, Code = g.Select(x => x.ProductCode).FirstOrDefault() })
                .ToDictionaryAsync(x => x.ProductId, x => x.Code ?? "");

            // 封面圖字典（IsPrimary desc, SortOrder asc, ProductimgId desc）
            var imgMap = await _context.Set<SProductImage>().AsNoTracking()
                .Where(i => ids.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Url = g.OrderByDescending(i => i.IsPrimary)
                           .ThenBy(i => i.SortOrder)
                           .ThenByDescending(i => i.ProductimgId)
                           .Select(i => i.ProductimgUrl)
                           .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.Url ?? "");

            // 組成清單 VM（缺少欄位給預設）
            var list = basics.Select(b => new ProductIndexRowVM
            {
                ProductId = b.ProductId,
                ProductName = b.ProductName ?? "",
                ProductType = b.ProductType ?? "",
                Price = b.Price,                          // decimal 非 nullable
                CurrencyCode = b.CurrencyCode ?? "TWD",

                ShipmentQuantity = 0,                     // 新 DB 無 → 先 0
                IsActive = true,                          // 新 DB 無 → 先 true
                ProductCreatedAt = DateTime.MinValue,     // 新 DB 無 → 先 MinValue

                ProductCode = codeMap.GetValueOrDefault(b.ProductId, ""),
                CoverUrl = imgMap.GetValueOrDefault(b.ProductId, "")
            }).ToList();

            // 排序（代碼以數字尾碼排序；無代碼最後）
            list = sort switch
            {
                "name" => list.OrderBy(x => x.ProductName).ToList(),
                "qty" => list.OrderByDescending(x => x.ShipmentQuantity).ToList(), // 目前都 0，接上庫存後生效
                "price" => list.OrderByDescending(x => x.Price).ToList(),
                _ => list.OrderBy(x => ProductHelpers.ParseCodeNumber(x.ProductCode)).ToList()
            };

            // 分頁
            var total = list.Count;
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 100);
            var pageList = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Paging = new PagingVM { Page = page, PageSize = pageSize, Total = total };

            return PartialView("Partials/_List", pageList);
        }

        // ---------------- Data: Cards Mode ----------------
        [HttpGet]
        public async Task<IActionResult> Cards(
            string? q = null, string? type = null, int? supplierId = null, int? platformId = null, int? merchTypeId = null,
            bool? hasImage = null, string status = "all", string sort = "code", int page = 1, int pageSize = 12)
        {
            var query = _context.Set<SProductInfo>().AsNoTracking().AsQueryable();

            // 關鍵字
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.ProductName.Contains(q!));

            // 類型（以對應 detail 是否存在判斷）
            var t = ProductHelpers.NormalizeType(type);
            if (!string.IsNullOrEmpty(t))
            {
                if (t == "game")
                    query = query.Where(p => _context.Set<SGameProductDetail>().Any(g => g.ProductId == p.ProductId));
                else // "notgame"
                    query = query.Where(p => _context.Set<SOtherProductDetail>().Any(o => o.ProductId == p.ProductId));
            }

            // 供應商 / 平台 / 周邊分類
            if (supplierId.HasValue)
                query = query.Where(p =>
                    _context.Set<SGameProductDetail>().Any(g => g.ProductId == p.ProductId && g.SupplierId == supplierId.Value) ||
                    _context.Set<SOtherProductDetail>().Any(o => o.ProductId == p.ProductId && o.SupplierId == supplierId.Value)
                );

            if (platformId.HasValue)
                query = query.Where(p =>
                    _context.Set<SGameProductDetail>().Any(g => g.ProductId == p.ProductId && g.PlatformId == platformId.Value)
                );

            if (merchTypeId.HasValue)
                query = query.Where(p =>
                    _context.Set<SOtherProductDetail>().Any(o => o.ProductId == p.ProductId && o.MerchTypeId == merchTypeId.Value)
                );

            // 有圖 / 無圖
            if (hasImage.HasValue)
            {
                if (hasImage.Value)
                    query = query.Where(p => _context.Set<SProductImage>().Any(i => i.ProductId == p.ProductId));
                else
                    query = query.Where(p => !_context.Set<SProductImage>().Any(i => i.ProductId == p.ProductId));
            }

            // 取基本欄位（新 DB 沒有 is_active/qty/created_at）
            var basics = await query
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.ProductType,
                    p.Price,
                    p.CurrencyCode
                })
                .ToListAsync();

            var ids = basics.Select(x => x.ProductId).ToList();

            // 代碼字典（新欄位 ProductCode）
            var codeMap = await _context.Set<SProductCode>().AsNoTracking()
                .Where(c => ids.Contains(c.ProductId))
                .GroupBy(c => c.ProductId)
                .Select(g => new { ProductId = g.Key, Code = g.Select(x => x.ProductCode).FirstOrDefault() })
                .ToDictionaryAsync(x => x.ProductId, x => x.Code ?? "");

            // 封面圖字典（IsPrimary desc, SortOrder asc, ProductimgId desc）
            var imgMap = await _context.Set<SProductImage>().AsNoTracking()
                .Where(i => ids.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Url = g.OrderByDescending(i => i.IsPrimary)
                           .ThenBy(i => i.SortOrder)
                           .ThenByDescending(i => i.ProductimgId)
                           .Select(i => i.ProductimgUrl)
                           .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.Url ?? "");

            // 遊戲 / 周邊細節資料（一次撈出避免 N+1）
            var gameMeta = await _context.SGameProductDetails.AsNoTracking()
                .Where(g => ids.Contains(g.ProductId))
                .Select(g => new { g.ProductId, g.PlatformId, g.SupplierId })
                .ToListAsync();

            var otherMeta = await _context.SOtherProductDetails.AsNoTracking()
                .Where(o => ids.Contains(o.ProductId))
                .Select(o => new { o.ProductId, o.MerchTypeId, o.Size, o.Color, o.SupplierId })
                .ToListAsync();

            // 供應商字典
            var supMap = await _context.Set<SSupplier>().AsNoTracking()
                .ToDictionaryAsync(s => s.SupplierId, s => s.SupplierName);

            // 平台字典（只撈 cards 需要的 id）
            var platformIds = gameMeta.Where(x => x.PlatformId.HasValue)
                .Select(x => x.PlatformId!.Value).Distinct().ToList();

            var platformMap = await _context.Set<SPlatform>().AsNoTracking()
                .Where(p => platformIds.Contains(p.PlatformId))
                .ToDictionaryAsync(p => p.PlatformId, p => p.PlatformName);

            // 周邊類型字典（只撈 cards 需要的 id）
            var merchIds = otherMeta.Where(x => x.MerchTypeId.HasValue)
                .Select(x => x.MerchTypeId!.Value).Distinct().ToList();

            var merchMap = await _context.Set<SMerchType>().AsNoTracking()
                .Where(m => merchIds.Contains(m.MerchTypeId))
                .ToDictionaryAsync(m => m.MerchTypeId, m => m.MerchTypeName);

            // 組成卡片資料
            var cards = basics.Select(b =>
            {
                var card = new ProductCardVM
                {
                    ProductId = b.ProductId,
                    ProductName = b.ProductName ?? "",
                    ProductType = b.ProductType ?? "",             // ← 用資料庫實際值
                    Price = b.Price,
                    CurrencyCode = b.CurrencyCode ?? "TWD",
                    IsActive = true,                             // 新 DB 無此欄位，預設為上架
                    ProductCode = codeMap.GetValueOrDefault(b.ProductId, ""),
                    CoverUrl = imgMap.GetValueOrDefault(b.ProductId, "")
                };
                // 遊戲商品
                var g = gameMeta.FirstOrDefault(x => x.ProductId == b.ProductId);
                if (g != null)
                {
                    // PlatformName
                    if (g.PlatformId.HasValue && platformMap.TryGetValue(g.PlatformId.Value, out var platformName))
                        card.PlatformName = platformName;

                    // SupplierName
                    if (supMap.TryGetValue(g.SupplierId, out var gSupplier))
                        card.SupplierName = gSupplier;
                }

                // 周邊商品
                var o = otherMeta.FirstOrDefault(x => x.ProductId == b.ProductId);
                if (o != null)
                {
                    // MerchTypeName
                    if (o.MerchTypeId.HasValue && merchMap.TryGetValue(o.MerchTypeId.Value, out var merchName))
                        card.MerchTypeName = merchName;

                    // SupplierName（若前面遊戲段已填，這裡只在有找到時覆蓋）
                    if (supMap.TryGetValue(o.SupplierId, out var oSupplier))
                        card.SupplierName = oSupplier;

                    card.Color = o.Color;
                    card.Size = o.Size;
                }


                return card;
            }).ToList();

            // 排序
            cards = sort switch
            {
                "name" => cards.OrderBy(x => x.ProductName).ToList(),
                "price" => cards.OrderByDescending(x => x.Price).ToList(),
                _ => cards.OrderBy(x => ProductHelpers.ParseCodeNumber(x.ProductCode)).ToList()
            };

            // 分頁
            var total = cards.Count;
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 6, 48);
            var pageList = cards.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Paging = new PagingVM { Page = page, PageSize = pageSize, Total = total };

            return PartialView("Partials/_Cards", pageList);
        }


        // ---------------- Create (right overlay) ----------------
        [HttpGet]
        public async Task<IActionResult> CreatePanel()
        {
            await FillDropdownsAsync();
            // 不再設定 IsActive / ShipmentQuantity（VM 沒有 & DB 也沒有）
            var vm = new ProductInfoFormVM { CurrencyCode = "TWD" };
            return PartialView("Partials/_Form", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInfoFormVM vm, List<IFormFile>? images)
        {
            var type = ProductHelpers.NormalizeType(vm.ProductType);

            // ====== 驗證（依你目前的 DB 結構）======
            if (string.IsNullOrEmpty(vm.ProductName)) ModelState.AddModelError(nameof(vm.ProductName), "商品名稱必填");
            if (string.IsNullOrEmpty(type)) ModelState.AddModelError(nameof(vm.ProductType), "請選類別");
            if (!vm.SupplierId.HasValue) ModelState.AddModelError(nameof(vm.SupplierId), "供應商必選");
            // 注意：platform_id / merch_type_id 皆為 NULLABLE → 不強制必填
            if (!ModelState.IsValid)
            {
                await FillDropdownsAsync();
                return PartialView("Partials/_Form", vm);
            }

            // ====== 主表（S_ProductInfo）======
            var e = new SProductInfo
            {
                ProductName = vm.ProductName!.Trim(),
                ProductType = type,
                Price = vm.Price,
                CurrencyCode = vm.CurrencyCode ?? "TWD"  // ★ 保險寫法
            };
            _context.Add(e);
            await _context.SaveChangesAsync(); // 先拿 ProductId

            // ====== 明細（依類型；只寫入存在於你 SQL 的欄位）======
            if (type == "game")
            {
                _context.Add(new SGameProductDetail
                {
                    ProductId = e.ProductId,
                    ProductName = e.ProductName,                 // NOT NULL（detail表）
                    ProductDescription = vm.GameProductDescription,
                    SupplierId = vm.SupplierId!.Value,          // NOT NULL
                    PlatformId = vm.PlatformId,                 // NULLABLE
                    DownloadLink = vm.DownloadLink
                    // is_deleted 由 DB 預設 0，不用手動給
                });
            }
            else // notgame
            {
                _context.Add(new SOtherProductDetail
                {
                    ProductId = e.ProductId,
                    ProductName = e.ProductName,                 // NOT NULL（detail表）
                    ProductDescription = vm.OtherProductDescription,
                    SupplierId = vm.SupplierId!.Value,          // NOT NULL
                    MerchTypeId = vm.MerchTypeId,                // NULLABLE
                    DigitalCode = vm.DigitalCode,
                    Size = vm.Size,
                    Color = vm.Color,
                    Weight = vm.Weight,
                    Dimensions = vm.Dimensions,
                    Material = vm.Material
                    // is_deleted 由 DB 預設 0，不用手動給
                });
            }

            // ====== 產品代碼（你的新欄位是 ProductCode）======
            var code = await GenerateProductCodeAsync(type);
            _context.Add(new SProductCode
            {
                ProductId = e.ProductId,
                ProductCode = code   // ✅ 新資料表的欄位名稱
            });


            // ====== 圖片（多張）======
            if (images is { Count: > 0 })
            {
                var paths = await SaveUploadedFilesAsync(images, e.ProductName);
                int sort = 0;
                foreach (var rel in paths)
                {
                    _context.Add(new SProductImage
                    {
                        ProductId = e.ProductId,
                        ProductimgUrl = rel,
                        IsPrimary = (sort == 0),
                        SortOrder = sort++,
                        ProductimgUpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = $"已新增「{e.ProductName}」" });
        }

        // ============================================================
        // 產生產品代碼 (新欄位 ProductCode 版本)
        // GM 開頭 = game，OT 開頭 = notgame，其餘 XX
        // ============================================================
        private async Task<string> GenerateProductCodeAsync(string? productType)
        {
            var isGame = string.Equals(productType, "game", StringComparison.OrdinalIgnoreCase);
            var isNotGame = string.Equals(productType, "notgame", StringComparison.OrdinalIgnoreCase);
            var prefix = isGame ? "GM" : (isNotGame ? "OT" : "XX");

            // 取得目前相同前綴最後一筆代碼
            var last = await _context.Set<SProductCode>()
                .AsNoTracking()
                .Where(c => c.ProductCode != null && c.ProductCode.StartsWith(prefix))
                .OrderByDescending(c => c.ProductCode)
                .Select(c => c.ProductCode)
                .FirstOrDefaultAsync();

            long next = 1;
            if (!string.IsNullOrEmpty(last) && last.Length >= 12)
            {
                var tail = last.Substring(2);
                if (long.TryParse(tail, out var n))
                    next = n + 1;
            }

            // 嘗試生成最多 5 次防碰撞
            for (int i = 0; i < 5; i++)
            {
                var cand = prefix + next.ToString("D10");
                var exists = await _context.Set<SProductCode>()
                    .AsNoTracking()
                    .AnyAsync(x => x.ProductCode == cand);

                if (!exists)
                    return cand;
                next++;
            }

            // fallback: 用 timestamp 當後綴
            return prefix + DateTime.UtcNow.Ticks.ToString("D10")[..10];
        }


        // ---------------- Edit (right overlay) ----------------
        [HttpGet]
        public async Task<IActionResult> EditPanel(int id)
        {
            var p = await _context.Set<SProductInfo>().FindAsync(id);
            if (p == null) return NotFound();

            var vm = await MapToFormVM(p);
            await FillDropdownsAsync();
            return PartialView("Partials/_Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductInfoFormVM vm, List<IFormFile>? images)
        {
            var p = await _context.Set<SProductInfo>().FindAsync(vm.ProductId);
            if (p == null) return Json(new { ok = false, message = "資料不存在" });

            var type = ProductHelpers.NormalizeType(vm.ProductType);

            // 基本驗證（配合你的 DB 結構）
            if (string.IsNullOrWhiteSpace(vm.ProductName))
                ModelState.AddModelError(nameof(vm.ProductName), "商品名稱必填");
            if (string.IsNullOrEmpty(type))
                ModelState.AddModelError(nameof(vm.ProductType), "請選類別");
            if (!vm.SupplierId.HasValue)
                ModelState.AddModelError(nameof(vm.SupplierId), "供應商必選");

            // 依類型補充必要欄位驗證（僅限 DB 真的存在的欄位）
            if (type == "game")
            {
                // platform_id 允許為 NULL，不強制
            }
            else // notgame
            {
                // merch_type_id 允許為 NULL，不強制
            }

            if (!ModelState.IsValid)
            {
                await FillDropdownsAsync();
                return PartialView("Partials/_Form", vm);
            }
            // ====== 主表（S_ProductInfo）======
            var e = new SProductInfo
            {
                ProductName = vm.ProductName!.Trim(),
                ProductType = type,
                Price = vm.Price,
                CurrencyCode = vm.CurrencyCode ?? "TWD"  // ★ 保險寫法
            };

            // 你目前的 S_ProductInfo 沒有 ShipmentQuantity / IsActive / ProductUpdatedAt → 不要寫
            // p.ShipmentQuantity = ...
            // p.IsActive = ...
            // p.ProductUpdatedAt = ...

            // ====== 更新 / 建立對應明細 ======
            if (type == "game")
            {
                // Game 明細
                var d = await _context.Set<SGameProductDetail>()
                    .FirstOrDefaultAsync(x => x.ProductId == p.ProductId);

                if (d == null)
                {
                    d = new SGameProductDetail { ProductId = p.ProductId };
                    _context.Add(d);
                }

                // 必填：supplier_id（NOT NULL）→ 已驗證 HasValue
                d.SupplierId = vm.SupplierId!.Value;

                // 可空：platform_id
                d.PlatformId = vm.PlatformId;

                // 其它存在的欄位
                d.DownloadLink = vm.DownloadLink;
                d.ProductDescription = vm.GameProductDescription;

                // 你的 S_GameProductDetails 沒有 PlatformName / GameType / IsActive → 不要指派
                // d.PlatformName = ...
                // d.GameType = ...
                // d.IsActive = ...

                // 刪除 notgame 的對應明細（若存在）
                var o = await _context.Set<SOtherProductDetail>()
                    .FirstOrDefaultAsync(x => x.ProductId == p.ProductId);
                if (o != null) _context.Remove(o);
            }
            else
            {
                // Other 明細
                var d = await _context.Set<SOtherProductDetail>()
                    .FirstOrDefaultAsync(x => x.ProductId == p.ProductId);

                if (d == null)
                {
                    d = new SOtherProductDetail { ProductId = p.ProductId };
                    _context.Add(d);
                }

                // 必填：supplier_id（NOT NULL）→ 已驗證 HasValue
                d.SupplierId = vm.SupplierId!.Value;

                // 可空：merch_type_id
                d.MerchTypeId = vm.MerchTypeId;

                // 其它存在的欄位
                d.DigitalCode = vm.DigitalCode;
                d.Size = vm.Size;
                d.Color = vm.Color;
                d.Weight = vm.Weight;
                d.Dimensions = vm.Dimensions;
                d.Material = vm.Material;
                d.ProductDescription = vm.OtherProductDescription;

                // 你的 S_OtherProductDetails 沒有 StockQuantity / IsActive → 不要指派
                // d.StockQuantity = ...
                // d.IsActive = ...

                // 刪除 game 的對應明細（若存在）
                var g = await _context.Set<SGameProductDetail>()
                    .FirstOrDefaultAsync(x => x.ProductId == p.ProductId);
                if (g != null) _context.Remove(g);
            }

            // ========== 新圖（沿用你既有的存檔邏輯）==========
            if (images is { Count: > 0 })
            {
                var paths = await SaveUploadedFilesAsync(images, p.ProductName);
                int sortBase = await _context.Set<SProductImage>().AsNoTracking()
                    .Where(x => x.ProductId == p.ProductId)
                    .MaxAsync(x => (int?)x.SortOrder) ?? 0;

                int sort = sortBase + 1;
                foreach (var rel in paths)
                {
                    _context.Add(new SProductImage
                    {
                        ProductId = p.ProductId,
                        ProductimgUrl = rel,
                        IsPrimary = false,
                        SortOrder = sort++,
                        ProductimgUpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = $"已更新「{p.ProductName}」" });
        }


        // ---------------- Toggle Active (by flipping detail.is_deleted) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var p = await _context.Set<SProductInfo>().FindAsync(id);
            if (p == null) return Json(new { ok = false, message = "不存在" });

            // 嘗試找 game / other 任一 detail（理論上只會有其一）
            var g = await _context.Set<SGameProductDetail>().FirstOrDefaultAsync(x => x.ProductId == id);
            var o = await _context.Set<SOtherProductDetail>().FirstOrDefaultAsync(x => x.ProductId == id);

            bool? active = null;

            if (g != null)
            {
                g.IsDeleted = !g.IsDeleted; // flip
                active = !g.IsDeleted;
            }
            if (o != null)
            {
                o.IsDeleted = !o.IsDeleted; // flip
                active = !o.IsDeleted;
            }

            if (active == null)
                return Json(new { ok = false, message = "找不到對應的 detail" });

            await _context.SaveChangesAsync();
            return Json(new { ok = true, active }); // active=true 代表未刪除(上架)
        }

        // ---------------- Deactivate (force detail.is_deleted = true) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var p = await _context.Set<SProductInfo>().FindAsync(id);
            if (p == null) return Json(new { ok = false, message = "不存在" });

            var g = await _context.Set<SGameProductDetail>().FirstOrDefaultAsync(x => x.ProductId == id);
            var o = await _context.Set<SOtherProductDetail>().FirstOrDefaultAsync(x => x.ProductId == id);

            if (g == null && o == null)
                return Json(new { ok = false, message = "找不到對應的 detail" });

            if (g != null) g.IsDeleted = true;
            if (o != null) o.IsDeleted = true;

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }




        private static int ParseCodeNumber(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return int.MaxValue;
            var s = new string(code.Trim().SkipWhile(ch => !char.IsDigit(ch)).TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(s, out var n) ? n : int.MaxValue;
        }

        private string ProductImagesAbsFolder => Path.Combine(_host.WebRootPath, "images", "products");
        private const string ProductImagesRelFolder = "/images/products";
        private static readonly HashSet<string> _allowedExts = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static string MakeSafeFileName(string? prefer, string ext)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var guid = Guid.NewGuid().ToString("N");
            var baseName = string.IsNullOrWhiteSpace(prefer) ? "img" : Regex.Replace(prefer, @"[^a-zA-Z0-9\-_]+", "-");
            baseName = baseName.Trim('-');
            if (baseName.Length > 40) baseName = baseName.Substring(0, 40);
            return $"{baseName}_{stamp}_{guid}{ext}";
        }

        private async Task<List<string>> SaveUploadedFilesAsync(IEnumerable<IFormFile> files, string? preferName)
        {
            Directory.CreateDirectory(ProductImagesAbsFolder);
            var rels = new List<string>();
            foreach (var f in files)
            {
                if (f == null || f.Length == 0) continue;
                var ext = Path.GetExtension(f.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !_allowedExts.Contains(ext)) continue;
                var fname = MakeSafeFileName(preferName, ext);
                var abs = Path.Combine(ProductImagesAbsFolder, fname);
                using (var fs = System.IO.File.Create(abs)) { await f.CopyToAsync(fs); }
                rels.Add($"{ProductImagesRelFolder}/{fname}".Replace('\\', '/'));
            }
            return rels;
        }

        private async Task<ProductInfoFormVM> MapToFormVM(SProductInfo e)
        {
            var vm = new ProductInfoFormVM
            {
                ProductId = e.ProductId,
                ProductName = e.ProductName,
                ProductType = e.ProductType,
                Price = e.Price,                 // decimal 非 nullable
                CurrencyCode = e.CurrencyCode ?? "TWD",
                // 不存在：IsActive / ShipmentQuantity → 不要回填
            };

            if (e.ProductType == "game")
            {
                var d = await _context.Set<SGameProductDetail>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ProductId == e.ProductId);

                if (d != null)
                {
                    vm.SupplierId = d.SupplierId;   // int (NOT NULL)
                    vm.PlatformId = d.PlatformId;   // int? → 可 null
                    vm.DownloadLink = d.DownloadLink;
                    vm.GameProductDescription = d.ProductDescription; // 對應你 EF 的屬性名稱
                                                                      // 新 DB 沒有 PlatformName / GameType / IsActive
                }
            }
            else
            {
                var d = await _context.Set<SOtherProductDetail>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ProductId == e.ProductId);

                if (d != null)
                {
                    vm.SupplierId = d.SupplierId;
                    vm.MerchTypeId = d.MerchTypeId;      // int? → 可 null
                    vm.DigitalCode = d.DigitalCode;
                    vm.Size = d.Size;
                    vm.Color = d.Color;
                    vm.Weight = d.Weight;
                    vm.Dimensions = d.Dimensions;
                    vm.Material = d.Material;
                    vm.OtherProductDescription = d.ProductDescription;
                    // 新 DB 沒有 StockQuantity / IsActive
                }
            }
            return vm;
        }
        private async Task FillDropdownsAsync()
        {
            // 供應商主檔
            ViewBag.SupplierList = await _context.Set<SSupplier>()     // ← 你的 DbSet 名稱若是 SSuppliers/Supplier 請改成相符的
                .AsNoTracking()
                .OrderBy(s => s.SupplierName)
                .Select(s => new { s.SupplierId, s.SupplierName })
                .ToListAsync();

            // 平台主檔
            ViewBag.PlatformList = await _context.Set<SPlatform>()
                .AsNoTracking()
                .OrderBy(p => p.PlatformName)
                .Select(p => new { p.PlatformId, p.PlatformName })
                .ToListAsync();

            // 周邊類別主檔
            ViewBag.MerchTypeList = await _context.Set<SMerchType>()
                .AsNoTracking()
                .OrderBy(m => m.MerchTypeName)
                .Select(m => new { m.MerchTypeId, m.MerchTypeName })
                .ToListAsync();

            ViewBag.CurrencyList = new[] { "TWD", "USD", "JPY", "EUR", "CNY", "HKD", "KRW" };
        }


    }
}

