using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace GPA.Utils
{
    public static class LoadNativeLibraryExtension
    {
        public static void LoadDinkToPdfNativeLibrary(this IServiceCollection services, string rootPath)
        {
            var dll = "libwkhtmltox.dll";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                dll = "libwkhtmltox.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                dll = "libwkhtmltox.dylib";
            }

            var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(Path.Combine(AppContext.BaseDirectory, dll));
        }
    }
}
