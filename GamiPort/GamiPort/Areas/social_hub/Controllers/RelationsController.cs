// =============================================================
// 交友 API：前端用 JSON 丟進 RelationCommand，服務驗證通過才寫入 DB
// 免登入可測（[AllowAnonymous]）；若之後要開權限，改回 [Authorize] 即可
// 回傳統一小駝峰鍵名：{ noOp, relationId, newStatusId, newStatusCode, reason }
// =============================================================
using GamiPort.Areas.social_hub.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[ApiController]
	[Route("social_hub/relations")]
	public sealed class RelationsController : ControllerBase
	{
		private readonly IRelationService _service;
		public RelationsController(IRelationService service) => _service = service;

		[AllowAnonymous]                   // 測試期免登入；上線可改 [Authorize]
		[HttpPost("exec")]
		[ValidateAntiForgeryToken]         // 若你不想帶 Token，移除此屬性即可
		public async Task<IActionResult> Exec([FromBody] RelationCommand input, CancellationToken ct)
		{
			var r = await _service.ExecuteAsync(input, ct);
			if (!r.Succeeded) return BadRequest(new { reason = r.Reason });

			// 成功或 No-Op 都回 200，讓前端依 noOp 決定 UI 提示
			return Ok(new
			{
				noOp = r.NoOp,
				relationId = r.RelationId,
				newStatusId = r.NewStatusId,       // ★ 新增
				newStatusCode = r.NewStatusCode,
				reason = r.Reason
			});
		}
	}
}
