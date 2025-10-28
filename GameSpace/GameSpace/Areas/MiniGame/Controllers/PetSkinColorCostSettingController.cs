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
    public class PetSkinColorCostSettingController : MiniGameBaseController
    {
        private readonly IPetSkinColorCostSettingService _service;

        public PetSkinColorCostSettingController(GameSpacedatabaseContext context, IPetSkinColorCostSettingService service) : base(context)
        {
            _service = service;
        }

        // GET: PetSkinColorCostSetting
        public async Task<IActionResult> Index()
        {
            var models = await _service.GetAllAsync();
            return View(models);
        }

        // GET: PetSkinColorCostSetting/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _service.GetByIdAsync(id.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        // GET: PetSkinColorCostSetting/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PetSkinColorCostSetting/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetSkinColorCostSetting model)
        {
            if (ModelState.IsValid)
            {
                var success = await _service.CreateAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物換色成本設定新增成功！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "新增失敗，請稍後再試。";
                }
            }
            return View(model);
        }

        // GET: PetSkinColorCostSetting/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _service.GetByIdAsync(id.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: PetSkinColorCostSetting/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetSkinColorCostSetting model)
        {
            if (id != model.SettingId) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _service.UpdateAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物換色成本設定更新成功！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "更新失敗，請稍後再試。";
                }
            }
            return View(model);
        }

        // GET: PetSkinColorCostSetting/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _service.GetByIdAsync(id.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: PetSkinColorCostSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "寵物換色成本設定刪除成功！";
            }
            else
            {
                TempData["ErrorMessage"] = "刪除失敗，請稍後再試。";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: PetSkinColorCostSetting/ToggleActive/5
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

