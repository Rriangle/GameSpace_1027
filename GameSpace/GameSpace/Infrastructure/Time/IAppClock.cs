using System;

namespace GameSpace.Infrastructure.Time
{
	/// <summary>
	/// 應用程式層時間服務：
	/// - 寫入：一律使用 UTC（UtcNow / ToUtc）
	/// - 顯示：轉成 App 時區（預設台灣：Asia/Taipei）
	/// 同時支援 DateTime 及 DateTimeOffset。
	/// </summary>
	public interface IAppClock
	{
		// 時鐘
		DateTime UtcNow { get; }
		DateTimeOffset UtcNowOffset { get; }

		// App 時區
		TimeZoneInfo AppTimeZone { get; }

		// 轉換：UTC → App
		DateTime ToAppTime(DateTime utc);
		DateTimeOffset ToAppTime(DateTimeOffset utc);

		// 轉換：App → UTC
		DateTime ToUtc(DateTime local);
		DateTimeOffset ToUtc(DateTimeOffset local);

		// 格式化（顯示用）
		string FormatApp(DateTime? utc, string format = "yyyy-MM-dd HH:mm");
		string FormatApp(DateTimeOffset? utc, string format = "yyyy-MM-dd HH:mm");
	}
}
