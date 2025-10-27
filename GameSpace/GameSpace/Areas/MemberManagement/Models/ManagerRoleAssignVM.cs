using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameSpace.Areas.MemberManagement.Models
{
	public class ManagerRoleAssignVM
	{
		[Display(Name = "管理者編號")]
		public int ManagerId { get; set; }

		[Display(Name = "管理者姓名")]
		public string? ManagerName { get; set; }

		[Display(Name = "角色")]
		[Required(ErrorMessage = "請選擇角色")]
		public int SelectedRoleId { get; set; }

		// 下拉選單資料
		public IEnumerable<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
	}
}
