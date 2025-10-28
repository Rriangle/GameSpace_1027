using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物查詢服務介面
    /// </summary>
    public interface IPetQueryService
    {
        /// <summary>
        /// 取得會員寵物清單（分頁）
        /// </summary>
        Task<PetAdminListPagedResult> GetPetListAsync(PetAdminListQueryModel query);

        /// <summary>
        /// 取得單一寵物詳細資料
        /// </summary>
        Task<PetAdminDetailViewModel?> GetPetDetailAsync(int petId);

        /// <summary>
        /// 取得寵物膚色更換歷史記錄（分頁）
        /// </summary>
        Task<PetColorChangeHistoryPagedResult> GetColorChangeHistoryAsync(PetColorChangeHistoryQueryModel query);

        /// <summary>
        /// 取得寵物背景更換歷史記錄（分頁）
        /// </summary>
        Task<PetBackgroundChangeHistoryPagedResult> GetBackgroundChangeHistoryAsync(PetBackgroundChangeHistoryQueryModel query);
    }
}
