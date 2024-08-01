using AutoMapper;
using GPA.Api.Utils.Filters;
using GPA.Common.DTOs;
using GPA.Dtos.General;
using GPA.Services.General;
using GPA.Services.General.Email;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.General.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("general/[controller]")]
    [ApiController()]
    public class EmailProvidersController : ControllerBase
    {
        private readonly IEmailProviderService _emailProviderService;
        private readonly IEmailServiceFactory _emailServiceFactory;
        private readonly IMapper _mapper;

        public EmailProvidersController(
            IEmailProviderService emailProviderService,
            IEmailServiceFactory emailServiceFactory,
            IMapper mapper)
        {
            _emailProviderService = emailProviderService;
            _emailServiceFactory = emailServiceFactory;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Email}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _emailProviderService.GetByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Email}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _emailProviderService.GetAllAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Email}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(EmailConfigurationCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _emailProviderService.CreateConfigurationAsync(model);
            return Ok();
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Email}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(EmailConfigurationUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _emailProviderService.UpdateConfigurationAsync(model);
            return NoContent();
        }

        [HttpPost("mail/send")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Email}", permission: Permissions.Send)]
        public async Task<IActionResult> SendMail(EmailMessage message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _emailServiceFactory.SendMessageAsync(message);
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Email}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _emailProviderService.RemoveAsync(id);
            return NoContent();
        }
    }
}
