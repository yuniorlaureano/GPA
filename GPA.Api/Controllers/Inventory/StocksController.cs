using AutoMapper;
using FluentValidation;
using GPA.Api.Extensions;
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
        private readonly IValidator<OutputCreationDto> _outputCreationValidator;
        private readonly IValidator<StockCreationDto> _stockCreationValidator;

        public StocksController(
            IStockService StockService,
            IMapper mapper,
            IValidator<OutputCreationDto> outputCreationValidator,
            IValidator<StockCreationDto> stockCreationValidator)
        {
            _stockService = StockService;
            _mapper = mapper;
            _outputCreationValidator = outputCreationValidator;
            _stockCreationValidator = stockCreationValidator;
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
            return Ok(await _stockService.GetProductCatalogAsync(filter));
        }

        [HttpGet("existence")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.ReadExistence)]
        public async Task<IActionResult> GetExistence([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _stockService.GetExistenceAsync(filter));
        }

        [HttpPost("input")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.RegisterInput)]
        public async Task<IActionResult> RegisterInput(StockCreationDto model)
        {
            var validationResult = _stockCreationValidator.Validate(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var entity = await _stockService.AddAsync(model);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPost("output")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.RegisterOutput)]
        public async Task<IActionResult> RegisterOutput(OutputCreationDto model)
        {
            var validationResult = _outputCreationValidator.Validate(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var entity = await _stockService.AddAsync(model.AsStoCreationDto);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut("input")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.UpdateInput)]
        public async Task<IActionResult> UpdateInput(StockCreationDto model)
        {
            var validationResult = _stockCreationValidator.Validate(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            await _stockService.UpdateInputAsync(model);
            return NoContent();
        }

        [HttpPut("output")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.UpdateOutput)]
        public async Task<IActionResult> UpdateOutput(OutputCreationDto model)
        {
            var validationResult = _outputCreationValidator.Validate(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            await _stockService.UpdateOutputAsync(model.AsStoCreationDto);
            return NoContent();
        }

        [HttpPost("{stockId}/attachment/upload")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.Upload)]
        public async Task<IActionResult> UploadAttachment(Guid stockId, IFormCollection files)
        {
            if (files?.Files is { Count: 0 })
            {
                return BadRequest(new string[] { "No contiene archivos para subir." });
            }

            foreach (var file in files.Files)
            {
                await _stockService.SaveAttachment(stockId, file);
            }

            return NoContent();
        }

        [HttpGet("{stockId}/attachments")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.Read)]
        public async Task<IActionResult> GetAttachmentsAsync(Guid stockId)
        {
            return Ok(await _stockService.GetAttachmentByStockIdAsync(stockId));
        }

        [HttpPost("attachments/{attachmentId}/download")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Inventory}.{Components.Stock}", permission: Permissions.Download)]
        public async Task<IActionResult> DownloadFile([FromRoute] Guid attachmentId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorMessage());
            }

            try
            {
                var (file, fileName) = await _stockService.DownloadAttachmentAsync(attachmentId);
                if (file is not null)
                {
                    return File(file, "application/octet-stream", fileName);

                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return NotFound("No se encontró el archivo.");
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
