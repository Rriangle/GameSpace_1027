using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class PetLevelRewardSettingController : MiniGameBaseController
    {
        private readonly IPetLevelRewardSettingService _service;
        private readonly ILogger<PetLevelRewardSettingController> _logger;

        public PetLevelRewardSettingController(
            GameSpacedatabaseContext context,
            IPetLevelRewardSettingService service,
            ILogger<PetLevelRewardSettingController> logger) : base(context)
        {
            _service = service;
            _logger = logger;
        }

        // GET: MiniGame/PetLevelRewardSetting
        public async Task<IActionResult> Index(PetLevelRewardSettingSearchViewModel? searchModel)
        {
            try
            {
                searchModel ??= new PetLevelRewardSettingSearchViewModel();

                var settings = await _service.SearchAsync(searchModel);
                var (totalCount, totalPages) = await _service.GetPaginationInfoAsync(searchModel);
                var statistics = await _service.GetStatisticsAsync();

                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = searchModel.Page;
                ViewBag.PageSize = searchModel.PageSize;
                ViewBag.Statistics = statistics;

                _logger.LogInformation("顯示寵物升級獎勵設定列表，共 {Count} 筆", settings.Count());
                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示寵物升級獎勵設定列表時發生錯誤");
                TempData["ErrorMessage"] = "載入資料時發生錯誤，請稍後再試";
                return View(new List<PetLevelRewardSettingListViewModel>());
            }
        }

        // GET: MiniGame/PetLevelRewardSetting/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var setting = await _service.GetByIdAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到寵物升級獎勵設定 ID: {Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("顯示寵物升級獎勵設定詳細資料 ID: {Id}", id);
                return View(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示寵物升級獎勵設定詳細資料 ID: {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "載入資料時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: MiniGame/PetLevelRewardSetting/Create
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("顯示建立寵物升級獎勵設定表單");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示建立寵物升級獎勵設定表單時發生錯誤");
                TempData["ErrorMessage"] = "載入表單時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MiniGame/PetLevelRewardSetting/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetLevelRewardSettingCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("建立寵物升級獎勵設定表單驗證失敗");
                    return View(model);
                }

                var result = await _service.CreateAsync(model);
                if (result == null)
                {
                    ModelState.AddModelError("", "建立失敗，可能是等級已存在或發生其他錯誤");
                    _logger.LogWarning("建立寵物升級獎勵設定失敗");
                    return View(model);
                }

                _logger.LogInformation("成功建立寵物升級獎勵設定 ID: {Id}", result.SettingId);
                TempData["SuccessMessage"] = "寵物升級獎勵設定建立成功";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立寵物升級獎勵設定時發生錯誤");
                ModelState.AddModelError("", "建立時發生錯誤，請稍後再試");
                return View(model);
            }
        }

        // GET: MiniGame/PetLevelRewardSetting/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var setting = await _service.GetByIdAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到要編輯的寵物升級獎勵設定 ID: {Id}", id);
                    return NotFound();
                }

                var editModel = new PetLevelRewardSettingEditViewModel
                {
                    SettingId = setting.SettingId,
                    LevelRangeStart = setting.LevelRangeStart,
                    LevelRangeEnd = setting.LevelRangeEnd,
                    PointsReward = setting.PointsReward,
                    Description = setting.Description,
                    IsActive = setting.IsActive,
                    DisplayOrder = setting.DisplayOrder
                };

                _logger.LogInformation("顯示編輯寵物升級獎勵設定表單 ID: {Id}", id);
                return View(editModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示編輯寵物升級獎勵設定表單 ID: {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "載入表單時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MiniGame/PetLevelRewardSetting/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetLevelRewardSettingEditViewModel model)
        {
            try
            {
                if (id != model.SettingId)
                {
                    _logger.LogWarning("編輯寵物升級獎勵設定 ID 不匹配: {Id} != {ModelId}", id, model.SettingId);
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("編輯寵物升級獎勵設定表單驗證失敗 ID: {Id}", id);
                    return View(model);
                }

                var result = await _service.UpdateAsync(model);
                if (result == null)
                {
                    ModelState.AddModelError("", "更新失敗，可能是等級已存在或發生其他錯誤");
                    _logger.LogWarning("更新寵物升級獎勵設定失敗 ID: {Id}", id);
                    return View(model);
                }

                _logger.LogInformation("成功更新寵物升級獎勵設定 ID: {Id}", id);
                TempData["SuccessMessage"] = "寵物升級獎勵設定更新成功";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物升級獎勵設定 ID: {Id} 時發生錯誤", id);
                ModelState.AddModelError("", "更新時發生錯誤，請稍後再試");
                return View(model);
            }
        }

        // GET: MiniGame/PetLevelRewardSetting/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var setting = await _service.GetByIdAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到要刪除的寵物升級獎勵設定 ID: {Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("顯示刪除寵物升級獎勵設定確認頁面 ID: {Id}", id);
                return View(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示刪除寵物升級獎勵設定確認頁面 ID: {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "載入資料時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MiniGame/PetLevelRewardSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("刪除寵物升級獎勵設定失敗 ID: {Id}", id);
                    TempData["ErrorMessage"] = "刪除失敗，找不到指定的設定";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("成功刪除寵物升級獎勵設定 ID: {Id}", id);
                TempData["SuccessMessage"] = "寵物升級獎勵設定刪除成功";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物升級獎勵設定 ID: {Id} 時發生錯誤", id);
                TempData["ErrorMessage"] = "刪除時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MiniGame/PetLevelRewardSetting/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _service.ToggleStatusAsync(id);
                if (!result)
                {
                    _logger.LogWarning("切換寵物升級獎勵設定狀態失敗 ID: {Id}", id);
                    return Json(new { success = false, message = "切換狀態失敗" });
                }

                _logger.LogInformation("成功切換寵物升級獎勵設定狀態 ID: {Id}", id);
                return Json(new { success = true, message = "狀態切換成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換寵物升級獎勵設定狀態 ID: {Id} 時發生錯誤", id);
                return Json(new { success = false, message = "切換狀態時發生錯誤" });
            }
        }

        // GET: MiniGame/PetLevelRewardSetting/Statistics
        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _service.GetStatisticsAsync();
                _logger.LogInformation("取得寵物升級獎勵設定統計資料");
                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得統計資料時發生錯誤");
                return Json(new { error = "取得統計資料時發生錯誤" });
            }
        }

    }
}

