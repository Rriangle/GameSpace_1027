using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.Login.Models
{
	public class ChangePasswordVM
	{
		[Required(ErrorMessage = "請輸入舊密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "舊密碼")]
		public string OldPassword { get; set; } = null!;

		[Required(ErrorMessage = "請輸入新密碼")]
		[DataType(DataType.Password)]
		[StringLength(32, MinimumLength = 8, ErrorMessage = "密碼長度需介於 8~32 字元")]
		[Display(Name = "新密碼")]
		public string NewPassword { get; set; } = null!;

		[Required(ErrorMessage = "請再次確認新密碼")]
		[DataType(DataType.Password)]
		[Compare(nameof(NewPassword), ErrorMessage = "兩次輸入的新密碼不一致")]
		[Display(Name = "確認新密碼")]
		public string ConfirmPassword { get; set; } = null!;
	}
}
