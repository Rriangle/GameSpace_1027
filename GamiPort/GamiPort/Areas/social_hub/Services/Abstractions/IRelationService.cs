using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>
	/// 交友動作命令。
	/// </summary>
	public sealed record RelationCommand(
		int ActorUserId,          // 操作者
		int TargetUserId,         // 目標對象
		string ActionCode,        // friend_request / accept / reject / cancel_request / block / unblock / unfriend / set_nickname
		string? Nickname = null   // 僅 set_nickname 使用
	);

	/// <summary>
	/// 交友動作結果（同時回新狀態 ID + Code，前端可顯示，後端可用 ID 判斷）。
	/// </summary>
	public sealed record RelationResult(
		bool Succeeded,           // 是否成功（非驗證/DB 錯誤）
		bool NoOp,                // 本次是否沒有造成狀態變更
		int? RelationId,          // 關係主鍵
		string? NewStatusCode,    // 新狀態碼（"pending" / "accepted" / "blocked" / "removed" / "rejected" / "none"...）
		int? NewStatusId,         // 新狀態 ID（1=pending, 2=accepted, 3=blocked, 4=removed, 5=rejected, 6=none）
		string? Reason            // 失敗/拒絕時的理由（BadRequest 用）
	);

	public interface IRelationService
	{
		Task<RelationResult> ExecuteAsync(RelationCommand cmd, CancellationToken ct = default);
	}
}
