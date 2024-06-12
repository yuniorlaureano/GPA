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
    public class StockCyclesController : ControllerBase
    {
        private readonly IStockCycleService _stockCycleService;
        private readonly IMapper _mapper;

        public StockCyclesController(IStockCycleService stockCycleService, IMapper mapper)
        {
            _stockCycleService = stockCycleService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _stockCycleService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _stockCycleService.GetAllAsync(search));
        }

        [HttpPost()]
        public async Task<IActionResult> Open(StockCycleCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cycleId = await _stockCycleService.OpenCycleAsync(model);
            return Created(Url.Action(nameof(Get)), new { id = cycleId });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _stockCycleService.RemoveAsync(id);
            return NoContent();
        }
    }
}
