using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.social_hub.Models
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "請輸入帳號或 ID")]
		[Display(Name = "帳號或 ID")]
		public string Account { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "密碼")]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "記住我（7 天）")]
		public bool RememberMe { get; set; }

		public string? ReturnUrl { get; set; }
	}
}
