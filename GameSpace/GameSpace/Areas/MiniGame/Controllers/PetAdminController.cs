using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Auth;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物系統管理控制器
    /// 統一入口點，提供 _MiniGameAdminTabs 需要的路由
    /// </summary>
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class PetAdminController : MiniGameBaseController
    {
        public PetAdminController(GameSpacedatabaseContext context)
            : base(context)
        {
        }

        /// <summary>
        /// 整體規則設定
        /// </summary>
        public IActionResult RuleSettings()
        {
            return RedirectToAction("SystemRules", "AdminPet");
        }

        /// <summary>
        /// 會員個別寵物設定
        /// </summary>
        public IActionResult MemberEdit(int? id)
        {
            return RedirectToAction("IndividualSettings", "AdminPet", new { id });
        }

        /// <summary>
        /// 會員個別寵物清單與查詢
        /// </summary>
        public IActionResult MemberList(string searchTerm = "", string sortBy = "name", int page = 1, int pageSize = 20)
        {
            return RedirectToAction("QueryPets", "AdminPet", new { searchTerm, sortBy, page, pageSize });
        }

        /// <summary>
        /// 換膚色紀錄
        /// </summary>
        public IActionResult SkinChangeLog(int? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            return RedirectToAction("ColorChangeHistory", "AdminPet", new { userId, startDate, endDate, page, pageSize, changeType = "skin" });
        }

        /// <summary>
        /// 換背景紀錄
        /// </summary>
        public IActionResult BackgroundChangeLog(int? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            return RedirectToAction("ColorChangeHistory", "AdminPet", new { userId, startDate, endDate, page, pageSize, changeType = "background" });
        }
    }
}
