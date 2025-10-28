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
    public class PetBackgroundCostSettingController : MiniGameBaseController
    {
        private readonly IPetBackgroundCostSettingService _service;

        public PetBackgroundCostSettingController(GameSpacedatabaseContext context, IPetBackgroundCostSettingService service) : base(context)
        {
            _service = service;
        }

        // GET: PetBackgroundCostSetting
        public async Task<IActionResult> Index()
        {
            var models = await _service.GetAllAsync();
            return View(models);
        }

        // GET: PetBackgroundCostSetting/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _service.GetByIdAsync(id.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        // GET: PetBackgroundCostSetting/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PetBackgroundCostSetting/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetBackgroundCostSetting model)
        {
            if (ModelState.IsValid)
            {
                var success = await _service.CreateAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物換背景成本設定新增成功！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "新增失敗，請稍後再試。";
                }
            }
            return View(model);
        }

        // GET: PetBackgroundCostSetting/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _service.GetByIdAsync(id.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: PetBackgroundCostSetting/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetBackgroundCostSetting model)
        {
            if (id != model.SettingId) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _service.UpdateAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物換背景成本設定更新成功！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "更新失敗，請稍後再試。";
                }
            }
            return View(model);
        }

        // GET: PetBackgroundCostSetting/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _service.GetByIdAsync(id.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: PetBackgroundCostSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "寵物換背景成本設定刪除成功！";
            }
            else
            {
                TempData["ErrorMessage"] = "刪除失敗，請稍後再試。";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: PetBackgroundCostSetting/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var success = await _service.ToggleActiveAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "狀態切換成功！";
            }
            else
            {
                TempData["ErrorMessage"] = "狀態切換失敗，請稍後再試。";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

