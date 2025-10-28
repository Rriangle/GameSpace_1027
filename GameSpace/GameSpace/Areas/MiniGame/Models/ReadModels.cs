namespace GameSpace.Areas.MiniGame.Models
{
    // 優惠券讀取模型
    public class CouponReadModel
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public string CouponTypeName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinSpend { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int PointsCost { get; set; }
        public string? Description { get; set; }
    }

    // 電子券讀取模型
    public class EVoucherReadModel
    {
        public int EvoucherId { get; set; }
        public string EVoucherCode { get; set; } = string.Empty;
        public string EVoucherTypeName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public decimal ValueAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int PointsCost { get; set; }
        public string? Description { get; set; }
    }

    // 寵物讀取模型
    public class PetReadModel
    {
        public int PetID { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Cleanliness { get; set; }
        public int Health { get; set; }
        public string SkinColor { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public DateTime LevelUpTime { get; set; }
        public DateTime SkinColorChangedTime { get; set; }
        public DateTime BackgroundColorChangedTime { get; set; }
        public int PointsChangedSkinColor { get; set; }
        public int PointsChangedBackgroundColor { get; set; }
        public int PointsGainedLevelUp { get; set; }
        public DateTime PointsGainedTimeLevelUp { get; set; }
    }

    // 遊戲記錄讀取模型
    public class GameReadModel
    {
        public int GameID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string GameType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Result { get; set; }
        public int PointsEarned { get; set; }
        public int ExpEarned { get; set; }
        public int CouponEarned { get; set; }
        public string? SessionID { get; set; }
    }

    // 錢包歷史讀取模型
    public class WalletHistoryReadModel
    {
        public int LogID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public int PointsChanged { get; set; }
        public string? ItemCode { get; set; }
        public string? Description { get; set; }
        public DateTime ChangeTime { get; set; }
    }

    // 簽到記錄讀取模型
    public class SignInReadModel
    {
        public int LogID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public DateTime PointsGainedTime { get; set; }
        public int ExpGained { get; set; }
        public DateTime ExpGainedTime { get; set; }
        public string? CouponGained { get; set; }
        public DateTime CouponGainedTime { get; set; }
    }

    // 管理員讀取模型
    public class ManagerReadModel
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
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
    }

    // 用戶讀取模型
    public class UserReadModel
    {
        public int UserId { get; set; }
        public string User_name { get; set; } = string.Empty;
        public string User_Account { get; set; } = string.Empty;
        public string User_email { get; set; } = string.Empty;
        public bool User_EmailConfirmed { get; set; }
        public int User_AccessFailedCount { get; set; }
        public bool User_LockoutEnabled { get; set; }
        public DateTime? UserLockoutEnd { get; set; }
        public DateTime User_registration_date { get; set; }
        public int UserPoint { get; set; }
        public int PetCount { get; set; }
        public int GameCount { get; set; }
        public int SignInCount { get; set; }
    }

    // 統計數據讀取模型
    public class StatisticsReadModel
    {
        public int TotalUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGames { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TotalPoints { get; set; }
        public int ActiveUsersToday { get; set; }
        public int GamesPlayedToday { get; set; }
        public int SignInsToday { get; set; }
        public decimal TotalPointsIssued { get; set; }
        public decimal TotalPointsUsed { get; set; }
    }
}

