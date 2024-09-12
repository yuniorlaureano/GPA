using GPA.Business.Services.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Services.Report.Extensions
{
    public static class BusinessReportExtensions
    {
        public static void AddBusinessReportServices(this IServiceCollection services)
        {
            services.AddTransient<IReportExcel, ReportExcel>();
            services.AddTransient<IStockReportsService, StockReportsService>();
        }
    }
}
