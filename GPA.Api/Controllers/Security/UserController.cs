using AutoMapper;
using GPA.Business.Security;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Dtos.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Api.Controllers.Security
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("security/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<GPAUser> _userManager;
        private readonly GPADbContext _context;
        private readonly IMapper _mapper;
        public UserController(UserManager<GPAUser> userManager, GPADbContext context, IMapper mapper)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        //[AllowAnonymous]
        //[HttpPost()]
        //public async Task<IActionResult> Get()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (model is null)
        //    {
        //        return BadRequest();
        //    }

        //    var entity = _mapper.Map<GPAUser>(model);
        //    entity.Id = Guid.Empty;
        //    await _userManager.CreateAsync(entity, model.Password);
        //    return Created();
        //}

        [AllowAnonymous]
        [HttpPost()]
        public async Task<IActionResult> Post(GPAUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model is null)
            {
                return BadRequest();
            }

            var entity = _mapper.Map<GPAUser>(model);
            entity.Id = Guid.Empty;
            await _userManager.CreateAsync(entity, model.Password);
            return Created();
        }

        [HttpPut()]
        public async Task<IActionResult> Put(GPAUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model is null)
            {
                return BadRequest();
            }

            var savedEntity = _userManager.FindByIdAsync(model.Id.ToString());

            if (savedEntity == null)
            {
                return BadRequest();
            }

            var entity = _mapper.Map<GPAUser>(model);            
            await _userManager.UpdateAsync(entity);
            return NoContent();
        }
    }
}
