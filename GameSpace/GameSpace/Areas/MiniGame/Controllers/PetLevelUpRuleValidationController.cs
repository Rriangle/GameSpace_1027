using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class PetLevelUpRuleValidationController : MiniGameBaseController
    {
        private readonly IPetLevelUpRuleValidationService _validationService;
        private readonly ILogger<PetLevelUpRuleValidationController> _logger;

        public PetLevelUpRuleValidationController(
            GameSpacedatabaseContext context,
            IPetLevelUpRuleValidationService validationService,
            ILogger<PetLevelUpRuleValidationController> logger) : base(context)
        {
            _validationService = validationService;
            _logger = logger;
        }

        // GET: PetLevelUpRuleValidation
        public async Task<IActionResult> Index()
        {
            try
            {
                var consistencyResult = await _validationService.ValidateLevelUpRuleConsistencyAsync();
                return View(consistencyResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則驗證結果時發生錯誤");
                TempData["ErrorMessage"] = "載入驗證結果失敗";
                return View(new GameSpace.Areas.MiniGame.Models.ValidationResult { IsValid = false, Errors = new List<string> { "載入驗證結果失敗" } });
            }
        }

        // POST: PetLevelUpRuleValidation/ValidateCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateCreate(PetLevelUpRuleCreateViewModel model)
        {
            try
            {
                var result = await _validationService.ValidateLevelUpRuleAsync(model);
                return Json(new { 
                    success = result.IsValid, 
                    errors = result.Errors, 
                    warnings = result.Warnings,
                    data = result.Data 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證建立升級規則時發生錯誤");
                return Json(new { success = false, errors = new[] { "驗證過程中發生錯誤" } });
            }
        }

        // POST: PetLevelUpRuleValidation/ValidateUpdate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateUpdate(PetLevelUpRuleEditViewModel model)
        {
            try
            {
                var result = await _validationService.ValidateLevelUpRuleUpdateAsync(model);
                return Json(new { 
                    success = result.IsValid, 
                    errors = result.Errors, 
                    warnings = result.Warnings,
                    data = result.Data 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證更新升級規則時發生錯誤");
                return Json(new { success = false, errors = new[] { "驗證過程中發生錯誤" } });
            }
        }

        // POST: PetLevelUpRuleValidation/ValidateBulk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateBulk(List<PetLevelUpRuleEditViewModel> models)
        {
            try
            {
                var result = await _validationService.ValidateLevelUpRuleBulkAsync(models);
                return Json(new { 
                    success = result.IsValid, 
                    errors = result.Errors, 
                    warnings = result.Warnings,
                    data = result.Data 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證批量升級規則時發生錯誤");
                return Json(new { success = false, errors = new[] { "驗證過程中發生錯誤" } });
            }
        }

        // GET: PetLevelUpRuleValidation/Consistency
        public async Task<IActionResult> Consistency()
        {
            try
            {
                var result = await _validationService.ValidateLevelUpRuleConsistencyAsync();
                return Json(new { 
                    success = result.IsValid, 
                    errors = result.Errors, 
                    warnings = result.Warnings,
                    data = result.Data 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查升級規則一致性時發生錯誤");
                return Json(new { success = false, errors = new[] { "檢查過程中發生錯誤" } });
            }
        }

        // GET: PetLevelUpRuleValidation/Logic
        public async Task<IActionResult> Logic(int level, int experienceRequired, int pointsReward, int expReward)
        {
            try
            {
                var result = await _validationService.ValidateLevelUpRuleLogicAsync(level, experienceRequired, pointsReward, expReward);
                return Json(new { 
                    success = result.IsValid, 
                    errors = result.Errors, 
                    warnings = result.Warnings,
                    data = result.Data 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查升級規則邏輯時發生錯誤");
                return Json(new { success = false, errors = new[] { "檢查過程中發生錯誤" } });
            }
        }

        // GET: PetLevelUpRuleValidation/Report
        public async Task<IActionResult> Report()
        {
            try
            {
                var consistencyResult = await _validationService.ValidateLevelUpRuleConsistencyAsync();
                
                var report = new
                {
                    timestamp = DateTime.UtcNow,
                    consistency = consistencyResult,
                    summary = new
                    {
                        totalErrors = consistencyResult.Errors.Count,
                        totalWarnings = consistencyResult.Warnings.Count,
                        isValid = consistencyResult.IsValid
                    }
                };

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生升級規則驗證報告時發生錯誤");
                TempData["ErrorMessage"] = "產生驗證報告失敗";
                return View(new {
                    timestamp = DateTime.UtcNow,
                    consistency = new GameSpace.Areas.MiniGame.Models.ValidationResult { IsValid = false, Errors = new List<string> { "產生驗證報告失敗" } },
                    summary = new { totalErrors = 1, totalWarnings = 0, isValid = false }
                });
            }
        }
    }
}

