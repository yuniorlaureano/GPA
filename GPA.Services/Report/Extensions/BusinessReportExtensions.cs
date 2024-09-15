using DinkToPdf;
using DinkToPdf.Contracts;
using GPA.Business.Services.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Services.Report.Extensions
{
    public static class BusinessReportExtensions
    {
        public static void AddBusinessReportServices(this IServiceCollection services)
        {
            services.AddTransient<IReportExcel, ReportExcel>();
            services.AddTransient<IStockReportsService, InventoryService>();
            services.AddTransient<IReportPdfBase, ReportPdfBase>();

            services.AddSingleton<IConverter>(provider =>
            {
                return new SynchronizedConverter(new PdfTools());
            });
        }
    }
}
