using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string UserAccount { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public bool UserEmailConfirmed { get; set; }

    public bool UserPhoneNumberConfirmed { get; set; }

    public bool UserTwoFactorEnabled { get; set; }

    public int UserAccessFailedCount { get; set; }

    public bool UserLockoutEnabled { get; set; }

    public DateTime? UserLockoutEnd { get; set; }

    public DateTime CreateAccount { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    public virtual ICollection<EvoucherRedeemLog> EvoucherRedeemLogs { get; set; } = new List<EvoucherRedeemLog>();

    public virtual ICollection<Evoucher> Evouchers { get; set; } = new List<Evoucher>();

    public virtual ICollection<GroupBlock> GroupBlockBlockedByUsers { get; set; } = new List<GroupBlock>();

    public virtual ICollection<GroupBlock> GroupBlockUsers { get; set; } = new List<GroupBlock>();

    public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<GroupReadState> GroupReadStates { get; set; } = new List<GroupReadState>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual MemberSalesProfile? MemberSalesProfile { get; set; }

    public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public virtual ICollection<PlayerMarketOrderInfo> PlayerMarketOrderInfoBuyers { get; set; } = new List<PlayerMarketOrderInfo>();

    public virtual ICollection<PlayerMarketOrderInfo> PlayerMarketOrderInfoSellers { get; set; } = new List<PlayerMarketOrderInfo>();

    public virtual ICollection<PlayerMarketProductInfo> PlayerMarketProductInfos { get; set; } = new List<PlayerMarketProductInfo>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    public virtual ICollection<Relation> RelationRequestedByNavigations { get; set; } = new List<Relation>();

    public virtual ICollection<Relation> RelationUserIdLargeNavigations { get; set; } = new List<Relation>();

    public virtual ICollection<Relation> RelationUserIdSmallNavigations { get; set; } = new List<Relation>();

    public virtual ICollection<SoOrderInfo> SoOrderInfos { get; set; } = new List<SoOrderInfo>();

    public virtual ICollection<SupportTicketMessage> SupportTicketMessages { get; set; } = new List<SupportTicketMessage>();

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();

    public virtual ICollection<ThreadPost> ThreadPosts { get; set; } = new List<ThreadPost>();

    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();

    public virtual UserHome? UserHome { get; set; }

    public virtual UserIntroduce? UserIntroduce { get; set; }

    public virtual UserRight? UserRight { get; set; }

    public virtual UserSalesInformation? UserSalesInformation { get; set; }

    public virtual ICollection<UserSignInStat> UserSignInStats { get; set; } = new List<UserSignInStat>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();

    public virtual UserWallet? UserWallet { get; set; }

    public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
}
