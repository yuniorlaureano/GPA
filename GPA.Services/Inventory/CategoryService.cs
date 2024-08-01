using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface ICategoryService
    {
        public Task<CategoryDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<CategoryDto>> GetAllAsync(RequestFilterDto search, Expression<Func<Category, bool>>? expression = null);

        public Task<CategoryDto?> AddAsync(CategoryDto categoryDto);

        public Task UpdateAsync(CategoryDto categoryDto);

        public Task RemoveAsync(Guid id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public CategoryService(
            ICategoryRepository repository,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<CategoryDto?> GetByIdAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<ResponseDto<CategoryDto>> GetAllAsync(RequestFilterDto search, Expression<Func<Category, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.OrderByDescending(x => x.Id)
                    .Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<CategoryDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<CategoryDto>>(categories)
            };
        }

        public async Task<CategoryDto> AddAsync(CategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.CreatedBy = _userContextService.GetCurrentUserId();
            category.CreatedAt = DateTimeOffset.UtcNow;
            var savedCategory = await _repository.AddAsync(category);
            return _mapper.Map<CategoryDto>(savedCategory);
        }

        public async Task UpdateAsync(CategoryDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newCategory = _mapper.Map<Category>(dto);
            newCategory.Id = dto.Id.Value;
            newCategory.UpdatedBy = _userContextService.GetCurrentUserId();
            newCategory.UpdatedAt = DateTimeOffset.UtcNow;
            var savedCategory = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedCategory, newCategory, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedCategory = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedCategory);
        }
    }
}
