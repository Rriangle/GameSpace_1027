// ---- 服務命名空間（一般 using）----
// ---- MiniGame Area ----
using GameSpace.Areas.MiniGame.config;

// ---- 社群 Hub / 過濾器 / 共用登入 ----
using GameSpace.Areas.social_hub.Auth;          // ★ IUserContextReader, AuthConstants
using GameSpace.Areas.social_hub.Hubs;
using GameSpace.Areas.social_hub.Permissions;
using GameSpace.Data;
using GameSpace.Infrastructure.Login;
using GameSpace.Infrastructure.Time;
using GameSpace.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Connections; // for HttpTransportType
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// ---- ASP.NET Core 路由/DI（為路由列印診斷用）----
using Microsoft.EntityFrameworkCore;

// ---- 型別別名（避免撞名）----
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;
using INotificationServiceAlias = GameSpace.Areas.social_hub.Services.INotificationService;
using ManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Permissions.ManagerPermissionService;
using MuteFilterAlias = GameSpace.Areas.social_hub.Services.MuteFilter;
using NotificationServiceAlias = GameSpace.Areas.social_hub.Services.NotificationService;

namespace GameSpace
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// ========== 1) 組態與連線字串 ==========
			var identityConn = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// ========== 2) DbContexts ==========
			builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(identityConn));
			builder.Services.AddDbContext<GameSpacedatabaseContext>(opt => opt.UseSqlServer(gameSpaceConn));
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// ========== 3) Identity ==========
			builder.Services
				.AddDefaultIdentity<IdentityUser>(opt => opt.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// MVC 預設會加這行
			builder.Services.AddControllersWithViews();


			// ========== 4) MVC / Razor ==========
			builder.Services.AddControllersWithViews(o =>
			{
				o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
				// 防止瀏覽器回放舊頁（切帳號時 Sidebar 不會回放舊 HTML）
				o.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
			});
			builder.Services.AddRazorPages();

			// ========== 5) SignalR ==========
			builder.Services.AddSignalR();

			// ========== 6) MiniGame Area 服務 ==========
			builder.Services.AddMiniGameServices(builder.Configuration);

			// ========== 7) social_hub 相關服務 ==========
			builder.Services.AddMemoryCache();

			builder.Services.Configure<GameSpace.Areas.social_hub.Services.MuteFilterOptions>(o =>
			{
				o.MaskStyle = GameSpace.Areas.social_hub.Services.MaskStyle.Asterisks;
				o.FixedLabel = "【封鎖】";
				o.FuzzyBetweenCjkChars = true;
				o.MaskExactLength = false;
				o.CacheTtlSeconds = 30;
				o.UsePerWordReplacement = true;
			});

			// 時鐘（App 時區）
			builder.Services.AddSingleton<IAppClock>(sp => new AppClock(TimeZones.Taipei));

			// 穢語過濾 / 通知
			builder.Services.AddScoped<IMuteFilterAlias, MuteFilterAlias>();
			builder.Services.AddScoped<INotificationServiceAlias, NotificationServiceAlias>();

			// ★ 必須：HttpContext
			builder.Services.AddHttpContextAccessor();

			// ★ 這裡改成 Scoped（或 Transient），絕對不要 Singleton
			builder.Services.AddScoped<IUserContextReader, UserContextReader>();

			// 權限服務（Gate + 細權限）
			builder.Services.AddScoped<IManagerPermissionService, ManagerPermissionServiceAlias>();

			// ========== 8) CORS（可選） ==========
			var corsOrigins = builder.Configuration.GetSection("Cors:Chat:Origins").Get<string[]>();
			if (corsOrigins is { Length: > 0 })
			{
				builder.Services.AddCors(opt =>
				{
					opt.AddPolicy("chat", p => p
						.WithOrigins(corsOrigins)
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials());
				});
			}

			// ========== 9) Session ==========
			builder.Services.AddSession(opt =>
			{
				opt.IdleTimeout = TimeSpan.FromMinutes(30);
				opt.Cookie.HttpOnly = true;
				opt.Cookie.IsEssential = true;
				opt.Cookie.SameSite = SameSiteMode.Lax;
			});

			// ========== 10) 共用登入介面 ==========
			builder.Services.AddScoped<ILoginIdentity, CookieAndAdminCookieLoginIdentity>();

			// ========== 11) 後台 Cookie 方案（AdminCookie） ==========
			builder.Services.AddAuthentication(options => { /* 保留 Identity 預設 */ })
			.AddCookie(AuthConstants.AdminCookieScheme, opt =>
			{
				opt.LoginPath = "/Login";
				opt.LogoutPath = "/Login/Logout";
				opt.AccessDeniedPath = "/Login/Denied";
				opt.Cookie.Name = "GameSpace.Admin";
				opt.SlidingExpiration = true;
				opt.ExpireTimeSpan = TimeSpan.FromHours(4);

				// AJAX 401/403 不重導
				opt.Events = new CookieAuthenticationEvents
				{
					OnRedirectToLogin = ctx =>
					{
						var isAjax =
							string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
							|| ctx.Request.Headers["Accept"].Any(v => v?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
							|| ctx.Request.Path.StartsWithSegments("/Login/Me", StringComparison.OrdinalIgnoreCase);

						if (isAjax)
						{
							ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
							return Task.CompletedTask;
						}
						ctx.Response.Redirect(ctx.RedirectUri);
						return Task.CompletedTask;
					},
					OnRedirectToAccessDenied = ctx =>
					{
						var isAjax =
							string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
							|| ctx.Request.Headers["Accept"].Any(v => v?.Contains("json", StringComparison.OrdinalIgnoreCase) == true);

						if (isAjax)
						{
							ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
							return Task.CompletedTask;
						}
						ctx.Response.Redirect(ctx.RedirectUri);
						return Task.CompletedTask;
					}
				};
			});

		

			// ========== 12) 授權政策（需要就用） ==========
			builder.Services.AddAuthorization(options =>
			{
				// MiniGame Area 管理員政策（所有管理控制器必備）
				options.AddPolicy("AdminOnly", p => p.RequireClaim("IsManager", "true"));

				// Granular permission policies
				options.AddPolicy("CanManageShopping", p => p.RequireClaim("perm:Shopping", "true"));
				options.AddPolicy("CanAdmin", p => p.RequireClaim("perm:Admin", "true"));
				options.AddPolicy("CanMessage", p => p.RequireClaim("perm:Message", "true"));
				options.AddPolicy("CanUserStatus", p => p.RequireClaim("perm:UserStat", "true"));
				options.AddPolicy("CanPet", p => p.RequireClaim("perm:Pet", "true"));
				options.AddPolicy("CanCS", p => p.RequireClaim("perm:CS", "true"));
			});

			// ========== 13) Anti-Forgery 設定 ==========
			builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

			var app = builder.Build();

			// ========== 14) Middleware 管線 ==========
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Maintenance");
				// app.UseHsts();
			}

			app.UseStatusCodePagesWithReExecute("/Home/Http{0}");
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			if (corsOrigins is { Length: > 0 })
				app.UseCors("chat");

			app.UseCookiePolicy(new CookiePolicyOptions
			{
				MinimumSameSitePolicy = app.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
				Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
			});

			// ★ Session 要在 Authentication 之前
			app.UseSession();

			app.UseAuthentication();
			app.UseAuthorization();

			// ========== 15) 路由 ==========
			app.MapControllers();

			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapRazorPages();

			// ========== 16) SignalR ==========
			app.MapHub<ChatHub>("/social_hub/chatHub", opts =>
			{
				opts.Transports =
					HttpTransportType.WebSockets |
					HttpTransportType.ServerSentEvents |
					HttpTransportType.LongPolling;
			});

			// ========== 17) 啟動 ==========
			await app.RunAsync();
		}
	}
}
