// =============================================================
// 控制器：交友 +（視條件）通知（同一交易；任一步失敗就回滾）
// 端點：POST /social_hub/combos/relation_and_notify
// 回傳：{ noOp, relationId, newStatusId, newStatusCode, notificationId?, reason? }
// =============================================================
using System;
using System.Threading;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Services.Abstractions; // IRelationService / INotificationStore / RelationCommand / StoreCommand / RelationResult
using GamiPort.Models;                                  // GameSpacedatabaseContext
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GamiPort.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[ApiController]
	[Route("social_hub/combos")]
	public sealed class CombosController : ControllerBase
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IRelationService _relations;
		private readonly INotificationStore _notify;

		public CombosController(GameSpacedatabaseContext db, IRelationService relations, INotificationStore notify)
		{
			_db = db;
			_relations = relations;
			_notify = notify;
		}

		// 與 RelationService 對齊（ID 判斷更穩）
		private const int STATUS_PENDING_ID = 1;
		private const int STATUS_ACCEPTED_ID = 2;

		public sealed record RelationAndNotifyRequest(
			RelationCommand relation,
			StoreCommand? notification,
			string notifyIf = "on-change" // "always" | "on-change" | "on-accepted" | "on-pending" | "never"
		);

		[AllowAnonymous] // 測試期免登入；上線可改 [Authorize]
		[HttpPost("relation_and_notify")]
		[ValidateAntiForgeryToken] // 前端 fetch header 帶 RequestVerificationToken
		public async Task<IActionResult> RelationAndNotify([FromBody] RelationAndNotifyRequest input, CancellationToken ct)
		{
			IDbContextTransaction? tx = null;
			var started = false;

			try
			{
				if (_db.Database.CurrentTransaction is null)
				{
					tx = await _db.Database.BeginTransactionAsync(ct);
					started = true;
				}

				// (1) 交友
				var rr = await _relations.ExecuteAsync(input.relation, ct);
				if (!rr.Succeeded)
				{
					if (started) await tx!.RollbackAsync(ct);
					return BadRequest(new
					{
						reason = rr.Reason,
						noOp = rr.NoOp,
						relationId = rr.RelationId,
						newStatusId = rr.NewStatusId,
						newStatusCode = rr.NewStatusCode
					});
				}

				// (2) 視條件發通知
				if (ShouldNotify(input.notifyIf, rr))
				{
					if (input.notification is null)
					{
						if (started) await tx!.RollbackAsync(ct);
						return BadRequest(new
						{
							reason = "缺少通知參數。",
							noOp = rr.NoOp,
							relationId = rr.RelationId,
							newStatusId = rr.NewStatusId,
							newStatusCode = rr.NewStatusCode
						});
					}

					var nr = await _notify.CreateAsync(input.notification, ct);
					if (!nr.Succeeded)
					{
						if (started) await tx!.RollbackAsync(ct);
						return BadRequest(new
						{
							reason = nr.Reason,
							noOp = rr.NoOp,
							relationId = rr.RelationId,
							newStatusId = rr.NewStatusId,
							newStatusCode = rr.NewStatusCode
						});
					}

					if (started) await tx!.CommitAsync(ct);
					return Ok(new
					{
						noOp = rr.NoOp,
						relationId = rr.RelationId,
						newStatusId = rr.NewStatusId,
						newStatusCode = rr.NewStatusCode,
						notificationId = nr.NotificationId
					});
				}
				else
				{
					if (started) await tx!.CommitAsync(ct);
					return Ok(new
					{
						noOp = rr.NoOp,
						relationId = rr.RelationId,
						newStatusId = rr.NewStatusId,
						newStatusCode = rr.NewStatusCode
					});
				}
			}
			catch (DbUpdateException ex)
			{
				if (started && tx is not null) await tx.RollbackAsync(ct);
				return BadRequest(new { reason = ex.GetType().Name });
			}
			finally
			{
				if (tx is not null) await tx.DisposeAsync();
			}
		}

		// 用 ID 判斷 on-accepted/on-pending；其它維持 on-change 與 always/never
		private static bool ShouldNotify(string notifyIf, RelationResult rr)
		{
			var code = (notifyIf ?? "on-change").Trim().ToLowerInvariant();

			return code switch
			{
				"never" => false,
				"always" => true,
				"on-change" => !rr.NoOp,
				"on-accepted" => rr.NewStatusId == STATUS_ACCEPTED_ID && !rr.NoOp,
				"on-pending" => rr.NewStatusId == STATUS_PENDING_ID && !rr.NoOp,
				_ => !rr.NoOp
			};
		}
	}
}
