using AutoMapper;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
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

        public CategoriesController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _categoryService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _categoryService.GetAllAsync(search));
        }

        [HttpPost()]
        public async Task<IActionResult> Create(CategoryDto category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _categoryService.AddAsync(category);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        public async Task<IActionResult> Update(CategoryDto category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _categoryService.UpdateAsync(category);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _categoryService.RemoveAsync(id);
            return NoContent();
        }
    }
}
