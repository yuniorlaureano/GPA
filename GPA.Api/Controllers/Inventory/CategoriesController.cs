using AutoMapper;
using FluentValidation;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Utils.Caching;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Inventory.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("inventory/[controller]")]
    [ApiController()]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryDto> _validator;
        private readonly IGenericCache<ResponseDto<CategoryDto>> _cache;

        public CategoriesController(
            ICategoryService categoryService,
            IMapper mapper,
            IValidator<CategoryDto> validator,
            IGenericCache<ResponseDto<CategoryDto>> cache)
        {
            _categoryService = categoryService;
            _mapper = mapper;
            _validator = validator;
            _cache = cache;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Category}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _categoryService.GetCategoryAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Category}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            //var categories = await _cache.GetOrCreate(CacheType.Common, $"{filter.Page}-{filter.PageSize}-{filter.Search}", async () =>
            //{
            //    return await _categoryService.GetCategoriesAsync(filter);
            //});
            var categories = await _categoryService.GetCategoriesAsync(filter);
            return Ok(categories);
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Category}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(CategoryDto category)
        {
            var validationResult = await _validator.ValidateAsync(category);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var entity = await _categoryService.AddAsync(category);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Category}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(CategoryDto category)
        {
            var validationResult = await _validator.ValidateAsync(category);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            await _categoryService.UpdateAsync(category);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Category}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _categoryService.RemoveAsync(id);
            return NoContent();
        }
    }
}
