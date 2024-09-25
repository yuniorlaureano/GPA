using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GPA.Utils.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IExceptionHandlerService _exceptionHandlerService;
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IExceptionHandlerService exceptionHandlerService)
        {
            _next = next;
            _logger = logger;
            _exceptionHandlerService = exceptionHandlerService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var result = _exceptionHandlerService.ProcessException(exception, context);
            return context.Response.WriteAsync(result);
        }
    }
}
