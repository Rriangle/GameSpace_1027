using System.ComponentModel.DataAnnotations;


namespace GameSpace.Areas.MemberManagement.Models
{ 

public partial class ManagerRolePermissionVM
{
    [Display(Name = "管理員角色編號")]
	public int ManagerRoleId { get; set; }
    [Display(Name = "管理員角色名稱")]
	[Required(ErrorMessage = "請輸入職位名稱")]
	[StringLength(16, ErrorMessage = "職位名稱長度不可超過 16 個字")]
	public string RoleName { get; set; } = string.Empty;
		[Display(Name = "最高權限系統管理")] 
	public bool AdministratorPrivilegesManagement { get; set; }
    [Display(Name = "使用者系統管理")]
	public bool UserStatusManagement { get; set; }
    [Display(Name = "商品系統管理")]
	public bool ShoppingPermissionManagement { get; set; }
    [Display(Name = "論壇系統管理")]

	public bool MessagePermissionManagement { get; set; }
    [Display(Name = "小遊戲系統管理")]
	public bool PetRightsManagement { get; set; }
    [Display(Name = "社群系統管理")]
	public bool CustomerService { get; set; }

		//public List<string> ManagerNames { get; set; } = new List<string>();
		//public ManagerRoleEntry ManagerRole { get; set; }
		
	}
}
