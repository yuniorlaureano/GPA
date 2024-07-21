using AutoMapper;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Invoice;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoice;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Invoice.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("invoice/[controller]")]
    [ApiController()]
    public class ReceivableAccountsController : ControllerBase
    {
        private readonly IReceivableAccountService _service;
        private readonly IMapper _mapper;

        public ReceivableAccountsController(IReceivableAccountService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpGet("invoice/{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Read)]
        public async Task<IActionResult> GetByInvoiceIdAsync(Guid id)
        {
            return Ok(await _service.GetByInvoiceIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _service.GetAllAsync(search));
        }

        [HttpGet("summary")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Read)]
        public async Task<IActionResult> GetReceivableSummary([FromQuery] SearchDto search)
        {
            return Ok(await _service.GetReceivableSummaryAsync(search));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(ClientPaymentsDetailCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _service.AddAsync(model);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(ClientPaymentsDetailCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _service.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.ReceivableAccount}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.RemoveAsync(id);
            return NoContent();
        }
    }
}
