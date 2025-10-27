using System;
using System.Collections.Generic;

namespace GameSpace.Infrastructure.Login
{
	/// <summary>代表目前請求的登入身分（統一輸出）。</summary>
	public sealed class LoginIdentityResult
	{
		// === 基本身分 ===
		public int? UserId { get; init; }
		public int? ManagerId { get; init; }

		/// <summary>"manager" | "user" | "guest"</summary>
		public string Kind { get; init; } = "guest";

		/// <summary>若為 manager 則回 ManagerId，否則回 UserId，預設 0。</summary>
		public int EffectiveId => ManagerId ?? UserId ?? 0;

		/// <summary>是否已有有效身分（manager 或 user）。</summary>
		public bool IsAuthenticated => ManagerId.HasValue || UserId.HasValue;

		/// <summary>顯示名稱（通常來自 Name claim）。</summary>
		public string? DisplayName { get; init; }

		/// <summary>Email（若有）。</summary>
		public string? Email { get; init; }

		/// <summary>是否標記為管理員（"IsManager" or 有 mgr:id / perm:*）。</summary>
		public bool IsManager { get; init; }

		// === 授權 ===
		public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
		public IReadOnlyDictionary<string, bool> Perms { get; init; } = new Dictionary<string, bool>();

		// === 票證/診斷（選用） ===
		public string? AuthScheme { get; init; }
		public DateTimeOffset? ExpiresUtc { get; init; }
		public IReadOnlyList<(string Type, string Value)> AllClaims { get; init; } = Array.Empty<(string, string)>();
	}
}
