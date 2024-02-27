using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Data.Inventory.Extensions
{
    public static class DataInventoryExtensions
    {
        public static void AddDataInventoryRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IProductLocationRepository, ProductLocationRepository>();
            services.AddTransient<IProviderRepository, ProviderRepository>();
            services.AddTransient<IProviderAddressRepository, ProviderAddressRepository>();
            services.AddTransient<IStockRepository, StockRepository>();
        }
    }
}
