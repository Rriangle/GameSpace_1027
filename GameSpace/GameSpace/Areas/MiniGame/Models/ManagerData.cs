using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理員資料表
    /// </summary>
    [Table("ManagerData")]
    public class ManagerData
    {
        [Key]
        [Column("Manager_Id")]
        public int ManagerId { get; set; }

        [Column("Manager_Name")]
        [MaxLength(30)]
        public string? ManagerName { get; set; }

        [Column("Manager_Account")]
        [MaxLength(30)]
        public string? ManagerAccount { get; set; }

        [Column("Manager_Password")]
        [MaxLength(200)]
        public string? ManagerPassword { get; set; }

        [Column("Administrator_registration_date")]
        public DateTime? AdministratorRegistrationDate { get; set; }

        [Column("Manager_Email")]
        [MaxLength(255)]
        [Required]
        public string ManagerEmail { get; set; } = string.Empty;

        [Column("Manager_EmailConfirmed")]
        public bool ManagerEmailConfirmed { get; set; } = false;

        [Column("Manager_AccessFailedCount")]
        public int ManagerAccessFailedCount { get; set; } = 0;

        [Column("Manager_LockoutEnabled")]
        public bool ManagerLockoutEnabled { get; set; } = true;

        [Column("Manager_LockoutEnd")]
        public DateTime? ManagerLockoutEnd { get; set; }

        // 導航屬性
        public virtual ICollection<ManagerRolePermission> ManagerRoles { get; set; } = new List<ManagerRolePermission>();
    }
}



