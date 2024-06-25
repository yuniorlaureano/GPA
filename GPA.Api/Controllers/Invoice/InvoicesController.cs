using AutoMapper;
using FluentValidation;
using GPA.Business.Services.Invoice;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Invoice.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("invoice/[controller]")]
    [ApiController()]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IMapper _mapper;
        private readonly IValidator<InvoiceUpdateDto> _updateValidator;
        private readonly IValidator<InvoiceDto> _createValidator;

        public InvoicesController(
            IInvoiceService invoiceService,
            IValidator<InvoiceUpdateDto> updateValidator,
            IValidator<InvoiceDto> createValidator,
            IMapper mapper)
        {
            _invoiceService = invoiceService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _invoiceService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _invoiceService.GetAllAsync(search));
        }

        [HttpPost()]
        public async Task<IActionResult> Create(InvoiceDto invoice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = await _invoiceService.AddAsync(invoice);
                return Created(Url.Action(nameof(Get), new { id = entity.Id }), entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut()]
        public async Task<IActionResult> Update(InvoiceUpdateDto invoice)
        {
            var result = await _updateValidator.ValidateAsync(invoice);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            try
            {
                await _invoiceService.UpdateAsync(invoice);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _invoiceService.RemoveAsync(id);
            return NoContent();
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _invoiceService.CancelAsync(id);
            return NoContent();
        }
    }
}
