using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IMiniGameAdminService
    {
        // 會員點數系統
        Task<UserWallet?> GetUserPointsAsync(int userId);
        Task<List<UserWallet>> GetAllUserPointsAsync();
        Task<bool> UpdateUserPointsAsync(int userId, int points);
        Task<GameSpace.Areas.MiniGame.Models.ViewModels.PagedResult<UserWallet>> QueryUserPointsAsync(CouponQueryModel query);
        Task<List<GameSpace.Models.User>> GetUsersAsync();
        Task<bool> AdjustUserPointsAsync(int userId, int points, string reason);

        // 商城優惠券系統
        Task<List<UserCouponReadModel>> GetUserCouponsAsync(int userId);
        Task<bool> AddUserCouponAsync(int userId, int couponTypeId, int quantity = 1);
        Task<bool> RemoveUserCouponAsync(int couponId);
        Task<List<GameSpace.Models.CouponType>> GetCouponTypesAsync();
        Task<bool> IssueCouponToUserAsync(int userId, int couponTypeId, int quantity);
        Task<bool> RemoveCouponFromUserAsync(int userId, int couponTypeId);
        Task<GameSpace.Areas.MiniGame.Models.ViewModels.PagedResult<UserCouponReadModel>> QueryUserCouponsAsync(CouponQueryModel query);

        // 電子優惠券系統
        Task<List<Evoucher>> GetUserEVouchersAsync(int userId);
        Task<bool> AddUserEVoucherAsync(int userId, int evoucherTypeId, int quantity = 1);
        Task<bool> RemoveUserEVoucherAsync(int evoucherId);
        Task<List<EvoucherType>> GetEVoucherTypesAsync();
        Task<bool> IssueEVoucherToUserAsync(int userId, int evoucherTypeId, int quantity);
        Task<bool> RemoveEVoucherFromUserAsync(int userId, int evoucherTypeId);
        Task<GameSpace.Areas.MiniGame.Models.ViewModels.PagedResult<Evoucher>> QueryUserEVouchersAsync(EVoucherQueryModel query);

        // 簽到系統
        Task<List<UserSignInStat>> GetUserSignInRecordsAsync(int userId);
        Task<bool> AddSignInRecordAsync(int userId, DateTime signInDate);
        Task<bool> RemoveSignInRecordAsync(int signInId);
        Task<GameSpace.Areas.MiniGame.Models.ViewModels.PagedResult<UserSignInStat>> GetSignInStatsAsync();
        Task<SignInRuleReadModel> GetSignInRuleAsync();
        Task<bool> AddUserSignInRecordAsync(int userId, DateTime signInDate);
        Task<bool> RemoveUserSignInRecordAsync(int userId, DateTime signInDate);
        Task<GameSpace.Models.User?> GetUserByIdAsync(int userId);
        Task<List<SignInRecordReadModel>> GetUserSignInHistoryAsync(int userId);

        // 寵物系統
        Task<GameSpace.Models.Pet?> GetUserPetAsync(int userId);
        Task<List<GameSpace.Models.Pet>> GetAllPetsAsync();
        Task<bool> UpdatePetAsync(int userId, string petName, string color, string background, int experience, int level, int hunger, int happiness, int health, int energy, int cleanliness);
        Task<List<GameSpace.Models.Pet>> GetPetsAsync(PetQueryModel query);
        Task<PetSummary> GetPetSummaryAsync();
        Task<PetRuleReadModel> GetPetRuleAsync();
        Task<GameSpace.Models.Pet?> GetPetDetailAsync(int petId);
        Task<bool> UpdatePetDetailsAsync(int petId, PetUpdateModel model);
        Task<List<PetSkinColorChangeLog>> GetPetSkinColorChangeLogsAsync(PetQueryModel query);
        Task<List<PetBackgroundColorChangeLog>> GetPetBackgroundColorChangeLogsAsync(PetQueryModel query);

        // 小遊戲系統
        Task<List<GameSpace.Models.MiniGame>> GetUserGameRecordsAsync(int userId);
        Task<bool> AddGameRecordAsync(int userId, DateTime startTime, DateTime? endTime, string result, int pointsReward, int expReward, int couponReward);
        Task<List<GameSpace.Models.MiniGame>> GetGameRecordsAsync(GameSpace.Areas.MiniGame.Models.ViewModels.GameQueryModel query);
        Task<GameSummary> GetGameSummaryAsync();
        Task<GameRuleReadModel> GetGameRuleAsync();
        Task<GameSpace.Models.MiniGame?> GetGameDetailAsync(int playId);

        // 總覽系統
        Task<WalletSummary> GetWalletSummaryAsync();

        // 交易記錄
        Task<GameSpace.Areas.MiniGame.Models.ViewModels.PagedResult<WalletTransaction>> QueryWalletTransactionsAsync(CouponQueryModel query);

        // 規則設定
        SignInRuleReadModel GetSignInRule();
        Task<bool> UpdateSignInRuleAsync(SignInRuleUpdateModel model);
        PetRuleReadModel GetPetRule();
        Task<bool> UpdatePetRuleAsync(PetRuleUpdateModel model);
        GameRuleReadModel GetGameRule();
        Task<bool> UpdateGameRuleAsync(GameRuleUpdateModel model);
    }
}



