using AutoFixture;
using GPA.Business.Services.Common;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.Common;
using GPA.Common.DTOs.Inventory;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class ProductServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IProductService _productService;
        private readonly IProductLocationService _productLocationService;
        private readonly ICategoryService _categoryService;
        private readonly IUnitService _unitService;

        public ProductServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependenyBuilder.GetServices();
            _productService = _services.GetRequiredService<IProductService>();
            _productLocationService = _services.GetRequiredService<IProductLocationService>();
            _categoryService = _services.GetRequiredService<ICategoryService>();
            _unitService = _services.GetRequiredService<IUnitService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {

            var productDependencies = await CreateDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .Without(x => x.Id)
                .Create();

            var dto = await _productService.AddAsync(product);
            var existing = await _productService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            var productDependencies = await CreateDependencies();
            
            for (int i = 0; i < 3; i++)
            {
                var product = _fixture
                    .Build<ProductCreationDto>()
                    .With(x => x.Code)
                    .With(x => x.Photo)
                    .With(x => x.UnitId, productDependencies.UnitId)
                    .With(x => x.CategoryId, productDependencies.CategoryId)
                    .With(x => x.ProductLocationId, productDependencies.LocationId)
                    .Without(x => x.Id)
                    .Create();

                await _productService.AddAsync(product);
            }

            var availables = await _productService.GetAllAsync(new GPA.Common.DTOs.SearchDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
            var productDependencies = await CreateDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .Without(x => x.Id)
                .Create();

            var added = await _productService.AddAsync(product);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var productDependencies = await CreateDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .Without(x => x.Id)
                .Create();

            var added = await _productService.AddAsync(product);

            product.Id = added.Id.Value;
            product.Photo = "Modified Photo";

            await _productService.UpdateAsync(product);

            var updated = await _productService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Photo, added.Photo);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            var productDependencies = await CreateDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .Without(x => x.Id)
                .Create();

            var added = await _productService.AddAsync(product);
            await _productService.RemoveAsync(added.Id.Value);
            var existing = await _productService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }


        private async Task<(Guid LocationId, Guid CategoryId, Guid UnitId)> CreateDependencies()
        {
            var productLocation = _fixture
                .Build<ProductLocationDto>()
                .With(x => x.Code, Guid.NewGuid().ToString().Substring(0, 20))
                .Without(x => x.Id)
                .Create();

            var category = _fixture
               .Build<CategoryDto>()
               .Without(x => x.Id)
               .Create();

            var unit = _fixture
                .Build<UnitDto>()
                .Without(x => x.Id)
                .Create();

            var productLocationResult = await _productLocationService.AddAsync(productLocation);
            var categoryResult = await _categoryService.AddAsync(category);
            var unitResult = await _unitService.AddAsync(unit);

            //await Task.WhenAll(itemResult, productLocationResult, categoryResult, unitResult);

            return (
                productLocationResult.Id.Value,
                categoryResult.Id.Value,
                unitResult.Id.Value
            );
        }
    }
}