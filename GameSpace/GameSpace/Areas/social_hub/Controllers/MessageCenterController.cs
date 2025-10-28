// Areas/social_hub/Controllers/MessageCenterController.cs
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Areas.social_hub.Services;
using GameSpace.Infrastructure.Login;
using GameSpace.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[SocialHubAuth]
	public class MessageCenterController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly INotificationService _notificationService;
		private readonly ILoginIdentity _login;

		private const string AdminCookieScheme = "AdminCookie";

		public MessageCenterController(
			GameSpacedatabaseContext context,
			INotificationService notificationService,
			ILoginIdentity login)
		{
			_context = context;
			_notificationService = notificationService;
			_login = login;
		}

		// 身分來源優先序：Items → AdminCookie → HttpContext.User → ILoginIdentity → 舊 cookies
		private async Task<(bool isUser, bool isManager, int? userId, int? managerId)> GetIdentityAsync()
		{
			// Items（SocialHubAuth 放入）
			if (HttpContext?.Items != null)
			{
				var kindObj = HttpContext.Items["gs_kind"] as string;
				var idObj = HttpContext.Items["gs_id"];
				if (idObj is int idFromItems && idFromItems > 0 && !string.IsNullOrWhiteSpace(kindObj))
				{
					if (string.Equals(kindObj, "user", StringComparison.OrdinalIgnoreCase))
						return (true, false, idFromItems, null);
					if (string.Equals(kindObj, "manager", StringComparison.OrdinalIgnoreCase))
						return (false, true, null, idFromItems);
				}
			}

			// AdminCookie
			var auth = await HttpContext.AuthenticateAsync(AdminCookieScheme);
			if (auth.Succeeded && auth.Principal?.Identity?.IsAuthenticated == true)
			{
				var idStr = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
				var isMgr = auth.Principal.HasClaim(c => c.Type == "IsManager" && string.Equals(c.Value, "true", StringComparison.OrdinalIgnoreCase));
				if (int.TryParse(idStr, out var idVal) && idVal > 0)
					return isMgr ? (false, true, null, idVal) : (true, false, idVal, null);
			}

			// HttpContext.User
			var u = HttpContext?.User;
			if (u?.Identity?.IsAuthenticated == true)
			{
				var idStr = u.FindFirstValue(ClaimTypes.NameIdentifier);
				var isMgr = u.HasClaim(c => c.Type == "IsManager" && string.Equals(c.Value, "true", StringComparison.OrdinalIgnoreCase));
				if (int.TryParse(idStr, out var idVal2) && idVal2 > 0)
					return isMgr ? (false, true, null, idVal2) : (true, false, idVal2, null);
			}

			// ILoginIdentity
			var me = await _login.GetAsync();
			if (me.IsAuthenticated)
			{
				if (string.Equals(me.Kind, "user", StringComparison.OrdinalIgnoreCase) && me.UserId is int uid)
					return (true, false, uid, null);
				if (string.Equals(me.Kind, "manager", StringComparison.OrdinalIgnoreCase) && me.ManagerId is int mid)
					return (false, true, null, mid);
			}

			// 舊 cookies
			string? idCookie = null, kindCookie = null;
			if (Request.Cookies.TryGetValue("gs_id", out var gsid)) idCookie = gsid;
			if (Request.Cookies.TryGetValue("gs_kind", out var gskind)) kindCookie = gskind;
			if (string.IsNullOrWhiteSpace(idCookie))
			{
				if (Request.Cookies.TryGetValue("sh_uid", out var su)) { idCookie = su; kindCookie = "user"; }
				else if (Request.Cookies.TryGetValue("sh_mid", out var sm)) { idCookie = sm; kindCookie = "manager"; }
			}
			if (int.TryParse(idCookie, out var idVal3) && idVal3 > 0)
			{
				if (string.Equals(kindCookie, "user", StringComparison.OrdinalIgnoreCase)) return (true, false, idVal3, null);
				if (string.Equals(kindCookie, "manager", StringComparison.OrdinalIgnoreCase)) return (false, true, null, idVal3);
			}

			return (false, false, null, null);
		}

		// 收件匣
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var (isUser, isManager, userId, managerId) = await GetIdentityAsync();
			if (!(isUser || isManager)) return Unauthorized();

			var q =
				from r in _context.NotificationRecipients
				join n in _context.Notifications on r.NotificationId equals n.NotificationId
				join s in _context.NotificationSources on n.SourceId equals s.SourceId
				join a in _context.NotificationActions on n.ActionId equals a.ActionId
				join su0 in _context.Users on n.SenderUserId equals su0.UserId into suj
				from su in suj.DefaultIfEmpty()
				join sm0 in _context.ManagerData on n.SenderManagerId equals sm0.ManagerId into smj
				from sm in smj.DefaultIfEmpty()
				where (isUser && r.UserId == userId) || (isManager && r.ManagerId == managerId)
				orderby n.CreatedAt descending
				select new NotificationViewModel
				{
					RecipientId = r.RecipientId,
					NotificationId = n.NotificationId,
					NotificationTitle = n.Title,
					NotificationMessage = n.Message,
					SourceName = s.SourceName,
					ActionName = a.ActionName,
					SenderName = su != null ? (su.UserName ?? su.UserAccount)
											: (sm != null ? sm.ManagerName : null),
					CreatedAt = n.CreatedAt,
					ReadAt = r.ReadAt
				};

			var list = await q.AsNoTracking().ToListAsync();
			return View(list);
		}

		// 標記已讀
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkRead(int id)
		{
			var (isUser, isManager, userId, managerId) = await GetIdentityAsync();
			if (!(isUser || isManager)) return Unauthorized();

			var rec = await _context.NotificationRecipients.FirstOrDefaultAsync(r => r.RecipientId == id);
			if (rec == null) return NotFound();

			bool isOwner = (isUser && rec.UserId == userId) || (isManager && rec.ManagerId == managerId);
			if (!isOwner) return Forbid();

			if (rec.ReadAt == null)
			{
				rec.ReadAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		// 建立頁（GET）
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			await FillCreateDropdownsAsync();

			var (isUser, isManager, userId, managerId) = await GetIdentityAsync();
			ViewBag.SenderBanner = isUser ? $"使用者 #{userId}"
							   : isManager ? $"管理員 #{managerId}"
							   : "未登入";
			ViewBag.IsManager = isManager;

			return View(new Notification());
		}

		// 建立頁：下拉
		private async Task FillCreateDropdownsAsync(int? sourceId = null, int? actionId = null, int? groupId = null, string? recipientMode = null)
		{
			ViewBag.Sources = new SelectList(
				await _context.NotificationSources.AsNoTracking().OrderBy(s => s.SourceId).ToListAsync(),
				"SourceId", "SourceName", sourceId);

			ViewBag.Actions = new SelectList(
				await _context.NotificationActions.AsNoTracking().OrderBy(a => a.ActionId).ToListAsync(),
				"ActionId", "ActionName", actionId);

			ViewBag.Users = new SelectList(
				await _context.Users.AsNoTracking()
					.OrderBy(u => u.UserId)
					.Select(u => new { u.UserId, Name = (u.UserName ?? u.UserAccount) ?? ("User #" + u.UserId) })
					.ToListAsync(),
				"UserId", "Name"
			);

			var managersRaw = await _context.ManagerData
				.AsNoTracking()
				.OrderBy(m => m.ManagerId)
				.Select(m => new { m.ManagerId, m.ManagerName })
				.ToListAsync();

			var managerItems = managersRaw.Select(m => new
			{
				m.ManagerId,
				Name = string.IsNullOrWhiteSpace(m.ManagerName) ? $"管理員 #{m.ManagerId}" : m.ManagerName
			}).ToList();

			ViewBag.Managers = new SelectList(managerItems, "ManagerId", "Name");

			ViewBag.Groups = new SelectList(
				await _context.Groups.AsNoTracking().OrderBy(g => g.GroupId).ToListAsync(),
				"GroupId", "GroupName", groupId);

			ViewBag.RecipientMode = recipientMode ?? "specific";
		}

		// 建立（POST）
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("Title,Message,SourceId,ActionId,GroupId")] Notification input,
			[FromForm] List<int> recipientIds,
			[FromForm] string recipientMode)
		{
			if (!ModelState.IsValid)
			{
				await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
				var (iu, im, uid, mid) = await GetIdentityAsync();
				ViewBag.SenderBanner = iu ? $"使用者 #{uid}" : im ? $"管理員 #{mid}" : "未登入";
				ViewBag.IsManager = im;
				return View(input);
			}

			var (isUser, isManager, userId, managerId) = await GetIdentityAsync();
			if (!(isUser || isManager)) return Unauthorized();

			int? senderUserId = isUser ? userId : null;
			int? senderManagerId = isManager ? managerId : null;

			// 使用者收件
			IEnumerable<int> userRecipients = Enumerable.Empty<int>();
			recipientMode = (recipientMode ?? "specific").Trim().ToLowerInvariant();

			switch (recipientMode)
			{
				case "specific":
					userRecipients = (recipientIds ?? new List<int>()).Distinct().Where(x => x > 0);
					if (!userRecipients.Any())
					{
						ViewBag.AllErrors = "請至少選擇一位使用者，或改用「全體使用者」或「指定群組」。";
						await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
						ViewBag.SenderBanner = isUser ? $"使用者 #{userId}" : $"管理員 #{managerId}";
						ViewBag.IsManager = isManager;
						return View(input);
					}
					break;

				case "all_users":
					userRecipients = await _context.Users.AsNoTracking().Select(u => u.UserId).ToListAsync();
					if (!userRecipients.Any())
					{
						ViewBag.AllErrors = "目前系統內沒有任何使用者可寄送。";
						await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
						ViewBag.SenderBanner = isUser ? $"使用者 #{userId}" : $"管理員 #{managerId}";
						ViewBag.IsManager = isManager;
						return View(input);
					}
					break;

				case "group":
					if (input.GroupId is null or <= 0)
					{
						ViewBag.AllErrors = "請先選擇群組，或改用其他收件方式。";
						await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
						ViewBag.SenderBanner = isUser ? $"使用者 #{userId}" : $"管理員 #{managerId}";
						ViewBag.IsManager = isManager;
						return View(input);
					}
					userRecipients = await _context.GroupMembers
						.AsNoTracking()
						.Where(gm => gm.GroupId == input.GroupId && gm.LeftAt == null)
						.Select(gm => gm.UserId)
						.ToListAsync();
					if (!userRecipients.Any())
					{
						ViewBag.AllErrors = "這個群組目前沒有成員可寄送。";
						await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
						ViewBag.SenderBanner = isUser ? $"使用者 #{userId}" : $"管理員 #{managerId}";
						ViewBag.IsManager = isManager;
						return View(input);
					}
					break;

				case "none_users":
					// 不寄使用者，改由管理員收件區控制
					userRecipients = Enumerable.Empty<int>();
					break;

				default:
					ViewBag.AllErrors = "不支援的使用者收件模式。";
					await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
					ViewBag.SenderBanner = isUser ? $"使用者 #{userId}" : $"管理員 #{managerId}";
					ViewBag.IsManager = isManager;
					return View(input);
			}

			// 管理員收件（可留空）
			IEnumerable<int> managerRecipients = Enumerable.Empty<int>();
			var mgrMode = (Request.Form["mgrRecipientMode"].ToString() ?? "none").Trim().ToLowerInvariant();
			switch (mgrMode)
			{
				case "mgr_single":
					{
						var singleRaw = Request.Form["managerRecipientSingleId"].ToString();
						if (int.TryParse(singleRaw, out var singleId) && singleId > 0)
							managerRecipients = new[] { singleId };
						break;
					}
				case "mgr_specific":
					{
						managerRecipients = (Request.Form["managerRecipientIds"]
							.Select(v => int.TryParse(v, out var x) ? x : 0))
							.Where(x => x > 0)
							.Distinct()
							.ToList();
						break;
					}
				case "all_managers":
					{
						managerRecipients = await _context.ManagerData
							.AsNoTracking()
							.Select(m => m.ManagerId)
							.ToListAsync();
						break;
					} 
				case "none":
				default:
					managerRecipients = Enumerable.Empty<int>();
					break;
			}

			// 至少要有一邊有收件人
			if (!userRecipients.Any() && !managerRecipients.Any())
			{
				ViewBag.AllErrors = "請至少選擇一位收件人（使用者或管理員）。";
				await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
				ViewBag.SenderBanner = isUser ? $"使用者 #{userId}" : $"管理員 #{managerId}";
				ViewBag.IsManager = isManager;
				return View(input);
			}

			var added = await _notificationService.CreateAsync(
				input,
				userRecipients,
				managerRecipients,
				senderUserId,
				senderManagerId
			);

			TempData["Msg"] = $"✅ 已建立通知 #{input.NotificationId}，成功寄給 {added} 位收件人。";
			return RedirectToAction(nameof(Index));
		}




		// 明細（自動已讀）
		[HttpGet]
		public async Task<IActionResult> Detail(int id)
		{
			var (isUser, isManager, userId, managerId) = await GetIdentityAsync();
			if (!(isUser || isManager)) return Unauthorized();

			var row = await (
				from nr in _context.NotificationRecipients
				join n in _context.Notifications on nr.NotificationId equals n.NotificationId
				join s in _context.NotificationSources on n.SourceId equals s.SourceId into sj
				from s in sj.DefaultIfEmpty()
				join a in _context.NotificationActions on n.ActionId equals a.ActionId into aj
				from a in aj.DefaultIfEmpty()
				join g in _context.Groups on n.GroupId equals g.GroupId into gj
				from g in gj.DefaultIfEmpty()
				join su in _context.Users on n.SenderUserId equals su.UserId into suj
				from su in suj.DefaultIfEmpty()
				join sm in _context.ManagerData on n.SenderManagerId equals sm.ManagerId into smj
				from sm in smj.DefaultIfEmpty()
				where nr.RecipientId == id
				   && ((isUser && nr.UserId == userId) || (isManager && nr.ManagerId == managerId))
				select new
				{
					nr.RecipientId,
					nr.NotificationId,
					IsRead = (nr.ReadAt != null),
					ReadAt = nr.ReadAt,
					Title = n.Title,
					Msg = n.Message,
					SourceName = s != null ? s.SourceName : null,
					ActionName = a != null ? a.ActionName : null,
					GroupName = g != null ? g.GroupName : null,
					SenderUserName = su != null ? (su.UserName ?? su.UserAccount) : null,
					SenderManagerName = sm != null ? sm.ManagerName : null,
					n.CreatedAt
				}
			).AsNoTracking().FirstOrDefaultAsync();

			if (row == null) return NotFound();

			var vm = new NotificationDetailViewModel
			{
				RecipientId = row.RecipientId,
				NotificationId = row.NotificationId,
				NotificationTitle = row.Title ?? string.Empty,
				NotificationMessage = (row.Msg ?? string.Empty).Trim(),
				SourceName = row.SourceName,
				ActionName = row.ActionName,
				GroupName = row.GroupName,
				SenderName = row.SenderUserName ?? row.SenderManagerName ?? "系統",
				SenderType = row.SenderUserName != null ? "user" : (row.SenderManagerName != null ? "manager" : "system"),
				CreatedAt = row.CreatedAt,
				IsRead = row.IsRead,
				ReadAt = row.ReadAt,
				FullSourcePath = $"{(row.SourceName ?? "未設定")} → {(row.ActionName ?? "未設定")}",
				Links = null
			};

			if (!vm.IsRead)
			{
				var rec = await _context.NotificationRecipients.FirstOrDefaultAsync(r => r.RecipientId == id);
				bool isOwner = (isUser && rec?.UserId == userId) || (isManager && rec?.ManagerId == managerId);
				if (rec != null && isOwner && rec.ReadAt == null)
				{
					rec.ReadAt = DateTime.UtcNow;
					await _context.SaveChangesAsync();

					vm.IsRead = true;
					vm.ReadAt = rec.ReadAt;
					ViewBag.AutoRead = true;
				}
			}

			return View(vm);
		}

		// 管理端：通知清單
		[HttpGet]
		//[RequireManagerPermissions(Admin = true)]
		public async Task<IActionResult> Admin()
		{
			var list = await _context.Notifications
				.AsNoTracking()
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();

			return View("Admin", list);
		}

		// 管理端：依角色群發（管理員收件）
		[HttpPost]
		[ValidateAntiForgeryToken]
		//[RequireManagerPermissions(Admin = true)]
		public async Task<IActionResult> BroadcastToRole(
			int roleId,
			[Bind("Title,Message,SourceId,ActionId")] Notification template)
		{
			var (isUser, isManager, userId, managerId) = await GetIdentityAsync();
			int? senderUserId = isUser ? userId : null;
			int? senderManagerId = isManager ? managerId : null;

			var managerIds = await _context.ManagerData
				.AsNoTracking()
				.Where(m => m.ManagerRoles.Any(rp => rp.ManagerRoleId == roleId))
				.Select(m => m.ManagerId)
				.Distinct()
				.ToListAsync();

			var added = await _notificationService.CreateAsync(
				template,
				Enumerable.Empty<int>(),
				managerIds,
				senderUserId,
				senderManagerId
			);

			TempData["Toast"] = $"📣 角色群發完成（有效收件人：{added} 位）。";
			return RedirectToAction(nameof(Admin));
		}

		// 偵錯：目前身分
		[HttpGet]
		public async Task<IActionResult> WhoAmI()
		{
			var (isUser, isManager, userId, managerId) = await GetIdentityAsync();
			return Json(new { isUser, isManager, userId, managerId });
		}
	}
}
