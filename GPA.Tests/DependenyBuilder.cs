using GPA.Business.Common.Extensions;
using GPA.Business.Inventory.Extensions;
using GPA.Business.Invoice.Extensions;
using GPA.Business.Security.Extensions;
using GPA.Bussiness.Services.Common.Mappers;
using GPA.Bussiness.Services.Common.Validator;
using GPA.Bussiness.Services.Inventory.Mappers;
using GPA.Bussiness.Services.Inventory.Validator;
using GPA.Bussiness.Services.Invoice.Mappers;
using GPA.Bussiness.Services.Invoice.Validator;
using GPA.Bussiness.Services.Security.Mappers;
using GPA.Data;
using GPA.Data.Common.Extensions;
using GPA.Data.Inventory.Extensions;
using GPA.Data.Invoice.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests
{
    internal class DependenyBuilder
    {
        public static IServiceProvider GetServices()
        {
            var services = new ServiceCollection();
            var configurations = GetConfiguration();
            services.AddSingleton<IConfiguration>(configurations);

            services.AddDbContext<GPADbContext>(options =>
                            options.UseSqlServer(configurations.GetConnectionString("Default"), p => p.MigrationsAssembly("GPA.Api")));

            services.AddInventoryMappers();
            services.AddInventoryValidators();
            services.AddDataInventoryRepositories(configurations);
            services.AddBusinessInventoryServices();

            services.AddInvoiceMappers();
            services.AddInvoiceValidators();
            services.AddDataInvoiceRepositories();
            services.AddBusinessInvoiceServices();

            services.AddCommonMappers();
            services.AddCommonValidators();
            services.AddDataCommonRepositories();
            services.AddBusinessCommonServices();

            services.AddSecurityMappers();
            services.AddBusinessSecurityServices();

            return services.BuildServiceProvider();
        }

        public static IConfiguration GetConfiguration()
        {
            var configuration = new ConfigurationBuilder();
            var builtConfiguration = configuration
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            return builtConfiguration;
        }
    }
}
