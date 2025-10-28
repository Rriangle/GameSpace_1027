// Infrastructure/Login/AppClaimsFactory.cs
// 用途：每次建立/刷新 ClaimsPrincipal（登入、RefreshSignIn、安全戳驗證）時，
//       自動把你的 Users.UserId 補進 Claim("AppUserId")，
//       來源：先解析 IdentityUser.Id = "U:<id>"；否則以 UserName 對應 Users 表。

using System.Security.Claims;
using GamiPort.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GamiPort.Infrastructure.Login
{
	public sealed class AppClaimsFactory : UserClaimsPrincipalFactory<IdentityUser>
	{
		private readonly GameSpacedatabaseContext _db;

		public AppClaimsFactory(
			UserManager<IdentityUser> userManager,
			IOptions<IdentityOptions> optionsAccessor,
			GameSpacedatabaseContext db)
			: base(userManager, optionsAccessor) => _db = db;

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
		{
			var id = await base.GenerateClaimsAsync(user);

			// A) 你的 LoginController 讓 IdentityUser.Id = "U:<UserId>" → 直接解析
			if (!string.IsNullOrEmpty(user.Id) && user.Id.StartsWith("U:")
				&& int.TryParse(user.Id.AsSpan(2), out var uid) && uid > 0)
			{
				id.AddClaim(new Claim("AppUserId", uid.ToString()));
				return id;
			}

			// B) 否則以 UserName 對應你的 Users 表（UserAccount / UserName 任一）
			var name = user.UserName ?? string.Empty;
			if (!string.IsNullOrWhiteSpace(name))
			{
				var map = await _db.Users.AsNoTracking()
					.Where(u => u.UserAccount == name || u.UserName == name)
					.Select(u => (int?)u.UserId)
					.FirstOrDefaultAsync();

				if (map is > 0)
					id.AddClaim(new Claim("AppUserId", map.Value.ToString()));
			}

			return id;
		}
	}
}
