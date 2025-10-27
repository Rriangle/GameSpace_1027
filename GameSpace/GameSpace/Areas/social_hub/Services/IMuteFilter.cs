// Areas/social_hub/Services/IMuteFilter.cs
using System.Threading;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Services
{
	public interface IMuteFilter
	{
		/// 重新載入資料庫詞庫（立即清快取）
		Task RefreshAsync(CancellationToken ct = default);

		/// 顯示端遮罩（同步）
		string Apply(string input);

		/// 顯示端遮罩（非同步便捷）
		Task<string> ApplyAsync(string input, CancellationToken ct = default);

		/// === 舊程式相容用別名 ===
		string Filter(string input);
		Task<string> FilterAsync(string input, CancellationToken ct = default);
	}

	public sealed class MuteFilterOptions
	{
		/// 遮罩樣式（當 replacement 為空時用此樣式）
		public MaskStyle MaskStyle { get; set; } = MaskStyle.Asterisks;

		/// MaskStyle=FixedLabel 時的固定字串
		public string FixedLabel { get; set; } = "【封鎖】";

		/// CJK 模糊比對（允許符號/空白/零寬字元夾字）
		public bool FuzzyBetweenCjkChars { get; set; } = true;

		/// 星號是否精準等長（會略慢；預設關閉）
		public bool MaskExactLength { get; set; } = false;

		/// 快取秒數（預設 30 秒；可在 appsettings 覆蓋）
		public int CacheTtlSeconds { get; set; } = 30;

		/// ★ 啟用「每詞自訂替代」：使用 Mutes.replacement ；為空時退回到上方樣式
		public bool UsePerWordReplacement { get; set; } = false;
	}

	public enum MaskStyle
	{
		Asterisks,
		FixedLabel
	}
}
