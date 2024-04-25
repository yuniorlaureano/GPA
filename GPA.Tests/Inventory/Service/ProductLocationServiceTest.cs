using AutoFixture;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.Inventory;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class ProductLocationServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IProductLocationService _productLocationService;

        public ProductLocationServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependenyBuilder.GetServices();
            _productLocationService = _services.GetRequiredService<IProductLocationService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
            var productLocation = _fixture
                .Build<ProductLocationDto>()
                .With(x => x.Code, Guid.NewGuid().ToString().Substring(0,20))
                .Without(x => x.Id)
                .Create();

            var dto = await _productLocationService.AddAsync(productLocation);
            var existing = await _productLocationService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
                var productLocation = _fixture
                    .Build<ProductLocationDto>()
                    .With(x => x.Code, Guid.NewGuid().ToString().Substring(0, 20))
                    .Without(x => x.Id)
                    .Create();

                await _productLocationService.AddAsync(productLocation);
            }

            var availables = await _productLocationService.GetAllAsync(new GPA.Common.DTOs.SearchDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
            var productLocation = _fixture
                .Build<ProductLocationDto>()
                .With(x => x.Code, Guid.NewGuid().ToString().Substring(0, 20))
                .Without(x => x.Id)
                .Create();

            var added = await _productLocationService.AddAsync(productLocation);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var productLocation = _fixture
                .Build<ProductLocationDto>()
                .With(x => x.Code, Guid.NewGuid().ToString().Substring(0, 20))
                .Without(x => x.Id)
                .Create();

            var added = await _productLocationService.AddAsync(productLocation);
            var existing = await _productLocationService.GetByIdAsync(added.Id.Value);

            existing.Name = "Modified Name";

            await _productLocationService.UpdateAsync(existing);

            var updated = await _productLocationService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Name, added.Name);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            var productLocation = _fixture
                .Build<ProductLocationDto>()
                .With(x => x.Code, Guid.NewGuid().ToString().Substring(0, 20))
                .Without(x => x.Id)
                .Create();

            var added = await _productLocationService.AddAsync(productLocation);
            await _productLocationService.RemoveAsync(added.Id.Value);
            var existing = await _productLocationService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }
    }
}