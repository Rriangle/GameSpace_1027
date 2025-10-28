// =======================
// Program.cs（純 Cookie 版；不使用 Identity，不會建立 AspNetUsers）
// 目的：使用自訂 Cookie 驗證；保留 MVC / RazorPages、Anti-forgery、路由與開發期 EF 偵錯。
// 並新增：SignalR Hub 映射、我方統一介面 IAppCurrentUser（集中「吃登入」）、ILoginIdentity 備援。
// =======================


using GamiPort.Models;                     // GameSpacedatabaseContext（業務資料）
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.Services.Application;
using Microsoft.AspNetCore.Identity;       // 僅用 IPasswordHasher<User> / PasswordHasher<User>（升級舊明文）
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using GamiPort.Areas.Login.Services;       // IEmailSender / SmtpEmailSender（若尚未設定可換 NullEmailSender）

// === 新增的 using（本檔新增了這些服務/端點） ===
using GamiPort.Infrastructure.Security;    // ★ 我方統一介面 IAppCurrentUser / AppCurrentUser
using GamiPort.Infrastructure.Login;       // ★ 備援解析 ILoginIdentity / ClaimFirstLoginIdentity
using GamiPort.Areas.social_hub.Hubs;      // ★ ChatHub（SignalR Hub）

namespace GamiPort
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// ------------------------------------------------------------
			// 連線字串（兩個 DbContext 理論可共用；本版僅使用業務 DB）
			// 會依序找 "GameSpace" -> "GameSpacedatabase"；都沒有就丟錯
			// ------------------------------------------------------------
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// ------------------------------------------------------------
			// DbContext（只註冊業務 DB；不註冊 Identity 的 ApplicationDbContext）
			// ------------------------------------------------------------
			builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
			{
				options.UseSqlServer(gameSpaceConn);
				// 若多為查詢頁面可開 NoTracking 以省記憶體與變更追蹤成本
				// options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			});

			// EF 開發者例外頁（顯示完整 EF 例外、/migrations 端點）
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// ------------------------------------------------------------
			// 驗證：使用 Cookie（完全不走 Identity）
			// ------------------------------------------------------------
			builder.Services
				.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(opts =>
				{
					// 依你的登入頁面實際路由（Area=Login, Controller=Login, Action=Login）
					opts.LoginPath = "/Login/Login/Login";
					opts.LogoutPath = "/Login/Login/Logout";
					opts.AccessDeniedPath = "/Login/Login/Denied";

					// Cookie 屬性
					opts.Cookie.Name = "GamiPort.User"; // 與後台 Cookie 保持不同名，避免互相覆蓋
					opts.Cookie.HttpOnly = true;
					opts.Cookie.SameSite = SameSiteMode.Lax; // 防 CSRF；若需跨站再調整
					opts.ExpireTimeSpan = TimeSpan.FromDays(7);
					opts.SlidingExpiration = true; // 活動期間自動順延有效期
				});

			// 授權（有 [Authorize] / Policy 時會用到；本案先走預設）
			builder.Services.AddAuthorization();

			// ------------------------------------------------------------
			// 專案服務（通知、好友關係等）
			// ------------------------------------------------------------
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<INotificationStore, NotificationStore>();
			builder.Services.AddScoped<IRelationService, RelationService>();


			// === Chat 服務註冊（缺這兩個會讓 ChatHub 無法被建立） ===
			// IChatService：與資料庫互動（寫訊息、計算未讀…）→ 需用 DbContext，建議 Scoped
			builder.Services.AddScoped<IChatService,ChatService>();

			// IChatNotifier：透過 IHubContext<ChatHub> 對用戶/群組廣播 → 可 Singleton（IHubContext 執行緒安全）
			builder.Services.AddSingleton<IChatNotifier, SignalRChatNotifier>();

			// ------------------------------------------------------------
			// MVC / RazorPages / JSON 命名策略 & Anti-forgery
			// ------------------------------------------------------------
			builder.Services.AddControllersWithViews()
				// JSON 保留原本的屬性大小寫（不轉 camelCase）
				.AddJsonOptions(opt => { opt.JsonSerializerOptions.PropertyNamingPolicy = null; });

			builder.Services.AddRazorPages();

			// AJAX 的防偽 Token（前端請以 header "RequestVerificationToken" 帶入）
			builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

			// ------------------------------------------------------------
			// 其他輔助服務（不走 Identity 但仍可用的雜湊、Email、取用 HttpContext）
			// ------------------------------------------------------------
			builder.Services.AddHttpContextAccessor();

			// 保留他原本的服務（不要動）：他自己的 ICurrentUserService
			// 我們不覆蓋它，以免影響原先依賴；我們另走自己的 IAppCurrentUser。
			builder.Services.AddScoped<GamiPort.Services.ICurrentUserService, GamiPort.Services.CurrentUserService>();

			// 雜湊服務（升級舊明文密碼用）：IPasswordHasher<User>
			builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

			// 郵件發送（若尚未設定 SMTP，先改成 NullEmailSender 比較保險）
			builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

			// ------------------------------------------------------------
			// ★ 我方統一介面：IAppCurrentUser（集中讀取當前登入者資訊）
			//   讀取順序：AppUserId Claim -> NameIdentifier（純數字或 "U:<id>" 可解析）
			//            -> 備援呼叫 ILoginIdentity（必要時查一次 DB 對應）
			//   並使用 HttpContext.Items 做「同一請求快取」避免多次查詢。
			// ------------------------------------------------------------
			builder.Services.AddScoped<IAppCurrentUser, AppCurrentUser>();

			// 備援服務：ILoginIdentity -> ClaimFirstLoginIdentity
			// 說明：當 Claims 不完整（沒有 AppUserId、或 NameIdentifier 不能直接解析）時，
			//       AppCurrentUser 會呼叫它做一次較穩健的對應（必要時查 DB）。
			builder.Services.AddScoped<ILoginIdentity, ClaimFirstLoginIdentity>();

			// ------------------------------------------------------------
			// ★ SignalR（聊天室必備）
			//   1) Services：AddSignalR()
			//   2) Endpoints：MapHub<ChatHub>("/social_hub/chathub")
			//   這樣前端呼叫 /social_hub/chathub/negotiate 才不會 404。
			// ------------------------------------------------------------
			builder.Services.AddSignalR();


			// ★ SignalR（聊天室必備）— 開啟詳細錯誤與穩定心跳
			builder.Services.AddSignalR(options =>
			{
				options.EnableDetailedErrors = true;                 // 讓前端拿到更清楚的錯誤
				options.KeepAliveInterval = TimeSpan.FromSeconds(15);// 伺服器送 keep-alive 的頻率
				options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // 客端容忍逾時
			});


			// ------------------------------------------------------------
			// 建立 App
			// ------------------------------------------------------------
			var app = builder.Build();

			// ------------------------------------------------------------
			// HTTP Pipeline
			// ------------------------------------------------------------
			if (app.Environment.IsDevelopment())
			{
				// 顯示 EF 相關的開發頁、/migrations 端點
				app.UseMigrationsEndPoint();

				// 啟動時快速檢查 DB 連線（提早發現連線字串或權限問題）
				using var scope = app.Services.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();
				_ = db.Database.CanConnect(); // 回傳 bool；此處只為提早觸發連線測試
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();

			// 驗證一定在授權之前
			app.UseAuthentication();
			app.UseAuthorization();

			// MVC Controllers（含 Attribute Routing）
			app.MapControllers();

			// 傳統路由（先 Areas 再 default）
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			// Razor Pages（若你有使用）
			app.MapRazorPages();

			// ★ SignalR Hub 端點（路徑要與前端完全一致）
			// 前端若以 withUrl("/social_hub/chathub") 連線，就要映射同一路徑；
			// 注意：Hub 路徑不受 Areas 影響，是全域端點。
			app.MapHub<ChatHub>("/social_hub/chathub");

			app.Run();
		}
	}
}
