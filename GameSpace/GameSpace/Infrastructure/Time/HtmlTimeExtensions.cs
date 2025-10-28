// Path: /Infrastructure/Time/HtmlTimeExtensions.cs
using System;
using System.Net;
using GameSpace.Infrastructure.Time;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
	/// <summary>
	/// Razor 顯示：把「DB 的 UTC」轉成 App 時區（預設 Asia/Taipei）再格式化。
	/// - 支援 DateTime / DateTimeOffset 與 Nullable 多載
	/// - Nullable 版提供 empty 參數（預設 "—"）
	/// - DateTime.Kind 若為 Unspecified，一律當作 UTC
	/// </summary>
	public static class HtmlTimeExtensions
	{
		private static IAppClock Clock(this IHtmlHelper html)
			=> html.ViewContext.HttpContext.RequestServices.GetRequiredService<IAppClock>();

		private static IHtmlContent Txt(string s) => new HtmlString(WebUtility.HtmlEncode(s));

		// ===== DateTime（非 nullable）=====
		/// <summary>@Html.AppTime(DateTime utc, "yyyy/MM/dd HH:mm")</summary>
		public static IHtmlContent AppTime(this IHtmlHelper html,
										   DateTime utc,
										   string format = "yyyy/MM/dd HH:mm")
		{
			// 將 Unspecified 視為 UTC；其它一律轉成 UTC
			if (utc.Kind == DateTimeKind.Unspecified)
				utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
			else
				utc = utc.ToUniversalTime();

			var local = html.Clock().ToAppTime(utc);
			return Txt(local.ToString(format));
		}

		// ===== DateTime?（nullable，含 empty）=====
		/// <summary>@Html.AppTime(DateTime? utc, "yyyy/MM/dd HH:mm", empty:"—")</summary>
		public static IHtmlContent AppTime(this IHtmlHelper html,
										   DateTime? utc,
										   string format = "yyyy/MM/dd HH:mm",
										   string? empty = "—")
		{
			if (!utc.HasValue) return Txt(empty ?? "—");
			return html.AppTime(utc.Value, format);
		}

		// ===== DateTimeOffset（非 nullable）=====
		/// <summary>@Html.AppTime(DateTimeOffset utc, "yyyy/MM/dd HH:mm")</summary>
		public static IHtmlContent AppTime(this IHtmlHelper html,
										   DateTimeOffset utc,
										   string format = "yyyy/MM/dd HH:mm")
		{
			var local = html.Clock().ToAppTime(utc);
			return Txt(local.ToString(format));
		}

		// ===== DateTimeOffset?（nullable，含 empty）=====
		/// <summary>@Html.AppTime(DateTimeOffset? utc, "yyyy/MM/dd HH:mm", empty:"—")</summary>
		public static IHtmlContent AppTime(this IHtmlHelper html,
										   DateTimeOffset? utc,
										   string format = "yyyy/MM/dd HH:mm",
										   string? empty = "—")
		{
			if (!utc.HasValue) return Txt(empty ?? "—");
			return html.AppTime(utc.Value, format);
		}
	}
}
