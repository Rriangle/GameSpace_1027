using System.ComponentModel.DataAnnotations;



namespace GameSpace.Models.ViewModels
{
	public class LoginInputVM
	{
		[Required(ErrorMessage = "請輸入帳號")]
		[Display(Name = "管理者帳號")]
		public string ManagerAccount { get; set; } = "";

		[Required(ErrorMessage = "請輸入密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "密碼")]
		public string ManagerPassword { get; set; } = "";
	}
}
