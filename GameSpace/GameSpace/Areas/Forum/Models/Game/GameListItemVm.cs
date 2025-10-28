namespace GameSpace.Areas.Forum.Models.Game
{
    public class GameListItemVm
    {
        public int GameId { get; set; }
        public string Name { get; set; } = "";
        public string? NameZh { get; set; }
        public string? Genre { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
