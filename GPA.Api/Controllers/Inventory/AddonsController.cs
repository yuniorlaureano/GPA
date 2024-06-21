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
    public class AddonsController : ControllerBase
    {
        private readonly IAddonService _addonService;
        private readonly IMapper _mapper;

        public AddonsController(IAddonService addonService, IMapper mapper)
        {
            _addonService = addonService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _addonService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _addonService.GetAllAsync(search));
        }

        [HttpPost()]
        public async Task<IActionResult> Create(AddonDto addon)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _addonService.AddAsync(addon);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        public async Task<IActionResult> Update(AddonDto addon)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _addonService.UpdateAsync(addon);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _addonService.RemoveAsync(id);
            return NoContent();
        }
    }
}
