using System.ComponentModel.DataAnnotations;

namespace GameSpace.Models.ViewModels
{
	public class VerifyEmailInputVM
	{
		[Display(Name = "驗證碼")]
		[Required(ErrorMessage = "請輸入驗證碼")]
		[StringLength(6, MinimumLength = 6, ErrorMessage = "驗證碼為 6 碼")]
		public string Code { get; set; } = "";

		// 顯示用
		public string? MaskedEmail { get; set; }
	}
}
