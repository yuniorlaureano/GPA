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
    public class StockCyclesController : ControllerBase
    {
        private readonly IStockCycleService _stockCycleService;
        private readonly IMapper _mapper;
        private readonly IValidator<StockCycleCreationDto> _creationValidator;

        public StockCyclesController(
            IStockCycleService stockCycleService,
            IMapper mapper,
            IValidator<StockCycleCreationDto> creationValidator)
        {
            _stockCycleService = stockCycleService;
            _mapper = mapper;
            _creationValidator = creationValidator;
        }

        [HttpGet("{id}", Name = "GetById")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _stockCycleService.GetStockCycleAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _stockCycleService.GetStockCyclesAsync(filter));
        }

        [HttpPost("open")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Open)]
        public async Task<IActionResult> Open(StockCycleCreationDto model)
        {
            var validationResult = await _creationValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var cycleId = await _stockCycleService.OpenCycleAsync(model);
            return CreatedAtAction(nameof(Get), new { id = cycleId }, cycleId);
        }

        [HttpPut("close/{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Close)]
        public async Task<IActionResult> Close([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _stockCycleService.CloseCycleAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _stockCycleService.RemoveAsync(id);
            return NoContent();
        }
    }
}
