using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MemberManagement.Models
{
	public class UserPermissionVM
	{
		[Display(Name = "使用者編號")]
		public int UserId { get; set; }

		[Display(Name = "暱稱")]
		public string UserNickName { get; set; } = string.Empty;

		[Display(Name = "帳號狀態")]
		public bool UserStatus { get; set; }

		[Display(Name = "商城權限")]
		public bool ShoppingPermission { get; set; }

		[Display(Name = "訊息權限")]
		public bool MessagePermission { get; set; }

		[Display(Name = "銷售權限")]
		public bool SalesAuthority { get; set; }
	}
}
