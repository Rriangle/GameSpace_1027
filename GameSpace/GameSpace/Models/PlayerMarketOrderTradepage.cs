using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PlayerMarketOrderTradepage
{
    public int POrderTradepageId { get; set; }

    public int? POrderId { get; set; }

    public int? PProductId { get; set; }

    public int? POrderPlatformFee { get; set; }

    public DateTime? SellerTransferredAt { get; set; }

    public DateTime? BuyerReceivedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual PlayerMarketOrderInfo? POrder { get; set; }

    public virtual PlayerMarketProductInfo? PProduct { get; set; }

    public virtual ICollection<PlayerMarketTradeMsg> PlayerMarketTradeMsgs { get; set; } = new List<PlayerMarketTradeMsg>();
}
