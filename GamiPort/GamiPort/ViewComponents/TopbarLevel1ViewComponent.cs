using Microsoft.AspNetCore.Mvc;
using GamiPort.Models.ViewModels;
using System.Security.Claims;

namespace GamiPort.ViewComponents
{
	public class TopbarLevel1ViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			var user = HttpContext.User;
			var vm = new TopbarVM
			{
				IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
				NickName = user?.FindFirst("UserNickName")?.Value
						   ?? user?.Identity?.Name
						   ?? "訪客"
			};
			return View(vm);
		}
	}
}
