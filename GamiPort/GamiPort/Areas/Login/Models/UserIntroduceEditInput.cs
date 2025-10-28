using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.Login.Models
{
	public class UserIntroduceEditInput
	{
		[Required, StringLength(30)]
		public string UserNickName { get; set; } = null!;

		[Required, RegularExpression("^(M|F)$")]
		public string Gender { get; set; } = null!;

		[Required, Phone, StringLength(20)]
		public string Cellphone { get; set; } = null!;

		[Required, StringLength(200)]
		public string Address { get; set; } = null!;

		public string? UserIntroduce1 { get; set; }
	}
}