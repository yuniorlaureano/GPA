using Microsoft.Extensions.DependencyInjection;

namespace GPA.Data.Common.Extensions
{
    public static class DataCommonExtensions
    {
        public static void AddDataCommonRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUnitRepository, UnitRepository>();
        }
    }
}
