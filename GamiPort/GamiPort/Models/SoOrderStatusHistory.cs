using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SoOrderStatusHistory
{
    public long HistoryId { get; set; }

    public int OrderId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public int? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Note { get; set; }

    public virtual ManagerDatum? ChangedByNavigation { get; set; }

    public virtual SoOrderInfo Order { get; set; } = null!;
}
