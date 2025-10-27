using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers.Settings
{
    /// <summary>
    /// 點數設定管理控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class PointsSettingsController : MiniGameBaseController
    {
        private readonly IPetColorChangeSettingsService _colorSettingsService;
        private readonly IPetBackgroundChangeSettingsService _backgroundSettingsService;
        private readonly IPointsSettingsStatisticsService _statisticsService;
        private readonly ILogger<PointsSettingsController> _logger;

        public PointsSettingsController(
            GameSpacedatabaseContext context,
            IPetColorChangeSettingsService colorSettingsService,
            IPetBackgroundChangeSettingsService backgroundSettingsService,
            IPointsSettingsStatisticsService statisticsService,
            ILogger<PointsSettingsController> logger) : base(context)
        {
            _colorSettingsService = colorSettingsService;
            _backgroundSettingsService = backgroundSettingsService;
            _statisticsService = statisticsService;
            _logger = logger;
        }

        /// <summary>
        /// 點數設定管理首頁
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var colorSettings = await _colorSettingsService.GetAllAsync();
                var backgroundSettings = await _backgroundSettingsService.GetAllAsync();

                var viewModel = new PointsSettingsIndexViewModel
                {
                    ColorSettings = colorSettings.Select(s => new PetColorChangeSettingsViewModel
                    {
                        Id = s.Id,
                        ColorName = s.ColorName,
                        RequiredPoints = s.RequiredPoints,
                        ColorCode = s.ColorCode,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    }).ToList(),
                    BackgroundSettings = backgroundSettings.Select(s => new PetBackgroundChangeSettingsViewModel
                    {
                        Id = s.Id,
                        BackgroundName = s.BackgroundName,
                        PointsRequired = s.RequiredPoints,
                        BackgroundCode = s.BackgroundCode,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得點數設定管理頁面時發生錯誤");
                TempData["ErrorMessage"] = "取得設定資料時發生錯誤，請稍後再試";
                return View(new PointsSettingsIndexViewModel());
            }
        }

        /// <summary>
        /// 點數設定統計頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var totalColorSettings = await _statisticsService.GetTotalColorSettingsAsync();
                var totalBackgroundSettings = await _statisticsService.GetTotalBackgroundSettingsAsync();
                var activeColorSettings = await _statisticsService.GetActiveColorSettingsAsync();
                var activeBackgroundSettings = await _statisticsService.GetActiveBackgroundSettingsAsync();
                var totalColorPoints = await _statisticsService.GetTotalColorPointsAsync();
                var totalBackgroundPoints = await _statisticsService.GetTotalBackgroundPointsAsync();

                var viewModel = new PointsSettingsStatisticsViewModel
                {
                    TotalColorSettings = totalColorSettings,
                    TotalBackgroundSettings = totalBackgroundSettings,
                    ActiveColorSettings = activeColorSettings,
                    ActiveBackgroundSettings = activeBackgroundSettings,
                    TotalColorPoints = totalColorPoints,
                    TotalBackgroundPoints = totalBackgroundPoints
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得點數設定統計時發生錯誤");
                TempData["ErrorMessage"] = "取得統計資料時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 重導向到寵物換色設定管理
        /// </summary>
        [HttpGet]
        public IActionResult ColorSettings()
        {
            return RedirectToAction("Index", "PetColorChangeSettings");
        }

        /// <summary>
        /// 重導向到寵物換背景設定管理
        /// </summary>
        [HttpGet]
        public IActionResult BackgroundSettings()
        {
            return RedirectToAction("Index", "PetBackgroundChangeSettings");
        }
    }
}

