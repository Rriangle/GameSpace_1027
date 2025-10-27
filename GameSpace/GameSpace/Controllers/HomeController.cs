// Controllers/HomeController.cs
using System;
using System.Diagnostics;
using System.Security.Claims;
using GameSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Controllers
{
	[AllowAnonymous] // 讓未登入的訪客也能看到維修頁
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		public HomeController(ILogger<HomeController> logger) => _logger = logger;

		public IActionResult Index()
		{
			try
			{
				if (User?.Identity?.IsAuthenticated == true && User.HasClaim(c => c.Type == "ManagerId"))
				{
					// 若 AdminDashboard 在某個 Area，改成：new { area = "MemberManagement" }
					return RedirectToAction("Index", "AdminDashboard");
				}
				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Home/Index 發生例外，改導維修頁");
				return RedirectToAction(nameof(Maintenance));
			}
		}

		public IActionResult Privacy() => View();

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
			=> View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

		public IActionResult Dashboard() => View();

		// ===== 維修頁 =====
		[HttpGet]
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Maintenance()
		{
			Response.StatusCode = 503; // Service Unavailable
			return View();             // 對應 Views/Home/Maintenance.cshtml
		}

		// 統一處理狀態碼（配合 Program.cs 的 UseStatusCodePagesWithReExecute("/Home/Http{0}")）
		[HttpGet("Home/Http{code:int}")]
		public IActionResult Http(int code)
		{
			if (code >= 500 || code == 404)
			{
				Response.StatusCode = 503;
				return View("Maintenance");
			}
			return View("Error"); // 404 等可用你現有的 Error.cshtml
		}

		// 測試：造一個未處理例外（瀏覽 /Home/Boom 應導向維修頁）
		[HttpGet("Home/Force500")]
		public IActionResult Force500() => StatusCode(500);
	}
}
