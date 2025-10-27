using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.ViewModels;

public class SignInRuleConfigViewModel
{
    [Display(Name = "瘥暺")]
    [Range(0, 1000)]
    public int DailyPoints { get; set; }

    [Display(Name = "Max Consecutive Days")]
    [Range(0, 5000)]
    public int WeeklyBonusPoints { get; set; }

    [Display(Name = "瘥??暺")]
    [Range(0, 10000)]
    public int MonthlyBonusPoints { get; set; }

    [Display(Name = "???憭拇閬?")]
    [Range(1, 30)]
    public int ConsecutiveDaysRequired { get; set; }

    [Display(Name = "閬??膩")]
    [StringLength(500)]
    public string? Description { get; set; }
}
