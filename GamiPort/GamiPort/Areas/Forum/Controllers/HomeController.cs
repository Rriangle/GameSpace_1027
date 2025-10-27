using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.Forum.Controllers
{
	[Area("Forum")]
	public class HomeController : Controller
	{
		
		public IActionResult Index()
		{
			return View();
		}
	}
}
