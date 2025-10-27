using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class MemberSalesProfile
{
    public int UserId { get; set; }

    public int? BankCode { get; set; }

    public string? BankAccountNumber { get; set; }

    public byte[]? AccountCoverPhoto { get; set; }

    public virtual User User { get; set; } = null!;
}
