using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Filters
{
    public class MiniGameAdminOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 檢查是否已認證
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            // 檢查是否有管理員權限
            if (!context.HttpContext.User.HasClaim("IsManager", "true"))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}

