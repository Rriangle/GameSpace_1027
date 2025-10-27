using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
    public class CodeCenterDashboardVM
    {
        public int PrefixKinds { get; set; }
        public int Platforms { get; set; }
        public int Genres { get; set; }                      // ← 原 GameTypes 改為 Genres
        public int MerchTypes { get; set; }

        public List<CodeCountItemVM> PrefixDistribution { get; set; } = new();
        public List<CodeCountItemVM> PlatformTop { get; set; } = new();
        public List<CodeCountItemVM> GenreTop { get; set; } = new();   // ← 原 GameTypeTop 改為 GenreTop
        public List<CodeCountItemVM> MerchTypeTop { get; set; } = new();
    }

    public class CodeCountItemVM
    {
        public string Key { get; set; } = "";
        public int Count { get; set; }
    }

    // ========== Prefix ==========
    public class CodePrefixVM
    {
        public int Id { get; set; }  // 對應 SProductCodeRule.RuleId

        [Required, MaxLength(10)]    // DB 的 Prefix 長度上限是 10
        public string Prefix { get; set; } = "";

        [Required, MaxLength(50)]    // 對應 ProductType（DB 有唯一索引）
        public string ProductType { get; set; } = "";

        [Range(1, 30)]               // 對應 PadLength (byte)，常見是 10
        public int PadLength { get; set; } = 10;

        // 顯示用（計算而來，非 DB 欄位）
        public int ProductCount { get; set; }

        // 供畫面顯示使用（若 DB 有 Description 可對應）
        [MaxLength(100)]
        public string PrefixDescription { get; set; } = "";
    }

    // ========== Platform ==========
    public class PlatformVM
    {
        public int PlatformId { get; set; }

        [Required, MaxLength(50)]
        public string PlatformName { get; set; } = "";

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public int ProductCount { get; set; }
    }

    // ========== Genre ==========
    public class GenreVM
    {
        public int GenreId { get; set; }

        [Required, MaxLength(50)]
        public string GenreName { get; set; } = "";

        // 新 DB 的 SGameGenre 一般只有 Id/Name，以下兩個如果 DB 沒欄位就保持畫面層預設用，不入庫
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public int ProductCount { get; set; }
    }

    // ========== MerchType ==========
    public class MerchTypeVM
    {
        public int MerchTypeId { get; set; }

        [Required, MaxLength(50)]
        public string MerchTypeName { get; set; } = "";

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public int ProductCount { get; set; }
    }
}
