using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class PetLevelUpRuleController : MiniGameBaseController
    {
        private readonly IPetLevelUpRuleService _levelUpRuleService;
        private readonly IPetLevelUpRuleValidationService _validationService;
        private readonly ILogger<PetLevelUpRuleController> _logger;

        public PetLevelUpRuleController(
            GameSpacedatabaseContext context,
            IPetLevelUpRuleService levelUpRuleService,
            IPetLevelUpRuleValidationService validationService,
            ILogger<PetLevelUpRuleController> logger) : base(context)
        {
            _levelUpRuleService = levelUpRuleService;
            _validationService = validationService;
            _logger = logger;
        }

        // GET: PetLevelUpRule
        public async Task<IActionResult> Index()
        {
            try
            {
                var rules = await _levelUpRuleService.GetAllLevelUpRulesAsync();
                
                // 取得驗證結果
                var validationResult = await _validationService.ValidateLevelUpRuleConsistencyAsync();
                
                ViewBag.ValidationResult = validationResult;
                
                return View(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則列表時發生錯誤");
                TempData["ErrorMessage"] = "載入升級規則列表失敗";
                return View(new List<PetLevelUpRule>());
            }
        }

        // GET: PetLevelUpRule/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var rule = await _levelUpRuleService.GetLevelUpRuleByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的升級規則";
                    return RedirectToAction(nameof(Index));
                }

                return View(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則詳細資料時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入升級規則詳細資料失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: PetLevelUpRule/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PetLevelUpRule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetLevelUpRuleCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 使用新的驗證服務
                var validationResult = await _validationService.ValidateLevelUpRuleAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return View(model);
                }

                // 顯示警告
                if (validationResult.Warnings.Any())
                {
                    foreach (var warning in validationResult.Warnings)
                    {
                        TempData["WarningMessage"] += warning + "; ";
                    }
                }

                var createdBy = 1; // 這裡應該從當前使用者取得
                var success = await _levelUpRuleService.CreateLevelUpRuleAsync(model, createdBy);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "升級規則建立成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "建立升級規則失敗");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立升級規則時發生錯誤");
                ModelState.AddModelError("", "建立升級規則時發生錯誤");
                return View(model);
            }
        }

        // GET: PetLevelUpRule/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rule = await _levelUpRuleService.GetLevelUpRuleByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的升級規則";
                    return RedirectToAction(nameof(Index));
                }

                var model = new PetLevelUpRuleEditViewModel
                {
                    Id = rule.Id,
                    Level = rule.Level,
                    RequiredExp = rule.RequiredExp,  // Use actual entity property
                    PointsReward = rule.RewardPoints,  // Map RewardPoints to PointsReward
                    IsActive = rule.IsActive,
                    Description = rule.Description  // Map Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則編輯資料時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入升級規則編輯資料失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: PetLevelUpRule/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetLevelUpRuleEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 使用新的驗證服務
                var validationResult = await _validationService.ValidateLevelUpRuleUpdateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return View(model);
                }

                // 顯示警告
                if (validationResult.Warnings.Any())
                {
                    foreach (var warning in validationResult.Warnings)
                    {
                        TempData["WarningMessage"] += warning + "; ";
                    }
                }

                var updatedBy = 1; // 這裡應該從當前使用者取得
                var success = await _levelUpRuleService.UpdateLevelUpRuleAsync(model, updatedBy);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "升級規則更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "更新升級規則失敗");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新升級規則時發生錯誤");
                ModelState.AddModelError("", "更新升級規則時發生錯誤");
                return View(model);
            }
        }

        // GET: PetLevelUpRule/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rule = await _levelUpRuleService.GetLevelUpRuleByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的升級規則";
                    return RedirectToAction(nameof(Index));
                }

                return View(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則刪除資料時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入升級規則刪除資料失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: PetLevelUpRule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _levelUpRuleService.DeleteLevelUpRuleAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "升級規則刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除升級規則失敗";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除升級規則時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "刪除升級規則時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: PetLevelUpRule/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var success = await _levelUpRuleService.ToggleLevelUpRuleStatusAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "升級規則狀態切換成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "切換升級規則狀態失敗";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換升級規則狀態時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "切換升級規則狀態時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: PetLevelUpRule/Validation
        public async Task<IActionResult> Validation()
        {
            try
            {
                var validationResult = await _validationService.ValidateLevelUpRuleConsistencyAsync();
                return RedirectToAction("Index", "PetLevelUpRuleValidation", new { area = "MiniGame" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則驗證結果時發生錯誤");
                TempData["ErrorMessage"] = "載入驗證結果失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: PetLevelUpRule/ValidateCreate
        [HttpPost]
        public async Task<IActionResult> ValidateCreate(PetLevelUpRuleCreateViewModel model)
        {
            try
            {
                var result = await _validationService.ValidateLevelUpRuleAsync(model);
                return Json(new { 
                    success = result.IsValid, 
                    errors = result.Errors, 
                    warnings = result.Warnings 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證建立升級規則時發生錯誤");
                return Json(new { success = false, errors = new[] { "驗證過程中發生錯誤" } });
            }
        }

        // POST: PetLevelUpRule/ValidateUpdate
        [HttpPost]
        public async Task<IActionResult> ValidateUpdate(PetLevelUpRuleEditViewModel model)
        {
            try
            {
                var result = await _validationService.ValidateLevelUpRuleUpdateAsync(model);
                return Json(new { 
                    success = result.IsValid, 
                    errors = result.Errors, 
                    warnings = result.Warnings 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證更新升級規則時發生錯誤");
                return Json(new { success = false, errors = new[] { "驗證過程中發生錯誤" } });
            }
        }
    }
}

