using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Infrastructure.Security
{
	/// <summary>
	/// 全站統一取得「目前使用者」資訊的介面（不動他原本的服務）。
	/// </summary>
	public interface IAppCurrentUser
	{
		/// <summary>是否已通過身分驗證。</summary>
		bool IsAuthenticated { get; }

		/// <summary>
		/// 快速取得整數 UserId（只讀 Claims，不打 DB；拿不到則回 0）。
		/// 若要保險（含必要時 DB 對應），請呼叫 <see cref="GetUserIdAsync"/>.
		/// </summary>
		int UserId { get; }

		/// <summary>（可能為 null）暱稱：優先用 Claim("UserNickName")，次之 Identity.Name。</summary>
		string? NickName { get; }

		/// <summary>（可能為 null）Email：來自 ClaimTypes.Email。</summary>
		string? Email { get; }

		/// <summary>原始 ClaimsPrincipal。</summary>
		ClaimsPrincipal Principal { get; }

		/// <summary>
		/// 可靠取得整數 UserId：先讀 Claims，拿不到時才以 ILoginIdentity 做備援（同請求快取）。
		/// </summary>
		ValueTask<int> GetUserIdAsync(CancellationToken ct = default);
	}
}
