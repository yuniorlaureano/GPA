using AutoFixture;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.Inventory;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class ItemServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IItemService _itemService;

        public ItemServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependenyBuilder.GetServices();
            _itemService = _services.GetRequiredService<IItemService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
            var item = _fixture
                .Build<ItemDto>()
                .Without(x => x.Id)
                .Create();

            var dto = await _itemService.AddAsync(item);
            var existing = await _itemService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
                var item = _fixture
                    .Build<ItemDto>()
                    .Without(x => x.Id)
                    .Create();

                await _itemService.AddAsync(item);
            }

            var availables = await _itemService.GetAllAsync(new GPA.Common.DTOs.SearchDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
            var item = _fixture
                .Build<ItemDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _itemService.AddAsync(item);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var item = _fixture
                .Build<ItemDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _itemService.AddAsync(item);
            var existing = await _itemService.GetByIdAsync(added.Id.Value);

            existing.Name = "Modified Name";

            await _itemService.UpdateAsync(existing);

            var updated = await _itemService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Name, added.Name);
        }

        [Fact]
        public async Task DeleteUpdate()
        {
            var item = _fixture
                .Build<ItemDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _itemService.AddAsync(item);
            await _itemService.RemoveAsync(added.Id.Value);
            var existing = await _itemService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }
    }
}