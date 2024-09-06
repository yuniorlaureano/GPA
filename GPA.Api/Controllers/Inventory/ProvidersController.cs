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
    public class ProvidersController : ControllerBase
    {
        private readonly IProviderService _providerService;
        private readonly IValidator<ProviderDto> _validator;
        private readonly IMapper _mapper;

        public ProvidersController(IProviderService ProviderService, IValidator<ProviderDto> validator, IMapper mapper)
        {
            _providerService = ProviderService;
            _validator = validator;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Provider}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _providerService.GetProviderByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Provider}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _providerService.GetProvidersAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Provider}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(ProviderDto model)
        {
            var validationResult = await _validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var entity = await _providerService.AddAsync(model);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Provider}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(ProviderDto model)
        {
            var validationResult = await _validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            await _providerService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Provider}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _providerService.RemoveAsync(id);
            return NoContent();
        }
    }
}
