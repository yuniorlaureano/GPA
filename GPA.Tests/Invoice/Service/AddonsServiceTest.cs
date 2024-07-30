using AutoFixture;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Invoice.Service
{
    public class AddonsServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IAddonService _addonService;

        public AddonsServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _addonService = _services.GetRequiredService<IAddonService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
            var addon = _fixture
                .Build<AddonDto>()
                .With(x => x.Concept, "ITBIS")
                .With(x => x.Value, 16)
                .With(x => x.IsDiscount, false)
                .With(x => x.Type, AddonsType.PERCENTAGE)
                .Without(x => x.Id)
                .Create();

            var dto = await _addonService.AddAsync(addon);
            var existing = await _addonService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
                var addon = _fixture
                .Build<AddonDto>()
                .Without(x => x.Id)
                .Create();

                await _addonService.AddAsync(addon);
            }

            var availables = await _addonService.GetAllAsync(new GPA.Common.DTOs.SearchDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldCreateOne()
        {
            var addon = _fixture
                .Build<AddonDto>()
                .With(x => x.Concept, "Día de las madre")
                .With(x => x.Value, 5)
                .With(x => x.IsDiscount, true)
                .With(x => x.Type, AddonsType.PERCENTAGE)
                .Without(x => x.Id)
                .Create();

            var added = await _addonService.AddAsync(addon);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var addon = _fixture
                .Build<AddonDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _addonService.AddAsync(addon);
            var existing = await _addonService.GetByIdAsync(added.Id.Value);

            existing.Concept = "Modified Name";

            await _addonService.UpdateAsync(existing);

            var updated = await _addonService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Concept, added.Concept);
        }

        [Fact]
        public async Task DeleteOne()
        {
            var addon = _fixture
                .Build<AddonDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _addonService.AddAsync(addon);
            await _addonService.RemoveAsync(added.Id.Value);
            var existing = await _addonService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }
    }
}