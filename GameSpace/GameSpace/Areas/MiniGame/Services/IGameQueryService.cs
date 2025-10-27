using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 撠??脫閰Ｘ????ｇ?Admin 撠嚗?
    /// </summary>
    public interface IGameQueryService
    {
        /// <summary>
        /// ???閬?閮剖?嚗??∟身摰??萸活?賊??塚?
        /// </summary>
        Task<GameRuleViewModel> GetGameRulesAsync();

        /// <summary>
        /// ?亥岷??蝝???舀蝭拚????
        /// </summary>
        /// <param name="query">?亥岷璇辣</param>
        /// <returns>?蝝??????/returns>
        Task<GameRecordsListViewModel> QueryGameRecordsAsync(GameRecordQueryModel query);

        /// <summary>
        /// ???桃??蝝?底蝝啗???
        /// </summary>
        /// <param name="playId">?閮? ID</param>
        Task<GameRecordDetailViewModel?> GetGameRecordDetailAsync(int playId);

        /// <summary>
        /// ???蝯梯?鞈?
        /// </summary>
        Task<Models.ViewModels.GameStatisticsViewModel> GetGameStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// ??雿輻???仿??脫活??
        /// </summary>
        Task<int> GetUserTodayGameCountAsync(int userId);

        /// <summary>
        /// ?????漲閮剖??”
        /// </summary>
        Task<List<GameLevelSettingViewModel>> GetGameLevelSettingsAsync();
    }
}
