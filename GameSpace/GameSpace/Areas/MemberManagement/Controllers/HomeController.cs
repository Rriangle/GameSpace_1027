using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	public class HomeController : Controller
	{
		//public IActionResult User()
		//{
		//	return View();
		//}
		public IActionResult Manager()
		{
			return View();
		}
		public IActionResult Index()
		{
			return View();
		}
	}
}
