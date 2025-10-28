using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class CsAgentPermission
{
    public int AgentPermissionId { get; set; }

    public int AgentId { get; set; }

    public bool CanAssign { get; set; }

    public bool CanTransfer { get; set; }

    public bool CanAccept { get; set; }

    public bool CanEditMuteAll { get; set; }

    public virtual CsAgent Agent { get; set; } = null!;
}
