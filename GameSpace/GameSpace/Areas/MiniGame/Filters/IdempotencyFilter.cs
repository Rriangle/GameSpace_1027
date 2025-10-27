using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace GameSpace.Areas.MiniGame.Filters
{
    /// <summary>
    /// 冪等性過濾器 - 基於 X-Idempotency-Key header 實作 60秒防重機制
    /// </summary>
    public class IdempotencyFilter : ActionFilterAttribute
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<IdempotencyFilter> _logger;

        public IdempotencyFilter(IMemoryCache cache, ILogger<IdempotencyFilter> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 在 Action 執行前檢查冪等性
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 僅對 POST/PUT/PATCH/DELETE 方法檢查冪等性
            var method = context.HttpContext.Request.Method;
            if (method != "POST" && method != "PUT" && method != "PATCH" && method != "DELETE")
            {
                await next();
                return;
            }

            // 檢查 X-Idempotency-Key header
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey) || 
                string.IsNullOrWhiteSpace(idempotencyKey))
            {
                _logger.LogWarning("缺少 X-Idempotency-Key header: {Method} {Path}, TraceID={TraceID}",
                    method, context.HttpContext.Request.Path, context.HttpContext.TraceIdentifier);

                context.Result = new BadRequestObjectResult(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "缺少冪等性金鑰",
                    status = 400,
                    detail = "寫入操作必須提供 X-Idempotency-Key header",
                    instance = context.HttpContext.Request.Path.Value,
                    traceId = context.HttpContext.TraceIdentifier
                });
                return;
            }

            var key = idempotencyKey.ToString();
            var cacheKey = $"idempotency:{key}";

            // 檢查是否在 60秒內重複請求
            if (_cache.TryGetValue(cacheKey, out var cachedResult))
            {
                _logger.LogWarning("檢測到重複請求: IdempotencyKey={IdempotencyKey}, Method={Method}, Path={Path}, TraceID={TraceID}",
                    key, method, context.HttpContext.Request.Path, context.HttpContext.TraceIdentifier);

                // 返回快取的結果或 409 Conflict
                context.Result = new ConflictObjectResult(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    title = "重複請求",
                    status = 409,
                    detail = "60秒內檢測到相同的冪等性金鑰",
                    instance = context.HttpContext.Request.Path.Value,
                    traceId = context.HttpContext.TraceIdentifier,
                    idempotencyKey = key
                });
                return;
            }

            // 執行 Action
            var executedContext = await next();

            // 如果執行成功，快取結果 60秒
            if (executedContext.Result is OkObjectResult || 
                executedContext.Result is CreatedResult || 
                executedContext.Result is NoContentResult ||
                (executedContext.Result is RedirectToActionResult && executedContext.Exception == null))
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60),
                    Priority = CacheItemPriority.Low
                };

                _cache.Set(cacheKey, new
                {
                    timestamp = DateTime.UtcNow,
                    method = method,
                    path = context.HttpContext.Request.Path.Value,
                    statusCode = context.HttpContext.Response.StatusCode
                }, cacheOptions);

                _logger.LogInformation("冪等性金鑰已快取: IdempotencyKey={IdempotencyKey}, Method={Method}, Path={Path}, TraceID={TraceID}",
                    key, method, context.HttpContext.Request.Path, context.HttpContext.TraceIdentifier);
            }
        }
    }
}

