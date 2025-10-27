using System;
using System.Runtime.InteropServices;

namespace GameSpace.Infrastructure.Time
{
	/// <summary>跨平台時區解析：Linux/macOS 用 "Asia/Taipei"；Windows 用 "Taipei Standard Time"</summary>
	public static class TimeZones
	{
		private static TimeZoneInfo? _taipei;

		public static TimeZoneInfo Taipei
		{
			get
			{
				if (_taipei != null) return _taipei;

				// 非 Windows（Linux/macOS）
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					try { _taipei = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei"); return _taipei; } catch { }
				}

				// Windows
				try { _taipei = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"); return _taipei; } catch { }

				// 保底
				_taipei = TimeZoneInfo.Utc;
				return _taipei;
			}
		}
	}
}
