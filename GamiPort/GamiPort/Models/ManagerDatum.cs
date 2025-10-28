using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class ManagerDatum
{
    public int ManagerId { get; set; }

    public string? ManagerName { get; set; }

    public string? ManagerAccount { get; set; }

    public string? ManagerPassword { get; set; }

    public DateTime? AdministratorRegistrationDate { get; set; }

    public string ManagerEmail { get; set; } = null!;

    public bool ManagerEmailConfirmed { get; set; }

    public int ManagerAccessFailedCount { get; set; }

    public bool ManagerLockoutEnabled { get; set; }

    public DateTime? ManagerLockoutEnd { get; set; }

    public virtual ICollection<CsAgent> CsAgentCreatedByManagerNavigations { get; set; } = new List<CsAgent>();

    public virtual CsAgent? CsAgentManager { get; set; }

    public virtual ICollection<CsAgent> CsAgentUpdatedByManagerNavigations { get; set; } = new List<CsAgent>();

    public virtual ICollection<Mute> Mutes { get; set; } = new List<Mute>();

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PetBackgroundCostSetting> PetBackgroundCostSettings { get; set; } = new List<PetBackgroundCostSetting>();

    public virtual ICollection<SoOrderStatusHistory> SoOrderStatusHistories { get; set; } = new List<SoOrderStatusHistory>();

    public virtual ICollection<SupportTicket> SupportTicketAssignedManagers { get; set; } = new List<SupportTicket>();

    public virtual ICollection<SupportTicketAssignment> SupportTicketAssignmentAssignedByManagers { get; set; } = new List<SupportTicketAssignment>();

    public virtual ICollection<SupportTicketAssignment> SupportTicketAssignmentFromManagers { get; set; } = new List<SupportTicketAssignment>();

    public virtual ICollection<SupportTicketAssignment> SupportTicketAssignmentToManagers { get; set; } = new List<SupportTicketAssignment>();

    public virtual ICollection<SupportTicket> SupportTicketClosedByManagers { get; set; } = new List<SupportTicket>();

    public virtual ICollection<SupportTicketMessage> SupportTicketMessages { get; set; } = new List<SupportTicketMessage>();

    public virtual ICollection<SystemSetting> SystemSettings { get; set; } = new List<SystemSetting>();

    public virtual ICollection<ManagerRolePermission> ManagerRoles { get; set; } = new List<ManagerRolePermission>();
}
