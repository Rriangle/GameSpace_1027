using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}