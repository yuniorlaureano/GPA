using GPA.Data;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Fixtures
{
    internal class CleanUpDbFixture
    {
        internal static CleanUpDbFixture Current = new();

        private CleanUpDbFixture()
        {
            var services = DependenyBuilder.GetServices();
            var context = services.GetRequiredService<GPADbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
