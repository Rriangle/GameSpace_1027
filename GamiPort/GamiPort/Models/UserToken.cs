using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class UserToken
{
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string Provider { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime ExpireAt { get; set; }

    public virtual User User { get; set; } = null!;
}
