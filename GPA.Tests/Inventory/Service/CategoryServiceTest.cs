using AutoFixture;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs.Inventory;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Inventory.Service
{
    public class CategoryServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly ICategoryService _categoryService;

        public CategoryServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _categoryService = _services.GetRequiredService<ICategoryService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
            var category = _fixture
                .Build<CategoryDto>()
                .Without(x => x.Id)
                .Create();

            var dto = await _categoryService.AddAsync(category);
            var existing = await _categoryService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
                var category = _fixture
                    .Build<CategoryDto>()
                    .Without(x => x.Id)
                    .Create();

                await _categoryService.AddAsync(category);
            }

            var availables = await _categoryService.GetAllAsync(new GPA.Common.DTOs.RequestFilterDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldOneCreate()
        {
            var category = _fixture
                .Build<CategoryDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _categoryService.AddAsync(category);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var category = _fixture
                .Build<CategoryDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _categoryService.AddAsync(category);
            var existing = await _categoryService.GetByIdAsync(added.Id.Value);

            existing.Name = "Modified Name";

            await _categoryService.UpdateAsync(existing);

            var updated = await _categoryService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Name, added.Name);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            var category = _fixture
                .Build<CategoryDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _categoryService.AddAsync(category);
            await _categoryService.RemoveAsync(added.Id.Value);
            var existing = await _categoryService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }
    }
}