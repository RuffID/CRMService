namespace CRMService.Middleware
{
    public sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[Middleware] Request cancelled. Path={Path}, TraceId={TraceId}",
                    context.Request.Path, context.TraceIdentifier);
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Middleware] Unhandled exception. Path={Path}, TraceId={TraceId}",
                    context.Request.Path, context.TraceIdentifier);

                // Вернуть 500 (или маппить доменные исключения в 4xx/5xx)
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                // опционально JSON-ответ
                // await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error", traceId = context.TraceIdentifier });
            }
        }
    }
}
