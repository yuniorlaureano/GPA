using GPA.Utils.CodeGenerators;
using GPA.Utils.Exceptions;
using GPA.Utils.Middleware;
using GPA.Utils.Permissions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace GPA.Utils.Extensions
{
    public static class UtilExtensions
    {
        public static void AddUtils(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            var stackTraceOption = configuration.GetValue<bool?>("IncludeOnClientStackTrace");
            var includeStackTrace = stackTraceOption == true;

            services.AddScoped<IPermissionComparer, PermissionComparer>();
            services.AddExceptionHandler<AttachmentDeserializingException>(HttpStatusCode.Unauthorized, includeStackTrace);
            services.AddExceptionHandler<AttachmentNotFoundException>(HttpStatusCode.BadRequest, includeStackTrace);
            services.AddSingleton(new InvoiceCodeGenerator());
            services.AddSingleton(new ProductCodeGenerator());
        }
    }
}
