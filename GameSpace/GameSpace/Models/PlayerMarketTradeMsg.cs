using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PlayerMarketTradeMsg
{
    public int TradeMsgId { get; set; }

    public int? POrderTradepageId { get; set; }

    public string? MsgFrom { get; set; }

    public string? MessageText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual PlayerMarketOrderTradepage? POrderTradepage { get; set; }
}
