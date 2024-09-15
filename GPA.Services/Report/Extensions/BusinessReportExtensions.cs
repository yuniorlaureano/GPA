using DinkToPdf;
using DinkToPdf.Contracts;
using GPA.Business.Services.Inventory;
using GPA.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

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
                context.LoadUnmanagedLibrary(LoadNativeLibrary());
                return new SynchronizedConverter(new PdfTools());
            });
        }

        private static string LoadNativeLibrary()
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var libraryPath = string.Empty;

            if (architecture == Architecture.X64)
            {
                libraryPath = Path.Combine(
                    basePath,
                    "Report",
                    "dinktopdflibs",
                    "v0.12.4",
                    "64 bit",
                    LoadDinkToPdfDllBasedOnOS());
            }
            else if (architecture == Architecture.X86)
            {
                libraryPath = Path.Combine(
                    basePath,
                    "Report",
                    "dinktopdflibs",
                    "v0.12.4", "32 bit",
                    LoadDinkToPdfDllBasedOnOS());
            }

            if (!string.IsNullOrEmpty(libraryPath))
            {
                return libraryPath;
            }
            throw new Exception("Error loading dinktopdf library");
        }

        private static string LoadDinkToPdfDllBasedOnOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "libwkhtmltox.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "libwkhtmltox.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "libwkhtmltox.dylib";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
