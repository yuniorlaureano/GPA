using GPA.Utils.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Utils.Extensions
{
    public static class UtilExtensions
    {
        public static void AddUtils(this IServiceCollection services)
        {
            services.AddScoped<IPermissionComparer, PermissionComparer>();
        }
    }
}
