// =============================================================
// 路徑：Areas/social_hub/Controllers/NotificationsController.cs
// 目的：提供通知寫入 API（前端按鈕 data-* → JSON → 服務驗證 → 成功寫 DB）
// 設計：
//   - 免登入可測（[AllowAnonymous]）；若之後要上線再加回 [Authorize]
//   - 保留 [ValidateAntiForgeryToken]（較安全；測試期也可拿掉）
//   - 回傳一律使用「小駝峰鍵名」（notificationId / reason），前端較直覺
// 依賴：INotificationStore（集中驗證與寫入；未通過直接拒絕，不動 DB）
// =============================================================
using GamiPort.Areas.social_hub.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[ApiController]                            // 啟用自動 Model 驗證、400 回傳等行為
	[Route("social_hub/notifications")]        // 基底路由：/social_hub/notifications/*
	public sealed class NotificationsController : ControllerBase
	{
		private readonly INotificationStore _store;
		public NotificationsController(INotificationStore store) => _store = store;

		// ---------------------------------------------------------
		// POST /social_hub/notifications/ajax
		// - 免登入（測試方便）；若要啟用授權可把 [AllowAnonymous] 換成 [Authorize]
		// - ValidateAntiForgeryToken：若保留，前端要送 Header "RequestVerificationToken"
		// ---------------------------------------------------------
		[AllowAnonymous]                        // ← 測試期免登入；上線可改回 [Authorize]
		[HttpPost("ajax")]
		[ValidateAntiForgeryToken]              // ← 若不想帶 Token，可移除此屬性與前端 Token Header
		public async Task<IActionResult> Ajax([FromBody] StoreCommand input, CancellationToken ct)
		{
			// 呼叫服務層完成全部驗證與（若通過）寫入
			var r = await _store.CreateAsync(input, ct);

			// 失敗：400 + 小駝峰鍵名（reason）
			if (!r.Succeeded)
				return BadRequest(new { reason = r.Reason });

			// 成功：200 + 小駝峰鍵名（notificationId）
			return Ok(new { notificationId = r.NotificationId });
		}
	}
}
