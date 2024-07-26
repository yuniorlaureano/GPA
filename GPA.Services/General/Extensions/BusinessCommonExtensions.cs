using GPA.Business.Services.General;
using GPA.Services.General.Email;
using GPA.Services.General;
using Microsoft.Extensions.DependencyInjection;
using GPA.Dtos.General;

namespace GPA.Business.General.Extensions
{
    public static class BusinessCommonExtensions
    {
        public static void AddBusinessCommonServices(this IServiceCollection services)
        {
            services.AddTransient<IUnitService, UnitService>();
            services.AddTransient<IEmailService, SmtpEmailService>();
            services.AddTransient<IEmailServiceFactory, EmailServiceFactory>();
            services.AddTransient<IEmailProviderService, EmailProviderService>();
        }
    }
}
