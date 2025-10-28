using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
    public class EngagementDashboardVM
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public int RatingCount { get; set; }
        public int FavoriteCount { get; set; }
        public double AvgRating { get; set; }

        // ★ 排行是 DATE（EF 會是 DateOnly），這裡用 DateOnly? 才不會型別衝突
        public DateOnly? LastRankingDate { get; set; }
        public int LastRankingRows { get; set; }

        public List<TrendPointVM>? RatingsTrend { get; set; }
        public List<TrendPointVM>? FavoritesTrend { get; set; }
    }

    public class TrendPointVM
    {
        // 趨勢通常用天維度；來源是 DATETIME2 → 聚合成日，保留 DateTime 方便前端圖表直接吃
        public DateTime Day { get; set; }
        public int Count { get; set; }
    }

    //public class PagingVM
    //{
    //    public int Page { get; set; }
    //    public int PageSize { get; set; }
    //    public int Total { get; set; }
    //}

    // Ratings
    public class RatingRowVM
    {
        public long RatingId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public byte Rating { get; set; }
        public string? ReviewText { get; set; }
        public string Status { get; set; } = "PENDING";
        public DateTime CreatedAt { get; set; }
    }

    // Favorites
    public class FavoriteRowVM
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Rankings（SQL: DATE → EF: DateOnly）
    public class RankingRowVM
    {
        public int RankingId { get; set; }
        public byte PeriodType { get; set; }  // 1=DAILY,2=WEEKLY,3=MONTHLY,4=QUARTERLY,5=YEARLY
        public DateOnly RankingDate { get; set; }        // ★ 修正：DateOnly
        public int ProductId { get; set; }
        public string RankingMetric { get; set; } = "";  // rating/click/purchase/revenue/volume
        public int RankingPosition { get; set; }
        public decimal? MetricValueNum { get; set; }
        public decimal? TradingAmount { get; set; }
        public int? TradingVolume { get; set; }
        public string? MetricNote { get; set; }
    }

    // Revenue view（SQL: DATE → EF: DateOnly）
    public class RevenueRowVM
    {
        public byte PeriodType { get; set; }
        public DateOnly RankingDate { get; set; }  // ★ 修正：DateOnly
        public decimal? RevenueAmount { get; set; }
        public int? RevenueVolume { get; set; }
    }
}