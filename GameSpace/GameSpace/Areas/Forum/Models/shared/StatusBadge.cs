namespace GameSpace.Areas.Forum.view_Models.shared
{

    /// <summary>
    /// 因為這顯示邏輯會 多處共用    小工具：字串 → 標籤樣式
    /// </summary>
    public class StatusBadge
    {


        public static string ClassName(string? status) => status switch
        {
            "normal" => "badge bg-success",
            "hidden" => "badge bg-warning text-dark",
            "deleted" => "badge bg-danger",
            _ => "badge bg-secondary"
        };

        public static string Label(string? status) => status switch
        {
            "normal" => "正常",
            "hidden" => "隱藏",
            "deleted" => "刪除",
            _ => status ?? "未知"
        };
    }




}
