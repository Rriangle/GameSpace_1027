using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Services
{
    public class SignInRulesOptions
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PointsReward { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

