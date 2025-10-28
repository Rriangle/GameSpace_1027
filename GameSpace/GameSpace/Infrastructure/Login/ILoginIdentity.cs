using System.Threading;
using System.Threading.Tasks;

namespace GameSpace.Infrastructure.Login
{
	/// <summary>取得目前請求的登入身分。</summary>
	public interface ILoginIdentity
	{
		Task<LoginIdentityResult> GetAsync(CancellationToken ct = default);
	}
}
