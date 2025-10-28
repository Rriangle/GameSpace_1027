using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
