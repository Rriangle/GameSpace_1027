using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MemberManagement.Models
{
	[Table("ManagerRole")]
	[PrimaryKey(nameof(ManagerId), nameof(ManagerRoleId))]
	public class ManagerRoleEntry
	{
		[Display(Name = "管理者編號")]
		public int ManagerId { get; set; }

		[Display(Name = "管理者職位編號")]
		public int ManagerRoleId { get; set; }

		// 關聯到管理者
		public ManagerDatum ManagerDatum { get; set; }

		// 一個角色對應一筆權限
		public ManagerRolePermission ManagerRolePermission { get; set; }
	}
}
