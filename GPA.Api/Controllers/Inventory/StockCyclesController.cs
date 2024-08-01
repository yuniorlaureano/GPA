using AutoMapper;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GPA.Inventory.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("inventory/[controller]")]
    [ApiController()]
    public class StockCyclesController : ControllerBase
    {
        private readonly IStockCycleService _stockCycleService;
        private readonly IMapper _mapper;

        public StockCyclesController(IStockCycleService stockCycleService, IMapper mapper)
        {
            _stockCycleService = stockCycleService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetById")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _stockCycleService.GetByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _stockCycleService.GetAllAsync(filter));
        }

        [HttpPost("open")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Open)]
        public async Task<IActionResult> Open(StockCycleCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cycleId = await _stockCycleService.OpenCycleAsync(model);
            return CreatedAtAction(nameof(Get), new { id = cycleId }, cycleId);
        }

        [HttpPut("close/{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.StockCycle}", permission: Permissions.Close)]
        public async Task<IActionResult> Close([FromRoute]Guid id)
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
