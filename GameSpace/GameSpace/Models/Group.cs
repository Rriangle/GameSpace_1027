using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public int OwnerUserId { get; set; }

    public string GroupName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPrivate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<GroupBlock> GroupBlocks { get; set; } = new List<GroupBlock>();

    public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<GroupReadState> GroupReadStates { get; set; } = new List<GroupReadState>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User OwnerUser { get; set; } = null!;
}
