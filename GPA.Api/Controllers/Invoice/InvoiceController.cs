using AutoMapper;
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
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IMapper _mapper;

        public InvoiceController(IInvoiceService invoiceService, IMapper mapper)
        {
            _invoiceService = invoiceService;
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

            var entity = await _invoiceService.AddAsync(invoice);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        public async Task<IActionResult> Update(InvoiceDto invoice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _invoiceService.UpdateAsync(invoice);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _invoiceService.RemoveAsync(id);
            return NoContent();
        }
    }
}
