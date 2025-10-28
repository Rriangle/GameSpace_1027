using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SUserFavorite
{
    public int UserId { get; set; }

    public int ProductId { get; set; }

    public DateTime CreatedAt { get; set; }
}
