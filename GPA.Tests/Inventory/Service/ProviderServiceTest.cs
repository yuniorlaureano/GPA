using AutoFixture;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.Inventory;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class ProviderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IProviderService _providerService;

        public ProviderServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependenyBuilder.GetServices();
            _providerService = _services.GetRequiredService<IProviderService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
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

            var dto = await _providerService.AddAsync(provider);
            var existing = await _providerService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
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

                await _providerService.AddAsync(provider);
            }

            var availables = await _providerService.GetAllAsync(new GPA.Common.DTOs.SearchDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
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

            var added = await _providerService.AddAsync(provider);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
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

            var added = await _providerService.AddAsync(provider);
            var existing = await _providerService.GetByIdAsync(added.Id.Value);

            existing.Name = "Modified Name";

            await _providerService.UpdateAsync(existing);

            var updated = await _providerService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Name, added.Name);
        }

        [Fact]
        public async Task ShouldDelete()
        {
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

            var added = await _providerService.AddAsync(provider);
            await _providerService.RemoveAsync(added.Id.Value);
            var existing = await _providerService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }
    }
}