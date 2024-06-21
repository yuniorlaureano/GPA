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
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly IMapper _mapper;

        public StocksController(IStockService StockService, IMapper mapper)
        {
            _stockService = StockService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _stockService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _stockService.GetAllAsync(search));
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductCatalog([FromQuery] SearchDto search)
        {
            return Ok(await _stockService.GetProductCatalogAsync(search.Page, search.PageSize));
        }

        [HttpGet("existance")]
        public async Task<IActionResult> GetExistance([FromQuery] SearchDto search)
        {
            return Ok(await _stockService.GetExistanceAsync(search.Page, search.PageSize));
        }

        [HttpPost("input")]
        public async Task<IActionResult> RegisterInput(StockCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _stockService.AddAsync(model);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPost("output")]
        public async Task<IActionResult> RegisterOutput(OutputCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _stockService.AddAsync(model.AsStoCreationDto);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut("input")]
        public async Task<IActionResult> UpdateInput(StockCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _stockService.UpdateInputAsync(model);
            return NoContent();
        }

        [HttpPut("output")]
        public async Task<IActionResult> UpdateOutput(OutputCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _stockService.UpdateOutputAsync(model.AsStoCreationDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _stockService.RemoveAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel([FromRoute] Guid id)
        {
            await _stockService.CancelAsync(id);
            return NoContent();
        }
    }
}
