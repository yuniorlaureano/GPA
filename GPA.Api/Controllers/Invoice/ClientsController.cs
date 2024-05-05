using AutoMapper;
using GPA.Business.Services.Invoice;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Invoice.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("invoice/[controller]")]
    [ApiController()]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IMapper _mapper;

        public ClientsController(IClientService clientService, IMapper mapper)
        {
            _clientService = clientService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _clientService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _clientService.GetAllAsync(search));
        }

        [HttpPost()]
        public async Task<IActionResult> Create(ClientDto client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _clientService.AddAsync(client);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        public async Task<IActionResult> Update(ClientDto client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _clientService.UpdateAsync(client);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _clientService.RemoveAsync(id);
            return NoContent();
        }
    }
}
