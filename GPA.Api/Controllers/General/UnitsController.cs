using AutoMapper;
using FluentValidation;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.General;
using GPA.Common.DTOs;
using GPA.Common.DTOs.General;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.General.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("general/[controller]")]
    [ApiController()]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;
        private readonly IValidator<UnitDto> _validator;
        private readonly IMapper _mapper;

        public UnitsController(IUnitService unitService, IValidator<UnitDto> validator, IMapper mapper)
        {
            _unitService = unitService;
            _validator = validator;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Unit}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _unitService.GetByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Unit}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _unitService.GetAllAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Unit}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(UnitDto unit)
        {
            var validationResult = await _validator.ValidateAsync(unit);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var entity = await _unitService.AddAsync(unit);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Unit}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(UnitDto unit)
        {
            var validationResult = await _validator.ValidateAsync(unit);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            await _unitService.UpdateAsync(unit);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Unit}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _unitService.RemoveAsync(id);
            return NoContent();
        }
    }
}
