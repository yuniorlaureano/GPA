using AutoMapper;
using GPA.Business.Services.General;
using GPA.Common.DTOs;
using GPA.Common.DTOs.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.General.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("common/[controller]")]
    [ApiController()]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;
        private readonly IMapper _mapper;

        public UnitsController(IUnitService unitService, IMapper mapper)
        {
            _unitService = unitService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _unitService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _unitService.GetAllAsync(filter));
        }

        [HttpPost()]
        public async Task<IActionResult> Create(UnitDto unit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _unitService.AddAsync(unit);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        public async Task<IActionResult> Update(UnitDto unit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _unitService.UpdateAsync(unit);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _unitService.RemoveAsync(id);
            return NoContent();
        }
    }
}
