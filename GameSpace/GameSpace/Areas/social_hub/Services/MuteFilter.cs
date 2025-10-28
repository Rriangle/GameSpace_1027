// Areas/social_hub/Services/MuteFilter.cs
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Services
{
	public sealed class MuteFilter : IMuteFilter
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IMemoryCache _cache;
		private readonly MuteFilterOptions _opt;

		private const string CK_REGEX = "social_hub.mutes.regex.v2";
		private const string CK_ENTRIES = "social_hub.mutes.entries.v2";

		private Regex? _compiled;
		private Entry[] _entries = Array.Empty<Entry>();

		private sealed class Entry
		{
			public string Word { get; }
			public string? Replacement { get; }
			public string Pattern { get; }
			public Entry(string word, string? replacement, string pattern)
			{
				Word = word; Replacement = replacement; Pattern = pattern;
			}
		}

		public MuteFilter(GameSpacedatabaseContext db, IMemoryCache cache, IOptions<MuteFilterOptions> opt)
		{
			_db = db;
			_cache = cache;
			_opt = opt.Value;

			_compiled = _cache.Get<Regex>(CK_REGEX);
			_entries = _cache.Get<Entry[]>(CK_ENTRIES) ?? Array.Empty<Entry>();
		}

		public async Task RefreshAsync(CancellationToken ct = default)
		{
			// 取啟用詞庫，若同一詞出現多筆，以較新的為準
			var raws = await _db.Mutes.AsNoTracking()
				.Where(m => m.IsActive == true && m.Word != null && m.Word != "")
				.Select(m => new { m.MuteId, m.Word, m.Replacement })
				.ToListAsync(ct);

			var latest = raws
				.Where(x => !string.IsNullOrWhiteSpace(x.Word))
				.OrderByDescending(x => x.MuteId)
				.GroupBy(x => x.Word!.Trim(), StringComparer.OrdinalIgnoreCase)
				.Select(g => new { Word = g.Key, Replacement = g.First().Replacement })
				.ToList();

			_entries = BuildEntries(latest.Select(x => (x.Word, x.Replacement)), _opt);

			_compiled = BuildRegex(_entries);
			var ttl = TimeSpan.FromSeconds(Math.Max(5, _opt.CacheTtlSeconds));
			_cache.Set(CK_ENTRIES, _entries, ttl);
			if (_compiled != null) _cache.Set(CK_REGEX, _compiled, ttl);
		}

		public string Apply(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;

			var rx = _compiled ?? BuildFromCacheOrDb();
			if (rx == null) return input;

			return rx.Replace(input, MatchEvaluator);
		}

		public Task<string> ApplyAsync(string input, CancellationToken ct = default)
			=> Task.FromResult(Apply(input));

		// === 舊名稱相容 ===
		public string Filter(string input) => Apply(input);
		public Task<string> FilterAsync(string input, CancellationToken ct = default) => ApplyAsync(input, ct);

		// --- 內部 ---

		private Regex? BuildFromCacheOrDb()
		{
			if (_compiled != null) return _compiled;

			var cached = _cache.Get<Entry[]>(CK_ENTRIES);
			if (cached == null || cached.Length == 0)
			{
				// 同 Refresh，但同步、簡化版
				var raws = _db.Mutes.AsNoTracking()
					.Where(m => m.IsActive == true && m.Word != null && m.Word != "")
					.Select(m => new { m.MuteId, m.Word, m.Replacement })
					.ToList();

				var latest = raws
					.Where(x => !string.IsNullOrWhiteSpace(x.Word))
					.OrderByDescending(x => x.MuteId)
					.GroupBy(x => x.Word!.Trim(), StringComparer.OrdinalIgnoreCase)
					.Select(g => new { Word = g.Key, Replacement = g.First().Replacement })
					.ToList();

				_entries = BuildEntries(latest.Select(x => (x.Word, x.Replacement)), _opt);
			}
			else
			{
				_entries = cached;
			}

			_compiled = BuildRegex(_entries);
			var ttl = TimeSpan.FromSeconds(Math.Max(5, _opt.CacheTtlSeconds));
			if (_compiled != null) _cache.Set(CK_REGEX, _compiled, ttl);
			if (_entries.Length > 0) _cache.Set(CK_ENTRIES, _entries, ttl);
			return _compiled;
		}

		private static Entry[] BuildEntries(IEnumerable<(string Word, string? Replacement)> items, MuteFilterOptions opt)
		{
			// 去空白、去重（不分大小寫），長詞優先避免被短詞搶先匹配
			var list = items
				.Where(x => !string.IsNullOrWhiteSpace(x.Word))
				.Select(x => (Word: x.Word.Trim(), Replacement: (x.Replacement ?? "").Trim()))
				.GroupBy(x => x.Word, StringComparer.OrdinalIgnoreCase)
				.Select(g => g.First())
				.OrderByDescending(x => x.Word.Length)
				.ToList();

			var entries = new List<Entry>(list.Count);
			foreach (var it in list)
			{
				var pattern = BuildPattern(it.Word, opt);
				// 若未開啟「每詞自訂替代」，統一以空字串存放，後續會 fallback 到全域樣式
				var repl = opt.UsePerWordReplacement ? (string.IsNullOrEmpty(it.Replacement) ? null : it.Replacement) : null;
				entries.Add(new Entry(it.Word, repl, pattern));
			}
			return entries.ToArray();
		}

		private static string BuildPattern(string w, MuteFilterOptions opt)
		{
			if (!opt.FuzzyBetweenCjkChars || w.Length <= 1)
				return Regex.Escape(w);

			var sb = new StringBuilder();
			for (int i = 0; i < w.Length; i++)
			{
				sb.Append(Regex.Escape(w[i].ToString()));
				if (i < w.Length - 1)
					sb.Append(@"[\s\p{P}\p{Sk}\p{Mn}\u200B\u200C\u200D\u2060_\-]*");
			}
			return sb.ToString();
		}

		private static Regex? BuildRegex(Entry[] entries)
		{
			if (entries.Length == 0) return null;

			// 具名群組：(?<w0>...longer... )|(?<w1>... )|...
			var alts = new StringBuilder();
			alts.Append('(');
			for (int i = 0; i < entries.Length; i++)
			{
				if (i > 0) alts.Append('|');
				alts.Append("(?<w").Append(i).Append('>').Append(entries[i].Pattern).Append(')');
			}
			alts.Append(')');

			return new Regex(alts.ToString(),
				RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}

		private string MatchEvaluator(Match m)
		{
			// 找出是哪個詞命中
			int idx = -1;
			for (int i = 0; i < _entries.Length; i++)
			{
				if (m.Groups["w" + i].Success) { idx = i; break; }
			}

			// 優先使用每詞自訂 replacement
			if (idx >= 0)
			{
				var e = _entries[idx];
				if (!string.IsNullOrEmpty(e.Replacement))
					return e.Replacement!;
			}

			// 退回全域樣式
			if (_opt.MaskStyle == MaskStyle.FixedLabel)
				return _opt.FixedLabel;

			if (_opt.MaskExactLength)
			{
				var n = CountMeaningful(m.Value);
				if (n <= 0) n = m.Value.Length;
				return new string('＊', n);
			}
			return new string('＊', m.Value.Length);
		}

		private static int CountMeaningful(string s)
		{
			int count = 0;
			foreach (var ch in s)
			{
				var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
				if (char.IsWhiteSpace(ch)) continue;
				if (char.IsPunctuation(ch)) continue;
				if (cat == UnicodeCategory.Format) continue;         // 零寬等
				if (cat == UnicodeCategory.NonSpacingMark) continue; // 結合音標
				count++;
			}
			return count;
		}
	}
}
