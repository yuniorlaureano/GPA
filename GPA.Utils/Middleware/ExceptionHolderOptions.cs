using GPA.Utils.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace GPA.Utils.Middleware
{
    public interface IExceptionHandlerService
    {
        string? ProcessException<T>(T exceptionType, HttpContext context) where T : Exception;
    }

    public class ExceptionHandlerService : IExceptionHandlerService
    {
        private readonly bool _includeStackTrace;

        public ExceptionHandlerService(bool includeStackTrace = false)
        {
            _includeStackTrace = includeStackTrace;
        }

        public string? ProcessException<T>(T exception, HttpContext context) where T : Exception
        {
            context.Response.ContentType = "application/json";
            if (exception is IGPAException gPAException)
            {
                context.Response.StatusCode = (int)gPAException.StatusCode;
                return JsonSerializer.Serialize(new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = exception.Message,
                    Detail = _includeStackTrace ? exception.StackTrace : null
                });
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return JsonSerializer.Serialize(new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                Detail = _includeStackTrace ? exception.StackTrace : null
            });
        }
    }
}
