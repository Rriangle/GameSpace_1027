using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	public class SidebarController : Controller
	{
		// 有人誤點到這裡，就轉去正常頁面並附 sidebar 參數
		public IActionResult Admin()
			=> RedirectToAction("Index", "ManagerData",
				new { area = "MemberManagement", sidebar = "admin" });

		public IActionResult Member()
			=> RedirectToAction("Index", "UserIntroduces",
				new { area = "MemberManagement", sidebar = "member" });
	}
}