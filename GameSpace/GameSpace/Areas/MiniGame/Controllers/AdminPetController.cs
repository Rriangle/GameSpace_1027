using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminPetController : MiniGameBaseController
    {
        private readonly IPetService _petService;
        private readonly IPetRulesService _petRulesService;
        private readonly IPetQueryService _petQueryService;
        private readonly IPetMutationService _petMutationService;

        public AdminPetController(
            GameSpacedatabaseContext context,
            IPetService petService,
            IPetRulesService petRulesService,
            IPetQueryService petQueryService,
            IPetMutationService petMutationService)
            : base(context)
        {
            _petService = petService;
            _petRulesService = petRulesService;
            _petQueryService = petQueryService;
            _petMutationService = petMutationService;
        }

        // GET: AdminPet
        // Note: Pet entity only has basic properties (PetName, Level, Experience, etc.), not PetType/Description/Rarity
        public async Task<IActionResult> Index(string searchTerm = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var pets = await _petService.GetAllPetsAsync();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out int userId))
                {
                    pets = pets.Where(p => p.UserId == userId);
                }
                else
                {
                    pets = pets.Where(p => p.PetName.Contains(searchTerm));
                }
            }

            // 排序
            pets = sortBy switch
            {
                "level" => pets.OrderByDescending(p => p.Level),
                "exp" => pets.OrderByDescending(p => p.Experience),
                "health" => pets.OrderByDescending(p => p.Health),
                _ => pets.OrderBy(p => p.PetName)
            };

            // 分頁
            var totalCount = pets.Count();
            var pagedPets = pets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminPetIndexViewModel
            {
                Pets = new PagedResult<GameSpace.Models.Pet>
                {
                    Items = pagedPets,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            var allPets = await _petService.GetAllPetsAsync();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalPets = allPets.Count();
            ViewBag.HighLevelPets = allPets.Count(p => p.Level >= 10);
            ViewBag.HealthyPets = allPets.Count(p => p.Health >= 80);
            ViewBag.AverageLevel = allPets.Any() ? allPets.Average(p => (double)p.Level) : 0;
            ViewBag.AverageHealth = allPets.Any() ? allPets.Average(p => (double)p.Health) : 0;

            return View(viewModel);
        }

        // GET: AdminPet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _petService.GetPetByIdAsync(id.Value);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // GET: AdminPet/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminPet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminPetCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var pet = new GameSpace.Models.Pet
                {
                    UserId = model.UserId,
                    PetName = model.PetName,
                    SkinColor = model.SkinColor,
                    SkinColorChangedTime = DateTime.Now,
                    BackgroundColor = model.BackgroundColor,
                    BackgroundColorChangedTime = DateTime.Now,
                    // 初始化寵物狀態
                    Level = 1,
                    LevelUpTime = DateTime.Now,
                    Experience = 0,
                    Hunger = 100,
                    Mood = 100,
                    Stamina = 100,
                    Cleanliness = 100,
                    Health = 100,
                    PointsChangedSkinColor = 0,
                    PointsChangedBackgroundColor = 0,
                    PointsGainedLevelUp = 0,
                    PointsGainedTimeLevelUp = DateTime.Now
                };

                var result = await _petService.CreatePetAsync(pet);

                if (result)
                {
                    TempData["SuccessMessage"] = "寵物建立成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "建立寵物失敗");
                }
            }

            return View(model);
        }

        // GET: AdminPet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _petService.GetPetByIdAsync(id.Value);
            if (pet == null)
            {
                return NotFound();
            }

            var model = new AdminPetCreateViewModel
            {
                UserId = pet.UserId,
                PetName = pet.PetName,
                SkinColor = pet.SkinColor,
                BackgroundColor = pet.BackgroundColor
            };

            return View(model);
        }

        // POST: AdminPet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminPetCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var pet = await _petService.GetPetByIdAsync(id);
                if (pet == null)
                {
                    return NotFound();
                }

                pet.UserId = model.UserId;
                pet.PetName = model.PetName;
                pet.SkinColor = model.SkinColor;
                pet.BackgroundColor = model.BackgroundColor;

                var result = await _petService.UpdatePetAsync(pet);

                if (result)
                {
                    TempData["SuccessMessage"] = "寵物更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "更新寵物失敗");
                }
            }

            return View(model);
        }

        // GET: AdminPet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _petService.GetPetByIdAsync(id.Value);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // POST: AdminPet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _petService.DeletePetAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "寵物刪除成功";
            }
            else
            {
                TempData["ErrorMessage"] = "寵物刪除失敗";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換寵物狀態 - Pet entity doesn't have IsActive property, removed this functionality
        // [HttpPost]
        // public async Task<IActionResult> ToggleStatus(int id)
        // Note: Pet entity doesn't have IsActive property - this method is disabled

        // 獲取寵物統計數據
        [HttpGet]
        public async Task<IActionResult> GetPetStats()
        {
            var allPets = await _petService.GetAllPetsAsync();
            var stats = new
            {
                total = allPets.Count(),
                highLevel = allPets.Count(p => p.Level >= 10),
                healthy = allPets.Count(p => p.Health >= 80),
                hungry = allPets.Count(p => p.Hunger < 30),
                avgLevel = allPets.Any() ? allPets.Average(p => (double)p.Level) : 0,
                avgHealth = allPets.Any() ? allPets.Average(p => (double)p.Health) : 0
            };

            return Json(stats);
        }

        // 獲取寵物等級分佈
        [HttpGet]
        public async Task<IActionResult> GetPetRarityDistribution()
        {
            var allPets = await _petService.GetAllPetsAsync();

            var distribution = allPets
                .GroupBy(p => p.Level / 10 * 10) // Group by 10-level ranges (0-9, 10-19, etc.)
                .Select(g => new
                {
                    levelRange = $"Level {g.Key}-{g.Key + 9}",
                    count = g.Count()
                })
                .ToList();

            return Json(distribution);
        }

        // 獲取寵物皮膚顏色分佈
        [HttpGet]
        public async Task<IActionResult> GetPetTypeDistribution()
        {
            var allPets = await _petService.GetAllPetsAsync();

            var distribution = allPets
                .GroupBy(p => p.SkinColor)
                .Select(g => new
                {
                    skinColor = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToList();

            return Json(distribution);
        }

        // 獲取寵物健康度統計
        [HttpGet]
        public async Task<IActionResult> GetPetDropRateStats()
        {
            var allPets = await _petService.GetAllPetsAsync();

            var stats = new
            {
                avgHealth = allPets.Any() ? allPets.Average(p => (double)p.Health) : 0,
                avgHunger = allPets.Any() ? allPets.Average(p => (double)p.Hunger) : 0,
                avgMood = allPets.Any() ? allPets.Average(p => (double)p.Mood) : 0,
                avgStamina = allPets.Any() ? allPets.Average(p => (double)p.Stamina) : 0,
                avgCleanliness = allPets.Any() ? allPets.Average(p => (double)p.Cleanliness) : 0
            };

            return Json(stats);
        }

        // 獲取寵物顏色選項
        [HttpGet]
        public async Task<IActionResult> GetPetColorOptions()
        {
            try
            {
                var options = await _petService.GetAvailableColorsAsync();
                return Json(new { success = true, data = options });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 新增寵物顏色選項
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPetColorOption(string colorName, string colorValue, int displayOrder)
        {
            try
            {
                if (string.IsNullOrEmpty(colorName) || string.IsNullOrEmpty(colorValue))
                {
                    return Json(new { success = false, message = "顏色名稱和顏色值不能為空" });
                }

                var option = new PetColorOption
                {
                    ColorName = colorName,
                    ColorValue = colorValue,
                    DisplayOrder = displayOrder,
                    IsActive = true
                };

                var result = await _petRulesService.CreateColorOptionAsync(option);

                if (result)
                {
                    return Json(new { success = true, message = "寵物顏色選項新增成功" });
                }
                else
                {
                    return Json(new { success = false, message = "新增失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新寵物顏色選項
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetColorOption(int id, string colorName, string colorValue, int displayOrder, bool isActive)
        {
            try
            {
                var options = await _petRulesService.GetAllColorOptionsAsync();
                var option = options.FirstOrDefault(o => o.ColorOptionId == id);

                if (option == null)
                {
                    return Json(new { success = false, message = "找不到指定的顏色選項" });
                }

                option.ColorName = colorName;
                option.ColorValue = colorValue;
                option.DisplayOrder = displayOrder;
                option.IsActive = isActive;

                var result = await _petRulesService.UpdateColorOptionAsync(option);

                if (result)
                {
                    return Json(new { success = true, message = "寵物顏色選項更新成功" });
                }
                else
                {
                    return Json(new { success = false, message = "更新失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 刪除寵物顏色選項
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePetColorOption(int id)
        {
            try
            {
                var result = await _petRulesService.DeleteColorOptionAsync(id);

                if (result)
                {
                    return Json(new { success = true, message = "寵物顏色選項刪除成功" });
                }
                else
                {
                    return Json(new { success = false, message = "刪除失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取寵物背景選項
        [HttpGet]
        public async Task<IActionResult> GetPetBackgroundOptions()
        {
            try
            {
                var options = await _petService.GetAvailableBackgroundsAsync();
                return Json(new { success = true, data = options });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取升級規則
        [HttpGet]
        public async Task<IActionResult> GetPetLevelUpRules()
        {
            try
            {
                var rules = await _petRulesService.GetAllLevelUpRulesAsync();
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新升級規則
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetLevelUpRules(List<GameSpace.Areas.MiniGame.Models.PetLevelUpRule> rules)
        {
            try
            {
                if (rules == null || !rules.Any())
                {
                    return Json(new { success = false, message = "升級規則不能為空" });
                }

                // 更新每個規則
                foreach (var rule in rules)
                {
                    await _petRulesService.UpdateLevelUpRuleAsync(rule);
                }

                return Json(new { success = true, message = "寵物升級規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取互動獎勵規則
        [HttpGet]
        public async Task<IActionResult> GetPetInteractionBonusRules()
        {
            try
            {
                var rules = await _petRulesService.GetAllInteractionRulesAsync();
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新互動獎勵規則
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetInteractionBonusRules(List<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules> rules)
        {
            try
            {
                if (rules == null || !rules.Any())
                {
                    return Json(new { success = false, message = "互動狀態增益規則不能為空" });
                }

                // 更新每個規則
                foreach (var rule in rules)
                {
                    await _petRulesService.UpdateInteractionRuleAsync(rule);
                }

                return Json(new { success = true, message = "寵物互動狀態增益規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取換色成本
        [HttpGet]
        public async Task<IActionResult> GetPetColorChangeCost()
        {
            try
            {
                // 使用預設等級1的成本
                var cost = await _petRulesService.GetSkinColorCostForLevelAsync(1);
                return Json(new { success = true, data = new { pointsRequired = cost } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新換色成本
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetColorChangeCost(int pointsRequired)
        {
            try
            {
                if (pointsRequired < 0)
                {
                    return Json(new { success = false, message = "所需點數不能為負數" });
                }

                // 更新等級1的換色成本設定
                var settings = await _petRulesService.GetAllSkinColorSettingsAsync();
                var setting = settings.FirstOrDefault(s => s.RequiredLevel == 1);

                if (setting != null)
                {
                    setting.PointCost = pointsRequired;
                    await _petRulesService.UpdateSkinColorSettingAsync(setting);
                }

                return Json(new { success = true, message = "寵物換色所需點數設定成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取換背景成本
        [HttpGet]
        public async Task<IActionResult> GetPetBackgroundChangeCost()
        {
            try
            {
                var cost = await _petRulesService.GetBackgroundCostForLevelAsync(1);
                return Json(new { success = true, data = new { pointsRequired = cost } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新換背景成本
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetBackgroundChangeCost(int pointsRequired)
        {
            try
            {
                if (pointsRequired < 0)
                {
                    return Json(new { success = false, message = "所需點數不能為負數" });
                }

                var settings = await _petRulesService.GetAllBackgroundSettingsAsync();
                var setting = settings.FirstOrDefault(s => s.RequiredLevel == 1);

                if (setting != null)
                {
                    setting.PointCost = pointsRequired;
                    await _petRulesService.UpdateBackgroundSettingAsync(setting);
                }

                return Json(new { success = true, message = "寵物換背景所需點數設定成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: AdminPet/ColorChangeHistory
        /// <summary>
        /// 取得寵物膚色變更歷史記錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ColorChangeHistory(PetColorChangeHistoryQueryModel query)
        {
            try
            {
                // 驗證查詢參數
                if (query.PageNumber < 1)
                {
                    query.PageNumber = 1;
                }

                if (query.PageSize < 1 || query.PageSize > 100)
                {
                    query.PageSize = 20;
                }

                // 如果有結束日期但沒有開始日期，設定預設開始日期為30天前
                if (query.EndDate.HasValue && !query.StartDate.HasValue)
                {
                    query.StartDate = query.EndDate.Value.AddDays(-30);
                }

                // 呼叫服務層取得歷史記錄
                var result = await _petQueryService.GetColorChangeHistoryAsync(query);

                // 將查詢參數傳遞到 ViewBag 以便視圖使用
                ViewBag.UserId = query.UserId;
                ViewBag.PetId = query.PetId;
                ViewBag.StartDate = query.StartDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = query.EndDate?.ToString("yyyy-MM-dd");
                ViewBag.SortOrder = query.SortOrder;
                ViewBag.PageNumber = query.PageNumber;
                ViewBag.PageSize = query.PageSize;

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入膚色變更歷史記錄時發生錯誤：{ex.Message}";
                return View(new PetColorChangeHistoryPagedResult
                {
                    Items = new List<ColorChangeHistoryItemViewModel>(),
                    TotalCount = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalPages = 0
                });
            }
        }

        // GET: AdminPet/BackgroundChangeHistory
        /// <summary>
        /// 取得寵物背景變更歷史記錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BackgroundChangeHistory(PetBackgroundChangeHistoryQueryModel query)
        {
            try
            {
                // 驗證查詢參數
                if (query.PageNumber < 1)
                {
                    query.PageNumber = 1;
                }

                if (query.PageSize < 1 || query.PageSize > 100)
                {
                    query.PageSize = 20;
                }

                // 如果有結束日期但沒有開始日期，設定預設開始日期為30天前
                if (query.EndDate.HasValue && !query.StartDate.HasValue)
                {
                    query.StartDate = query.EndDate.Value.AddDays(-30);
                }

                // 呼叫服務層取得歷史記錄
                var result = await _petQueryService.GetBackgroundChangeHistoryAsync(query);

                // 將查詢參數傳遞到 ViewBag 以便視圖使用
                ViewBag.UserId = query.UserId;
                ViewBag.PetId = query.PetId;
                ViewBag.StartDate = query.StartDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = query.EndDate?.ToString("yyyy-MM-dd");
                ViewBag.SortOrder = query.SortOrder;
                ViewBag.PageNumber = query.PageNumber;
                ViewBag.PageSize = query.PageSize;

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入背景變更歷史記錄時發生錯誤：{ex.Message}";
                return View(new PetBackgroundChangeHistoryPagedResult
                {
                    Items = new List<BackgroundChangeHistoryItemViewModel>(),
                    TotalCount = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalPages = 0
                });
            }
        }

        // GET: AdminPet/QueryPets
        /// <summary>
        /// 查詢寵物清單（支援篩選、排序、分頁）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryPets(PetAdminListQueryModel query)
        {
            try
            {
                // 驗證並標準化查詢參數
                if (query.PageNumber < 1)
                {
                    query.PageNumber = 1;
                }

                if (query.PageSize < 1 || query.PageSize > 100)
                {
                    query.PageSize = 20;
                }

                // 驗證等級範圍
                if (query.MinLevel.HasValue && query.MinLevel.Value < 1)
                {
                    query.MinLevel = 1;
                }

                if (query.MaxLevel.HasValue && query.MaxLevel.Value > 100)
                {
                    query.MaxLevel = 100;
                }

                // 確保最小等級不大於最大等級
                if (query.MinLevel.HasValue && query.MaxLevel.HasValue && query.MinLevel > query.MaxLevel)
                {
                    var temp = query.MinLevel;
                    query.MinLevel = query.MaxLevel;
                    query.MaxLevel = temp;
                }

                // 驗證經驗值範圍
                if (query.MinExperience.HasValue && query.MinExperience.Value < 0)
                {
                    query.MinExperience = 0;
                }

                if (query.MaxExperience.HasValue && query.MaxExperience.Value < 0)
                {
                    query.MaxExperience = 0;
                }

                // 確保最小經驗值不大於最大經驗值
                if (query.MinExperience.HasValue && query.MaxExperience.HasValue && query.MinExperience > query.MaxExperience)
                {
                    var temp = query.MinExperience;
                    query.MinExperience = query.MaxExperience;
                    query.MaxExperience = temp;
                }

                // 標準化排序參數
                if (string.IsNullOrWhiteSpace(query.SortBy))
                {
                    query.SortBy = "name";
                }

                if (string.IsNullOrWhiteSpace(query.SortOrder))
                {
                    query.SortOrder = "asc";
                }

                // 呼叫服務層執行查詢
                var result = await _petQueryService.GetPetListAsync(query);

                // 將查詢參數傳遞到 ViewBag 以便視圖使用
                ViewBag.UserId = query.UserId;
                ViewBag.PetName = query.PetName;
                ViewBag.MinLevel = query.MinLevel;
                ViewBag.MaxLevel = query.MaxLevel;
                ViewBag.MinExperience = query.MinExperience;
                ViewBag.MaxExperience = query.MaxExperience;
                ViewBag.SkinColor = query.SkinColor;
                ViewBag.BackgroundColor = query.BackgroundColor;
                ViewBag.SearchTerm = query.SearchTerm;
                ViewBag.SortBy = query.SortBy;
                ViewBag.SortOrder = query.SortOrder;
                ViewBag.PageNumber = query.PageNumber;
                ViewBag.PageSize = query.PageSize;

                // 計算統計資訊
                if (result.Items.Any())
                {
                    ViewBag.TotalPets = result.TotalCount;
                    ViewBag.HealthyPets = result.Items.Count(p => p.Health >= 80);
                    ViewBag.AverageLevel = result.Items.Average(p => (double)p.Level);
                    ViewBag.MaxPetLevel = result.Items.Max(p => p.Level);
                }
                else
                {
                    ViewBag.TotalPets = 0;
                    ViewBag.HealthyPets = 0;
                    ViewBag.AverageLevel = 0;
                    ViewBag.MaxPetLevel = 0;
                }

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢寵物資料時發生錯誤：{ex.Message}";
                return View(new PetAdminListPagedResult
                {
                    Items = new List<PetAdminListItemViewModel>(),
                    TotalCount = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalPages = 0
                });
            }
        }

        // GET: AdminPet/PetDetail/5
        /// <summary>
        /// 顯示寵物詳細資料
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PetDetail(int id)
        {
            try
            {
                // 驗證參數
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "無效的寵物 ID";
                    return RedirectToAction(nameof(QueryPets));
                }

                // 呼叫服務層取得寵物詳細資料
                var petDetail = await _petQueryService.GetPetDetailAsync(id);

                // 檢查寵物是否存在
                if (petDetail == null)
                {
                    TempData["ErrorMessage"] = $"找不到 ID 為 {id} 的寵物";
                    return RedirectToAction(nameof(QueryPets));
                }

                // 將寵物 ID 傳遞到 ViewBag 以便視圖使用
                ViewBag.PetId = id;

                return View(petDetail);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入寵物詳細資料時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(QueryPets));
            }
        }

        // GET: AdminPet/IndividualSettings/5
        /// <summary>
        /// 顯示個別寵物設定編輯表單（名稱、膚色、背景）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> IndividualSettings(int? petId)
        {
            if (!petId.HasValue || petId <= 0)
            {
                // 顯示寵物選擇頁面而不是重定向
                try
                {
                    var pets = await _petService.GetAllPetsAsync();
                    return View("PetSelection", pets);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"載入寵物列表時發生錯誤：{ex.Message}";
                    return RedirectToAction(nameof(QueryPets));
                }
            }

            var pet = await _petService.GetPetByIdAsync(petId.Value);
            if (pet == null)
            {
                TempData["ErrorMessage"] = $"找不到寵物 ID: {petId.Value}";
                return RedirectToAction(nameof(Index));
            }

            var model = new PetUpdateModel
            {
                PetId = pet.PetId,
                PetName = pet.PetName,
                SkinColor = pet.SkinColor,
                BackgroundColor = pet.BackgroundColor,
                Level = pet.Level,
                Experience = pet.Experience,
                Hunger = pet.Hunger,
                Mood = pet.Mood,
                Stamina = pet.Stamina,
                Cleanliness = pet.Cleanliness,
                Health = pet.Health
            };

            ViewBag.PetId = petId;
            ViewBag.UserId = pet.UserId;
            ViewBag.CurrentPetName = pet.PetName;

            return View(model);
        }

        // POST: AdminPet/EditPet
        /// <summary>
        /// 儲存個別寵物變更（名稱、膚色、背景）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPet(PetUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "輸入資料驗證失敗，請檢查表單內容";
                return RedirectToAction(nameof(IndividualSettings), new { petId = model.PetId });
            }

            var pet = await _petService.GetPetByIdAsync(model.PetId);
            if (pet == null)
            {
                TempData["ErrorMessage"] = $"找不到寵物 ID: {model.PetId}";
                return RedirectToAction(nameof(Index));
            }

            var operatorId = 0;

            var hasNameChange = !string.IsNullOrWhiteSpace(model.PetName) && model.PetName != pet.PetName;
            var hasSkinColorChange = !string.IsNullOrWhiteSpace(model.SkinColor) && model.SkinColor != pet.SkinColor;
            var hasBackgroundColorChange = !string.IsNullOrWhiteSpace(model.BackgroundColor) && model.BackgroundColor != pet.BackgroundColor;

            var successMessages = new List<string>();
            var errorMessages = new List<string>();

            if (hasNameChange)
            {
                var nameInputModel = new PetBasicInfoInputModel
                {
                    PetId = model.PetId,
                    PetName = model.PetName,
                    UserId = pet.UserId
                };

                var nameResult = await _petMutationService.UpdatePetBasicInfoAsync(model.PetId, nameInputModel, operatorId);

                if (nameResult.Success)
                {
                    successMessages.Add($"寵物名稱已更新為「{model.PetName}」");
                }
                else
                {
                    errorMessages.Add($"名稱更新失敗：{nameResult.Message}");
                }
            }

            if (hasSkinColorChange || hasBackgroundColorChange)
            {
                var appearanceInputModel = new PetAppearanceInputModel
                {
                    PetId = model.PetId,
                    SkinColor = hasSkinColorChange ? model.SkinColor : pet.SkinColor,
                    BackgroundColor = hasBackgroundColorChange ? model.BackgroundColor : pet.BackgroundColor,
                    PointsCost = 0,
                    Description = "管理員調整寵物外觀"
                };

                var appearanceResult = await _petMutationService.UpdatePetAppearanceAsync(model.PetId, appearanceInputModel, operatorId);

                if (appearanceResult.Success)
                {
                    if (hasSkinColorChange && hasBackgroundColorChange)
                    {
                        successMessages.Add($"膚色和背景已更新");
                    }
                    else if (hasSkinColorChange)
                    {
                        successMessages.Add($"膚色已更新為 {model.SkinColor}");
                    }
                    else if (hasBackgroundColorChange)
                    {
                        successMessages.Add($"背景已更新為 {model.BackgroundColor}");
                    }
                }
                else
                {
                    errorMessages.Add($"外觀更新失敗：{appearanceResult.Message}");
                }
            }

            if (!hasNameChange && !hasSkinColorChange && !hasBackgroundColorChange)
            {
                TempData["InfoMessage"] = "沒有變更任何資料";
                return RedirectToAction(nameof(IndividualSettings), new { petId = model.PetId });
            }

            if (successMessages.Any())
            {
                TempData["SuccessMessage"] = string.Join("；", successMessages);
            }

            if (errorMessages.Any())
            {
                TempData["ErrorMessage"] = string.Join("；", errorMessages);
            }

            return RedirectToAction(nameof(IndividualSettings), new { petId = model.PetId });
        }

        // GET: AdminPet/SystemRules
        /// <summary>
        /// 顯示寵物系統規則設定頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SystemRules()
        {
            try
            {
                // 從 SystemSettings 讀取現有設定
                var model = new PetSystemRulesInputModel
                {
                    LevelUpExpBase = await GetSystemSettingIntAsync("Pet.LevelUpExpBase", 100),
                    LevelUpFormula = await GetSystemSettingAsync("Pet.LevelUpFormula", "Level 1-10: 40×level+60; 11-100: 0.8×level²+380; ≥101: 285.69×1.06^level"),
                    FeedBonus = await GetSystemSettingIntAsync("Pet.FeedBonus", 10),
                    CleanBonus = await GetSystemSettingIntAsync("Pet.CleanBonus", 10),
                    PlayBonus = await GetSystemSettingIntAsync("Pet.PlayBonus", 10),
                    SleepBonus = await GetSystemSettingIntAsync("Pet.SleepBonus", 10),
                    ExpBonus = await GetSystemSettingIntAsync("Pet.ExpBonus", 1),
                    ColorChangePoints = await GetSystemSettingIntAsync("Pet.ColorChangePoints", 2000),
                    BackgroundChangePoints = await GetSystemSettingIntAsync("Pet.BackgroundChangePoints", 1000),
                    AvailableColors = await GetSystemSettingAsync("Pet.AvailableColors", "#FFFFFF,#FFD700,#FF6B6B,#4ECDC4,#45B7D1,#FFA07A,#98D8C8,#F7DC6F,#BB8FCE,#85C1E2"),
                    AvailableBackgrounds = await GetSystemSettingAsync("Pet.AvailableBackgrounds", "#FFFFFF,#F0F0F0,#E8F5E9,#E3F2FD,#FFF3E0,#FCE4EC,#F3E5F5,#E0F2F1,#FFF8E1,#EFEBE9")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入系統規則設定時發生錯誤：{ex.Message}";
                return View(new PetSystemRulesInputModel());
            }
        }

        // POST: AdminPet/UpdateSystemRules
        /// <summary>
        /// 更新寵物系統規則設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSystemRules(PetSystemRulesInputModel model)
        {
            try
            {
                // 驗證 ModelState
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "輸入資料驗證失敗，請檢查所有欄位";
                    return View("SystemRules", model);
                }

                // 獲取當前管理員ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    TempData["ErrorMessage"] = "無法識別管理員身份，請重新登入";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // 呼叫 PetMutationService 更新系統規則
                var result = await _petMutationService.UpdatePetSystemRulesAsync(model, managerId.Value);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;

                    // 記錄操作日誌
                    await LogOperationAsync(
                        "UpdatePetSystemRules",
                        $"更新寵物系統規則：升級基礎經驗={model.LevelUpExpBase}, 餵食獎勵={model.FeedBonus}, 清潔獎勵={model.CleanBonus}, 玩耍獎勵={model.PlayBonus}, 哄睡獎勵={model.SleepBonus}, 經驗值倍數={model.ExpBonus}, 換色點數={model.ColorChangePoints}, 換背景點數={model.BackgroundChangePoints}"
                    );

                    return RedirectToAction(nameof(SystemRules));
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View("SystemRules", model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"更新系統規則時發生錯誤：{ex.Message}";
                return View("SystemRules", model);
            }
        }

        // ==================== AJAX 端點 (用於 QueryPets 頁面) ====================

        /// <summary>
        /// AJAX: 搜尋寵物（用於 QueryPets 頁面的動態搜尋）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchPets(int? userId, string userName, string petName, string petType,
            int? minLevel, int? maxLevel, string status, string color)
        {
            try
            {
                // 構建查詢模型
                var query = new PetAdminListQueryModel
                {
                    UserId = userId,
                    SearchTerm = userName,
                    PetName = petName,
                    MinLevel = minLevel,
                    MaxLevel = maxLevel,
                    SkinColor = color,
                    PageNumber = 1,
                    PageSize = 100  // AJAX 搜尋返回更多結果
                };

                // 執行查詢
                var result = await _petQueryService.GetPetListAsync(query);

                // 構建返回數據
                var data = result.Items.Select(p => new
                {
                    petId = p.PetId,
                    userId = p.UserId,
                    userName = p.UserName ?? "未知",
                    petName = p.PetName,
                    petType = "slime",  // Pet entity 沒有 PetType，使用預設值
                    level = p.Level,
                    status = DetermineStatus(p.Health, p.Hunger, p.Mood),
                    color = p.SkinColor ?? "default"
                }).ToList();

                // 計算統計資訊
                var statistics = new
                {
                    totalPets = result.TotalCount,
                    activePets = result.Items.Count(p => p.Health >= 80 && p.Hunger >= 30),
                    avgLevel = result.Items.Any() ? result.Items.Average(p => (double)p.Level) : 0,
                    maxLevel = result.Items.Any() ? result.Items.Max(p => p.Level) : 0
                };

                return Json(new { success = true, data, statistics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"搜尋失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// AJAX: 獲取寵物詳細資訊
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPetDetail(int petId)
        {
            try
            {
                if (petId <= 0)
                {
                    return Json(new { success = false, message = "無效的寵物 ID" });
                }

                var pet = await _petService.GetPetByIdAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到該寵物" });
                }

                // 獲取用戶資訊
                var user = await _context.Users.FindAsync(pet.UserId);

                var data = new
                {
                    userId = pet.UserId,
                    userName = user?.UserName ?? "未知",
                    petName = pet.PetName,
                    petType = "slime",  // Pet entity 沒有 PetType
                    level = pet.Level,
                    hp = pet.Health,
                    attack = 0,  // Pet entity 沒有 Attack 屬性
                    defense = 0,  // Pet entity 沒有 Defense 屬性
                    speed = 0,  // Pet entity 沒有 Speed 屬性
                    status = DetermineStatus(pet.Health, pet.Hunger, pet.Mood),
                    color = pet.SkinColor,
                    size = "medium",  // Pet entity 沒有 Size 屬性
                    specialEffects = pet.BackgroundColor,
                    createTime = pet.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    lastUpdate = pet.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    lastFeed = pet.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"獲取寵物詳情失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// AJAX: 調整寵物（主要用於調整等級）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustPet(int petId, int newLevel)
        {
            try
            {
                if (petId <= 0)
                {
                    return Json(new { success = false, message = "無效的寵物 ID" });
                }

                if (newLevel < 1 || newLevel > 100)
                {
                    return Json(new { success = false, message = "等級必須在 1-100 之間" });
                }

                var pet = await _petService.GetPetByIdAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到該寵物" });
                }

                // 獲取當前管理員ID
                var managerId = GetCurrentManagerId() ?? 0;

                // 使用 PetMutationService 調整寵物等級
                var statsInput = new PetStatsInputModel
                {
                    PetId = petId,
                    Level = newLevel,
                    Experience = 0,  // 設置為該等級的起始經驗值
                    Hunger = pet.Hunger,
                    Happiness = pet.Mood,  // Map Mood to Happiness
                    Mood = pet.Mood,
                    Stamina = pet.Stamina,
                    Cleanliness = pet.Cleanliness,
                    Health = pet.Health
                };

                var result = await _petMutationService.UpdatePetStatsAsync(petId, statsInput, managerId);

                if (result.Success)
                {
                    // 記錄操作日誌
                    await LogOperationAsync(
                        "AdjustPet",
                        $"調整寵物 {pet.PetName} (ID:{petId}) 等級從 {pet.Level} 到 {newLevel}"
                    );

                    return Json(new { success = true, message = $"寵物等級已調整為 {newLevel}" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"調整寵物失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// AJAX: 刪除寵物
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePet(int petId)
        {
            try
            {
                if (petId <= 0)
                {
                    return Json(new { success = false, message = "無效的寵物 ID" });
                }

                var pet = await _petService.GetPetByIdAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到該寵物" });
                }

                var petName = pet.PetName;
                var userId = pet.UserId;

                // 刪除寵物
                var result = await _petService.DeletePetAsync(petId);

                if (result)
                {
                    // 記錄操作日誌
                    await LogOperationAsync(
                        "DeletePet",
                        $"刪除寵物 {petName} (ID:{petId}, UserId:{userId})"
                    );

                    return Json(new { success = true, message = "寵物已成功刪除" });
                }
                else
                {
                    return Json(new { success = false, message = "刪除寵物失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"刪除寵物失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// 匯出寵物資料為 CSV
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportPets(int? userId, string userName, string petName, string petType,
            int? minLevel, int? maxLevel, string status, string color)
        {
            try
            {
                // 構建查詢模型
                var query = new PetAdminListQueryModel
                {
                    UserId = userId,
                    SearchTerm = userName,
                    PetName = petName,
                    MinLevel = minLevel,
                    MaxLevel = maxLevel,
                    SkinColor = color,
                    PageNumber = 1,
                    PageSize = 10000  // 匯出所有符合條件的記錄
                };

                // 執行查詢
                var result = await _petQueryService.GetPetListAsync(query);

                // 構建 CSV 內容
                var csvBuilder = new System.Text.StringBuilder();
                csvBuilder.AppendLine("會員ID,會員名稱,寵物名稱,等級,經驗值,生命值,飢餓度,心情,體力,清潔度,膚色,背景顏色");

                foreach (var pet in result.Items)
                {
                    csvBuilder.AppendLine($"{pet.UserId},{pet.UserName ?? "未知"},{pet.PetName},{pet.Level},{pet.Experience},{pet.Health},{pet.Hunger},{pet.Mood},{pet.Stamina},{pet.Cleanliness},{pet.SkinColor},{pet.BackgroundColor}");
                }

                var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var fileName = $"Pets_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"匯出失敗：{ex.Message}";
                return RedirectToAction(nameof(QueryPets));
            }
        }

        // ==================== 私有輔助方法 ====================

        /// <summary>
        /// 根據寵物屬性判斷狀態
        /// </summary>
        private string DetermineStatus(int health, int hunger, int mood)
        {
            if (health <= 0) return "死亡";
            if (health < 30 || hunger < 20 || mood < 20) return "生病";
            if (hunger < 50) return "睡眠";
            return "活躍";
        }

        /// <summary>
        /// 從系統設定讀取整數值
        /// </summary>
        private async Task<int> GetSystemSettingIntAsync(string key, int defaultValue)
        {
            try
            {
                var value = await GetSystemSettingAsync(key, defaultValue.ToString());
                return int.TryParse(value, out int result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}

