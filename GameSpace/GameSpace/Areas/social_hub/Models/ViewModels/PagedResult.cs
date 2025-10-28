using System;
using System.Collections.Generic;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	/// <summary>
	/// 簡單的分頁容器（清單 Partial 共用）
	/// </summary>
	public class PagedResult<T>
	{
		public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
		public int Page { get; init; }
		public int PageSize { get; init; }
		public int TotalCount { get; init; }

		public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);

		public bool HasPrev => Page > 1;
		public bool HasNext => Page < TotalPages;

		public static PagedResult<T> Empty(int page = 1, int pageSize = 10)
			=> new PagedResult<T> { Items = Array.Empty<T>(), Page = page, PageSize = pageSize, TotalCount = 0 };
	}
}
