using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Dtos.Cache;
using GPA.Entities.Report;
using GPA.Utils.Caching;
using GPA.Utils.CodeGenerators;
using GPA.Utils.Middleware;
using GPA.Utils.Permissions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Utils.Extensions
{
    public static class UtilExtensions
    {
        public static void AddUtils(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            var stackTraceOption = configuration.GetValue<bool?>("IncludeOnClientStackTrace");
            var includeStackTrace = stackTraceOption == true;

            services.AddScoped<IPermissionComparer, PermissionComparer>();
            services.AddSingleton<IExceptionHandlerService, ExceptionHandlerService>(services => new());
            services.AddSingleton(new InvoiceCodeGenerator());
            services.AddSingleton(new ProductCodeGenerator());
            services.AddSingleton<IGenericCache<string>, GenericCache<string>>();
            services.AddSingleton<IGenericCache<ReportTemplate>, GenericCache<ReportTemplate>>();
            services.AddSingleton<IGenericCache<ResponseDto<CategoryDto>>, GenericCache<ResponseDto<CategoryDto>>>();
            services.AddSingleton<IGenericCache<ResponseDto<ReasonDto>>, GenericCache<ResponseDto<ReasonDto>>>();
            services.AddSingleton<IGenericCache<UserPermissionProfileCache>, GenericCache<UserPermissionProfileCache>>();
        }
    }
}
