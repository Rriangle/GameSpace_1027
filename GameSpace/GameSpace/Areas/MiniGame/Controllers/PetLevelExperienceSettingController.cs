using GameSpace.Areas.MiniGame.Models.Settings;
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
    public class PetLevelExperienceSettingController : MiniGameBaseController
    {
        private readonly IPetLevelExperienceSettingService _service;

        public PetLevelExperienceSettingController(GameSpacedatabaseContext context, IPetLevelExperienceSettingService service) : base(context)
        {
            _service = service;
        }
        
        // GET: PetLevelExperienceSetting
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var settings = await _service.GetPagedAsync(page, pageSize);
            var totalCount = await _service.GetTotalCountAsync();
            
            var viewModel = new PetLevelExperienceSettingListViewModel
            {
                Settings = settings.Select(s => new PetLevelExperienceSettingViewModel
                {
                    Id = s.Id,
                    Level = s.Level,
                    RequiredExperience = s.RequiredExperience,
                    Description = s.Description,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };
            
            return View(viewModel);
        }
        
        // GET: PetLevelExperienceSetting/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }
            
            var viewModel = new PetLevelExperienceSettingViewModel
            {
                Id = setting.Id,
                Level = setting.Level,
                RequiredExperience = setting.RequiredExperience,
                Description = setting.Description,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };
            
            return View(viewModel);
        }
        
        // GET: PetLevelExperienceSetting/Create
        public IActionResult Create()
        {
            return View(new PetLevelExperienceSettingViewModel());
        }
        
        // POST: PetLevelExperienceSetting/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetLevelExperienceSettingViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var setting = new PetLevelExperienceSetting
                    {
                        Level = viewModel.Level,
                        RequiredExperience = viewModel.RequiredExperience,
                        Description = viewModel.Description
                    };
                    
                    await _service.CreateAsync(setting);
                    TempData["SuccessMessage"] = "寵物等級經驗值設定已成功建立";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
            return View(viewModel);
        }
        
        // GET: PetLevelExperienceSetting/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }
            
            var viewModel = new PetLevelExperienceSettingViewModel
            {
                Id = setting.Id,
                Level = setting.Level,
                RequiredExperience = setting.RequiredExperience,
                Description = setting.Description,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };
            
            return View(viewModel);
        }
        
        // POST: PetLevelExperienceSetting/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetLevelExperienceSettingViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var setting = new PetLevelExperienceSetting
                    {
                        Id = viewModel.Id,
                        Level = viewModel.Level,
                        RequiredExperience = viewModel.RequiredExperience,
                        Description = viewModel.Description
                    };
                    
                    await _service.UpdateAsync(setting);
                    TempData["SuccessMessage"] = "寵物等級經驗值設定已成功更新";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
            return View(viewModel);
        }
        
        // GET: PetLevelExperienceSetting/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }
            
            var viewModel = new PetLevelExperienceSettingViewModel
            {
                Id = setting.Id,
                Level = setting.Level,
                RequiredExperience = setting.RequiredExperience,
                Description = setting.Description,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };
            
            return View(viewModel);
        }
        
        // POST: PetLevelExperienceSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "寵物等級經驗值設定已成功刪除";
            }
            else
            {
                TempData["ErrorMessage"] = "刪除失敗，找不到指定的設定";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}

