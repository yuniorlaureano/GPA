using AutoFixture;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.Inventory;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class StoreServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IStoreService _storeService;

        public StoreServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _storeService = _services.GetRequiredService<IStoreService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
            var store = _fixture
                .Build<StoreDto>()
                .Without(x => x.Id)
                .Create();

            var dto = await _storeService.AddAsync(store);
            var existing = await _storeService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
                var store = _fixture
                    .Build<StoreDto>()
                    .Without(x => x.Id)
                    .Create();

                await _storeService.AddAsync(store);
            }

            var availables = await _storeService.GetAllAsync(new GPA.Common.DTOs.SearchDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
            var store = _fixture
                .Build<StoreDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _storeService.AddAsync(store);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var store = _fixture
                .Build<StoreDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _storeService.AddAsync(store);
            var existing = await _storeService.GetByIdAsync(added.Id.Value);

            existing.Name = "Modified Name";

            await _storeService.UpdateAsync(existing);

            var updated = await _storeService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Name, added.Name);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            var store = _fixture
                .Build<StoreDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _storeService.AddAsync(store);
            await _storeService.RemoveAsync(added.Id.Value);
            var existing = await _storeService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }
    }
}