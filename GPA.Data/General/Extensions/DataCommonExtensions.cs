using Microsoft.Extensions.DependencyInjection;

namespace GPA.Data.General.Extensions
{
    public static class DataCommonExtensions
    {
        public static void AddDataCommonRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUnitRepository, UnitRepository>();
            services.AddTransient<IEmailConfigurationRepository, EmailConfigurationRepository>();
            services.AddTransient<IBlobStorageConfigurationRepository, BlobStorageConfigurationRepository>();
        }
    }
}
