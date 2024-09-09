using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;
using GPA.Utils.Database;
using Microsoft.Extensions.Logging;

namespace GPA.Business.Services.Inventory
{
    public interface ICategoryService
    {
        Task<CategoryDto?> GetCategoryAsync(Guid id);
        Task<ResponseDto<CategoryDto>> GetCategoriesAsync(RequestFilterDto search);
        Task<CategoryDto?> AddAsync(CategoryDto categoryDto);
        Task UpdateAsync(CategoryDto categoryDto);
        Task RemoveAsync(Guid id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository repository,
            IUserContextService userContextService,
            IMapper mapper,
            ILogger<CategoryService> logger)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CategoryDto?> GetCategoryAsync(Guid id)
        {
            return _mapper.Map<CategoryDto>(await _repository.GetCategoryAsync(id));
        }

        public async Task<ResponseDto<CategoryDto>> GetCategoriesAsync(RequestFilterDto filter)
        {
            filter.Search = SearchHelper.ConvertSearchToString(filter);
            return new ResponseDto<CategoryDto>
            {
                Count = await _repository.GetCategoriesCountAsync(filter),
                Data = _mapper.Map<IEnumerable<CategoryDto>>(await _repository.GetCategoriesAsync(filter))
            };
        }

        public async Task<CategoryDto> AddAsync(CategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.CreatedBy = _userContextService.GetCurrentUserId();
            category.CreatedAt = DateTimeOffset.UtcNow;
            var savedCategory = await _repository.AddAsync(category);
            _logger.LogInformation("El usuario '{User}' ha creado la categoría '{Category}'", _userContextService.GetCurrentUserId(), savedCategory.Id);
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
            _logger.LogInformation("El usuario '{User}' ha modificado la categoría '{Category}'", _userContextService.GetCurrentUserId(), savedCategory.Id);
        }

        public async Task RemoveAsync(Guid id)
        {
            await _repository.SoftDeleteCategoryAsync(id);
            _logger.LogInformation("El usuario '{User}' ha eliminado la categoría '{Category}'", _userContextService.GetCurrentUserId(), id);
        }
    }
}
