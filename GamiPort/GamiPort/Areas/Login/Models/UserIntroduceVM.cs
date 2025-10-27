using GamiPort.Models;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GamiPort.Areas.Login.Models;
public partial class UserIntroduceVM
{
	public int UserId { get; set; }

	[Required, StringLength(30)]
	public string UserNickName { get; set; } = null!;

	[Required, RegularExpression("^(M|F)$")]
	public string Gender { get; set; } = null!;

	[Required, StringLength(20)]
	public string IdNumber { get; set; } = null!;

	[Required, Phone]
	public string Cellphone { get; set; } = null!;

	[Required, EmailAddress]
	public string Email { get; set; } = null!;

	[Required, StringLength(200)]
	public string Address { get; set; } = null!;

	public DateOnly DateOfBirth { get; set; } = DateOnly.FromDateTime(DateTime.Today);

	public DateTime CreateAccount { get; set; }

	public byte[]? UserPicture { get; set; }

	public string? UserIntroduce1 { get; set; }

	public virtual User User { get; set; } = null!;
}
