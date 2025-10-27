using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.Forum.Models.Game
{
    public class GameEditVm
    {
        public int Id { get; set; }
        public int? GameId { get; set; }
        public string Name { get; set; } = "";
        public int Source_Id { get; set; }
        public string SourceName { get; set; } = "";
        public string ExternalKey { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
       

        [Required(ErrorMessage = "名稱必填")]
        [StringLength(50)]
        

     
        public string? NameZh { get; set; }

        [StringLength(50)]
        public string? Genre { get; set; }
    }
}
