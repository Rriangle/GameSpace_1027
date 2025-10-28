// /Areas/OnlineStore/ViewModels/Common/PagingVM.cs
//
namespace GameSpace.Areas.OnlineStore.ViewModels.Common
{
    public class PagingVM
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int Total { get; set; } = 0;

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
    }
}

