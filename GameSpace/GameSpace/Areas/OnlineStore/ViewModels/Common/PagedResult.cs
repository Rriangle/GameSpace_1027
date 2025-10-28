// /Areas/OnlineStore/ViewModels/Common/PagedResult.cs
using GameSpace.Areas.OnlineStore.ViewModels.Common;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public PagingVM Paging { get; set; } = new PagingVM();
    }
}

