using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace GPA.Utils.Middleware
{
    public class ExceptionOption
    {
        public Type Exception { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public bool IncludeStackTrace { get; set; }
        public Func<string, string> MessageHandler { get; set; } = (message) => message;
    }

    public static class ExceptionHolderOptions
    {
        private static Dictionary<Type, ExceptionOption> _exceptionsOptions = new Dictionary<Type, ExceptionOption>();

        public static void AddExceptionHandler<T>(this IServiceCollection serviceCollection, HttpStatusCode statusCode, bool includeStackTrace = false, Func<string, string> messageHandler = null) where T : Exception
        {
            var exceptionOption = new ExceptionOption
            {
                Exception = typeof(T),
                StatusCode = statusCode,
                IncludeStackTrace = includeStackTrace
            };

            if (messageHandler is not null)
            {
                exceptionOption.MessageHandler = exceptionOption.MessageHandler;
            }

            _exceptionsOptions.TryAdd(typeof(T), exceptionOption);
        }

        public static string? ProcessException<T>(T exceptionType, HttpContext context) where T : Exception
        {
            context.Response.ContentType = "application/json";
            if (_exceptionsOptions.TryGetValue(exceptionType.GetType(), out var ex))
            {
                context.Response.StatusCode = (int)ex.StatusCode;
                return JsonSerializer.Serialize(new
                {
                    Message = ex.MessageHandler(exceptionType.Message),
                    Detailed = ex.IncludeStackTrace ? exceptionType.StackTrace : null
                });
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return JsonSerializer.Serialize(new
                {
                    Message = exceptionType.Message,
                });
            }
        }
    }
}
