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
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IMapper _mapper;

        public ItemsController(IItemService itemService, IMapper mapper)
        {
            _itemService = itemService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _itemService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _itemService.GetAllAsync(search));
        }

        [HttpPost()]
        public async Task<IActionResult> Create(ItemDto item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _itemService.AddAsync(item);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        public async Task<IActionResult> Update(ItemDto item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _itemService.UpdateAsync(item);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _itemService.RemoveAsync(id);
            return NoContent();
        }
    }
}
