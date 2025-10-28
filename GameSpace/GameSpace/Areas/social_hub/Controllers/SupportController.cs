// =============================================================
// File: Areas/social_hub/Controllers/SupportController.cs (annotated)
// Purpose: 客服工作台（Support Desk）
// Summary:
//   - 僅允許具客服權限 (CustomerService=true) 的管理員進入
//   - 提供工單列表、詳情、訊息往來、轉單/結單、統計徽章數等
//   - 以「目前指派分段」為核心概念，僅顯示/統計該段內的對話與未讀
// Notes:
//   - ★ 時間規範：DB 層一律存 UTC；顯示時才轉台灣時間（HtmlHelper 或 VM 組裝時）
//   - 使用 EF Core（Db = GameSpace.Models 別名）
//   - 路由採 Area + 傳統路由，URL 形如：/social_hub/Support/{Action}
//   - AJAX 動作回傳 200/4xx/409，以利前端顯示友善訊息
//   - 對於需要同頁刷新之流程（例如轉單）使用 TempData 傳遞一次性訊息
//   - 權限檢查集中於 IManagerPermissionService（Gate + can_assign）
//   - 讀取密集的列表查詢皆使用 AsNoTracking() 提升效能
//   - 請確保下列欄位有索引：SupportTickets(IsClosed, AssignedManagerId, LastMessageAt),
//     SupportTicketMessages(TicketId, SentAt), SupportTicketAssignment(TicketId, AssignedAt)
// =============================================================

using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.social_hub.Permissions;
using GameSpace.Infrastructure.Time;           // ★ IAppClock：統一時間（DB 存 UTC / 顯示轉台灣）
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // for Claims

// EF 實體別名
using Db = GameSpace.Models;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[SocialHubAuth] // 讓 Items: gs_kind / gs_id 先就位（僅做相容變數轉換，不做 Authenticate）
	[RequireManagerPermissions(CustomerService = true)] // 進入此控制器必須具客服 Gate
	public class SupportController : Controller
	{
		// ================================= [ Fields / Ctor ] ================================
		private readonly GameSpacedatabaseContext _db;
		private readonly IManagerPermissionService _perm;
		private readonly IAppClock _clock;              // ★ 統一時間服務（DB 寫入一律用 _clock.UtcNow）
		private const int MessagePageSize = 30;         // 聊天訊息分頁大小

		public SupportController(GameSpacedatabaseContext db, IManagerPermissionService perm, IAppClock clock)
		{
			_db = db;
			_perm = perm;
			_clock = clock;
		}

		// ============================== [ Identity Resolution ] =============================
		/// <summary>
		/// 解析目前登入的管理員編號（優先序：HttpContext.Items → Claims）。
		/// 不讀 cookies（SocialHubAuth 已處理相容寫入）。
		/// </summary>
		private int? ResolveManagerId()
		{
			// 1) SocialHubAuth 已在 Items 寫入：
			//    - gs_kind = bool (IsManager)
			//    - gs_id   = int  (IsManager ? ManagerId : UserId)
			if (HttpContext?.Items?.TryGetValue("gs_kind", out var kindObj) == true
				&& kindObj is bool isManager && isManager
				&& HttpContext.Items.TryGetValue("gs_id", out var idObj)
				&& idObj is int id && id > 0)
			{
				return id;
			}

			// 2) 備援：Claims（AdminCookie）
			var p = User;
			var mgrClaim = p?.FindFirst("mgr:id")?.Value;
			if (int.TryParse(mgrClaim, out var mid) && mid > 0) return mid;

			if (p?.HasClaim(c => c.Type == "IsManager" && c.Value == "true") == true)
			{
				var idStr = p.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (int.TryParse(idStr, out var fallback) && fallback > 0) return fallback;
			}

			return null;
		}

		#region Index / Counts
		// ===================================== [ Index ] ====================================
		/// <summary>客服工作台首頁骨架（Index.cshtml）。</summary>
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			// 觸發 Gate 的相容流程（雖然有全域 Attribute，但呼叫可讓上游快取/準備生效）
			var _ = await _perm.GetCustomerServiceContextAsync(HttpContext);

			var meId = ResolveManagerId();
			ViewBag.MeManagerId = meId; // View 以 int? 型別讀取最安全（未登入/非管理員時為 null）
			ViewBag.CanAssignToOthers = meId.HasValue && await _perm.HasCsAssignPermissionAsync(meId.Value);
			return View();
		}

		// =============================== [ Badge Counts API ] ===============================
		/// <summary>回傳徽章數（JSON）：assigned / unassigned / inprogress / closed。</summary>
		[HttpGet]
		public async Task<IActionResult> GetCounts()
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			// ★ 所有時間比較均使用 UTC；此 API 僅計數，無需轉時區
			var q = _db.SupportTickets.AsNoTracking();

			var assignedCount = await q.Where(t => t.IsClosed != true && t.AssignedManagerId == meId.Value).CountAsync();
			var unassignedCount = await q.Where(t => t.IsClosed != true && t.AssignedManagerId == null).CountAsync();
			var closedCount = await q.Where(t => t.IsClosed == true).CountAsync();
			var inprogressCount = await q.Where(t => t.IsClosed != true && (t.AssignedManagerId == null || t.AssignedManagerId != meId.Value)).CountAsync();

			return Ok(new { assigned = assignedCount, unassigned = unassignedCount, closed = closedCount, inprogress = inprogressCount });
		}
		#endregion

		#region List helpers
		// ===================== [ Common query helpers / projection / paging ] =================
		/// <summary>以關鍵字過濾：數字 → 以 TicketId/UserId 比對；文字 → Subject Contains。</summary>
		private static IQueryable<Db.SupportTicket> ApplyKeyword(IQueryable<Db.SupportTicket> src, string? q)
		{
			if (string.IsNullOrWhiteSpace(q)) return src;
			q = q.Trim();
			if (int.TryParse(q, out var num)) return src.Where(t => t.TicketId == num || t.UserId == num);
			return src.Where(t => t.Subject != null && t.Subject.Contains(q));
		}

		/// <summary>
		/// 投影成列表 VM：以 LastMessageAt/CreatedAt 作為顯示與排序依據；
		/// UnreadForMe 以管理員視角計算（僅未讀且由使用者發出）。
		/// ★ 注意：此處全部為 UTC 值；顯示時由 View 端轉台灣時間。
		/// </summary>
		private static IQueryable<TicketListItemVM> ProjectTicketListVM(
			IQueryable<Db.SupportTicket> query,
			GameSpacedatabaseContext db)
		{
			return query.Select(t => new TicketListItemVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				AssignedManagerId = t.AssignedManagerId,
				IsClosed = t.IsClosed == true,
				CreatedAt = t.CreatedAt,              // UTC
				LastMessageAt = t.LastMessageAt,      // UTC
				UnreadForMe = db.SupportTicketMessages
					.Where(m => m.TicketId == t.TicketId && m.SenderUserId != null && m.ReadByManagerAt == null)
					.Count()
			});
		}

		/// <summary>列表排序：最新對話/建立時間在前（UTC）。</summary>
		private static IQueryable<TicketListItemVM> OrderForList(IQueryable<TicketListItemVM> q)
			=> q.OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt);

		/// <summary>通用分頁器（避免分頁 0 或 pageSize 0 導致例外）。</summary>
		private static async Task<PagedResult<TicketListItemVM>> ToPagedAsync(
			IQueryable<TicketListItemVM> q, int page, int pageSize)
		{
			if (page <= 0) page = 1;
			if (pageSize <= 0) pageSize = 10;
			var total = await q.CountAsync();
			var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
			return new PagedResult<TicketListItemVM>
			{
				Items = items,
				Page = page,
				PageSize = pageSize,
				TotalCount = total
			};
		}
		#endregion

		#region Lists
		// =============================== [ Ticket list Partial ] =============================
		/// <summary>指派給我（未結單）。</summary>
		[HttpGet]
		public async Task<IActionResult> ListAssigned(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == meId.Value);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			// 對 Partial 的固定上下文
			ViewData["action"] = nameof(ListAssigned);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.HasCsAssignPermissionAsync(meId.Value);
			ViewBag.MeManagerId = meId.Value; // 讓 Partial 能判斷 "isMine"

			return PartialView("_TicketList", paged);
		}

		/// <summary>未指派（未結單）。</summary>
		[HttpGet]
		public async Task<IActionResult> ListUnassigned(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == null);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListUnassigned);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.HasCsAssignPermissionAsync(meId.Value);
			ViewBag.MeManagerId = meId.Value;
			return PartialView("_TicketList", paged);
		}

		/// <summary>進行中（未結單且不是「指派給我」）。需具 can_assign。</summary>
		[HttpGet]
		public async Task<IActionResult> ListInProgress(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();
			if (!await _perm.HasCsAssignPermissionAsync(meId.Value)) return Forbid();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && (t.AssignedManagerId == null || t.AssignedManagerId != meId.Value));

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListInProgress);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = true;
			ViewBag.MeManagerId = meId.Value;
			return PartialView("_TicketList", paged);
		}

		/// <summary>已結單。</summary>
		[HttpGet]
		public async Task<IActionResult> ListClosed(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed == true);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListClosed);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.HasCsAssignPermissionAsync(meId.Value);
			ViewBag.MeManagerId = meId.Value;
			return PartialView("_TicketList", paged);
		}
		#endregion

		#region Assignment history
		// ============================ [ Assignment History ] ============================
		/// <summary>部分檢視：歷史指派（排除最新一筆）。分頁上限 9 筆，每頁 3 筆。</summary>
		[HttpGet]
		public async Task<IActionResult> AssignmentHistory(int id, int page = 1)
		{
			const int pageSize = 3;
			const int maxRecords = 9;

			var baseQ = _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == id)
				.OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.AssignmentId)
				.Skip(1); // 排除最新（目前這筆）

			var totalActual = await baseQ.CountAsync();
			var total = Math.Min(totalActual, maxRecords);
			var totalPages = (int)Math.Ceiling(total / (double)pageSize);
			if (totalPages <= 0) totalPages = 1;
			if (page <= 0) page = 1;
			if (page > totalPages) page = totalPages;

			// 先限制最多 9 筆，再做分頁
			var items = await baseQ
				.Take(maxRecords)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			ViewBag.Page = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.TicketId = id;

			return PartialView("_AssignmentHistory", items);
		}

		/// <summary>獨立頁：完整歷史指派清單。</summary>
		// SupportController.cs → AssignmentHistoryPage
		[HttpGet]
		public async Task<IActionResult> AssignmentHistoryPage(int id, int page = 1)
		{
			const int pageSize = 10;

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var baseQ = _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == id)
				.OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.AssignmentId);

			var total = await baseQ.CountAsync();
			if (page <= 0) page = 1;
			var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
			if (page > totalPages) page = totalPages;

			var items = await baseQ
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var vm = new AssignmentHistoryPageVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				Items = items,
				Page = page,
				PageSize = pageSize,
				TotalCount = total
			};

			return View("AssignmentHistoryPage", vm);
		}

		#endregion

		#region Segment utilities
		// ============================== [ Segment Utilities ] ===============================
		/// <summary>
		/// 取得「目前指派人」的對話分段邊界（start = 最近一次指派給該人時間；end = 下一次指派時間）。
		/// 若未指派或查無記錄，回傳 (null, null)。
		/// ★ 注意：這些時間皆為 UTC；後續查詢也以 UTC 比較。
		/// </summary>
		private async Task<(DateTime? start, DateTime? end)> GetCurrentSegmentBoundsAsync(int ticketId, int? currentAssigneeId)
		{
			if (currentAssigneeId == null) return (null, null);

			var rec = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == ticketId && a.ToManagerId == currentAssigneeId.Value)
				.OrderBy(a => a.AssignedAt).ThenBy(a => a.AssignmentId)
				.LastOrDefaultAsync();

			if (rec == null) return (null, null);

			var next = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == ticketId &&
					   (a.AssignedAt > rec.AssignedAt ||
					   (a.AssignedAt == rec.AssignedAt && a.AssignmentId > rec.AssignmentId)))
				.OrderBy(a => a.AssignedAt).ThenBy(a => a.AssignmentId)
				.FirstOrDefaultAsync();

			return (rec.AssignedAt, next?.AssignedAt);
		}

		/// <summary>
		/// 取得某一筆 Assignment 的分段邊界與該筆記錄。
		/// ★ 全為 UTC；不在此處做時區轉換。
		/// </summary>
		private async Task<(DateTime? start, DateTime? end, Db.SupportTicketAssignment? rec)> GetAssignmentBoundsAsync(int ticketId, int assignmentId)
		{
			var rec = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.FirstOrDefaultAsync(a => a.TicketId == ticketId && a.AssignmentId == assignmentId);
			if (rec == null) return (null, null, null);

			var next = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == ticketId &&
					   (a.AssignedAt > rec.AssignedAt ||
					   (a.AssignedAt == rec.AssignedAt && a.AssignmentId > rec.AssignmentId)))
				.OrderBy(a => a.AssignedAt).ThenBy(a => a.AssignmentId)
				.FirstOrDefaultAsync();

			return (rec.AssignedAt, next?.AssignedAt, rec);
		}
		#endregion

		#region Ticket details / Chat
		// ============================ [ Ticket Details / Chat ] =============================
		/// <summary>
		/// 工單詳情 + 對話（主頁僅顯示「目前指派分段」）。
		/// 未指派：
		///   - 具 can_assign 者：導至 Reassign 頁
		///   - 一般客服：回首頁未指派分頁並提示
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Ticket(int id, int page = 1)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			// 未指派：安全導向
			if (t.AssignedManagerId == null)
			{
				if (await _perm.HasCsAssignPermissionAsync(meId.Value))
				{
					// 具有 can_assign → 直接帶去轉單頁
					return RedirectToAction(nameof(Reassign), new { id, returnUrl = Url.Action(nameof(Index), new { area = "social_hub", tab = "unassigned" }) });
				}

				// 一般客服 → 回到首頁的「未指派」分頁，並提示
				TempData["Warn"] = "此工單尚未指派，請先指派後再進入對話。";
				return RedirectToAction(nameof(Index), new { area = "social_hub", tab = "unassigned" });
			}

			var isClosed = t.IsClosed == true;
			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);

			// 目前指派分段（UTC）
			var (segStart, segEnd) = await GetCurrentSegmentBoundsAsync(id, t.AssignedManagerId);

			// 只有現任負責人把分段內未讀標記已讀（避免誤讀）
			if (t.AssignedManagerId == meId.Value && segStart != null)
			{
				var unreadQ = _db.SupportTicketMessages
					.Where(m => m.TicketId == id && m.SenderUserId != null && m.ReadByManagerAt == null)
					.Where(m => m.SentAt >= segStart.Value);
				if (segEnd != null) unreadQ = unreadQ.Where(m => m.SentAt < segEnd.Value);

				var unread = await unreadQ.ToListAsync();
				if (unread.Count > 0)
				{
					var now = _clock.UtcNow;   // ★ 寫入一律 UTC
					foreach (var m in unread) m.ReadByManagerAt = now;
					await _db.SaveChangesAsync();
				}
			}

			if (page <= 0) page = 1;

			// 對話資料（依分段篩選；UTC 比較）
			var baseQ = _db.SupportTicketMessages.AsNoTracking().Where(m => m.TicketId == id);
			if (segStart != null) baseQ = baseQ.Where(m => m.SentAt >= segStart.Value);
			if (segEnd != null) baseQ = baseQ.Where(m => m.SentAt < segEnd.Value);
			baseQ = baseQ.OrderByDescending(m => m.SentAt);

			var total = await baseQ.CountAsync();
			var pageItems = await baseQ.Skip((page - 1) * MessagePageSize).Take(MessagePageSize).ToListAsync();

			// ViewModel 保留 UTC；顯示端再轉台灣時間
			var messagesAsc = pageItems
				.OrderBy(m => m.SentAt)
				.Select(m => new SupportMessageVM
				{
					MessageId = m.MessageId,
					TicketId = m.TicketId,
					SenderUserId = m.SenderUserId,
					SenderManagerId = m.SenderManagerId,
					MessageText = m.MessageText ?? "",
					SentAt = m.SentAt, // UTC
					ReadByUserAt = m.ReadByUserAt,
					ReadByManagerAt = m.ReadByManagerAt,
					IsMine = (m.SenderManagerId != null && m.SenderManagerId.Value == meId.Value)
				})
				.ToList();

			var vm = new TicketDetailVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? "",
				IsClosed = isClosed,
				CreatedAt = t.CreatedAt,      // UTC
				ClosedAt = t.ClosedAt,        // UTC
				CloseNote = t.CloseNote,
				AssignedManagerId = t.AssignedManagerId,
				LastMessageAt = t.LastMessageAt, // UTC

				MeManagerId = meId.Value,
				// 自己接單：任何客服可用（不需 can_assign）
				CanAssignToMe = !isClosed && t.AssignedManagerId == null,
				// 轉單：現任負責人 或 具有 can_assign
				CanReassign = !isClosed && (t.AssignedManagerId == meId.Value || canAssignAny),
				// 結單：現任負責人 或 具有 can_assign
				CanClose = !isClosed && (t.AssignedManagerId == meId.Value || canAssignAny),

				Messages = messagesAsc,
				Page = page,
				PageSize = MessagePageSize,
				TotalMessages = total
			};

			return View(vm);
		}

		/// <summary>局部刷新訊息列（依目前指派分段）。</summary>
		[HttpGet]
		public async Task<IActionResult> MessageList(int id, int page = 1)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			if (page <= 0) page = 1;

			var ticket = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (ticket == null) return NotFound();
			if (ticket.AssignedManagerId == null)
				return StatusCode(409, "此工單尚未指派，請先指派後再查看對話。");

			var (segStart, segEnd) = await GetCurrentSegmentBoundsAsync(id, ticket.AssignedManagerId);

			var q = _db.SupportTicketMessages.AsNoTracking().Where(m => m.TicketId == id);
			if (segStart != null) q = q.Where(m => m.SentAt >= segStart.Value);
			if (segEnd != null) q = q.Where(m => m.SentAt < segEnd.Value);
			q = q.OrderByDescending(m => m.SentAt);

			var total = await q.CountAsync();

			// ★ 修正：Skip((page - 1) * MessagePageSize)
			var pageItems = await q.Skip((page - 1) * MessagePageSize).Take(MessagePageSize).ToListAsync();

			var messagesAsc = pageItems.OrderBy(m => m.SentAt).Select(m => new SupportMessageVM
			{
				MessageId = m.MessageId,
				TicketId = m.TicketId,
				SenderUserId = m.SenderUserId,
				SenderManagerId = m.SenderManagerId,
				MessageText = m.MessageText ?? "",
				SentAt = m.SentAt, // UTC
				ReadByUserAt = m.ReadByUserAt,
				ReadByManagerAt = m.ReadByManagerAt,
				IsMine = (m.SenderManagerId != null && m.SenderManagerId.Value == meId.Value)
			}).ToList();

			ViewBag.Page = page;
			ViewBag.PageSize = MessagePageSize;
			ViewBag.Total = total;

			return PartialView("_MessageList", messagesAsc);
		}
		#endregion

		#region Assignment conversation
		// ======================== [ Conversation for an Assignment ] ========================
		/// <summary>依某一筆 Assignment 顯示該分段對話（便於歷史追溯）。</summary>
		[HttpGet]
		public async Task<IActionResult> AssignmentConversation(int id, int aid, int page = 1)
		{
			const int pageSize = MessagePageSize;

			var (start, end, rec) = await GetAssignmentBoundsAsync(id, aid);
			if (rec == null) return NotFound();

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var q = _db.SupportTicketMessages.AsNoTracking().Where(m => m.TicketId == id);
			if (start != null) q = q.Where(m => m.SentAt >= start.Value);
			if (end != null) q = q.Where(m => m.SentAt < end.Value);
			q = q.OrderByDescending(m => m.SentAt);

			if (page <= 0) page = 1;
			var total = await q.CountAsync();
			var pageItems = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

			var messagesAsc = pageItems
				.OrderBy(m => m.SentAt)
				.Select(m => new SupportMessageVM
				{
					MessageId = m.MessageId,
					TicketId = m.TicketId,
					SenderUserId = m.SenderUserId,
					SenderManagerId = m.SenderManagerId,
					MessageText = m.MessageText ?? "",
					SentAt = m.SentAt, // UTC
					ReadByUserAt = m.ReadByUserAt,
					ReadByManagerAt = m.ReadByManagerAt,
					// 歷史分段視角：該段的負責人
					IsMine = (m.SenderManagerId != null && m.SenderManagerId == rec.ToManagerId)
				})
				.ToList();

			var vm = new AssignmentConversationVM
			{
				TicketId = id,
				AssignmentId = aid,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				FromManagerId = rec.FromManagerId,
				ToManagerId = rec.ToManagerId,
				AssignedByManagerId = rec.AssignedByManagerId,
				AssignedAt = rec.AssignedAt,  // UTC
				NextAssignedAt = end,         // UTC
				Note = rec.Note,
				Messages = messagesAsc,
				Page = page,
				PageSize = pageSize,
				TotalMessages = total
			};

			return View(vm);
		}
		#endregion

		#region Messaging / AssignToMe / Close / Reassign / TicketInfo
		// =================================== [ Messaging ] ==================================
		/// <summary>發送訊息（AJAX）。僅現任負責人可發送；超過 255 字將截斷。</summary>
		[HttpPost]
		public async Task<IActionResult> SendMessage(int id, [FromForm] string text)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return StatusCode(409, "此工單已結單，無法再發送訊息。");
			if (t.AssignedManagerId != meId.Value) return Forbid();

			text = (text ?? "").Trim();
			if (text.Length == 0) return BadRequest("訊息不可為空。");
			if (text.Length > 255) text = text.Substring(0, 255);

			var now = _clock.UtcNow; // ★ 寫入一律 UTC

			_db.SupportTicketMessages.Add(new Db.SupportTicketMessage
			{
				TicketId = id,
				SenderUserId = null,
				SenderManagerId = meId.Value,
				MessageText = text,
				SentAt = now,              // UTC
				ReadByUserAt = null,
				ReadByManagerAt = now      // 自己發出的訊息對管理員視角可視為已讀
			});

			t.LastMessageAt = now; // 更新工單最後訊息時間（利於列表排序）
			await _db.SaveChangesAsync();

			return Ok(new { ok = true, sentAt = now, text });
		}

		// ================================== [ Assign to me ] =================================
		/// <summary>指派給我（AJAX）。任何客服可自接未指派的工單。</summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignToMe(int id)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return StatusCode(409, "此工單已結單，無法指派。");

			if (t.AssignedManagerId == meId.Value)
				return Ok(new { ok = true, assignedManagerId = meId.Value });

			if (t.AssignedManagerId != null)
				return StatusCode(409, "此工單已指派給其他客服。");

			t.AssignedManagerId = meId.Value;

			_db.Set<Db.SupportTicketAssignment>().Add(new Db.SupportTicketAssignment
			{
				TicketId = id,
				FromManagerId = null,
				ToManagerId = meId.Value,
				AssignedByManagerId = meId.Value,
				AssignedAt = _clock.UtcNow, // ★ UTC
				Note = null
			});

			await _db.SaveChangesAsync();
			return Ok(new { ok = true, assignedManagerId = meId.Value });
		}

		// =================================== [ Close ticket ] ================================
		// 結單頁（GET）— 允許：現任負責人 / 具 can_assign（可視為可結任何工單）
		[HttpGet]
		public async Task<IActionResult> Close(int id, bool modal = false)
		{
			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || canAssignAny))
				return Forbid();

			var vm = new CloseTicketVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				AssignedManagerId = t.AssignedManagerId,
				CreatedAt = t.CreatedAt,          // UTC（顯示轉台灣）
				LastMessageAt = t.LastMessageAt   // UTC（顯示轉台灣）
			};

			if (modal)
			{
				ViewData["Partial"] = "_CloseForm"; // 用 ModalWrapper 載入 partial
				return View("~/Areas/social_hub/Views/Shared/ModalWrapper.cshtml", vm);
			}
			// 不是 modal 的話導回工單頁
			return RedirectToAction(nameof(Ticket), new { id });
		}

		// 結單（POST）— 由實際操作者寫入 closed_by_manager_id
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CloseConfirm(int id, [FromForm] string? closeNote, string? returnUrl = null)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			// 已結單就不重複寫
			if (t.IsClosed == true)
				return isAjax
					? Ok(new { ok = true, alreadyClosed = true })
					: (Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Ticket), new { id }));

			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || canAssignAny))
				return Forbid();

			// —— 寫入由誰結單（重點）——
			t.IsClosed = true;
			t.ClosedAt = _clock.UtcNow;        // ★ UTC
			t.ClosedByManagerId = meId.Value;  // ★ 操作者本人
			if (!string.IsNullOrWhiteSpace(closeNote))
			{
				closeNote = closeNote.Trim();
				t.CloseNote = closeNote.Length > 255 ? closeNote.Substring(0, 255) : closeNote;
			}
			else
			{
				t.CloseNote = null;
			}

			await _db.SaveChangesAsync();

			return isAjax
				? Ok(new { ok = true })
				: (Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Ticket), new { id }));
		}

		// ==================================== [ Reassign ] ==================================
		// 轉單頁（GET）
		[HttpGet]
		public async Task<IActionResult> Reassign(int id, string? returnUrl = null, bool modal = false)
		{
			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return Forbid();

			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || canAssignAny)) return Forbid();

			// 候選名單（具客服 Gate 者）
			var candidates = await _db.Set<ManagerRolePermission>()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers.Select(m => m.ManagerId))
				.Distinct()
				.OrderBy(m => m)
				.ToListAsync();

			var vm = new ReassignTicketVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? "",
				CurrentAssignedManagerId = t.AssignedManagerId,
				CandidateManagerIds = candidates
			};

			ViewBag.MeManagerId = meId;
			ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Ticket), new { id });

			if (modal)
			{
				ViewData["Partial"] = "_ReassignForm"; // 用 ModalWrapper 載入 partial
				return View("~/Areas/social_hub/Views/Shared/ModalWrapper.cshtml", vm);
			}

			return RedirectToAction(nameof(Ticket), new { id });
		}

		// 轉單（POST）
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ReassignConfirm(int id, [FromForm] int toManagerId,
														[FromForm] string? note, string? returnUrl = null)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();
			bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return Forbid();

			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || canAssignAny)) return Forbid();

			// 1) no-op：目標 == 目前
			if (t.AssignedManagerId == toManagerId)
			{
				if (isAjax) return Ok(new { ok = true, same = true });
				return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Ticket), new { id });
			}

			// 2) 只有「目前就是我」時，才擋 ‘指派給自己’
			bool assigningToSelf = (toManagerId == meId.Value);
			bool currentlyMine = (t.AssignedManagerId == meId.Value);
			if (assigningToSelf && currentlyMine)
			{
				return isAjax ? BadRequest("目前已由你負責，無需再指派給自己。")
							  : Forbid();
			}

			// 3) 目標必須具客服 Gate
			bool targetHasCS = await _db.Set<ManagerRolePermission>()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == toManagerId);

			if (!targetHasCS)
			{
				return isAjax ? BadRequest("目標不具客服權限，無法指派。")
							  : RedirectToAction(nameof(Reassign), new { id, returnUrl });
			}

			// 4) 寫入 Assignment（UTC）
			var fromId = t.AssignedManagerId;
			t.AssignedManagerId = toManagerId;

			_db.Set<Db.SupportTicketAssignment>().Add(new Db.SupportTicketAssignment
			{
				TicketId = id,
				FromManagerId = fromId,
				ToManagerId = toManagerId,
				AssignedByManagerId = meId.Value,
				AssignedAt = DateTime.UtcNow,
				Note = string.IsNullOrWhiteSpace(note) ? null : note!.Trim()[..Math.Min(255, note.Trim().Length)]
			});

			// === 轉單後直接清除未讀 ===
			// 定義「未讀」：使用者發送且 ReadByManagerAt 為 null 的訊息。
			// 一次性把整張工單目前的未讀清掉，避免徽章延續到新負責人。
			var now = DateTime.UtcNow;
			var unread = await _db.SupportTicketMessages
				.Where(m => m.TicketId == id && m.SenderUserId != null && m.ReadByManagerAt == null)
				.ToListAsync();

			if (unread.Count > 0)
			{
				foreach (var m in unread) m.ReadByManagerAt = now;
			}

			await _db.SaveChangesAsync();

			return isAjax ? Ok(new { ok = true })
						  : (Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Ticket), new { id }));
		}

		// 詳細資料（GET）
		[HttpGet]
		public async Task<IActionResult> TicketInfo(int id, bool modal = false)
		{
			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var vm = new TicketInfoVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? "",
				AssignedManagerId = t.AssignedManagerId,
				CreatedAt = t.CreatedAt,        // UTC
				LastMessageAt = t.LastMessageAt, // UTC
				IsClosed = t.IsClosed,
				ClosedAt = t.ClosedAt,          // UTC
				CloseNote = t.CloseNote
			};

			if (modal)
			{
				ViewData["Partial"] = "_TicketInfo";
				return View("~/Areas/social_hub/Views/Shared/ModalWrapper.cshtml", vm);
			}
			return RedirectToAction(nameof(Ticket), new { id });
		}
		#endregion
	}
}
