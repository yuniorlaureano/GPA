using Microsoft.Extensions.DependencyInjection;

namespace GPA.Data.Security.Extensions
{
    public static class DataSecurityExtensions
    {
        public static void AddDataSecurityRepositories(this IServiceCollection services)
        {
            services.AddTransient<IGPAUserRepository, GPAUserRepository>();
            services.AddTransient<IGPAProfileRepository, GPAProfileRepository>();
        }
    }
}
