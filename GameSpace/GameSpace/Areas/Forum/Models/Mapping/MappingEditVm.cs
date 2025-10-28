namespace GameSpace.Areas.Forum.Models.Mapping
{
    public class MappingEditVm
    {
        public int? Id { get; set; }
        public int GameId { get; set; }
        public int SourceId { get; set; }
        public string ExternalKey { get; set; } = "";
        public string? Note { get; set; }
        // 顯示用
        public string? GameName { get; set; }
    }
}
