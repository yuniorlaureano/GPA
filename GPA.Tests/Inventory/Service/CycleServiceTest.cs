using AutoFixture;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class CycleServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IStockCycleRepository _stockCycleRepository;

        public CycleServiceTest()
        {
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _stockCycleRepository = _services.GetRequiredService<IStockCycleRepository>();
        }

        //[Fact()]
        [Fact(Skip = "Debug only")]
        public async Task ShouldGetOne()
        {
            var cycle = new StockCycle()
            {
                StartDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),
                EndDate = new DateOnly(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, DateTime.Now.AddMonths(1).Day),
                Note = "This is just a test "
            };

            var dto = await _stockCycleRepository.OpenCycleAsync(cycle);
        }
    }
}