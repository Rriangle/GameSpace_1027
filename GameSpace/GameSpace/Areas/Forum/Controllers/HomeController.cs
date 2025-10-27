using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.Forum.Controllers
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
