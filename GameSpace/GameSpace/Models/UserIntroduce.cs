using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class UserIntroduce
{
    public int UserId { get; set; }

    public string UserNickName { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string IdNumber { get; set; } = null!;

    public string Cellphone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Address { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public DateTime CreateAccount { get; set; }

    public byte[]? UserPicture { get; set; }

    public string? UserIntroduce1 { get; set; }

    public virtual User User { get; set; } = null!;
}
