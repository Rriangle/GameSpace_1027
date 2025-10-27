using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MemberManagement.Models
{
	public class ManagerWithRoleVM
	{
		[Display(Name = "管理者編號")]
		public int ManagerId { get; set; }

		[Display(Name = "管理者姓名")]
		public string? ManagerName { get; set; }

		[Display(Name = "管理者角色編號")]
		public int ManagerRoleId { get; set; }

		[Display(Name = "職位名稱")]
		public string RoleName { get; set; } = string.Empty;


	}
}
