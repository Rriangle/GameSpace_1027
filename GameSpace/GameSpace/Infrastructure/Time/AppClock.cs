using System;

namespace GameSpace.Infrastructure.Time
{
	/// <summary>
	/// 時間轉換實作：
	/// - DateTime：未指定 Kind 視為 UTC；Local 先轉 UTC 再換時區
	/// - DateTimeOffset：保留 offset，透過 TimeZoneInfo 轉到目標時區
	/// </summary>
	public sealed class AppClock : IAppClock
	{
		public AppClock(TimeZoneInfo tz) => AppTimeZone = tz ?? TimeZoneInfo.Utc;

		public TimeZoneInfo AppTimeZone { get; }

		public DateTime UtcNow => DateTime.UtcNow;
		public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;

		// UTC → App
		public DateTime ToAppTime(DateTime utc)
		{
			var u = utc.Kind switch
			{
				DateTimeKind.Utc => utc,
				DateTimeKind.Local => utc.ToUniversalTime(),
				_ => DateTime.SpecifyKind(utc, DateTimeKind.Utc)
			};
			return TimeZoneInfo.ConvertTimeFromUtc(u, AppTimeZone);
		}

		public DateTimeOffset ToAppTime(DateTimeOffset utc)
			=> TimeZoneInfo.ConvertTime(utc, AppTimeZone);

		// App → UTC
		public DateTime ToUtc(DateTime local)
		{
			// 未指定 Kind 的 local 視為 App 時區時間
			var assumed = local.Kind switch
			{
				DateTimeKind.Utc => local,
				DateTimeKind.Local => local,
				_ => DateTime.SpecifyKind(local, DateTimeKind.Unspecified)
			};
			return TimeZoneInfo.ConvertTimeToUtc(assumed, AppTimeZone);
		}

		public DateTimeOffset ToUtc(DateTimeOffset local)
		{
			// 將傳入的 DateTimeOffset 視為「App 時區的鐘面時間」
			// 忽略它自帶的 offset（因為我們要以 AppTimeZone 解讀）
			var localClock = DateTime.SpecifyKind(local.DateTime, DateTimeKind.Unspecified);

			// 以 AppTimeZone 為來源時區，轉成 UTC（回傳 DateTime）
			var utc = TimeZoneInfo.ConvertTimeToUtc(localClock, AppTimeZone);

			// 包成 DateTimeOffset（UTC → offset = 0）
			return new DateTimeOffset(utc, TimeSpan.Zero);
		}
		// 格式化（顯示）
		public string FormatApp(DateTime? utc, string format = "yyyy-MM-dd HH:mm")
		{
			if (utc is null) return string.Empty;
			return ToAppTime(utc.Value).ToString(format);
		}

		public string FormatApp(DateTimeOffset? utc, string format = "yyyy-MM-dd HH:mm")
		{
			if (utc is null) return string.Empty;
			return ToAppTime(utc.Value).ToString(format);
		}
	}
}
