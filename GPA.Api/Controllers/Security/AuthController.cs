using GPA.Business.Security;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Dtos.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace GPA.Api.Controllers.Security
{
    [Route("security/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IGPAJwtService _jwtService;
        private readonly UserManager<GPAUser> _userManager;
        private readonly GPADbContext _context;
        public AuthController(IGPAJwtService jwtService, UserManager<GPAUser> userManager, GPADbContext context)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(GPAUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            var resultd = await _userManager.CheckPasswordAsync(user, model.Password);

            if (resultd)
            {
                return Unauthorized("El usaurio no es válido");
            }

            var roles = await _context.Roles.Include(r => r.Claims)
                    .Where(x => x.UserRoles.Any(ur => ur.UserId == user.Id))
                    .ToArrayAsync();

            //Todo: Logic to add the roles and roles clamins to the token. Consier serializng the cliams.
            /*
                Claim(Admin, json_with_the_claim)
                Create a reaquirement handler to quey the db searching for user and clieams and checking they belong
                .Write a custom query tha run like> Raw(select form roles where id = (select from userRole where rolid --))
             */

            var claims = new Claim[] { };
            var token = _jwtService.GenerateToken(new Dtos.Security.TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.EcdsaSha256Signature,
                Claims = claims
            });
            return Ok(token);
        }
    }
}
