using AutoMapper;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Security;
using GPA.Common.DTOs;
using GPA.Common.Entities.Security;
using GPA.Dtos.Security;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Api.Controllers.Security
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("security/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<GPAUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IGPAUserService _gPAUserService;
        public UsersController(UserManager<GPAUser> userManager, IMapper mapper, IGPAUserService gPAUserService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _gPAUserService = gPAUserService;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _gPAUserService.GetByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _gPAUserService.GetAllAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Create)]
        public async Task<IActionResult> Post(GPAUserUpdateDto model)
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

            var entity = _mapper.Map<GPAUser>(model);
            entity.Id = Guid.Empty;
            entity.Deleted = false;
            var result = await _userManager.CreateAsync(entity, $"Dummy-Password-{Guid.NewGuid().ToString()}*-$#$%");

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            return Created();
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Update)]
        public async Task<IActionResult> Put(GPAUserUpdateDto model)
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

            var savedEntity = await _userManager.FindByIdAsync(model.Id.ToString());

            if (savedEntity is null)
            {
                ModelState.AddModelError("model", "The requested user does not exists");
                return BadRequest(ModelState);
            }

            savedEntity.FirstName = model.FirstName;
            savedEntity.LastName = model.LastName;
            savedEntity.Email  = model.Email;
            savedEntity.UserName = model.UserName;

            await _userManager.UpdateAsync(savedEntity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var entity = await _userManager.FindByIdAsync(id.ToString());

            if (entity is null)
            {
                return BadRequest();
            }

            entity.Deleted = true;
            var result = await _userManager.UpdateAsync(entity);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
