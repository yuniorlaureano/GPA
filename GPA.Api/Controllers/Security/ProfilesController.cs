using GPA.Business.Services.Security;
using GPA.Common.DTOs;
using GPA.Dtos.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Api.Controllers.Security
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("security/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IGPAProfileService _gPAProfileService;

        public ProfilesController(IGPAProfileService gPAProfileService)
        {
            _gPAProfileService = gPAProfileService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _gPAProfileService.GetByIdAsync(id));
        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] SearchDto search)
        {
            return Ok(await _gPAProfileService.GetAllAsync(search));
        }

        [HttpPost()]
        public async Task<IActionResult> Post(GPAProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model is null)
            {
                ModelState.AddModelError("model", "The model is null");
                return BadRequest(ModelState);
            }

            var profile = await _gPAProfileService.AddAsync(model);
            return Created(Url.Action(action: "Get", new { id = profile.Id }), profile);
        }

        [HttpPut()]
        public async Task<IActionResult> Put(GPAProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model is null)
            {
                ModelState.AddModelError("model", "The model is null");
                return BadRequest(ModelState);
            }

            await _gPAProfileService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            await _gPAProfileService.RemoveAsync(id);
            return NoContent();
        }
    }
}
