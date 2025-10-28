using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ProductInfoAuditLog
{
    public long LogId { get; set; }

    public int ProductId { get; set; }

    public string ActionType { get; set; } = null!;

    public string FieldName { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ManagerId { get; set; }

    public DateTime ChangedAt { get; set; }

    public virtual ManagerDatum? Manager { get; set; }

    
}
