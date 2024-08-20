using AutoMapper;
using FluentValidation;
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
    public class AddonsController : ControllerBase
    {
        private readonly IAddonService _addonService;
        private readonly IMapper _mapper;
        private readonly IValidator<AddonDto> _validator;

        public AddonsController(
            IAddonService addonService, 
            IMapper mapper,
            IValidator<AddonDto> validator)
        {
            _addonService = addonService;
            _mapper = mapper;
            _validator = validator;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Addon}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _addonService.GetAddonsAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Addon}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _addonService.GetAddonsAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Addon}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(AddonDto addon)
        {
            var validationResult = await _validator.ValidateAsync(addon);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var entity = await _addonService.AddAsync(addon);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Addon}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(AddonDto addon)
        {
            var validationResult = await _validator.ValidateAsync(addon);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            await _addonService.UpdateAsync(addon);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Addon}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _addonService.RemoveAsync(id);
            return NoContent();
        }
    }
}
