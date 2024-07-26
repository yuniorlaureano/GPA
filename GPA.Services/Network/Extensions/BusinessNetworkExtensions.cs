using GPA.Business.Services.Inventory;
using GPA.Dtos.Network;
using GPA.Services.Network.Email;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Business.Network.Extensions
{
    public static class BusinessNetworkExtensions
    {
        public static void AddBusinessNetworkServices(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, SmtpEmailService>();
            services.AddTransient<IEmailServiceFactory, EmailServiceFactory>();
            services.AddTransient<IEmailProviderService, EmailProviderService>();
        }
    }
}
