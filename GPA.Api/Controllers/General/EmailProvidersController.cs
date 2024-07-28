using AutoMapper;
using GPA.Common.DTOs;
using GPA.Dtos.General;
using GPA.Services.General;
using GPA.Services.General.Email;
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
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _emailProviderService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _emailProviderService.GetAllAsync(search));
        }

        [HttpPost()]
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
        public async Task<IActionResult> Delete(Guid id)
        {
            await _emailProviderService.RemoveAsync(id);
            return NoContent();
        }
    }
}
