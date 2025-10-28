using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
