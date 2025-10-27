using GameSpace.Models.ViewModels;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class SoOrderInfoesController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public SoOrderInfoesController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

		// GET: OnlineStore/SoOrderInfoes
		public async Task<IActionResult> Index(DateTime? start, DateTime? end, string? q)
		{
			var orders = _context.SoOrderInfoes.AsNoTracking();

			if (start.HasValue)
				orders = orders.Where(o => o.OrderDate >= start.Value);

			if (end.HasValue)
			{
				var endNext = end.Value.Date.AddDays(1); // 含當天
				orders = orders.Where(o => o.OrderDate < endNext);
			}

			if (!string.IsNullOrWhiteSpace(q))
			{
				if (int.TryParse(q, out var uid))
					orders = orders.Where(o => o.UserId == uid || (o.OrderCode ?? "").Contains(q));
				else
					orders = orders.Where(o => (o.OrderCode ?? "").Contains(q));
			}

			var list = await orders
				.GroupJoin(
					_context.SoShipments.AsNoTracking(),
					o => o.OrderId,
					s => s.OrderId,
					(o, sgrp) => new { o, sgrp }
				)
				.Select(x => new OrderListItemVm
				{
					OrderId = x.o.OrderId,
					OrderCode = x.o.OrderCode,
					UserId = x.o.UserId,
					OrderDate = x.o.OrderDate,
					GrandTotal = x.o.GrandTotal,

					// 取最新一筆出貨狀態（以 UpdatedAt、再以 ShipmentId 倒序）
					ShipmentStatusCode = x.sgrp
						.OrderByDescending(s => s.UpdatedAt)
						.ThenByDescending(s => s.ShipmentId)
						.Select(s => s.Status)
						.FirstOrDefault()
				})
				.OrderByDescending(v => v.OrderDate)
				.ToListAsync();

			ViewData["start"] = start?.ToString("yyyy-MM-dd");
			ViewData["end"] = end?.ToString("yyyy-MM-dd");
			ViewData["q"] = q ?? string.Empty;
			ViewBag.Count = list.Count;

			return View(list);
		}

		// GET: OnlineStore/SoOrderInfoes/Details/5
public async Task<IActionResult> Details(int id)
		{
			// 訂單主檔
			var o = await _context.SoOrderInfoes.AsNoTracking()
				.Where(x => x.OrderId == id)
				.Select(x => new
				{
					x.OrderId,
					x.OrderCode,
					x.UserId,
					x.OrderDate,
					x.Subtotal,
					x.ShippingFee,
					//x.Discount,
					x.GrandTotal
				})
				.FirstOrDefaultAsync();

			if (o == null) return NotFound();

			// 地址快照
			var addr = await _context.SoOrderAddresses.AsNoTracking()
				.Where(a => a.OrderId == id)
				.Select(a => new { a.Recipient, a.Phone, a.Zipcode, a.City, a.Address1, a.Address2 })
				.FirstOrDefaultAsync();

			// 最新出貨狀態
			var shipStatus = await _context.SoShipments.AsNoTracking()
				.Where(s => s.OrderId == id)
				.OrderByDescending(s => s.UpdatedAt).ThenByDescending(s => s.ShipmentId)
				.Select(s => s.Status)
				.FirstOrDefaultAsync();

			// 明細
			// 明細（JOIN 產品表拿名稱；左外連接避免商品已被刪除/下架）
			var items = await (
				from i in _context.SoOrderItems.AsNoTracking()
				join p in _context.SProductInfos.AsNoTracking()  // ← 這個 DbSet 名稱請用你專案實際的，例如 SProductInfoes/SProductInfos
					on i.ProductId equals p.ProductId into gp
				from p in gp.DefaultIfEmpty()
				where i.OrderId == id
				orderby i.LineNo
				select new OrderDetailVm.OrderItemRow
				{
					LineNo = i.LineNo,
					ProductId = i.ProductId,
					ProductName = p != null ? p.ProductName : "(已下架/刪除)", // ← 關鍵
					UnitPrice = i.UnitPrice,
					Quantity = i.Quantity
				}
			).ToListAsync();

			// 組合 VM（若 Subtotal/ShippingFee/Discount 尚未建欄，保留 null 即可）
			var vm = new OrderDetailVm
			{
				OrderId = o.OrderId,
				OrderCode = o.OrderCode,
				UserId = o.UserId,
				OrderDate = o.OrderDate,
				Subtotal = o.Subtotal,
				ShippingFee = o.ShippingFee,
				//Discount = o.Discount,
				GrandTotal = o.GrandTotal,

				Recipient = addr?.Recipient,
				Phone = addr?.Phone,
				AddressFull = addr == null ? null : $"{addr.Zipcode} {addr.City} {addr.Address1} {addr.Address2}".Trim(),

				ShipmentStatusCode = shipStatus,
				Items = items
			};

			return View(vm);
		}

		// GET: OnlineStore/SoOrderInfoes/Create
		public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: OnlineStore/SoOrderInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,OrderCode,UserId,OrderDate,OrderStatus,PaymentStatus,OrderTotal,PaymentAt,ShippedAt,CompletedAt,Subtotal,DiscountTotal,ShippingFee,GrandTotal")] SoOrderInfo soOrderInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(soOrderInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", soOrderInfo.UserId);
            return View(soOrderInfo);
        }

        // GET: OnlineStore/SoOrderInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var soOrderInfo = await _context.SoOrderInfoes.FindAsync(id);
            if (soOrderInfo == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", soOrderInfo.UserId);
            return View(soOrderInfo);
        }

        // POST: OnlineStore/SoOrderInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderCode,UserId,OrderDate,OrderStatus,PaymentStatus,OrderTotal,PaymentAt,ShippedAt,CompletedAt,Subtotal,DiscountTotal,ShippingFee,GrandTotal")] SoOrderInfo soOrderInfo)
        {
            if (id != soOrderInfo.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(soOrderInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SoOrderInfoExists(soOrderInfo.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", soOrderInfo.UserId);
            return View(soOrderInfo);
        }

        // GET: OnlineStore/SoOrderInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var soOrderInfo = await _context.SoOrderInfoes
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (soOrderInfo == null)
            {
                return NotFound();
            }

            return View(soOrderInfo);
        }

        // POST: OnlineStore/SoOrderInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var soOrderInfo = await _context.SoOrderInfoes.FindAsync(id);
            if (soOrderInfo != null)
            {
                _context.SoOrderInfoes.Remove(soOrderInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SoOrderInfoExists(int id)
        {
            return _context.SoOrderInfoes.Any(e => e.OrderId == id);
        }
    }
}
