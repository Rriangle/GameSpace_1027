// 路徑：Areas/social_hub/Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.social_hub.Controllers
{
	/// <summary>
	/// Demo 首頁：提供「發通知」的測試頁（Index.cshtml）
	/// </summary>
	[Area("social_hub")]
	public sealed class HomeController : Controller
	{
		// GET: /social_hub/Home/Index
		public IActionResult Index() => View();
		// GET: /social_hub/Home/Friends
		public IActionResult Friends() => View();

		public IActionResult Combos() => View();
	}
}
