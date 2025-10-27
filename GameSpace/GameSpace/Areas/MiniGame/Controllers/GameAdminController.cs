using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Auth;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 小遊戲系統管理控制器
    /// 統一入口點，提供 _MiniGameAdminTabs 需要的路由
    /// </summary>
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class GameAdminController : MiniGameBaseController
    {
        public GameAdminController(GameSpacedatabaseContext context)
            : base(context)
        {
        }

        /// <summary>
        /// 遊戲規則設定
        /// </summary>
        public IActionResult RuleSettings()
        {
            return RedirectToAction("GameRules", "AdminMiniGame");
        }

        /// <summary>
        /// 查看會員遊戲紀錄
        /// </summary>
        public IActionResult Records(string searchTerm = "", string result = "", string sortBy = "recent", int page = 1, int pageSize = 20)
        {
            return RedirectToAction("ViewGameRecords", "AdminMiniGame", new { searchTerm, result, sortBy, page, pageSize });
        }
    }
}
