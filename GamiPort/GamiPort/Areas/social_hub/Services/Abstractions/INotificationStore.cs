using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>
	/// 通知的「薄但安全」寫入層：只做基本驗證＋寫入；不通過就拒絕。
	/// </summary>
	public interface INotificationStore
	{
		Task<StoreResult> CreateAsync(StoreCommand cmd, CancellationToken ct = default);
	}

	/// <summary>
	/// 呼叫方直接提供參數；服務不做名稱對應，只做存在性與長度驗證。
	/// </summary>
	public sealed record StoreCommand(
		int SourceId,
		int ActionId,
		int? ToUserId = null,
		int? ToManagerId = null,
		int? GroupId = null,
		int? SenderUserId = null,
		int? SenderManagerId = null,
		string Title = "",
		string? Message = null
	);

	/// <summary>統一回傳格式</summary>
	public sealed record StoreResult(
		bool Succeeded,
		int? NotificationId = null,
		NotificationError Error = NotificationError.None,
		string? Reason = null
	);

	public enum NotificationError
	{
		None = 0,
		SourceNotFound,
		ActionNotFound,
		GroupNotFound,
		InvalidRecipient,     // ToUserId 與 ToManagerId 皆為 null
		UserNotFound,
		ManagerNotFound,
		SenderUserNotFound,
		SenderManagerNotFound,
		TitleTooLong,         // 通知 Title 最長 100
		MessageTooLong,       // 通知 Message 最長 255
		DbError
	}
}
