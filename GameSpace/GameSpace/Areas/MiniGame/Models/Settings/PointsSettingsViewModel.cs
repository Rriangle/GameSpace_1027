using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    public class PointsSettingsIndexViewModel
    {
        public List<PetColorChangeSettingsViewModel> ColorSettings { get; set; } = new List<PetColorChangeSettingsViewModel>();
        public List<PetBackgroundChangeSettingsViewModel> BackgroundSettings { get; set; } = new List<PetBackgroundChangeSettingsViewModel>();
    }

    public class PointsSettingsStatisticsViewModel
    {
        [Display(Name = "寵物換色設定總數")]
        public int TotalColorSettings { get; set; }

        [Display(Name = "啟用的寵物換色設定")]
        public int ActiveColorSettings { get; set; }

        [Display(Name = "寵物換背景設定總數")]
        public int TotalBackgroundSettings { get; set; }

        [Display(Name = "啟用的寵物換背景設定")]
        public int ActiveBackgroundSettings { get; set; }

        [Display(Name = "寵物換色總點數")]
        public int TotalColorPoints { get; set; }

        [Display(Name = "寵物換背景總點數")]
        public int TotalBackgroundPoints { get; set; }

        [Display(Name = "總點數")]
        public int TotalPoints => TotalColorPoints + TotalBackgroundPoints;
    }
}

