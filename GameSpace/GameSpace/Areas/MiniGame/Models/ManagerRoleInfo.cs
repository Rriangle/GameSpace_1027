using System;
using System.Collections.Generic;

namespace GameSpace.Areas.MiniGame.Models
{
    public class ManagerRoleInfo
    {
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerAccount { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }
}


