using GPA.Api.Utils.Filters;
using GPA.Business.Services.Security;
using GPA.Common.DTOs;
using GPA.Dtos.Security;
using GPA.Services.Security;
using GPA.Utils.Profiles;
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
        private readonly ILogger<ProfilesController> _logger;
        private readonly IUserContextService _userContextService;

        public ProfilesController(
            IGPAProfileService gPAProfileService,
            IUserContextService userContextService,
            ILogger<ProfilesController> logger)
        {
            _gPAProfileService = gPAProfileService;
            _userContextService = userContextService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _gPAProfileService.GetProfilesByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _gPAProfileService.GetProfilesAsync(filter));
        }

        [HttpGet("master-profile")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.Read)]
        public IActionResult GetMasterProfile()
        {
            return Ok(ProfileConstants.MasterProfile);
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.Create)]
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
            _logger.LogInformation("El usuario '{User}' ha creado el perfil '{Profile}'", _userContextService.GetCurrentUserName(), model.Name);
            return Created(Url.Action(action: "Get", new { id = profile.Id }), profile);
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.Update)]
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

            try
            {
                _logger.LogInformation("El usuario '{User}' ha modificado el perfil '{Profile}'", _userContextService.GetCurrentUserName(), model.Name);
                await _gPAProfileService.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        [HttpPut("{profileId}/assign/users/{userId}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.AssignProfile)]
        public async Task<IActionResult> AssignProfileToUser(Guid profileId, Guid userId)
        {
            await _gPAProfileService.AssignProfileToUser(profileId, userId);
            _logger.LogInformation("El usuario '{User}' ha asignado el perfil '{Profile}' a {UserId}", _userContextService.GetCurrentUserName(), profileId, userId);
            return NoContent();
        }

        [HttpGet("{profileId}/users")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.Read)]
        public async Task<IActionResult> GetUsers(Guid profileId, Guid userId, [FromQuery] RequestFilterDto filter)
        {
            var users = await _gPAProfileService.GetUsers(profileId, filter);
            return Ok(users);
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetProfilesByUser(Guid userId)
        {
            return Ok(await _gPAProfileService.GetProfilesByUserId(userId));
        }

        [HttpDelete("{profileId}/users/{userId}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.UnAssignProfile)]
        public async Task<IActionResult> UnAssignProfileFromUser(Guid profileId, Guid userId)
        {
            if (profileId == Guid.Empty || userId == Guid.Empty)
            {
                return BadRequest();
            }

            await _gPAProfileService.UnAssignProfileFromUser(profileId, userId);
            _logger.LogInformation("El usuario '{User}' ha desasignado el perfil '{Profile}' a {UserId}", _userContextService.GetCurrentUserName(), profileId, userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.Profile}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                await _gPAProfileService.RemoveAsync(id);
                _logger.LogInformation("El usuario '{User}' ha eliminado el perfil '{Profile}'", _userContextService.GetCurrentUserName(), id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }
    }
}
