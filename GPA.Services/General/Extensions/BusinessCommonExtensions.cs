using GPA.Business.Services.General;
using GPA.Dtos.General;
using GPA.Services.General;
using GPA.Services.General.BlobStorage;
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
            services.AddTransient<IBlobStorageConfigurationService, BlobStorageConfigurationService>();
            services.AddTransient<IBlobStorageService, AWSS3Service>();
            services.AddTransient<IBlobStorageService, GCPBucketService>();
            services.AddTransient<IBlobStorageService, AzureBlobService>();
            services.AddTransient<IBlobStorageServiceFactory, BlobStorageServiceFactory>();
            services.AddTransient<IPrintService, PrintService>();
        }
    }
}
