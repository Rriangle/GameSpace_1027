using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GamiPort.Services
{
	public class CurrentUserService : ICurrentUserService
	{
		private readonly IHttpContextAccessor _http;
		public CurrentUserService(IHttpContextAccessor http) => _http = http;

		private ClaimsPrincipal? Principal => _http.HttpContext?.User;

		public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

		public int? UserId
		{
			get
			{
				var id = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
				return int.TryParse(id, out var x) ? x : null;
			}
		}

		public string? NickName =>
			Principal?.FindFirst("UserNickName")?.Value ?? Principal?.Identity?.Name;

		public string? Email => Principal?.FindFirst(ClaimTypes.Email)?.Value;
	}
}
