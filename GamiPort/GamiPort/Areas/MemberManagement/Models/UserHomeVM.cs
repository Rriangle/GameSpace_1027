using System.ComponentModel.DataAnnotations;
using GamiPort.Models;

namespace GamiPort.Areas.MemberManagement.Models;

public partial class UserHomeVM
{
    public int UserId { get; set; }

    public string? Title { get; set; }

    public byte[]? Theme { get; set; }

    public bool IsPublic { get; set; }

    public string? HomeCode { get; set; }

    public int VisitCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
