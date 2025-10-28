// ===== CouponTypesController.cs =====
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Controllers
{

	[Area("MiniGame")]
	public class CouponTypesController : Controller
	{
		private readonly GameSpacedatabaseContext _context;

		public CouponTypesController(GameSpacedatabaseContext context)
		{
			_context = context;
		}

		// GET: /CouponTypes
		// 支援以 q 搜索名稱；預設依 ValidTo 由近到遠，再以 CouponTypeId 排序
		public async Task<IActionResult> Index(string? q = null)
		{
			var query = _context.CouponTypes.AsQueryable();

			if (!string.IsNullOrWhiteSpace(q))
			{
				var kw = q.Trim();
				query = query.Where(x => x.Name.Contains(kw));
			}

			var list = await query
				.OrderBy(x => x.ValidTo)
				.ThenBy(x => x.CouponTypeId)
				.ToListAsync();

			ViewBag.Query = q;
			return View(list);
		}

		// GET: /CouponTypes/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.CouponTypes
				.FirstOrDefaultAsync(m => m.CouponTypeId == id);
			if (entity == null) return NotFound();

			return View(entity);
		}

		// GET: /CouponTypes/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: /CouponTypes/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("CouponTypeId,Name,DiscountType,DiscountValue,MinSpend,ValidFrom,ValidTo,PointsCost,Description")] CouponType model)
		{
			Normalize(model);
			ValidateBusinessRules(model);

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			_context.Add(model);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		// GET: /CouponTypes/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.CouponTypes.FindAsync(id);
			if (entity == null) return NotFound();

			return View(entity);
		}

		// POST: /CouponTypes/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("CouponTypeId,Name,DiscountType,DiscountValue,MinSpend,ValidFrom,ValidTo,PointsCost,Description")] CouponType model)
		{
			if (id != model.CouponTypeId) return NotFound();

			Normalize(model);
			ValidateBusinessRules(model);

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				_context.Update(model);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!await CouponTypeExists(model.CouponTypeId))
					return NotFound();
				else
					throw;
			}

			return RedirectToAction(nameof(Index));
		}

		// GET: /CouponTypes/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.CouponTypes
				.FirstOrDefaultAsync(m => m.CouponTypeId == id);
			if (entity == null) return NotFound();

			return View(entity);
		}

		// POST: /CouponTypes/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var entity = await _context.CouponTypes.FindAsync(id);
			if (entity != null)
			{
				_context.CouponTypes.Remove(entity);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		// ===== 私有輔助 =====

		// 規格化欄位：去除前後空白、標準化 DiscountType
		private static void Normalize(CouponType m)
		{
			m.Name = (m.Name ?? string.Empty).Trim();
			m.DiscountType = (m.DiscountType ?? string.Empty).Trim();
			if (string.Equals(m.DiscountType, "amount", StringComparison.OrdinalIgnoreCase))
				m.DiscountType = "Amount";
			else if (string.Equals(m.DiscountType, "percent", StringComparison.OrdinalIgnoreCase))
				m.DiscountType = "Percent";
		}

		// 基本商業規則驗證（不修改你的 Model 類，直接在控制器做）
		private void ValidateBusinessRules(CouponType m)
		{
			// Name
			if (string.IsNullOrWhiteSpace(m.Name))
				ModelState.AddModelError(nameof(m.Name), "名稱不可空白。");

			// DiscountType
			var isAmount = string.Equals(m.DiscountType, "Amount", StringComparison.OrdinalIgnoreCase);
			var isPercent = string.Equals(m.DiscountType, "Percent", StringComparison.OrdinalIgnoreCase);
			if (!isAmount && !isPercent)
				ModelState.AddModelError(nameof(m.DiscountType), "折扣類型僅支援 Amount 或 Percent。");

			// DiscountValue
			if (isPercent)
			{
				if (!(m.DiscountValue > 0m && m.DiscountValue < 1m))
					ModelState.AddModelError(nameof(m.DiscountValue), "Percent 類型時，DiscountValue 必須介於 0 與 1 之間（例如 0.15 代表 85 折）。");
			}
			else if (isAmount)
			{
				if (m.DiscountValue < 0m)
					ModelState.AddModelError(nameof(m.DiscountValue), "Amount 類型時，DiscountValue 不可小於 0。");
			}

			// MinSpend / PointsCost
			if (m.MinSpend < 0m)
				ModelState.AddModelError(nameof(m.MinSpend), "最低消費不可小於 0。");
			if (m.PointsCost < 0)
				ModelState.AddModelError(nameof(m.PointsCost), "點數成本不可小於 0。");

			// 日期
			if (m.ValidTo < m.ValidFrom)
				ModelState.AddModelError(nameof(m.ValidTo), "有效迄日不可早於有效起日。");
		}

		private async Task<bool> CouponTypeExists(int id)
		{
			return await _context.CouponTypes.AnyAsync(e => e.CouponTypeId == id);
		}
	}
}
