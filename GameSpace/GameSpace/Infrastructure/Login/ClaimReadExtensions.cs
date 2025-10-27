using System.Security.Claims;

namespace GameSpace.Infrastructure.Login
{
	internal static class ClaimReadExtensions
	{
		public static string? GetClaimValue(this ClaimsPrincipal principal, string claimType)
			=> principal?.FindFirst(claimType)?.Value;
	}
}
