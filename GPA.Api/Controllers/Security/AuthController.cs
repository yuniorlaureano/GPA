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
using System.Text;
using System.Text.Json;

namespace GPA.Api.Controllers.Security
{
    [Authorize(AuthenticationSchemes = "Bearer")]
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
        public async Task<IActionResult> Login(GPAAuthDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user is null)
            {
                ModelState.AddModelError("usuario", "El usuario no existe");
                return BadRequest(ModelState);
            }

            var resultd = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!resultd)
            {
                ModelState.AddModelError("usuario", "Usuario inválido");
                return BadRequest(ModelState);
            }

            var roles = await _context.Roles.Include(r => r.RoleClaims)
                    .Where(x => x.UserRoles.Any(ur => ur.UserId == user.Id))
                    .ToArrayAsync();

            var claims = new List<Claim>();
            foreach (var role in roles)
            {
                var permissions = role.RoleClaims.Select(x => $"m:{x.ClaimType}p:{x.ClaimValue}").ToArray();
                var permissionsAsByte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(permissions));
                var base64 = Convert.ToBase64String(permissionsAsByte);
                claims.Add(new Claim(ClaimTypes.Role, $"rid:{role.Id}rn:{role.Name}pm:{base64}"));
            }
            claims.Add(new Claim("FullName", $"{user.FirstName} {user.LastName}"));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim("UserId", user.Id.ToString()));
            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = claims.ToArray()
            });
            return Ok(new { token = token });
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpDto model)
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

            var passwordHasher = new PasswordHasher<GPAUser>();
            var entity = new GPAUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName,
            };
            entity.PasswordHash = passwordHasher.HashPassword(entity, model.Password);
            var result = await _userManager.CreateAsync(entity, model.Password);

            if (result.Succeeded)
            {
                return Created();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }
    }
}
