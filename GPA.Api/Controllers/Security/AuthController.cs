using FluentValidation;
using GPA.Api.Utils.Filters;
using GPA.Business.Security;
using GPA.Business.Services.Security;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Dtos.Security;
using GPA.Services.General.Email;
using GPA.Utils.Constants.Claims;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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
        private readonly IGPAProfileService _gPAProfileService;
        private readonly IValidator<SignUpDto> _signUpValidator;
        private readonly IEmailServiceFactory _emailServiceFactory;

        public AuthController(
            IGPAJwtService jwtService,
            UserManager<GPAUser> userManager,
            GPADbContext context,
            IGPAProfileService gPAProfileService,
            IValidator<SignUpDto> signUpValidator,
            IEmailServiceFactory emailServiceFactory)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _context = context;
            _gPAProfileService = gPAProfileService;
            _signUpValidator = signUpValidator;
            _emailServiceFactory = emailServiceFactory;
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

            var claims = new List<Claim>
            {
                new Claim(GPAClaimTypes.FullName, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(GPAClaimTypes.UserId, user.Id.ToString())
            };

            var profileId = await AssignProfileAsClaimIfUserHasOnlyOneProfile(user.Id, claims);

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = claims.ToArray()
            });

            var permissions = await GetProfilePermissions(profileId);

            return Ok(new { token = token, permissions = permissions });
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpDto model)
        {
            if (model is null)
            {
                ModelState.AddModelError("model", "The model is null");
                return BadRequest(ModelState);
            }

            var validationResult = await _signUpValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var entity = new GPAUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName,
            };
            var passwordHasher = new PasswordHasher<GPAUser>();
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

        [HttpGet("profile/{profileId}/change")]
        public async Task<IActionResult> ChooseProfile(Guid profileId)
        {
            var user = User;
            var userId = user.Claims.FirstOrDefault(x => x.Type == GPAClaimTypes.UserId).Value;
            if (userId is null)
            {
                return BadRequest("empty userid claim");
            }

            var exists = await _gPAProfileService.ProfileExists(profileId, Guid.Parse(userId));
            if (!exists)
            {
                return new ForbidResult("does not have access to this profile");
            }

            var claims = user.Claims.Where(x => x.Type != GPAClaimTypes.ProfileId).ToList();
            claims.Add(new Claim(GPAClaimTypes.ProfileId, profileId.ToString()));

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = claims.ToArray()
            });

            var permissions = await GetProfilePermissions(profileId.ToString());

            return Ok(new { token = token, permissions = permissions });
        }

        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Auth}", permission: Permissions.UpdateUserProfile)]
        [HttpPut("users/{userId}/profile/edit")]
        public async Task<IActionResult> EditProfile([FromRoute] Guid userId, [FromBody] UserProfileDto model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                ModelState.AddModelError("usuario", "El usuario no existe");
                return BadRequest(ModelState);
            }

            var userClaim = User.Claims.FirstOrDefault(x => x.Type == GPAClaimTypes.UserId);
            if (user.Id.ToString() != userClaim.Value)
            {
                ModelState.AddModelError("usuario", "Solo debe modificar su propio usuario");
                return BadRequest(ModelState);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.SecurityStamp = Guid.NewGuid().ToString();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("usuario", "Error modificando el usuario");
                return BadRequest(ModelState);
            }

            var cliams = User.Claims.Where(x => x.Type != GPAClaimTypes.FullName).ToList();
            cliams.Add(new Claim(GPAClaimTypes.FullName, $"{user.FirstName} {user.LastName}"));

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = cliams.ToArray()
            });

            var permissions = await GetProfilePermissions(User.Claims.First(x => x.Type == GPAClaimTypes.ProfileId).Value);

            return Ok(new { token = token, permissions = permissions });
        }

        [AllowAnonymous]
        [HttpGet("totp/send/{email}")]
        public async Task<IActionResult> SendTOTPCode([FromRoute] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                ModelState.AddModelError("usuario", "No existe usuario registrado con ese correo");
                return BadRequest(ModelState);
            }

            var emailProvider = new EmailTokenProvider<GPAUser>();
            var token = await emailProvider.GenerateAsync("Email", _userManager, user);

            var message = new EmailMessage
            {
                Subject = "Código de verificación",
                Body = $"Su código de verificación es: {token}",
                To = new List<string> { email },
            };

            await _emailServiceFactory.SendMessageAsync(message);
                        
            return Ok();
        }

        private async Task<string> AssignProfileAsClaimIfUserHasOnlyOneProfile(Guid userId, List<Claim> claims)
        {
            var profiles = await _gPAProfileService.GetProfilesByUserId(userId);

            if (profiles is { Count: 1 })
            {
                var profileId = profiles.FirstOrDefault()?.Id?.ToString();
                claims.Add(new Claim(GPAClaimTypes.ProfileId, profileId ?? ""));
                return profileId;
            }
            else
            {
                claims.Add(new Claim(GPAClaimTypes.ProfileId, ""));
                return string.Empty;
            }
        }

        private async Task<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, bool>>>>> GetProfilePermissions(string profileId)
        {
            var profile = new List<GPA.Utils.Profiles.Profile>();
            if (profileId is not { Length: 0 })
            {
                var gpaProfile = await _gPAProfileService.GetByIdAsync(Guid.Parse(profileId!));
                if (gpaProfile is { Value: { Length: > 0 } })
                {
                    profile = JsonSerializer.Deserialize<List<GPA.Utils.Profiles.Profile>>(gpaProfile.Value, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    });
                }
            }

            return ProfileConstants.InlineMasterProfile(profile);
        }
    }
}
