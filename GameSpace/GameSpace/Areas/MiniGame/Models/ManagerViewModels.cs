using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理員列表視圖模型
    /// </summary>
    public class ManagerListViewModel
    {
        public int Manager_Id { get; set; }
        public string Manager_Name { get; set; } = string.Empty;
        public string Manager_Account { get; set; } = string.Empty;
        public string Manager_Email { get; set; } = string.Empty;
        public bool Manager_EmailConfirmed { get; set; }
        public bool Manager_LockoutEnabled { get; set; }
        public DateTime? Manager_LockoutEnd { get; set; }
        public DateTime Administrator_registration_date { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        
        /// <summary>
        /// 是否被鎖定
        /// </summary>
        public bool IsLocked => Manager_LockoutEnabled && Manager_LockoutEnd.HasValue && Manager_LockoutEnd > DateTime.Now;
        
        /// <summary>
        /// 狀態文字
        /// </summary>
        public string StatusText
        {
            get
            {
                if (IsLocked) return "已鎖定";
                if (!Manager_EmailConfirmed) return "未確認";
                return "正常";
            }
        }
    }

    /// <summary>
    /// 管理員列表頁面視圖模型
    /// </summary>
    public class AdminManagerIndexViewModel
    {
        public List<ManagerListViewModel> Managers { get; set; } = new List<ManagerListViewModel>();
        public string SearchTerm { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> RoleOptions { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    }

    /// <summary>
    /// 角色信息
    /// </summary>
    public class RoleInfo
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }

    /// <summary>
    /// 管理員詳情視圖模型
    /// </summary>
    public class ManagerDetailsViewModel
    {
        public int Manager_Id { get; set; }
        public string Manager_Name { get; set; } = string.Empty;
        public string Manager_Account { get; set; } = string.Empty;
        public string Manager_Email { get; set; } = string.Empty;
        public bool Manager_EmailConfirmed { get; set; }
        public int Manager_AccessFailedCount { get; set; }
        public bool Manager_LockoutEnabled { get; set; }
        public DateTime? Manager_LockoutEnd { get; set; }
        public DateTime Administrator_registration_date { get; set; }
        public List<RoleInfo> Roles { get; set; } = new List<RoleInfo>();
        
        /// <summary>
        /// 是否被鎖定
        /// </summary>
        public bool IsLocked => Manager_LockoutEnabled && Manager_LockoutEnd.HasValue && Manager_LockoutEnd > DateTime.Now;
    }

    /// <summary>
    /// 新增管理員視圖模型
    /// </summary>
    public class ManagerCreateViewModel
    {
        [Required(ErrorMessage = "管理員姓名不能為空")]
        [StringLength(50, ErrorMessage = "管理員姓名長度不能超過50個字符")]
        [Display(Name = "管理員姓名")]
        public string Manager_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "管理員帳號不能為空")]
        [StringLength(50, ErrorMessage = "管理員帳號長度不能超過50個字符")]
        [Display(Name = "管理員帳號")]
        public string Manager_Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "管理員密碼不能為空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "管理員密碼長度必須在6-100個字符之間")]
        [Display(Name = "管理員密碼")]
        public string Manager_Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "確認密碼不能為空")]
        [Compare("Manager_Password", ErrorMessage = "確認密碼與密碼不一致")]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "管理員電子郵件不能為空")]
        [StringLength(100, ErrorMessage = "管理員電子郵件長度不能超過100個字符")]
        [EmailAddress(ErrorMessage = "管理員電子郵件格式不正確")]
        [Display(Name = "管理員電子郵件")]
        public string Manager_Email { get; set; } = string.Empty;

        [Display(Name = "電子郵件確認狀態")]
        public bool Manager_EmailConfirmed { get; set; } = false;

        [Display(Name = "分配角色")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();

        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> RoleOptions { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    }

    /// <summary>
    /// 編輯管理員視圖模型
    /// </summary>
    public class ManagerEditViewModel
    {
        public int Manager_Id { get; set; }

        [Required(ErrorMessage = "管理員姓名不能為空")]
        [StringLength(50, ErrorMessage = "管理員姓名長度不能超過50個字符")]
        [Display(Name = "管理員姓名")]
        public string Manager_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "管理員帳號不能為空")]
        [StringLength(50, ErrorMessage = "管理員帳號長度不能超過50個字符")]
        [Display(Name = "管理員帳號")]
        public string Manager_Account { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "管理員密碼長度必須在6-100個字符之間")]
        [Display(Name = "新密碼（留空則不修改）")]
        public string? Manager_Password { get; set; }

        [Compare("Manager_Password", ErrorMessage = "確認密碼與密碼不一致")]
        [Display(Name = "確認新密碼")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "管理員電子郵件不能為空")]
        [StringLength(100, ErrorMessage = "管理員電子郵件長度不能超過100個字符")]
        [EmailAddress(ErrorMessage = "管理員電子郵件格式不正確")]
        [Display(Name = "管理員電子郵件")]
        public string Manager_Email { get; set; } = string.Empty;

        [Display(Name = "電子郵件確認狀態")]
        public bool Manager_EmailConfirmed { get; set; }

        [Display(Name = "帳號鎖定啟用狀態")]
        public bool Manager_LockoutEnabled { get; set; }

        [Display(Name = "分配角色")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();

        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> RoleOptions { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    }

    /// <summary>
    /// 刪除管理員視圖模型
    /// </summary>
    public class ManagerDeleteViewModel
    {
        public int Manager_Id { get; set; }
        public string Manager_Name { get; set; } = string.Empty;
        public string Manager_Account { get; set; } = string.Empty;
        public string Manager_Email { get; set; } = string.Empty;
        public DateTime Administrator_registration_date { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}

