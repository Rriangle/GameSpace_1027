using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class PlayerMarketProductInfo
{
    public int PProductId { get; set; }

    public string? PProductType { get; set; }

    public string? PProductTitle { get; set; }

    public string? PProductName { get; set; }

    public string? PProductDescription { get; set; }

    public int? ProductId { get; set; }

    public int? SellerId { get; set; }

    public string? PStatus { get; set; }

    public decimal? Price { get; set; }

    public string? PProductImgId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PlayerMarketOrderInfo> PlayerMarketOrderInfos { get; set; } = new List<PlayerMarketOrderInfo>();

    public virtual ICollection<PlayerMarketOrderTradepage> PlayerMarketOrderTradepages { get; set; } = new List<PlayerMarketOrderTradepage>();

    public virtual ICollection<PlayerMarketProductImg> PlayerMarketProductImgs { get; set; } = new List<PlayerMarketProductImg>();

    public virtual ICollection<PlayerMarketRanking> PlayerMarketRankings { get; set; } = new List<PlayerMarketRanking>();

    public virtual User? Seller { get; set; }
}
