using GPA.Business.Services.General;
using GPA.Dtos.General;
using GPA.Services.General;
using GPA.Services.General.Email;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Business.General.Extensions
{
    public static class BusinessCommonExtensions
    {
        public static void AddBusinessCommonServices(this IServiceCollection services)
        {
            services.AddTransient<IUnitService, UnitService>();
            services.AddTransient<IEmailService, SmtpEmailService>();
            services.AddTransient<IEmailService, SendGridEmailService>();
            services.AddTransient<IEmailServiceFactory, EmailServiceFactory>();
            services.AddTransient<IEmailProviderService, EmailProviderService>();
        }
    }
}
