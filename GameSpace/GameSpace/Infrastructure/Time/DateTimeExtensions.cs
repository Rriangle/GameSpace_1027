using System;

namespace GameSpace.Infrastructure.Time
{
	public static class DateTimeExtensions
	{
		public static DateTime EnsureUtc(this DateTime dt) =>
			dt.Kind switch
			{
				DateTimeKind.Utc => dt,
				DateTimeKind.Local => dt.ToUniversalTime(),
				_ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
			};

		public static DateTime? EnsureUtc(this DateTime? dt) =>
			dt.HasValue ? dt.Value.EnsureUtc() : (DateTime?)null;

		public static DateTime ToTaipei(this DateTime utc) =>
			TimeZoneInfo.ConvertTimeFromUtc(utc.EnsureUtc(), TimeZones.Taipei);

		public static DateTime? ToTaipei(this DateTime? utc) =>
			utc.HasValue ? utc.Value.ToTaipei() : (DateTime?)null;
	}
}
