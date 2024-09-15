using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace GPA.Utils
{
    public static class LoadNativeLibraryExtension
    {
        public static void LoadDinkToPdfNativeLibrary(this IServiceCollection services, string rootPath)
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            var basePath = AppContext.BaseDirectory;
            var libraryPath = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                libraryPath = architecture == Architecture.X64
                    ? Path.Combine(basePath, "libs", "dinktopdflibs", "64 bit", "libwkhtmltox.dll")
                    : Path.Combine(basePath, "libs", "dinktopdflibs", "32 bit", "libwkhtmltox.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                libraryPath = architecture == Architecture.X64
                    ? Path.Combine(basePath, "libs", "dinktopdflibs", "64 bit", "libwkhtmltox.so")
                    : Path.Combine(basePath, "libs", "dinktopdflibs", "32 bit", "libwkhtmltox.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                libraryPath = architecture == Architecture.X64
                    ? Path.Combine(basePath, "libs", "dinktopdflibs", "64 bit", "libwkhtmltox.dylib")
                    : Path.Combine(basePath, "libs", "dinktopdflibs", "32 bit", "libwkhtmltox.dylib");
            }

            var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(libraryPath);

            // Inject logger to log
            var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DinkToPdf");
            logger.LogInformation("***********************************************************************************************************");
            logger.LogInformation("Native library loaded successfully.");
            logger.LogInformation(rootPath);
            logger.LogInformation(basePath);
        }
    }
}
