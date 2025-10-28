using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Text.Json;

namespace GameSpace.Areas.MiniGame.Filters
{
    public class MiniGameProblemDetailsFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception != null)
            {
                var problemDetails = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "An error occurred while processing your request.",
                    status = GetStatusCode(context.Exception),
                    detail = GetErrorMessage(context.Exception),
                    instance = context.HttpContext.Request.Path
                };

                context.Result = new JsonResult(problemDetails)
                {
                    StatusCode = GetStatusCode(context.Exception)
                };

                context.HttpContext.Response.ContentType = "application/problem+json";
                context.ExceptionHandled = true;
            }
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                NotImplementedException => (int)HttpStatusCode.NotImplemented,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private string GetErrorMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => "參數不能為空",
                ArgumentException => "參數錯誤",
                UnauthorizedAccessException => "未授權訪問",
                KeyNotFoundException => "找不到指定的資源",
                _ => "發生內部錯誤，請稍後再試"
            };
        }
    }
}

