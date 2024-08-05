using AutoMapper;
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
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _stockService.GetByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.ReadTransactions)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _stockService.GetStocksAsync(filter));
        }

        [HttpGet("products")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.ReadProducts)]
        public async Task<IActionResult> GetProductCatalog([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _stockService.GetProductCatalogAsync(filter.Page, filter.PageSize));
        }

        [HttpGet("existance")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.ReadExistence)]
        public async Task<IActionResult> GetExistence([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _stockService.GetExistenceAsync(filter));
        }

        [HttpPost("input")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.RegisterInput)]
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
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.RegisterOutput)]
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
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.UpdateInput)]
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
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.UpdateOutput)]
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
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _stockService.RemoveAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/cancel")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.Cancel)]
        public async Task<IActionResult> Cancel([FromRoute] Guid id)
        {
            await _stockService.CancelAsync(id);
            return NoContent();
        }
    }
}
