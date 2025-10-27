using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SoPayMethod
{
    public int PayMethodId { get; set; }

    public string MethodCode { get; set; } = null!;

    public string MethodName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SoOrderInfo> SoOrderInfos { get; set; } = new List<SoOrderInfo>();
}
