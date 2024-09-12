using GPA.Data.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Data.Report.Extensions
{
    public static class DataReportExtensions
    {
        public static void AddDataReportRepositories(this IServiceCollection services)
        {
            services.AddTransient<IStockReportRepository, StockReportRepository>();
        }
    }
}
