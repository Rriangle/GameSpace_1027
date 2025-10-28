using System;
using System.Threading;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage; // ★ 新增這行

namespace GamiPort.Areas.social_hub.Services.Application
{
	public sealed class NotificationStore : INotificationStore
	{
		private readonly GameSpacedatabaseContext _db;
		private const int TitleMaxLen = 100;
		private const int MessageMaxLen = 255;

		public NotificationStore(GameSpacedatabaseContext db) => _db = db;

		public async Task<StoreResult> CreateAsync(StoreCommand cmd, CancellationToken ct = default)
		{
			// 0) 文字長度檢查
			var title = (cmd.Title ?? string.Empty).Trim();
			var message = cmd.Message?.Trim();
			if (title.Length > TitleMaxLen) return Fail(NotificationError.TitleTooLong, $"Title 長度上限 {TitleMaxLen}。");
			if (message is { Length: > MessageMaxLen }) return Fail(NotificationError.MessageTooLong, $"Message 長度上限 {MessageMaxLen}。");

			// 1) 基本存在性
			var hasSource = await _db.NotificationSources.AsNoTracking().AnyAsync(s => s.SourceId == cmd.SourceId, ct);
			if (!hasSource) return Fail(NotificationError.SourceNotFound, $"SourceId={cmd.SourceId} 不存在。");

			var hasAction = await _db.NotificationActions.AsNoTracking().AnyAsync(a => a.ActionId == cmd.ActionId, ct);
			if (!hasAction) return Fail(NotificationError.ActionNotFound, $"ActionId={cmd.ActionId} 不存在。");

			if (cmd.GroupId is int gid)
			{
				var hasGroup = await _db.Groups.AsNoTracking().AnyAsync(g => g.GroupId == gid, ct);
				if (!hasGroup) return Fail(NotificationError.GroupNotFound, $"GroupId={gid} 不存在。");
			}

			// 2) 收件人檢查
			if (cmd.ToUserId is null && cmd.ToManagerId is null)
				return Fail(NotificationError.InvalidRecipient, "至少需要一個收件人（使用者或管理員）。");

			if (cmd.ToUserId is int toUid)
			{
				var userExists = await _db.Users.AsNoTracking().AnyAsync(u => u.UserId == toUid, ct);
				if (!userExists) return Fail(NotificationError.UserNotFound, $"UserId={toUid} 不存在。");
			}
			if (cmd.ToManagerId is int toMid)
			{
				var mgrExists = await _db.ManagerData.AsNoTracking().AnyAsync(m => m.ManagerId == toMid, ct);
				if (!mgrExists) return Fail(NotificationError.ManagerNotFound, $"ManagerId={toMid} 不存在。");
			}

			// 3) 寄件人（可選）
			if (cmd.SenderUserId is int suid)
			{
				var senderUserOk = await _db.Users.AsNoTracking().AnyAsync(u => u.UserId == suid, ct);
				if (!senderUserOk) return Fail(NotificationError.SenderUserNotFound, $"SenderUserId={suid} 不存在。");
			}
			if (cmd.SenderManagerId is int smid)
			{
				var senderMgrOk = await _db.ManagerData.AsNoTracking().AnyAsync(m => m.ManagerId == smid, ct);
				if (!senderMgrOk) return Fail(NotificationError.SenderManagerNotFound, $"SenderManagerId={smid} 不存在。");
			}

			// 4) 交易控制（★關鍵修正：支援外部交易）
			var hasAmbient = _db.Database.CurrentTransaction != null; // 外面是否已經有交易
			IDbContextTransaction? tx = null;

			try
			{
				if (!hasAmbient)
					tx = await _db.Database.BeginTransactionAsync(ct); // 只有沒有外部交易時才自己開

				// 4.1 寫入主檔
				var n = new Notification
				{
					SourceId = cmd.SourceId,
					ActionId = cmd.ActionId,
					GroupId = cmd.GroupId,
					SenderUserId = cmd.SenderUserId,
					SenderManagerId = cmd.SenderManagerId,
					Title = title,
					Message = message,
					CreatedAt = DateTime.UtcNow
				};
				_db.Notifications.Add(n);
				await _db.SaveChangesAsync(ct); // 取得 NotificationId

				// 4.2 寫入收件人（至少一個）
				if (cmd.ToUserId is int toUserId)
				{
					_db.NotificationRecipients.Add(new NotificationRecipient
					{
						NotificationId = n.NotificationId,
						// ⚠ 這裡請用你實體的實際欄位名：
						// 如果你的欄位叫 UserId / ManagerId 就用下面兩行；
						// 如果是 ToUserId / ToManagerId，請改成相對應欄位。
						UserId = toUserId,
						ManagerId = null
						// ToUserId = toUserId
					});
				}
				if (cmd.ToManagerId is int toManagerId)
				{
					_db.NotificationRecipients.Add(new NotificationRecipient
					{
						NotificationId = n.NotificationId,
						UserId = null,
						ManagerId = toManagerId
						// ToManagerId = toManagerId
					});
				}

				await _db.SaveChangesAsync(ct);

				if (!hasAmbient)
					await tx!.CommitAsync(ct); // 只有自己開的交易才在這裡 Commit

				return new StoreResult(true, n.NotificationId);
			}
			catch (DbUpdateException ex)
			{
				if (tx is not null)
					await tx.RollbackAsync(ct); // 只有自己開的交易才在這裡 Rollback

				return Fail(NotificationError.DbError, ex.GetType().Name);
			}
			finally
			{
				if (tx is not null)
					await tx.DisposeAsync(); // 只有自己開的才需要 Dispose
			}
		}

		private static StoreResult Fail(NotificationError e, string reason)
			=> new(false, null, e, reason);
	}
}
