using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Infrastructure.Login
{
	/// <summary>從 HttpContext（含 AdminCookie）讀取身分與授權資訊。</summary>
	public sealed class CookieAndAdminCookieLoginIdentity : ILoginIdentity
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IAuthenticationSchemeProvider _schemeProvider;

		private const string AdminCookieScheme = "AdminCookie";

		public CookieAndAdminCookieLoginIdentity(
			IHttpContextAccessor httpContextAccessor,
			IAuthenticationSchemeProvider schemeProvider)
		{
			_httpContextAccessor = httpContextAccessor;
			_schemeProvider = schemeProvider;
		}

		public async Task<LoginIdentityResult> GetAsync(CancellationToken ct = default)
		{
			var http = _httpContextAccessor.HttpContext;
			if (http == null) return new LoginIdentityResult();

			// 先用管線還原的 User；必要時再顯式 Authenticate 一次
			var principal = http.User;
			if (principal?.Identities?.Any(i => i.IsAuthenticated) != true)
			{
				var schemes = await _schemeProvider.GetAllSchemesAsync();
				if (schemes.Any(s => string.Equals(s.Name, AdminCookieScheme, StringComparison.Ordinal)))
				{
					var auth = await http.AuthenticateAsync(AdminCookieScheme);
					if (auth?.Succeeded == true && auth.Principal != null)
						principal = auth.Principal;
				}
			}

			if (principal?.Identities?.Any(i => i.IsAuthenticated) != true)
				return new LoginIdentityResult(); // guest

			// 常見 claims
			string? nameId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
			string? mgrIdStr = principal.FindFirstValue("mgr:id");
			string? usrIdStr = principal.FindFirstValue("usr:id"); // 若前台也走同機制可用
			string? name = principal.Identity?.Name;
			string? email = principal.FindFirstValue(ClaimTypes.Email);

			bool isManagerFlag =
				principal.HasClaim(c => string.Equals(c.Type, "IsManager", StringComparison.OrdinalIgnoreCase)) ||
				!string.IsNullOrEmpty(mgrIdStr) ||
				principal.Claims.Any(c => c.Type.StartsWith("perm:", StringComparison.OrdinalIgnoreCase));

			int? mgrId = TryParseInt(mgrIdStr) ?? (isManagerFlag ? TryParseInt(nameId) : null);
			int? usrId = TryParseInt(usrIdStr) ?? (!isManagerFlag ? TryParseInt(nameId) : null);

			// 角色（Role + "role" 去重）
			var roles = principal.Claims
				.Where(c => c.Type == ClaimTypes.Role || string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase))
				.Select(c => (c.Value ?? "").Trim())
				.Where(v => !string.IsNullOrWhiteSpace(v))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();

			// perms：「出現即 true」
			var perms = principal.Claims
				.Where(c => c.Type.StartsWith("perm:", StringComparison.OrdinalIgnoreCase))
				.GroupBy(c => c.Type, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, _ => true, StringComparer.OrdinalIgnoreCase);

			// 票證屬性（可選）
			DateTimeOffset? expires = null;
			string? schemeUsed = null;
			var authResult = await http.AuthenticateAsync(AdminCookieScheme);
			if (authResult?.Succeeded == true)
			{
				expires = authResult.Properties?.ExpiresUtc;
				schemeUsed = AdminCookieScheme;
			}

			var all = principal.Claims.Select(c => (c.Type, c.Value ?? "")).ToArray();
			var kind = isManagerFlag ? "manager" : (usrId.HasValue ? "user" : "guest");

			return new LoginIdentityResult
			{
				UserId = usrId,
				ManagerId = mgrId,
				Kind = kind,
				DisplayName = name,
				Email = email,
				IsManager = isManagerFlag,
				Roles = roles,
				Perms = perms,
				AuthScheme = schemeUsed,
				ExpiresUtc = expires,
				AllClaims = all
			};
		}

		private static int? TryParseInt(string? s)
			=> int.TryParse(s, out var v) ? v : (int?)null;
	}
}
