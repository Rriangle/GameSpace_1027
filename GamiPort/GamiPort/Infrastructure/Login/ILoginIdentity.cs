// Infrastructure/Login/ILoginIdentity.cs
// 提供全站統一的「目前使用者」查詢入口（後端任何地方都注入這個服務來拿 UserId）。
// LoginSnapshot：回傳是否已登入、整數 UserId、來源字串（Claim/IdentityId/AccountMap/None…）等。

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Infrastructure.Login
{
	public interface ILoginIdentity
	{
		/// <summary>
		/// 取得目前使用者的身分快照（是否已登入、整數 UserId、名稱等）。
		/// </summary>
		Task<LoginSnapshot> GetAsync(CancellationToken ct = default);
	}

	/// <summary>
	/// 身分查詢結果資料模型。
	/// </summary>
	public sealed record LoginSnapshot(
		bool IsAuthenticated,                     // 是否已登入
		int? UserId,                              // 你的 Users.UserId（整數）
		string? Source,                           // 身分來源（"Claim" / "IdentityId" / "AccountMap" / "None"...）
		string? Name = null,                      // Identity.Name（顯示用）
		string? IdentityUserId = null,            // AspNetUsers.Id（例如 "U:10000001"）
		IReadOnlyDictionary<string, string>? Extra = null // 需要時可帶額外資訊
	);
}
