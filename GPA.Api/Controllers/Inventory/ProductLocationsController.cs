using AutoMapper;
using GPA.Api.Extensions;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Inventory.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("inventory/[controller]")]
    [ApiController()]
    public class ProductLocationsController : ControllerBase
    {
        private readonly IProductLocationService _ProductLocationService;
        private readonly IMapper _mapper;

        public ProductLocationsController(IProductLocationService ProductLocationService, IMapper mapper)
        {
            _ProductLocationService = ProductLocationService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.ProductLocation}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _ProductLocationService.GetByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.ProductLocation}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _ProductLocationService.GetAllAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.ProductLocation}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(ProductLocationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorMessage());
            }

            var entity = await _ProductLocationService.AddAsync(model);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.ProductLocation}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(ProductLocationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _ProductLocationService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.ProductLocation}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _ProductLocationService.RemoveAsync(id);
            return NoContent();
        }
    }
}
