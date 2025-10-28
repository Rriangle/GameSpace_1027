using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.social_hub.Permissions; // IManagerPermissionService
using GameSpace.Areas.social_hub.Services;
using GameSpace.Infrastructure.Login;          // ILoginIdentity
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Controllers
{
	/// <summary>
	/// 穢語清單：列表 / 明細 / 新增 / 編輯 / 刪除 / 重載
	/// 入口 Gate 由 <see cref="SocialHubAuthAttribute"/> 負責。
	/// - 登入來源：<see cref="ILoginIdentity"/>（唯一）
	/// - 權限來源：<see cref="IManagerPermissionService"/>（只取 can_edit_mute_all）
	/// - 規則：
	///   1) 只要是管理員即可進入本頁（[SocialHubAuth] 已檔非管理身分）
	///   2) can_edit_mute_all = true → 能編輯/刪除所有詞
	///      can_edit_mute_all = false → 只能編輯/刪除「自己建立」的詞
	/// </summary>
	[Area("social_hub")]
	[SocialHubAuth]
	public class MutesController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IMuteFilter _muteFilter;
		private readonly ILoginIdentity _login;           // ★ 唯一登入來源
		private readonly IManagerPermissionService _perm; // ★ 權限服務（can_edit_mute_all）

		public MutesController(
			GameSpacedatabaseContext context,
			IMuteFilter muteFilter,
			ILoginIdentity login,
			IManagerPermissionService perm)
		{
			_context = context;
			_muteFilter = muteFilter;
			_login = login;
			_perm = perm;
		}

		/// <summary>
		/// 取目前頁面所需的 UI 身分資料。
		/// isManager：是否為管理員；managerId：管理員ID（非管理員時為 0）
		/// canManage：是否可顯示管理按鈕（這裡＝isManager）
		/// canAdminAll：是否有 can_edit_mute_all → 能編輯所有人的詞
		/// </summary>
		private async Task<(bool isManager, int managerId, bool canManage, bool canAdminAll)> GetUiContextAsync()
		{
			var me = await _login.GetAsync();
			var isMgr = me.IsAuthenticated
						&& string.Equals(me.Kind, "manager", StringComparison.OrdinalIgnoreCase)
						&& me.ManagerId.HasValue;
			var mid = isMgr ? me.ManagerId!.Value : 0;

			// 只要是管理員即可進入/看到管理功能（細項限制另判）
			bool canManage = isMgr;

			// 由 CS_Agent_Permission 取 can_edit_mute_all
			bool canAdminAll = false;
			if (isMgr)
				canAdminAll = await _perm.HasCsEditMuteAllPermissionAsync(mid);

			return (isMgr, mid, canManage, canAdminAll);
		}

		// GET: social_hub/Mutes
		public async Task<IActionResult> Index(int page = 1, string? q = null)
		{
			const int PageSize = 10;

			var ctx = await GetUiContextAsync();
			ViewBag.CanManageMutes = ctx.canManage;
			ViewBag.IsManager = ctx.isManager;
			ViewBag.CanAdminAll = ctx.canAdminAll; // 是否可編輯所有詞
			ViewBag.ManagerId = ctx.managerId;

			// 列表查詢：目前設計為「管理員看全部；非管理員則只看啟用」
			var baseQuery = _context.Mutes.AsNoTracking();
			if (!ctx.canManage)
				baseQuery = baseQuery.Where(m => m.IsActive == true);

			// 搜尋（支援：id:、mid: / mgr:、純數字、關鍵字、啟用/停用）
			var qTrim = (q ?? string.Empty).Trim();
			ViewBag.Q = qTrim;
			if (!string.IsNullOrEmpty(qTrim))
			{
				var qLower = qTrim.ToLowerInvariant();

				if (qLower.StartsWith("id:") && int.TryParse(qLower[3..], out var idv))
				{
					baseQuery = baseQuery.Where(m => m.MuteId == idv);
				}
				else if ((qLower.StartsWith("mid:") || qLower.StartsWith("mgr:")) &&
						  int.TryParse(qLower.Substring(qLower.IndexOf(':') + 1), out var midv))
				{
					baseQuery = baseQuery.Where(m => m.ManagerId.HasValue && m.ManagerId.Value == midv);
				}
				else
				{
					if (qTrim.All(char.IsDigit) && int.TryParse(qTrim, out var num))
					{
						baseQuery = baseQuery.Where(m =>
							m.MuteId == num ||
							(m.ManagerId.HasValue && m.ManagerId.Value == num));
					}
					else
					{
						bool activeTrue = qLower is "啟用" or "enabled" or "enable" or "on" or "true";
						bool activeFalse = qLower is "停用" or "disabled" or "disable" or "off" or "false";

						baseQuery = baseQuery.Where(m =>
							m.Word.Contains(qTrim) ||
							(m.Replacement != null && m.Replacement.Contains(qTrim)) ||
							(activeTrue && m.IsActive == true) ||
							(activeFalse && m.IsActive == false));
					}
				}
			}

			// 分頁
			var totalItems = await baseQuery.CountAsync();
			var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
			page = Math.Clamp(page, 1, totalPages);

			var list = await baseQuery
				.OrderByDescending(m => m.MuteId)
				.Skip((page - 1) * PageSize)
				.Take(PageSize)
				.ToListAsync();

			// 讓列表能顯示管理員名稱
			var ids = list.Where(x => x.ManagerId.HasValue).Select(x => x.ManagerId!.Value).Distinct().ToList();
			var nameMap = new Dictionary<int, string>();
			if (ids.Count > 0)
			{
				var managers = await _context.Set<ManagerDatum>()
					.AsNoTracking()
					.Where(m => ids.Contains(m.ManagerId))
					.ToListAsync();

				foreach (var mgr in managers)
				{
					var name = ExtractManagerName(mgr);
					nameMap[mgr.ManagerId] = string.IsNullOrWhiteSpace(name) ? $"管理員#{mgr.ManagerId}" : name;
				}
			}
			ViewBag.ManagerNames = nameMap;

			// 分頁資訊
			ViewBag.Page = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.TotalItems = totalItems;
			ViewBag.PageSize = PageSize;

			return View(list);
		}

		// GET: social_hub/Mutes/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var ctx = await GetUiContextAsync();
			ViewBag.CanManageMutes = ctx.canManage;
			ViewBag.IsManager = ctx.isManager;
			ViewBag.CanAdminAll = ctx.canAdminAll;
			ViewBag.ManagerId = ctx.managerId;

			var mute = await _context.Mutes.AsNoTracking().FirstOrDefaultAsync(m => m.MuteId == id.Value);
			if (mute == null) return NotFound();

			// 非管理員僅能看啟用
			if (!ctx.canManage && mute.IsActive != true) return NotFound();

			ViewBag.ManagerName = mute.ManagerId.HasValue ? await GetManagerNameAsync(mute.ManagerId.Value) : null;
			return View(mute);
		}

		// GET: social_hub/Mutes/Create
		[RequireManagerPermissions] // 屬性確保：一定是管理員且已登入
		public async Task<IActionResult> Create()
		{
			var ctx = await GetUiContextAsync();
			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = ctx.canAdminAll; // 顯示徽章用途
			ViewBag.ManagerId = ctx.managerId;
			return View();
		}

		// POST: social_hub/Mutes/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireManagerPermissions]
		public async Task<IActionResult> Create([Bind("Word,Replacement,IsActive")] Mute input)
		{
			const int MaxWordLength = 50;
			const int MaxReplacementLength = 50;

			input.Word = (input.Word ?? string.Empty).Trim();
			input.Replacement = (input.Replacement ?? string.Empty).Trim();

			// 基本驗證
			if (string.IsNullOrEmpty(input.Word))
				ModelState.AddModelError("Word", "請輸入詞彙。");
			else if (input.Word.Length > MaxWordLength)
				ModelState.AddModelError("Word", $"詞彙長度不可超過 {MaxWordLength} 個字。");

			if (!string.IsNullOrEmpty(input.Replacement) && input.Replacement.Length > MaxReplacementLength)
				ModelState.AddModelError("Replacement", $"替代字長度不可超過 {MaxReplacementLength} 個字。");

			// 重複詞檢查
			if (ModelState.IsValid)
			{
				var dup = await _context.Mutes.AsNoTracking().AnyAsync(x => x.Word == input.Word);
				if (dup) ModelState.AddModelError("Word", "此詞已存在。");
			}

			var ctx = await GetUiContextAsync();
			if (!ModelState.IsValid)
			{
				ViewBag.CanManageMutes = true;
				ViewBag.IsManager = true;
				ViewBag.CanAdminAll = ctx.canAdminAll;
				ViewBag.ManagerId = ctx.managerId;
				return View(input);
			}

			// 建立者寫入 ManagerId
			var me = await _login.GetAsync();
			var entity = new Mute
			{
				Word = input.Word,
				Replacement = string.IsNullOrWhiteSpace(input.Replacement) ? null : input.Replacement,
				IsActive = input.IsActive,
				CreatedAt = DateTime.UtcNow,
				ManagerId = (me.ManagerId ?? 0) > 0 ? me.ManagerId : null
			};

			_context.Mutes.Add(entity);
			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Toast.Success"] = "已新增詞彙並刷新過濾規則。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}

		// GET: social_hub/Mutes/Edit/5
		[RequireManagerPermissions]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.Mutes.FindAsync(id.Value);
			if (entity == null) return NotFound();

			var ctx = await GetUiContextAsync();
			ViewBag.ManagerName = entity.ManagerId.HasValue ? await GetManagerNameAsync(entity.ManagerId.Value) : null;

			// 非 can_edit_mute_all → 只能編輯自己建立的
			if (!(ctx.canAdminAll || (entity.ManagerId.HasValue && entity.ManagerId.Value == ctx.managerId)))
				return Forbid();

			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = ctx.canAdminAll;
			ViewBag.ManagerId = ctx.managerId;
			return View(entity);
		}

		// POST: social_hub/Mutes/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireManagerPermissions]
		public async Task<IActionResult> Edit(int id, [Bind("MuteId,Word,Replacement,IsActive")] Mute input)
		{
			const int MaxWordLength = 50;
			const int MaxReplacementLength = 50;

			if (id != input.MuteId) return NotFound();

			var entity = await _context.Mutes.FirstOrDefaultAsync(m => m.MuteId == id);
			if (entity == null) return NotFound();

			var ctx = await GetUiContextAsync();

			// 非 can_edit_mute_all → 只能編輯自己建立的
			if (!(ctx.canAdminAll || (entity.ManagerId.HasValue && entity.ManagerId.Value == ctx.managerId)))
				return Forbid();

			input.Word = (input.Word ?? string.Empty).Trim();
			input.Replacement = (input.Replacement ?? string.Empty).Trim();

			// 驗證
			if (string.IsNullOrEmpty(input.Word))
				ModelState.AddModelError("Word", "請輸入詞彙。");
			else if (input.Word.Length > MaxWordLength)
				ModelState.AddModelError("Word", $"詞彙長度不可超過 {MaxWordLength} 個字。");

			if (!string.IsNullOrEmpty(input.Replacement) && input.Replacement.Length > MaxReplacementLength)
				ModelState.AddModelError("Replacement", $"替代字長度不可超過 {MaxReplacementLength} 個字。");

			// 重複詞檢查（排除自己）
			if (ModelState.IsValid)
			{
				var dup = await _context.Mutes.AnyAsync(x => x.MuteId != id && x.Word == input.Word);
				if (dup) ModelState.AddModelError("Word", "此詞已存在。");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.CanManageMutes = true;
				ViewBag.IsManager = true;
				ViewBag.CanAdminAll = ctx.canAdminAll;
				ViewBag.ManagerId = ctx.managerId;
				ViewBag.ManagerName = entity.ManagerId.HasValue ? await GetManagerNameAsync(entity.ManagerId.Value) : null;

				// 回填目前值（避免 View 直接讀取 input）
				entity.Word = input.Word;
				entity.Replacement = string.IsNullOrWhiteSpace(input.Replacement) ? null : input.Replacement;
				entity.IsActive = input.IsActive;
				return View(entity);
			}

			// 實際更新
			entity.Word = input.Word;
			entity.Replacement = string.IsNullOrWhiteSpace(input.Replacement) ? null : input.Replacement;
			entity.IsActive = input.IsActive;

			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Toast.Success"] = "已更新詞彙並刷新過濾規則。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}

		// GET: social_hub/Mutes/Delete/5
		[RequireManagerPermissions]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.Mutes.AsNoTracking().FirstOrDefaultAsync(m => m.MuteId == id.Value);
			if (entity == null) return NotFound();

			var ctx = await GetUiContextAsync();
			ViewBag.ManagerName = entity.ManagerId.HasValue ? await GetManagerNameAsync(entity.ManagerId.Value) : null;

			// 非 can_edit_mute_all → 只能刪除自己建立的
			if (!(ctx.canAdminAll || (entity.ManagerId.HasValue && entity.ManagerId.Value == ctx.managerId)))
				return Forbid();

			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = ctx.canAdminAll;
			ViewBag.ManagerId = ctx.managerId;
			return View(entity);
		}

		// POST: social_hub/Mutes/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[RequireManagerPermissions]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var entity = await _context.Mutes.FirstOrDefaultAsync(m => m.MuteId == id);
			if (entity == null) return RedirectToAction(nameof(Index));

			var ctx = await GetUiContextAsync();

			// 非 can_edit_mute_all → 只能刪除自己建立的
			if (!(ctx.canAdminAll || (entity.ManagerId.HasValue && entity.ManagerId.Value == ctx.managerId)))
				return Forbid();

			_context.Mutes.Remove(entity);
			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Toast.Success"] = "已刪除詞彙並刷新過濾規則。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}

		/// <summary>
		/// 重載詞庫：支援 AJAX 與傳統導頁。
		/// - AJAX：回 200 + { ok:true }，前端可立即顯示成功 Toast（不換頁）
		/// - 傳統表單：TempData Toast + Redirect 到 Index
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireManagerPermissions]
		public async Task<IActionResult> Reload()
		{
			await _muteFilter.RefreshAsync();

			// 由前端在 fetch 加上 X-Requested-With: XMLHttpRequest
			if (Request.Headers.TryGetValue("X-Requested-With", out var x)
				&& string.Equals(x, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
			{
				return Ok(new { ok = true });
			}

			TempData["Toast.Success"] = "已重載詞庫快取。";
			TempData["Toast.AutoHideMs"] = 2000;
			return RedirectToAction(nameof(Index));
		}

		// ===== Helpers =====

		/// <summary>嘗試從 Manager 資料物件取出顯示名稱。</summary>
		private static string ExtractManagerName(object mgr)
		{
			if (mgr == null) return string.Empty;
			var t = mgr.GetType();
			foreach (var prop in new[] { "ManagerName", "Name", "DisplayName", "NickName", "Account", "UserName" })
			{
				var pi = t.GetProperty(prop);
				if (pi != null)
				{
					var val = pi.GetValue(mgr) as string;
					if (!string.IsNullOrWhiteSpace(val)) return val;
				}
			}
			return string.Empty;
		}

		/// <summary>查詢並回傳 Manager 顯示名稱（找不到回 null）。</summary>
		private async Task<string?> GetManagerNameAsync(int managerId)
		{
			var mgr = await _context.Set<ManagerDatum>()
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ManagerId == managerId);

			if (mgr == null) return null;
			var name = ExtractManagerName(mgr);
			return string.IsNullOrWhiteSpace(name) ? $"管理員#{managerId}" : name;
		}
	}
}
