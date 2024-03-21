using GPA.Business.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Business.Common.Extensions
{
    public static class BusinessCommonExtensions
    {
        public static void AddBusinessCommonServices(this IServiceCollection services)
        {
            services.AddTransient<IUnitService, UnitService>();
        }
    }
}
