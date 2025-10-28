using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminMiniGameController : MiniGameBaseController
    {
        private readonly IGameQueryService _gameQueryService;
        private readonly IGameMutationService _gameMutationService;

        public AdminMiniGameController(
            GameSpacedatabaseContext context,
            IGameQueryService gameQueryService,
            IGameMutationService gameMutationService)
            : base(context)
        {
            _gameQueryService = gameQueryService;
            _gameMutationService = gameMutationService;
        }

        // GET: AdminMiniGame
        // Note: MiniGame entity represents game play records, not game definitions
        public async Task<IActionResult> Index(string searchTerm = "", string result = "", string sortBy = "recent", int page = 1, int pageSize = 10)
        {
            var query = _context.MiniGames.Include(g => g.User).AsQueryable();

            // 搜尋功能 - 依使用者ID或結果
            if (!string.IsNullOrEmpty(searchTerm) && int.TryParse(searchTerm, out int userId))
            {
                query = query.Where(g => g.UserId == userId);
            }

            // 遊戲結果篩選
            if (!string.IsNullOrEmpty(result))
            {
                query = query.Where(g => g.Result == result);
            }

            // 排序
            query = sortBy switch
            {
                "level" => query.OrderByDescending(g => g.Level),
                "points" => query.OrderByDescending(g => g.PointsGained),
                "exp" => query.OrderByDescending(g => g.ExpGained),
                _ => query.OrderByDescending(g => g.StartTime)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var miniGames = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminMiniGameIndexViewModel
            {
                MiniGames = new PagedResult<GameSpace.Models.MiniGame>
                {
                    Items = miniGames,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Result = result;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalGames = totalCount;
            ViewBag.CompletedGames = await _context.MiniGames.CountAsync(g => g.Result == "勝利" || g.Result == "Win");
            ViewBag.AbortedGames = await _context.MiniGames.CountAsync(g => g.Aborted);
            ViewBag.TotalPointsAwarded = await _context.MiniGames.SumAsync(g => (int?)g.PointsGained) ?? 0;
            ViewBag.TotalExpAwarded = await _context.MiniGames.SumAsync(g => (int?)g.ExpGained) ?? 0;

            return View(viewModel);
        }

        // GET: AdminMiniGame/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.PlayId == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // GET: AdminMiniGame/Create
        public IActionResult Create()
        {
            ViewBag.Users = _context.Users.OrderBy(u => u.UserId).ToList();
            return View();
        }

        // POST: AdminMiniGame/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminMiniGameCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var miniGame = new GameSpace.Models.MiniGame
                {
                    UserId = model.UserId,
                    PetId = model.PetID,
                    Level = 1,
                    MonsterCount = 0,
                    SpeedMultiplier = 1.0m,
                    Result = model.Result ?? "進行中",
                    ExpGained = model.ExpEarned,
                    ExpGainedTime = DateTime.Now,
                    PointsGained = model.PointsEarned,
                    PointsGainedTime = DateTime.Now,
                    CouponGained = "",
                    CouponGainedTime = DateTime.Now,
                    HungerDelta = 0,
                    MoodDelta = 0,
                    StaminaDelta = 0,
                    CleanlinessDelta = 0,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Aborted = false
                };

                _context.Add(miniGame);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "遊戲記錄建立成功";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = _context.Users.OrderBy(u => u.UserId).ToList();
            return View(model);
        }

        // GET: AdminMiniGame/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames.FindAsync(id);
            if (miniGame == null)
            {
                return NotFound();
            }

            var model = new AdminMiniGameCreateViewModel
            {
                UserId = miniGame.UserId,
                PetID = miniGame.PetId,
                StartTime = miniGame.StartTime,
                EndTime = miniGame.EndTime,
                Result = miniGame.Result,
                PointsEarned = miniGame.PointsGained,
                ExpEarned = miniGame.ExpGained,
                CouponEarned = 0
            };

            ViewBag.Users = _context.Users.OrderBy(u => u.UserId).ToList();
            return View(model);
        }

        // POST: AdminMiniGame/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminMiniGameCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var miniGame = await _context.MiniGames.FindAsync(id);
                    if (miniGame == null)
                    {
                        return NotFound();
                    }

                    miniGame.UserId = model.UserId;
                    miniGame.PetId = model.PetID;
                    miniGame.StartTime = model.StartTime;
                    miniGame.EndTime = model.EndTime;
                    miniGame.Result = model.Result ?? miniGame.Result;
                    miniGame.PointsGained = model.PointsEarned;
                    miniGame.PointsGainedTime = DateTime.Now;
                    miniGame.ExpGained = model.ExpEarned;
                    miniGame.ExpGainedTime = DateTime.Now;

                    _context.Update(miniGame);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "遊戲記錄更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MiniGameExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Users = _context.Users.OrderBy(u => u.UserId).ToList();
            return View(model);
        }

        // GET: AdminMiniGame/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.PlayId == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // POST: AdminMiniGame/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var miniGame = await _context.MiniGames.FindAsync(id);
            if (miniGame != null)
            {
                _context.MiniGames.Remove(miniGame);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "遊戲記錄刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換遊戲狀態 - 改為標記遊戲為中止
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var miniGame = await _context.MiniGames.FindAsync(id);
            if (miniGame != null)
            {
                miniGame.Aborted = !miniGame.Aborted;
                if (miniGame.Aborted && miniGame.EndTime == null)
                {
                    miniGame.EndTime = DateTime.Now;
                }
                _context.Update(miniGame);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isAborted = miniGame.Aborted });
            }

            return Json(new { success = false });
        }

        // 獲取遊戲統計數據
        [HttpGet]
        public async Task<IActionResult> GetGameStats()
        {
            var stats = new
            {
                total = await _context.MiniGames.CountAsync(),
                completed = await _context.MiniGames.CountAsync(g => g.Result == "勝利" || g.Result == "Win"),
                aborted = await _context.MiniGames.CountAsync(g => g.Aborted),
                inProgress = await _context.MiniGames.CountAsync(g => g.EndTime == null && !g.Aborted),
                totalPointsAwarded = await _context.MiniGames.SumAsync(g => (int?)g.PointsGained) ?? 0,
                totalExpAwarded = await _context.MiniGames.SumAsync(g => (int?)g.ExpGained) ?? 0
            };

            return Json(stats);
        }

        // 獲取遊戲結果分佈
        [HttpGet]
        public async Task<IActionResult> GetGameTypeDistribution()
        {
            var distribution = await _context.MiniGames
                .GroupBy(g => g.Result)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToListAsync();

            return Json(distribution);
        }

        // 獲取遊戲遊玩統計 (前10名使用者)
        [HttpGet]
        public async Task<IActionResult> GetGamePlayStats()
        {
            var stats = await _context.MiniGames
                .GroupBy(g => g.UserId)
                .Select(g => new
                {
                    userId = g.Key,
                    userName = _context.Users.Where(u => u.UserId == g.Key).Select(u => u.UserName).FirstOrDefault(),
                    playCount = g.Count(),
                    totalPoints = g.Sum(x => x.PointsGained),
                    totalExp = g.Sum(x => x.ExpGained),
                    avgLevel = g.Average(x => (double)x.Level)
                })
                .OrderByDescending(g => g.playCount)
                .Take(10)
                .ToListAsync();

            return Json(stats);
        }

        // 獲取遊戲獎勵統計
        [HttpGet]
        public async Task<IActionResult> GetGameRevenueStats()
        {
            var stats = await _context.MiniGames
                .Where(g => g.EndTime != null)
                .GroupBy(g => g.EndTime.Value.Date)
                .Select(g => new
                {
                    date = g.Key,
                    playCount = g.Count(),
                    totalPoints = g.Sum(x => x.PointsGained),
                    totalExp = g.Sum(x => x.ExpGained),
                    completedGames = g.Count(x => x.Result == "勝利" || x.Result == "Win")
                })
                .OrderByDescending(g => g.date)
                .Take(30)
                .ToListAsync();

            return Json(stats);
        }

        // ==================== Game Rules Configuration ====================

        /// <summary>
        /// GET: AdminMiniGame/GameRules
        /// Display game rules configuration page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GameRules()
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    TempData["ErrorMessage"] = "您沒有權限查看遊戲規則設定";
                    return RedirectToAction(nameof(Index));
                }

                // Load current game rules
                var gameRules = await _gameQueryService.GetGameRulesAsync();

                // Load statistics - 暫時註解掉可能有問題的部分
                // var stats = await _gameQueryService.GetGameStatisticsAsync();

                // Pass additional data to view
                // ViewBag.Statistics = stats;
                ViewBag.TotalGamesPlayed = gameRules.TotalGamesPlayed;
                ViewBag.TodayGamesPlayed = gameRules.TodayGamesPlayed;

                return View(gameRules);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入遊戲規則時發生錯誤: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateGameRules
        /// Save game rules configuration
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGameRules(GameRulesInputModel model)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    TempData["ErrorMessage"] = "您沒有權限修改遊戲規則";
                    return RedirectToAction(nameof(GameRules));
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var gameRules = await _gameQueryService.GetGameRulesAsync();
                    var stats = await _gameQueryService.GetGameStatisticsAsync();
                    ViewBag.Statistics = stats;
                    TempData["ErrorMessage"] = "表單資料驗證失敗，請檢查輸入內容";
                    return View("GameRules", gameRules);
                }

                // Validate duration range
                if (model.MinDurationSeconds.HasValue && model.MaxDurationSeconds.HasValue)
                {
                    if (model.MinDurationSeconds.Value > model.MaxDurationSeconds.Value)
                    {
                        ModelState.AddModelError("MinDurationSeconds", "最小時長不能大於最大時長");
                        var gameRules = await _gameQueryService.GetGameRulesAsync();
                        var stats = await _gameQueryService.GetGameStatisticsAsync();
                        ViewBag.Statistics = stats;
                        TempData["ErrorMessage"] = "最小時長不能大於最大時長";
                        return View("GameRules", gameRules);
                    }
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    TempData["ErrorMessage"] = "無法取得管理員資訊";
                    return RedirectToAction(nameof(GameRules));
                }

                // Update game rules
                var (success, message) = await _gameMutationService.UpdateGameRulesAsync(model, managerId.Value);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    await LogOperationAsync("UpdateGameRules", $"更新遊戲規則: {model.GameName}");
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }

                return RedirectToAction(nameof(GameRules));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"更新遊戲規則時發生錯誤: {ex.Message}";
                return RedirectToAction(nameof(GameRules));
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateLevelSettings
        /// Save individual level configuration
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLevelSettings(LevelSettingsInputModel model)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    return Json(new { success = false, message = "您沒有權限修改關卡設定" });
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = $"驗證失敗: {string.Join(", ", errors)}" });
                }

                // Validate level settings
                var (isValid, errorMessage) = _gameMutationService.ValidateLevelSettings(
                    model.Level,
                    model.MonsterCount,
                    model.SpeedMultiplier,
                    model.WinPointsReward,
                    model.WinExpReward
                );

                if (!isValid)
                {
                    return Json(new { success = false, message = errorMessage });
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    return Json(new { success = false, message = "無法取得管理員資訊" });
                }

                // Update level settings
                var (success, message) = await _gameMutationService.UpdateLevelSettingsAsync(model, managerId.Value);

                if (success)
                {
                    await LogOperationAsync("UpdateLevelSettings", $"更新第 {model.Level} 關設定");
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新關卡設定時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateDailyLimit
        /// Update daily play limit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDailyLimit(DailyLimitInputModel model)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    return Json(new { success = false, message = "您沒有權限修改每日次數限制" });
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = $"驗證失敗: {string.Join(", ", errors)}" });
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    return Json(new { success = false, message = "無法取得管理員資訊" });
                }

                // Update daily limit
                var (success, message) = await _gameMutationService.UpdateDailyLimitAsync(model, managerId.Value);

                if (success)
                {
                    await LogOperationAsync("UpdateDailyLimit", $"更新每日次數限制為 {model.MaxPlaysPerDay} 次");
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新每日次數限制時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: AdminMiniGame/QueryRecords
        /// Alias for ViewGameRecords for backward compatibility
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryRecords(GameRecordQueryModel query)
        {
            return await ViewGameRecords(query);
        }

        /// <summary>
        /// GET: AdminMiniGame/ViewGameRecords
        /// Query and display game records with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewGameRecords(GameRecordQueryModel query)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.View"))
                {
                    TempData["ErrorMessage"] = "您沒有權限查看遊戲紀錄";
                    return RedirectToAction(nameof(Index));
                }

                // Validate date range
                if (query.StartDate.HasValue && query.EndDate.HasValue)
                {
                    if (query.StartDate.Value > query.EndDate.Value)
                    {
                        ModelState.AddModelError("StartDate", "開始日期不能晚於結束日期");
                        TempData["ErrorMessage"] = "開始日期不能晚於結束日期";
                        query.StartDate = null;
                        query.EndDate = null;
                    }
                }

                // Set default values if not provided
                if (query.PageNumber < 1) query.PageNumber = 1;
                if (query.PageSize < 1) query.PageSize = 20;
                if (query.PageSize > 100) query.PageSize = 100;

                // Query game records
                var result = await _gameQueryService.QueryGameRecordsAsync(query);

                // Get statistics for display
                var stats = await _gameQueryService.GetGameStatisticsAsync(query.StartDate, query.EndDate);

                // Pass data to view
                ViewBag.Statistics = stats;
                ViewBag.Query = query;
                ViewBag.TotalRecords = result.TotalCount;
                ViewBag.FilteredRecords = result.Records.Count;

                // Prepare result options for dropdown
                ViewBag.ResultOptions = new List<string> { "Win", "Lose", "Abort" };

                // Prepare level options for dropdown
                ViewBag.LevelOptions = Enumerable.Range(1, 10).ToList();

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢遊戲紀錄時發生錯誤: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== End of Game Rules Configuration ====================

        private bool MiniGameExists(int id)
        {
            return _context.MiniGames.Any(e => e.PlayId == id);
        }
    }
}

