using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class CsAgent
{
    public int AgentId { get; set; }

    public int ManagerId { get; set; }

    public bool IsActive { get; set; }

    public byte MaxConcurrent { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByManager { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedByManager { get; set; }

    public virtual ManagerDatum? CreatedByManagerNavigation { get; set; }

    public virtual CsAgentPermission? CsAgentPermission { get; set; }

    public virtual ManagerDatum Manager { get; set; } = null!;

    public virtual ManagerDatum? UpdatedByManagerNavigation { get; set; }
}
