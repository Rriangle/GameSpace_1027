using System;
using System.Collections.Generic;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 分頁結果模型 - 統一版本
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        /// <summary>
        /// Page 是 CurrentPage 的別名，為了向後相容
        /// </summary>
        public int Page
        {
            get => CurrentPage;
            set => CurrentPage = value;
        }

        /// <summary>
        /// PageNumber 是 CurrentPage 的別名，為了向後相容
        /// </summary>
        public int PageNumber
        {
            get => CurrentPage;
            set => CurrentPage = value;
        }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
