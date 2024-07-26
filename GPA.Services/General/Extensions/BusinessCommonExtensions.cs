using GPA.Business.Services.General;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Business.General.Extensions
{
    public static class BusinessCommonExtensions
    {
        public static void AddBusinessCommonServices(this IServiceCollection services)
        {
            services.AddTransient<IUnitService, UnitService>();
        }
    }
}
