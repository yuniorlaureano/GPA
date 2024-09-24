using AutoFixture;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.Inventory;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class ReasonServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IReasonService _reasonService;

        public ReasonServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _reasonService = _services.GetRequiredService<IReasonService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
            var reason = _fixture
                .Build<ReasonDto>()
                .Without(x => x.Id)
                .Create();

            var dto = await _reasonService.AddAsync(reason);
            var existing = await _reasonService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
                var reason = _fixture
                    .Build<ReasonDto>()
                    .Without(x => x.Id)
                    .Create();

                await _reasonService.AddAsync(reason);
            }

            var availables = await _reasonService.GetAllAsync(new GPA.Common.DTOs.RequestFilterDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
            var reason = _fixture
                .Build<ReasonDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _reasonService.AddAsync(reason);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var reason = _fixture
                .Build<ReasonDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _reasonService.AddAsync(reason);
            var existing = await _reasonService.GetByIdAsync(added.Id.Value);

            existing.Name = "Modified Name";

            await _reasonService.UpdateAsync(existing);

            var updated = await _reasonService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Name, added.Name);
        }

        //[Fact]
        //public async Task ShouldDelete()
        //{
        //    var reason = _fixture
        //        .Build<ReasonDto>()
        //        .Without(x => x.Id)
        //        .Create();

        //    var added = await _reasonService.AddAsync(reason);
        //    await _reasonService.RemoveAsync(added.Id.Value);
        //    var existing = await _reasonService.GetByIdAsync(added.Id.Value);

        //    Assert.Null(existing);
        //}
    }
}