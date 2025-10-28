namespace GameSpace.Areas.Forum.Models.Mapping
{
    public class MappingRowVm
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; } = "";
        public int SourceId { get; set; }
        public string SourceName { get; set; } = "";
        public string ExternalKey { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
    }
}
