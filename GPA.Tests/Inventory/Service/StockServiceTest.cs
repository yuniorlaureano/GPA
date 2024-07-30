using AutoFixture;
using GPA.Business.Services.General;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.General;
using GPA.Common.DTOs.Inventory;
using GPA.Entities.General;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class StockServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IStockService _stockService;
        private readonly ICategoryService _categoryService;
        private readonly IProductLocationService _productLocationService;
        private readonly IProductService _productService;
        private readonly IUnitService _unitService;

        private readonly IReasonService _seasonService;
        private readonly IStoreService _storeService;
        private readonly IProviderService _providerService;



        public StockServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _stockService = _services.GetRequiredService<IStockService>();
            _productService = _services.GetRequiredService<IProductService>();
            _productLocationService = _services.GetRequiredService<IProductLocationService>();
            _categoryService = _services.GetRequiredService<ICategoryService>();
            _unitService = _services.GetRequiredService<IUnitService>();

            _seasonService = _services.GetRequiredService<IReasonService>();
            _storeService = _services.GetRequiredService<IStoreService>();
            _providerService = _services.GetRequiredService<IProviderService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {

            var productDependencies = await ProductCreationDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .With(x => x.Type, new Random().Next(1, 2))
                .With(x => x.ExpirationDate, new DetailedDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                .Without(x => x.Id)
                .Without(x => x.Addons)
                .Create();

            var dto = await _productService.AddAsync(product);

            var stockDependencies = await StockCreationDependencies();

            var stockDetails = _fixture
                .Build<StockCreationDetailDto>()
                .With(x => x.ProductId, dto.Id)
                .Create();

            var stock = _fixture
                .Build<StockCreationDto>()
                .With(x => x.TransactionType, (int)TransactionType.Input)
                .With(x => x.StoreId, stockDependencies.StoreId)
                .With(x => x.ProviderId, stockDependencies.ProviderId)
                .With(x => x.ReasonId, stockDependencies.ReasonId)
                .With(x => x.StockDetails, new List<StockCreationDetailDto> { stockDetails })
                .With(x => x.Date, new DetailedDate(2023, 1, 1))
                .Without(x => x.Id)
                .Create();

            var stockDto = await _stockService.AddAsync(stock);
            var existing = await _stockService.GetByIdAsync(stockDto.Id.Value);

            Assert.Equal(stockDto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            var productDependencies = await ProductCreationDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .With(x => x.Type, new Random().Next(1, 2))
                .With(x => x.ExpirationDate, new DetailedDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                .Without(x => x.Id)
                .Without(x => x.Addons)
                .Create();

            var dto = await _productService.AddAsync(product);

            var stockDependencies = await StockCreationDependencies();

            var stockDetails = _fixture
                .Build<StockCreationDetailDto>()
                .With(x => x.ProductId, dto.Id)
                .Create();

            for (int i = 0; i < 3; i++)
            {
                var stock = _fixture
                .Build<StockCreationDto>()
                .With(x => x.TransactionType, (int)TransactionType.Input)
                .With(x => x.StoreId, stockDependencies.StoreId)
                .With(x => x.ProviderId, stockDependencies.ProviderId)
                .With(x => x.ReasonId, stockDependencies.ReasonId)
                .With(x => x.StockDetails, new List<StockCreationDetailDto> { stockDetails })
                .With(x => x.Date, new DetailedDate(2023, 1, 1))
                .Without(x => x.Id)
                .Create();

                await _stockService.AddAsync(stock);
            }

            var availables = await _stockService.GetAllAsync(new GPA.Common.DTOs.SearchDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
            var productDependencies = await ProductCreationDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .With(x => x.Type, new Random().Next(1, 2))
                .With(x => x.ExpirationDate, new DetailedDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                .Without(x => x.Id)
                .Without(x => x.Addons)
                .Create();

            var dto = await _productService.AddAsync(product);

            var stockDependencies = await StockCreationDependencies();

            var stockDetails = _fixture
                .Build<StockCreationDetailDto>()
                .With(x => x.ProductId, dto.Id)
                .Create();

            var stock = _fixture
                .Build<StockCreationDto>()
                .With(x => x.TransactionType, (int)TransactionType.Input)
                .With(x => x.StoreId, stockDependencies.StoreId)
                .With(x => x.ProviderId, stockDependencies.ProviderId)
                .With(x => x.ReasonId, stockDependencies.ReasonId)
                .With(x => x.StockDetails, new List<StockCreationDetailDto> { stockDetails })
                .With(x => x.Date, new DetailedDate(2023, 1, 1))
                .Without(x => x.Id)
                .Create();

            var added = await _stockService.AddAsync(stock);

            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var productDependencies = await ProductCreationDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .With(x => x.Type, new Random().Next(1, 2))
                .With(x => x.ExpirationDate, new DetailedDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                .Without(x => x.Id)
                .Without(x => x.Addons)
                .Create();

            var dto = await _productService.AddAsync(product);

            var stockDependencies = await StockCreationDependencies();

            var stockDetails = _fixture
                .Build<StockCreationDetailDto>()
                .With(x => x.ProductId, dto.Id)
                .Create();

            var stock = _fixture
                .Build<StockCreationDto>()
                .With(x => x.TransactionType, (int)TransactionType.Input)
                .With(x => x.StoreId, stockDependencies.StoreId)
                .With(x => x.ProviderId, stockDependencies.ProviderId)
                .With(x => x.ReasonId, stockDependencies.ReasonId)
                .With(x => x.Status, (int)StockStatus.Draft)
                .With(x => x.StockDetails, new List<StockCreationDetailDto> { stockDetails })
                .With(x => x.Date, new DetailedDate(2023, 1, 1))
                .Without(x => x.Id)
                .Create();

            var added = await _stockService.AddAsync(stock);

            stock.Id = added.Id.Value;
            stock.Description = "Modified Description";

            await _stockService.UpdateInputAsync(stock);

            var updated = await _stockService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Description, added.Description);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            var productDependencies = await ProductCreationDependencies();

            var product = _fixture
                .Build<ProductCreationDto>()
                .With(x => x.Code)
                .With(x => x.Photo)
                .With(x => x.UnitId, productDependencies.UnitId)
                .With(x => x.CategoryId, productDependencies.CategoryId)
                .With(x => x.ProductLocationId, productDependencies.LocationId)
                .With(x => x.Type, new Random().Next(1, 2))
                .With(x => x.ExpirationDate, new DetailedDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                .With(x => x.Type, new Random().Next(1, 2))
                .With(x => x.ExpirationDate, new DetailedDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                .Without(x => x.Id)
                .Without(x => x.Addons)
                .Create();

            var dto = await _productService.AddAsync(product);


            var stockDependencies = await StockCreationDependencies();

            var stockDetails = _fixture
                .Build<StockCreationDetailDto>()
                .With(x => x.ProductId, dto.Id)
                .Create();

            var stock = _fixture
                .Build<StockCreationDto>()
                .With(x => x.TransactionType, (int)TransactionType.Input)
                .With(x => x.StoreId, stockDependencies.StoreId)
                .With(x => x.ProviderId, stockDependencies.ProviderId)
                .With(x => x.ReasonId, stockDependencies.ReasonId)
                .With(x => x.StockDetails, new List<StockCreationDetailDto> { stockDetails })
                .With(x => x.Date, new DetailedDate(2023, 1, 1))
                .Without(x => x.Id)
                .Create();

            var added = await _stockService.AddAsync(stock);


            await _stockService.RemoveAsync(added.Id.Value);
            var existing = await _stockService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }

        private async Task<(Guid LocationId, Guid CategoryId, Guid UnitId)> ProductCreationDependencies()
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

        private async Task<(int ReasonId, Guid StoreId, Guid ProviderId)> StockCreationDependencies()
        {
            var reason = _fixture
                .Build<ReasonDto>()
                .Without(x => x.Id)
                .Create();

            var store = _fixture
               .Build<StoreDto>()
               .Without(x => x.Id)
               .Create();

            var provider = _fixture
                .Build<ProviderDto>()
                .With(x => x.Name, Guid.NewGuid().ToString().Substring(20))
                .With(x => x.Phone, Guid.NewGuid().ToString().Substring(20))
                .With(x => x.Email, Guid.NewGuid().ToString().Substring(20))
                .With(x => x.Street, Guid.NewGuid().ToString().Substring(20))
                .With(x => x.BuildingNumber, Guid.NewGuid().ToString().Substring(20))
                .With(x => x.City, Guid.NewGuid().ToString().Substring(20))
                .With(x => x.State, Guid.NewGuid().ToString().Substring(20))
                .With(x => x.Country, Guid.NewGuid().ToString().Substring(20))
                .With(x => x.PostalCode, Guid.NewGuid().ToString().Substring(20))
                .Without(x => x.Id)
                .Create();

            var reasonResult = await _seasonService.AddAsync(reason);
            var storeResult = await _storeService.AddAsync(store);
            var providerResult = await _providerService.AddAsync(provider);

            return (
                reasonResult.Id.Value,
                storeResult.Id.Value,
                providerResult.Id.Value
            );
        }
    }
}