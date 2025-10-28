using GameSpace.Models;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 每日遊戲次數限制設定控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class DailyGameLimitController : MiniGameBaseController
    {
        private readonly IDailyGameLimitService _service;
        private readonly DailyGameLimitValidationService _validationService;
        private readonly ILogger<DailyGameLimitController> _logger;

        public DailyGameLimitController(
            GameSpacedatabaseContext context,
            IDailyGameLimitService service,
            DailyGameLimitValidationService validationService,
            ILogger<DailyGameLimitController> logger) : base(context)
        {
            _service = service;
            _validationService = validationService;
            _logger = logger;
        }

        /// <summary>
        /// 每日遊戲次數限制設定列表頁面
        /// </summary>
        public async Task<IActionResult> Index(DailyGameLimitSearchViewModel? searchModel = null)
        {
            try
            {
                searchModel ??= new DailyGameLimitSearchViewModel();
                
                var (items, totalCount) = await _service.GetAllAsync(searchModel);
                var statistics = await _service.GetStatisticsAsync();

                ViewBag.SearchModel = searchModel;
                ViewBag.TotalCount = totalCount;
                ViewBag.Statistics = statistics;

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入每日遊戲次數限制設定列表時發生錯誤");
                TempData["ErrorMessage"] = "載入資料時發生錯誤，請稍後再試";
                return View(new List<DailyGameLimitListViewModel>());
            }
        }

        /// <summary>
        /// 建立每日遊戲次數限制設定頁面
        /// </summary>
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("顯示建立每日遊戲次數限制設定表單");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示建立每日遊戲次數限制設定表單時發生錯誤");
                TempData["ErrorMessage"] = "載入表單時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 建立每日遊戲次數限制設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DailyGameLimitCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("每日遊戲次數限制設定表單驗證失敗");
                    return View(model);
                }

                var success = await _service.CreateAsync(model, User.Identity?.Name);
                if (success)
                {
                    _logger.LogInformation("成功建立每日遊戲次數限制設定");
                    TempData["SuccessMessage"] = "每日遊戲次數限制設定建立成功！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("建立每日遊戲次數限制設定失敗");
                    TempData["ErrorMessage"] = "建立失敗，請稍後再試";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立每日遊戲次數限制設定時發生錯誤");
                TempData["ErrorMessage"] = "建立時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 每日遊戲次數限制設定詳細資料頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var setting = await _service.GetByIdAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到每日遊戲次數限制設定 ID: {Id}", id);
                    return NotFound();
                }

                var viewModel = new DailyGameLimitDetailsViewModel
                {
                    Id = setting.Id,
                    DailyLimit = setting.DailyLimit,
                    SettingName = setting.SettingName,
                    Description = setting.Description,
                    IsEnabled = setting.IsEnabled,
                    CreatedAt = setting.CreatedAt,
                    UpdatedAt = setting.UpdatedAt,
                    CreatedBy = setting.CreatedBy,
                    UpdatedBy = setting.UpdatedBy
                };

                _logger.LogInformation("顯示每日遊戲次數限制設定詳細資料 ID: {Id}", id);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示每日遊戲次數限制設定詳細資料 ID: {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "載入資料時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 編輯每日遊戲次數限制設定頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var setting = await _service.GetByIdAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到每日遊戲次數限制設定 ID: {Id}", id);
                    return NotFound();
                }

                var viewModel = new DailyGameLimitEditViewModel
                {
                    Id = setting.Id,
                    DailyLimit = setting.DailyLimit,
                    SettingName = setting.SettingName,
                    Description = setting.Description,
                    IsEnabled = setting.IsEnabled
                };

                _logger.LogInformation("顯示編輯每日遊戲次數限制設定表單 ID: {Id}", id);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示編輯每日遊戲次數限制設定表單 ID: {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "載入表單時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 更新每日遊戲次數限制設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DailyGameLimitEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("每日遊戲次數限制設定表單驗證失敗 ID: {Id}", model.Id);
                    return View(model);
                }

                var success = await _service.UpdateAsync(model, User.Identity?.Name);
                if (success)
                {
                    _logger.LogInformation("成功更新每日遊戲次數限制設定 ID: {Id}", model.Id);
                    TempData["SuccessMessage"] = "每日遊戲次數限制設定更新成功！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("更新每日遊戲次數限制設定失敗 ID: {Id}", model.Id);
                    TempData["ErrorMessage"] = "更新失敗，請稍後再試";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新每日遊戲次數限制設定 ID: {Id} 時發生錯誤", model.Id);
                TempData["ErrorMessage"] = "更新時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 刪除每日遊戲次數限制設定確認頁面
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var setting = await _service.GetByIdAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到每日遊戲次數限制設定 ID: {Id}", id);
                    return NotFound();
                }

                var viewModel = new DailyGameLimitDetailsViewModel
                {
                    Id = setting.Id,
                    DailyLimit = setting.DailyLimit,
                    SettingName = setting.SettingName,
                    Description = setting.Description,
                    IsEnabled = setting.IsEnabled,
                    CreatedAt = setting.CreatedAt,
                    UpdatedAt = setting.UpdatedAt,
                    CreatedBy = setting.CreatedBy,
                    UpdatedBy = setting.UpdatedBy
                };

                _logger.LogInformation("顯示刪除每日遊戲次數限制設定確認頁面 ID: {Id}", id);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示刪除每日遊戲次數限制設定確認頁面 ID: {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "載入資料時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 刪除每日遊戲次數限制設定
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (success)
                {
                    _logger.LogInformation("成功刪除每日遊戲次數限制設定 ID: {Id}", id);
                    TempData["SuccessMessage"] = "每日遊戲次數限制設定刪除成功！";
                }
                else
                {
                    _logger.LogWarning("刪除每日遊戲次數限制設定失敗 ID: {Id}", id);
                    TempData["ErrorMessage"] = "刪除失敗，請稍後再試";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除每日遊戲次數限制設定 ID: {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "刪除時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var success = await _service.ToggleStatusAsync(id, User.Identity?.Name);
                if (success)
                {
                    _logger.LogInformation("成功切換每日遊戲次數限制設定狀態 ID: {Id}", id);
                    return Json(new { success = true, message = "狀態切換成功" });
                }
                else
                {
                    _logger.LogWarning("切換每日遊戲次數限制設定狀態失敗 ID: {Id}", id);
                    return Json(new { success = false, message = "狀態切換失敗" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換每日遊戲次數限制設定狀態 ID: {Id} 時發生錯誤", id);
                return Json(new { success = false, message = "狀態切換時發生錯誤" });
            }
        }

        /// <summary>
        /// 取得目前設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCurrentSetting()
        {
            try
            {
                var setting = await _service.GetCurrentSettingAsync();
                if (setting == null)
                {
                    return Json(new { success = true, data = new { dailyLimit = 3 } });
                }

                return Json(new { success = true, data = new { dailyLimit = setting.DailyLimit } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得目前每日遊戲次數限制設定時發生錯誤");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新目前設定
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateCurrentSetting(int dailyLimit)
        {
            try
            {
                if (dailyLimit < 1)
                {
                    return Json(new { success = false, message = "每日遊戲次數限制不能小於1" });
                }

                var currentSetting = await _service.GetCurrentSettingAsync();
                if (currentSetting == null)
                {
                    // 建立新設定
                    var createModel = new DailyGameLimitCreateViewModel
                    {
                        DailyLimit = dailyLimit,
                        SettingName = "每日遊戲次數限制",
                        Description = "系統預設的每日遊戲次數限制",
                        IsEnabled = true
                    };

                    var success = await _service.CreateAsync(createModel, User.Identity?.Name);
                    if (success)
                    {
                        _logger.LogInformation("成功建立新的每日遊戲次數限制設定，限制次數: {DailyLimit}", dailyLimit);
                        return Json(new { success = true, message = "每日遊戲次數限制設定成功" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "設定失敗" });
                    }
                }
                else
                {
                    // 更新現有設定
                    var editModel = new DailyGameLimitEditViewModel
                    {
                        Id = currentSetting.Id,
                        DailyLimit = dailyLimit,
                        SettingName = currentSetting.SettingName,
                        Description = currentSetting.Description,
                        IsEnabled = currentSetting.IsEnabled
                    };

                    var success = await _service.UpdateAsync(editModel, User.Identity?.Name);
                    if (success)
                    {
                        _logger.LogInformation("成功更新每日遊戲次數限制設定，限制次數: {DailyLimit}", dailyLimit);
                        return Json(new { success = true, message = "每日遊戲次數限制設定成功" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "設定失敗" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新每日遊戲次數限制設定時發生錯誤");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}





