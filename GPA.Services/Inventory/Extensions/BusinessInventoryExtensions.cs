using GPA.Business.Services.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Business.Inventory.Extensions
{
    public static class BusinessInventoryExtensions
    {
        public static void AddBusinessInventoryServices(this IServiceCollection services)
        {
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IProductLocationService, ProductLocationService>();
            services.AddTransient<IProviderService, ProviderService>();
            services.AddTransient<IStockService, StockService>();
            services.AddTransient<IStoreService, StoreService>();
            services.AddTransient<IReasonService, ReasonService>();
            services.AddTransient<IStockCycleService, StockCycleService>();

        }
    }
}
