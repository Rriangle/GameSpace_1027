using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SGameGenre
{
    public int GenreId { get; set; }

    public string GenreName { get; set; } = null!;

    public virtual ICollection<SProductInfo> Products { get; set; } = new List<SProductInfo>();
}
