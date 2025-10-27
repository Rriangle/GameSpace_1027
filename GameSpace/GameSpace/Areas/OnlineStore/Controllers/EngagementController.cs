using GameSpace.Areas.OnlineStore.ViewModels;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class EngagementController : Controller
    {
        private readonly GameSpacedatabaseContext _context;
        public EngagementController(GameSpacedatabaseContext context) { _context = context; }

        // ================= Dashboard =================
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? from = null, DateTime? to = null)
        {
            // 建議統一用 UTC（避免不同伺服器/瀏覽器時區造成邊界誤差）
            var dateFrom = (from?.ToUniversalTime()) ?? DateTime.UtcNow.AddDays(-30);
            var dateTo = (to?.ToUniversalTime()) ?? DateTime.UtcNow;

            // 基本數字（近區間）
            var ratingQuery = _context.Set<SProductRating>()
                .AsNoTracking()
                .Where(x => x.CreatedAt >= dateFrom && x.CreatedAt < dateTo);

            var favoriteQuery = _context.Set<SUserFavorite>()
                .AsNoTracking()
                .Where(x => x.CreatedAt >= dateFrom && x.CreatedAt < dateTo);

            var ratingCount = await ratingQuery.CountAsync();
            var favoriteCount = await favoriteQuery.CountAsync();
            var ratingAvg = await ratingQuery.Select(x => (double?)x.Rating).AverageAsync() ?? 0.0;

            // 排行資料（最近日期）→ EF 對應為 DateOnly
            var lastRankingDate = await _context.Set<SOfficialStoreRanking>()
                .AsNoTracking()
                .Select(x => x.RankingDate)            // DateOnly
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            int rankingRows = 0;
            if (lastRankingDate != default)            // default(DateOnly)
            {
                rankingRows = await _context.Set<SOfficialStoreRanking>()
                    .AsNoTracking()
                    .Where(x => x.RankingDate == lastRankingDate)
                    .CountAsync();
            }

            // 互動趨勢（近 7 日，以日聚合）
            var trendFrom = DateTime.UtcNow.Date.AddDays(-6);
            var trendTo = DateTime.UtcNow.Date.AddDays(1);

            var ratingsTrend = await _context.Set<SProductRating>()
                .AsNoTracking()
                .Where(x => x.CreatedAt >= trendFrom && x.CreatedAt < trendTo)
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new TrendPointVM { Day = g.Key, Count = g.Count() })
                .OrderBy(g => g.Day)
                .ToListAsync();

            var favsTrend = await _context.Set<SUserFavorite>()
                .AsNoTracking()
                .Where(x => x.CreatedAt >= trendFrom && x.CreatedAt < trendTo)
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new TrendPointVM { Day = g.Key, Count = g.Count() })
                .OrderBy(g => g.Day)
                .ToListAsync();

            // ★ VM 的 LastRankingDate 請使用 DateOnly?（你前一則我也幫你改好了）
            ViewBag.Dashboard = new EngagementDashboardVM
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                RatingCount = ratingCount,
                FavoriteCount = favoriteCount,
                AvgRating = Math.Round(ratingAvg, 2),
                LastRankingDate = lastRankingDate == default ? (DateOnly?)null : lastRankingDate,
                LastRankingRows = rankingRows,
                RatingsTrend = ratingsTrend,
                FavoritesTrend = favsTrend
            };

            return View();
        }

        // ================= Ratings (list + export) =================
        [HttpGet]
        public async Task<IActionResult> Ratings(
            string? q = null, int? productId = null,
            int page = 1, int pageSize = 20,
            DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Set<SProductRating>().AsNoTracking().AsQueryable();

            if (productId.HasValue)
                query = query.Where(x => x.ProductId == productId.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => (x.ReviewText ?? "").Contains(q));

            if (from.HasValue)
                query = query.Where(x => x.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.CreatedAt < to.Value.AddDays(1));

            var total = await query.CountAsync();

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 200);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RatingRowVM
                {
                    RatingId = x.RatingId,
                    ProductId = x.ProductId,
                    UserId = x.UserId,
                    Rating = x.Rating,
                    ReviewText = x.ReviewText,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            // ★ 用完整命名空間避免撞名
            ViewBag.Paging = new GameSpace.Areas.OnlineStore.ViewModels.Common.PagingVM
            {
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return PartialView("Partials/_Ratings", items);
        }


        [HttpGet]
        public async Task<FileResult> ExportRatingsCsv(string? q = null, int? productId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Set<SProductRating>().AsNoTracking().AsQueryable();
            if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => (x.ReviewText ?? "").Contains(q));
            if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(x => x.CreatedAt < to.Value.AddDays(1));

            var list = await query.OrderByDescending(x => x.CreatedAt)
                .Select(x => new {
                    x.RatingId,
                    x.ProductId,
                    x.UserId,
                    x.Rating,
                    x.Status,
                    x.CreatedAt,
                    x.ReviewText
                }).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("rating_id,product_id,user_id,rating,status,created_at,review_text");
            foreach (var r in list)
            {
                var text = (r.ReviewText ?? "").Replace("\"", "\"\"");
                sb.AppendLine($"{r.RatingId},{r.ProductId},{r.UserId},{r.Rating},{r.Status},{r.CreatedAt:o},\"{text}\"");
            }
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"ratings_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
        }

        // ================= Favorites (list + export) =================
        [HttpGet]
        public async Task<IActionResult> Favorites(int? productId = null, int? userId = null, int page = 1, int pageSize = 20, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Set<SUserFavorite>().AsNoTracking().AsQueryable();
            if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
            if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
            if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(x => x.CreatedAt < to.Value.AddDays(1));

            var total = await query.CountAsync();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 200);

            var items = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new FavoriteRowVM
                {
                    ProductId = x.ProductId,
                    UserId = x.UserId,
                    CreatedAt = x.CreatedAt
                }).ToListAsync();

            ViewBag.Paging = new GameSpace.Areas.OnlineStore.ViewModels.Common.PagingVM { Page = page, PageSize = pageSize, Total = total };
            return PartialView("Partials/_Favorites", items);
        }

        [HttpGet]
        public async Task<FileResult> ExportFavoritesCsv(int? productId = null, int? userId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Set<SUserFavorite>().AsNoTracking().AsQueryable();
            if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
            if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
            if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(x => x.CreatedAt < to.Value.AddDays(1));

            var list = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("product_id,user_id,created_at");
            foreach (var r in list) sb.AppendLine($"{r.ProductId},{r.UserId},{r.CreatedAt:o}");
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"favorites_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
        }

        // ================= Rankings (list + export) =================
        [HttpGet]
        public async Task<IActionResult> Rankings(
            byte? periodType = null,
            string? metric = null,
            DateTime? rankingDate = null,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.Set<SOfficialStoreRanking>()
                .AsNoTracking()
                .AsQueryable();

            if (periodType.HasValue)
                query = query.Where(x => x.PeriodType == periodType.Value);

            if (!string.IsNullOrWhiteSpace(metric))
                query = query.Where(x => x.RankingMetric == metric);

            // ★ DB 是 DateOnly，參數是 DateTime? → 轉成 DateOnly 再比較
            if (rankingDate.HasValue)
            {
                var d = DateOnly.FromDateTime(rankingDate.Value.Date);
                query = query.Where(x => x.RankingDate == d);
            }

            var total = await query.CountAsync();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 10, 200);

            var items = await query
                .OrderBy(x => x.RankingPosition)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RankingRowVM
                {
                    RankingId = x.RankingId,
                    PeriodType = x.PeriodType,
                    // ★ 這裡改成直接給 DateOnly（因為 VM 是 DateOnly）
                    RankingDate = x.RankingDate,
                    ProductId = x.ProductId,
                    RankingMetric = x.RankingMetric,
                    RankingPosition = x.RankingPosition,
                    MetricValueNum = x.MetricValueNum,
                    TradingAmount = x.TradingAmount,
                    TradingVolume = x.TradingVolume,
                    MetricNote = x.MetricNote
                })
                .ToListAsync();

            // 如果專案內還有重複的 PagingVM 定義，先用完整命名空間止血
            ViewBag.Paging = new GameSpace.Areas.OnlineStore.ViewModels.Common.PagingVM
            {
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return PartialView("Partials/_Rankings", items);
        }

        [HttpGet]
        public async Task<FileResult> ExportRankingsCsv(
            byte? periodType = null,
            string? metric = null,
            DateTime? rankingDate = null)
        {
            var query = _context.Set<SOfficialStoreRanking>()
                .AsNoTracking()
                .AsQueryable();

            if (periodType.HasValue)
                query = query.Where(x => x.PeriodType == periodType.Value);

            if (!string.IsNullOrWhiteSpace(metric))
                query = query.Where(x => x.RankingMetric == metric);

            if (rankingDate.HasValue)
            {
                var d = DateOnly.FromDateTime(rankingDate.Value.Date);
                query = query.Where(x => x.RankingDate == d);
            }

            var list = await query
                .OrderBy(x => x.RankingPosition)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ranking_id,period_type,ranking_date,product_id,metric,position,metric_value,trading_amount,trading_volume,note");

            foreach (var r in list)
            {
                var noteEscaped = (r.MetricNote ?? "").Replace("\"", "\"\"");
                // ★ DateOnly 格式化用 yyyy-MM-dd
                sb.AppendLine($"{r.RankingId},{r.PeriodType},{r.RankingDate:yyyy-MM-dd},{r.ProductId},{r.RankingMetric},{r.RankingPosition},{r.MetricValueNum},{r.TradingAmount},{r.TradingVolume},\"{noteEscaped}\"");
            }

            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv", $"rankings_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
        }


        // ================= Revenue (aggregate view + export) =================
        // ================= Revenue (aggregate view from VIEW + export) =================
        [HttpGet]
        public async Task<IActionResult> Revenue(byte? periodType = null, DateTime? from = null, DateTime? to = null)
        {
            // 用你 DbContext 的視圖 DbSet
            var q = _context.SVRevenueByPeriods.AsNoTracking().AsQueryable();

            if (periodType.HasValue)
                q = q.Where(x => x.PeriodType == periodType.Value);
            
            // 將 DateTime? 轉成 DateOnly 再比較
            // 視圖欄位是 DateOnly；參數是 DateTime? → 轉成 DateOnly 比較
            if (from.HasValue)
            {
                var dFrom = DateOnly.FromDateTime(from.Value.Date);
                q = q.Where(x => x.RankingDate >= dFrom);
            }
            if (to.HasValue)
            {
                var dTo = DateOnly.FromDateTime(to.Value.Date);
                q = q.Where(x => x.RankingDate <= dTo);
            }

            var items = await q
                .OrderByDescending(x => x.RankingDate)
                .Select(x => new RevenueRowVM
                {
                    PeriodType = x.PeriodType,
                    RankingDate = x.RankingDate,  // ← 不要 ToDateTime
                    RevenueAmount = x.RevenueAmount,
                    RevenueVolume = x.RevenueVolume
                })
               .ToListAsync();

            return PartialView("Partials/_Revenue", items);
        }


        [HttpGet]
            public async Task<FileResult> ExportRevenueCsv(byte? periodType = null, DateTime? from = null, DateTime? to = null)
            {
                var q = _context.SVRevenueByPeriods.AsNoTracking().AsQueryable();

                if (periodType.HasValue)
                    q = q.Where(x => x.PeriodType == periodType.Value);

                if (from.HasValue)
                {
                    var dFrom = DateOnly.FromDateTime(from.Value.Date);
                    q = q.Where(x => x.RankingDate >= dFrom);
                }
                if (to.HasValue)
                {
                    var dTo = DateOnly.FromDateTime(to.Value.Date);
                    q = q.Where(x => x.RankingDate <= dTo);
                }

                var data = await q
                    .OrderByDescending(x => x.RankingDate)
                    .ToListAsync();

                var sb = new StringBuilder();
                sb.AppendLine("period_type,ranking_date,revenue_amount,revenue_volume");
                foreach (var r in data)
                {
                    var dateStr = r.RankingDate.ToString("yyyy-MM-dd"); // DateOnly 安全格式化
                    sb.AppendLine($"{r.PeriodType},{dateStr},{r.RevenueAmount},{r.RevenueVolume}");
                }

                var bytes = Encoding.UTF8.GetPreamble()
                    .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                    .ToArray();

                return File(bytes, "text/csv", $"revenue_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
            }
        
    }
}
