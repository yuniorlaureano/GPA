using DinkToPdf;
using DinkToPdf.Contracts;
using GPA.Business.Services.Inventory;
using GPA.Utils;
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
                var context = new CustomAssemblyLoadContext();
                context.LoadUnmanagedLibrary(
                    Path.Combine( AppDomain.CurrentDomain.BaseDirectory,
                    "Report", "dinktopdflibs", "v0.12.4", "64 bit",
                    "libwkhtmltox.dll"));
                return new SynchronizedConverter(new PdfTools());
            });
        }
    }
}
