using AutoMapper;
using FluentValidation;
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
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IValidator<ClientDto> _validator;
        private readonly IMapper _mapper;

        public ClientsController(
            IClientService clientService,
            IValidator<ClientDto> validator,
            IMapper mapper)
        {
            _clientService = clientService;
            _validator = validator;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Client}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _clientService.GetClientAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Client}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _clientService.GetClientsAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Client}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(ClientDto client)
        {
            var validationResult = await _validator.ValidateAsync(client);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            var entity = await _clientService.AddAsync(client);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Client}", permission: Permissions.Update)]
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
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Invoice}.{Components.Client}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _clientService.RemoveAsync(id);
            return NoContent();
        }
    }
}
