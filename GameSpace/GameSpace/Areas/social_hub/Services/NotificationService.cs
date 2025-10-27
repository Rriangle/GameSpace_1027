using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Services
{
	public class NotificationService : INotificationService
	{
		private readonly GameSpacedatabaseContext _db;

		public NotificationService(GameSpacedatabaseContext db)
		{
			_db = db;
		}

		/// <summary>
		/// 新簽章：同時支援使用者與管理員收件人。
		/// 回傳：實際寫入的收件總數。
		/// </summary>
		public async Task<int> CreateAsync(
			Notification notification,
			IEnumerable<int> userIds,
			IEnumerable<int> managerIds,
			int? senderUserId,
			int? senderManagerId)
		{
			if (notification == null) throw new ArgumentNullException(nameof(notification));

			// 正規化內容長度（避免超過欄位長度）
			var title = (notification.Title ?? string.Empty).Trim();
			var message = (notification.Message ?? string.Empty).Trim();
			if (title.Length > 255) title = title[..255];
			if (message.Length > 255) message = message[..255];

			// Sender 二擇一（同時有值時優先 Manager）
			int? sMgr = senderManagerId;
			int? sUser = senderUserId;
			if (sMgr.HasValue && sUser.HasValue) sUser = null;

			// 建立主檔
			var n = new Notification
			{
				Title = title,
				Message = message,
				SourceId = notification.SourceId,
				ActionId = notification.ActionId,
				GroupId = notification.GroupId,
				SenderUserId = sUser,
				SenderManagerId = sMgr,
				CreatedAt = DateTime.UtcNow
			};
			_db.Notifications.Add(n);
			await _db.SaveChangesAsync(); // 取得 NotificationId

			// 正規化收件人
			var distinctUserIds = (userIds ?? Enumerable.Empty<int>()).Where(id => id > 0).Distinct().ToList();
			var distinctManagerIds = (managerIds ?? Enumerable.Empty<int>()).Where(id => id > 0).Distinct().ToList();

			if (distinctUserIds.Count > 0)
			{
				distinctUserIds = await _db.Users
					.AsNoTracking()
					.Where(u => distinctUserIds.Contains(u.UserId))
					.Select(u => u.UserId)
					.ToListAsync();
			}

			if (distinctManagerIds.Count > 0)
			{
				distinctManagerIds = await _db.ManagerData
					.AsNoTracking()
					.Where(m => distinctManagerIds.Contains(m.ManagerId))
					.Select(m => m.ManagerId)
					.ToListAsync();
			}

			if (distinctUserIds.Count == 0 && distinctManagerIds.Count == 0)
				return 0;

			// 寫收件明細（你的模型沒有 DeliveredAt，就不要寫）
			var recs = new List<NotificationRecipient>(distinctUserIds.Count + distinctManagerIds.Count);
			foreach (var uid in distinctUserIds)
			{
				recs.Add(new NotificationRecipient
				{
					NotificationId = n.NotificationId,
					UserId = uid,
					ManagerId = null,
					ReadAt = null
				});
			}
			foreach (var mid in distinctManagerIds)
			{
				recs.Add(new NotificationRecipient
				{
					NotificationId = n.NotificationId,
					UserId = null,
					ManagerId = mid,
					ReadAt = null
				});
			}

			_db.NotificationRecipients.AddRange(recs);
			await _db.SaveChangesAsync();
			return recs.Count;
		}

		/// <summary>
		/// 舊簽章相容（只處理使用者收件）。委派到新簽章。
		/// </summary>
		public Task<int> CreateAsync(
			Notification notification,
			IEnumerable<int> userIds,
			int? senderUserId,
			int? senderManagerId)
			=> CreateAsync(notification, userIds, Enumerable.Empty<int>(), senderUserId, senderManagerId);
	}
}
