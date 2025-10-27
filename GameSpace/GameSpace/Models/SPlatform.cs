using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SPlatform
{
    public int PlatformId { get; set; }

    public string PlatformName { get; set; } = null!;

    public virtual ICollection<SGameProductDetail> SGameProductDetails { get; set; } = new List<SGameProductDetail>();
}
