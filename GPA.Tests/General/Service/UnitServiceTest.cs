using AutoFixture;
using GPA.Business.Services.General;
using GPA.Common.DTOs.General;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.General.Service
{
    public class UnitServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IUnitService _unitService;

        public UnitServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependenyBuilder.GetServices();
            _unitService = _services.GetRequiredService<IUnitService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
            var unit = _fixture
                .Build<UnitDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _unitService.AddAsync(unit);
            var existing = await _unitService.GetByIdAsync(added.Id.Value);

            Assert.Equal(added.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
                var unit = _fixture
                    .Build<UnitDto>()
                    .Without(x => x.Id)
                    .Create();

                await _unitService.AddAsync(unit);
            }

            var availables = await _unitService.GetAllAsync(new GPA.Common.DTOs.SearchDto { Page = 1, PageSize = 3});
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
            var unit = _fixture
                .Build<UnitDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _unitService.AddAsync(unit);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var unit = _fixture
                .Build<UnitDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _unitService.AddAsync(unit);
            var existing = await _unitService.GetByIdAsync(added.Id.Value);

            existing.Name = "Modified Name";

            await _unitService.UpdateAsync(existing);

            var updated = await _unitService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Name, added.Name);
        }

        [Fact]
        public async Task DeleteUpdate()
        {
            var unit = _fixture
                .Build<UnitDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _unitService.AddAsync(unit);
            await _unitService.RemoveAsync(added.Id.Value);
            var existing = await _unitService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }
    }
}