using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PlayerMarketProductImg
{
    public int PProductImgId { get; set; }

    public int? PProductId { get; set; }

    public string? PProductImgUrl { get; set; }

    public virtual PlayerMarketProductInfo? PProduct { get; set; }
}
