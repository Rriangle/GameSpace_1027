using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Controllers.Settings
{
    /// <summary>
    /// 寵物換色點數設定控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class PetColorChangeSettingsController : MiniGameBaseController
    {
        private readonly IPetColorChangeSettingsService _settingsService;
        private readonly ILogger<PetColorChangeSettingsController> _logger;

        public PetColorChangeSettingsController(
            GameSpacedatabaseContext context,
            IPetColorChangeSettingsService settingsService,
            ILogger<PetColorChangeSettingsController> logger) : base(context)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        /// <summary>
        /// 寵物換色點數設定列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var settings = await _settingsService.GetAllAsync();
                var viewModel = new PetColorChangeSettingsIndexViewModel
                {
                    Settings = settings.Select(s => new PetColorChangeSettingsViewModel
                    {
                        Id = s.Id,
                        ColorName = s.ColorName,
                        RequiredPoints = s.RequiredPoints,
                        ColorCode = s.ColorCode,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物換色設定列表時發生錯誤");
                TempData["ErrorMessage"] = "取得設定列表時發生錯誤，請稍後再試";
                return View(new PetColorChangeSettingsIndexViewModel());
            }
        }

        /// <summary>
        /// 新增寵物換色點數設定
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PetColorChangeSettingsViewModel());
        }

        /// <summary>
        /// 新增寵物換色點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetColorChangeSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var settings = new PetColorChangeSettings
                {
                    ColorName = model.ColorName,
                    RequiredPoints = model.RequiredPoints,
                    ColorCode = model.ColorCode,
                    IsActive = model.IsActive
                };

                await _settingsService.CreateAsync(settings);
                TempData["SuccessMessage"] = "成功新增寵物換色點數設定";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增寵物換色設定時發生錯誤");
                TempData["ErrorMessage"] = "新增設定時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 編輯寵物換色點數設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var settings = await _settingsService.GetByIdAsync(id);
                if (settings == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的設定";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new PetColorChangeSettingsViewModel
                {
                    Id = settings.Id,
                    ColorName = settings.ColorName,
                    RequiredPoints = settings.RequiredPoints,
                    ColorCode = settings.ColorCode,
                    IsActive = settings.IsActive
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物換色設定 {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "取得設定時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 編輯寵物換色點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetColorChangeSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var settings = new PetColorChangeSettings
                {
                    Id = id,
                    ColorName = model.ColorName,
                    RequiredPoints = model.RequiredPoints,
                    ColorCode = model.ColorCode,
                    IsActive = model.IsActive
                };

                await _settingsService.UpdateAsync(id, settings);
                TempData["SuccessMessage"] = "成功更新寵物換色點數設定";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "更新寵物換色設定 {Id} 時找不到記錄", id);
                TempData["ErrorMessage"] = "找不到指定的設定";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物換色設定 {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "更新設定時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 刪除寵物換色點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _settingsService.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "成功刪除寵物換色點數設定";
                }
                else
                {
                    TempData["ErrorMessage"] = "找不到指定的設定";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物換色設定 {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "刪除設定時發生錯誤，請稍後再試";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var result = await _settingsService.ToggleActiveAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "成功切換設定啟用狀態";
                }
                else
                {
                    TempData["ErrorMessage"] = "找不到指定的設定";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換寵物換色設定 {Id} 啟用狀態時發生錯誤", id);
                TempData["ErrorMessage"] = "切換啟用狀態時發生錯誤，請稍後再試";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

