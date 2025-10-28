using GameSpace.Areas.MiniGame.Services;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Filters
{
    public class MiniGameAdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            
            // 檢查是否已登入
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Home", new { area = "MiniGame" });
                return;
            }

            // 檢查是否為管理員
            var managerIdClaim = httpContext.User.FindFirst("ManagerId");
            if (managerIdClaim == null)
            {
                context.Result = new RedirectToActionResult("Login", "Home", new { area = "MiniGame" });
                return;
            }

            // 檢查管理員權限和狀態
            if (int.TryParse(managerIdClaim.Value, out int managerId))
            {
                var dbContext = httpContext.RequestServices.GetRequiredService<GameSpacedatabaseContext>();
                var manager = dbContext.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefault(m => m.ManagerId == managerId);

                if (manager == null)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "MiniGame" });
                    return;
                }

                // 檢查管理員是否有任何角色權限
                if (manager.ManagerRoles == null || !manager.ManagerRoles.Any())
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "MiniGame" });
                    return;
                }
            }
        }
    }
}

