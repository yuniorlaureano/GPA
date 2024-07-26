using Microsoft.Extensions.DependencyInjection;

namespace GPA.Data.Network.Extensions
{
    public static class DataNetworkExtensions
    {
        public static void AddDataNetworkRepositories(this IServiceCollection services)
        {
            services.AddTransient<IEmailConfigurationRepository, EmailConfigurationRepository>();
        }
    }
}
