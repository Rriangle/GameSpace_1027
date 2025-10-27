using System.Collections.Generic;
using System.Threading.Tasks;
using GameSpace.Models;

namespace GameSpace.Areas.social_hub.Services
{
	public interface INotificationService
	{
		/// <summary>
		/// 新簽章：同時支援「使用者與管理員收件人」。
		/// 回傳：實際寫入的收件總數（有效使用者＋有效管理員）。
		/// </summary>
		Task<int> CreateAsync(
			Notification notification,
			IEnumerable<int> userIds,
			IEnumerable<int> managerIds,
			int? senderUserId,
			int? senderManagerId);

		/// <summary>
		/// 舊簽章（僅使用者收件）— 仍保留相容，內部委派到新簽章。
		/// </summary>
		Task<int> CreateAsync(
			Notification notification,
			IEnumerable<int> userIds,
			int? senderUserId,
			int? senderManagerId);
	}
}
