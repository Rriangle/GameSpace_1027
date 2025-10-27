using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MiniGame.Controllers
{
	[Area("MiniGame")]
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
