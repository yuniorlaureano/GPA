using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace GPA.Utils
{
    public static class CustomAssemblyLoadContext
    {
        public static void LoadDinkToPdfNativeLibrary(this IServiceCollection services)
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

            if (!string.IsNullOrEmpty(libraryPath))
            {
                NativeLibrary.Load(libraryPath);
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported platform or architecture.");
            }
        }
    }
}
