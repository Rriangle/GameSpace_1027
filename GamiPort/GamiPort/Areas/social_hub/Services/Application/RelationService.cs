using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Services.Abstractions; // RelationCommand / RelationResult / IRelationService
using GamiPort.Models;                                  // GameSpacedatabaseContext / Relation / RelationStatus / User
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// 交友服務（集中驗證 + 狀態轉換；通過才寫入 DB）
	///
	/// 設計原則：
	///  1) 「以 StatusId 為主」判斷與寫入（穩定且直觀）；對前端則回 DB 的 StatusCode（如：PENDING/ACCEPTED…）。
	///  2) 關係唯一化：以 (UserIdSmall, UserIdLarge) = (min, max) 正規化，並依據資料庫唯一鍵 UQ_Relation_UserPair。
	///  3) No-Op：沒有實際變更就回 NoOp=true，避免重複通知/寫入。
	///  4) RequestedBy 僅在 PENDING 有意義；離開 PENDING 一律清空。
	///  5) 競態防護：新增/更新時若撞唯一鍵或狀態被他人改動，重讀現況並回合理結果。
	///
	/// 狀態機（依你的資料表記錄）：
	///   NONE(6)：無關係（解除好友/解除封鎖後的最終）
	///   PENDING(1) ← friend_request（從 NONE/REJECTED/REMOVED）
	///   PENDING(1) → ACCEPTED(2) ：accept（僅受邀方）
	///   PENDING(1) → REJECTED(5) ：reject（僅受邀方）
	///   PENDING(1) → REMOVED(4)  ：cancel_request（僅邀請方；歷史註記「已取消邀請」）
	///   *         → BLOCKED(3)   ：block（任一狀態可封鎖）
	///   BLOCKED(3) → NONE(6)     ：unblock（解除封鎖）
	///   ACCEPTED(2) → NONE(6)    ：unfriend（解除好友）
	///   ACCEPTED(2) → set_nickname（不改變狀態；僅更新暱稱）
	///
	/// 安全提醒：
	///   - 請於 Controller/Hub 以登入身分覆蓋 ActorUserId，勿信任前端送來的 ActorUserId。
	/// </summary>
	public sealed class RelationService : IRelationService
	{
		private readonly GameSpacedatabaseContext _db;
		public RelationService(GameSpacedatabaseContext db) => _db = db;

		// ===== 狀態 Id（請與資料庫 Relation_Status 對應）=============================
		private const int STATUS_PENDING = 1;
		private const int STATUS_ACCEPTED = 2;
		private const int STATUS_BLOCKED = 3;
		private const int STATUS_REMOVED = 4;
		private const int STATUS_REJECTED = 5;
		private const int STATUS_NONE = 6;

	

		// 暱稱長度限制（與 OnModelCreating HasMaxLength(10) 對應）
		private const int NICKNAME_MAXLEN = 10;

		// ===== 一次性快取：StatusId -> StatusCode（用 DB 的代碼回傳給前端）=============
		private static volatile bool _statusMapLoaded = false;
		private static readonly object _statusMapLock = new();
		private static System.Collections.Generic.Dictionary<int, string>? _statusCodeMap;

		/// <summary>第一次呼叫時載入 Relation_Status 的代碼映射。</summary>
		private async Task EnsureStatusMapAsync(CancellationToken ct)
		{
			if (_statusMapLoaded && _statusCodeMap is not null) return;

			lock (_statusMapLock)
			{
				if (_statusMapLoaded && _statusCodeMap is not null) return;
			}

			var list = await _db.Set<RelationStatus>()
				.AsNoTracking()
				.Select(s => new { s.StatusId, s.StatusCode })
				.ToListAsync(ct);

			var dict = list.ToDictionary(s => s.StatusId, s => s.StatusCode);

			lock (_statusMapLock)
			{
				_statusCodeMap = dict;
				_statusMapLoaded = true;
			}
		}

		/// <summary>取得 DB 定義的狀態代碼；若未載入或查無，退回 Id.ToString()（不致拋例外）。</summary>
		private static string CodeOf(int statusId)
			=> (_statusCodeMap is not null && _statusCodeMap.TryGetValue(statusId, out var code))
				? code
				: statusId.ToString();

		// ============================================================================
		// 入口：集中驗證 + 路由到各動作
		// ============================================================================
		public async Task<RelationResult> ExecuteAsync(RelationCommand cmd, CancellationToken ct = default)
		{
			// 先確保代碼映射已載入（每個請求只會首次載入一次）
			await EnsureStatusMapAsync(ct);

			// ---- 0) 基本輸入驗證（Actor 請在 Controller/Hub 以登入者覆蓋） ----
			var actor = cmd.ActorUserId;
			var target = cmd.TargetUserId;

			if (actor <= 0 || target <= 0) return Fail("UserId 不合法。");
			if (actor == target) return Fail("不可對自己操作。");
			if (string.IsNullOrWhiteSpace(cmd.ActionCode)) return Fail("ActionCode 必填。");

			// 使用者存在性（一次往返檢查兩人；也可省略讓 FK 代勞）
			var count = await _db.Users.AsNoTracking()
				.CountAsync(u => u.UserId == actor || u.UserId == target, ct);
			if (count != 2) return Fail("使用者不存在。");

			// ---- 1) 對稱 pair 正規化 ----
			var small = Math.Min(actor, target);
			var large = Math.Max(actor, target);

			// 當前關係（可能為 null）
			var row = await _db.Relations
				.SingleOrDefaultAsync(r => r.UserIdSmall == small && r.UserIdLarge == large, ct);

			// ---- 2) 動作路由 ----
			var action = cmd.ActionCode.Trim().ToLowerInvariant();
			return action switch
			{
				"friend_request" or "request" => await DoFriendRequest(row, small, large, actor, ct),
				"accept" => await DoAccept(row, actor, ct),
				"reject" => await DoReject(row, actor, ct),
				"cancel_request" or "cancel" => await DoCancel(row, actor, ct),
				"block" => await DoBlock(row, small, large, actor, ct),
				"unblock" => await DoUnblock(row, actor, ct),  // BLOCKED → NONE
				"unfriend" or "remove_friend" or "delete_friend"
													   => await DoUnfriend(row, actor, ct), // ACCEPTED → NONE
				"set_nickname" => await DoSetNickname(row, actor, cmd.Nickname, ct),
				_ => Fail("未知的 ActionCode。")
			};
		}

		// ============================================================================
		// 各動作實作（微優化：統一 UTC 時戳；所有非 PENDING 都清空 RequestedBy）
		// ============================================================================

		private async Task<RelationResult> DoFriendRequest(
	Relation? row, int small, int large, int actor, CancellationToken ct)
		{
			// A) 關係不存在 → 嘗試建立 PENDING（唯一鍵保護）
			if (row is null)
			{
				row = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = STATUS_PENDING,
					RequestedBy = actor,
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(row);

				try
				{
					await _db.SaveChangesAsync(ct);
					return Ok(row.RelationId, STATUS_PENDING);
				}
				catch (DbUpdateException)
				{
					// 可能是兩邊同時發請求撞唯一鍵 → 重讀現況（NoOp/Changed）
					row = await _db.Relations.SingleAsync(
						r => r.UserIdSmall == small && r.UserIdLarge == large, ct);
					return NoOp(row.RelationId, row.StatusId, "關係已由其他操作建立/更新。");
				}
			}

			// B) 已存在 → 依狀態處理
			switch (row.StatusId)
			{
				case STATUS_BLOCKED:
					return Fail("目前為封鎖狀態，無法送出邀請。");

				case STATUS_ACCEPTED:
					return NoOp(row.RelationId, STATUS_ACCEPTED, "已是好友。");

				case STATUS_PENDING:
					// 我自己送過邀請 → 不再重複
					if (row.RequestedBy == actor)
						return NoOp(row.RelationId, STATUS_PENDING, "邀請已送出。");

					// 對方已邀我 → 維持 PENDING，提示前端顯示「去按接受」
					return NoOp(row.RelationId, STATUS_PENDING, "對方已發出邀請，請改用 accept。");

				case STATUS_REJECTED:
				case STATUS_REMOVED:
				case STATUS_NONE:
					row.StatusId = STATUS_PENDING;
					row.RequestedBy = actor;
					row.UpdatedAt = DateTime.UtcNow;

					try
					{
						await _db.SaveChangesAsync(ct);
						return Ok(row.RelationId, STATUS_PENDING);
					}
					catch (DbUpdateException)
					{
						// 極少數競態：被其他操作改掉 → 重讀現況回應
						var fresh = await _db.Relations.SingleAsync(
							r => r.UserIdSmall == row.UserIdSmall && r.UserIdLarge == row.UserIdLarge, ct);
						return NoOp(fresh.RelationId, fresh.StatusId, "關係狀態已被其他操作變更。");
					}
			}

			return Fail("目前狀態不允許送出邀請。");
		}


		private async Task<RelationResult> DoAccept(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_PENDING) return Fail("關係狀態已變更，請重新整理後再試。");
			if (row.RequestedBy == actor) return Fail("邀請方不得自行接受。");

			row.StatusId = STATUS_ACCEPTED;
			row.RequestedBy = null;
			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_ACCEPTED);
		}

		private async Task<RelationResult> DoReject(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_PENDING) return Fail("關係狀態已變更，請重新整理後再試。");
			if (row.RequestedBy == actor) return Fail("邀請方不得拒絕。");

			row.StatusId = STATUS_REJECTED;
			row.RequestedBy = null;
			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_REJECTED);
		}

		private async Task<RelationResult> DoCancel(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_PENDING) return Fail("關係狀態已變更，請重新整理後再試。");
			if (row.RequestedBy != actor) return Fail("僅邀請方可取消邀請。");

			row.StatusId = STATUS_REMOVED;   // 僅代表「取消邀請」的歷史註記
			row.RequestedBy = null;
			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_REMOVED);
		}

		private async Task<RelationResult> DoBlock(Relation? row, int small, int large, int actor, CancellationToken ct)
		{
			if (row is null)
			{
				row = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = STATUS_BLOCKED,
					RequestedBy = null,
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(row);

				try
				{
					await _db.SaveChangesAsync(ct);
					return Ok(row.RelationId, STATUS_BLOCKED);
				}
				catch (DbUpdateException)
				{
					// 撞唯一鍵 → 讀現況
					row = await _db.Relations.SingleAsync(r => r.UserIdSmall == small && r.UserIdLarge == large, ct);
					if (row.StatusId == STATUS_BLOCKED)
						return NoOp(row.RelationId, STATUS_BLOCKED, "已在封鎖狀態。");

					// 改為封鎖
					row.StatusId = STATUS_BLOCKED;
					row.RequestedBy = null;
					row.UpdatedAt = DateTime.UtcNow;
					await _db.SaveChangesAsync(ct);
					return Ok(row.RelationId, STATUS_BLOCKED);
				}
			}

			if (row.StatusId == STATUS_BLOCKED)
				return NoOp(row.RelationId, STATUS_BLOCKED, "已在封鎖狀態。");

			row.StatusId = STATUS_BLOCKED;
			row.RequestedBy = null;
			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_BLOCKED);
		}

		private async Task<RelationResult> DoUnblock(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_BLOCKED) return Fail("不是封鎖狀態。");

			row.StatusId = STATUS_NONE;  // 解除封鎖 → 無關係
			row.RequestedBy = null;
			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_NONE);
		}

		private async Task<RelationResult> DoUnfriend(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_ACCEPTED) return Fail("僅在好友狀態可解除好友。");

			row.StatusId = STATUS_NONE;  // 解除好友 → 無關係
			row.RequestedBy = null;
			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_NONE);
		}

		private async Task<RelationResult> DoSetNickname(Relation? row, int actor, string? nickname, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_ACCEPTED) return Fail("非好友狀態不可設定暱稱。");

			var newName = string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim();
			if (newName is not null && newName.Length > NICKNAME_MAXLEN)
				return Fail($"暱稱長度上限 {NICKNAME_MAXLEN}。");

			if (string.Equals(row.FriendNickname, newName, StringComparison.Ordinal))
				return NoOp(row.RelationId, STATUS_ACCEPTED, "暱稱未變更。");

			row.FriendNickname = newName;
			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_ACCEPTED);
		}

		// ============================================================================
		// 統一回傳封裝（newStatusCode 一律用 DB 的 StatusCode）
		// ============================================================================

		private static RelationResult Ok(int relationId, int newStatusId, string? reason = null)
			=> new(true, false, relationId, CodeOf(newStatusId), newStatusId, reason);

		private static RelationResult NoOp(int relationId, int newStatusId, string? reason = null)
			=> new(true, true, relationId, CodeOf(newStatusId), newStatusId, reason);

		private static RelationResult Fail(string reason)
			=> new(false, false, null, null, null, reason);
	}
}
