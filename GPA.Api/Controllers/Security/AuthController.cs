using FluentValidation;
using GPA.Api.Utils.Filters;
using GPA.Business.Security;
using GPA.Business.Services.Security;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Dtos.Audit;
using GPA.Dtos.General;
using GPA.Dtos.Security;
using GPA.Services.General.BlobStorage;
using GPA.Services.General.Email;
using GPA.Services.General.Security;
using GPA.Utils.Constants.Claims;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        private readonly IAesHelper _aesHelper;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IGPAJwtService jwtService,
            UserManager<GPAUser> userManager,
            GPADbContext context,
            IGPAProfileService gPAProfileService,
            IValidator<SignUpDto> signUpValidator,
            IEmailServiceFactory emailServiceFactory,
            IAesHelper aesHelper,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _context = context;
            _gPAProfileService = gPAProfileService;
            _signUpValidator = signUpValidator;
            _emailServiceFactory = emailServiceFactory;
            _aesHelper = aesHelper;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LogInDto model)
        {
            _logger.LogInformation("User: '{User}' login attempt.", model.UserName);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("User: '{User}' login failed. '{@Error}'", model.UserName, ModelState.Select(x => x.Value));
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user is null)
            {
                _logger.LogWarning("User: '{User}' login failed. El usuario no existe", model.UserName);
                ModelState.AddModelError("usuario", "El usuario no existe");
                return BadRequest(ModelState);
            }

            var resultd = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!resultd)
            {
                _logger.LogWarning("User: '{User}' login failed. Usuario inválido", model.UserName);
                ModelState.AddModelError("usuario", "Usuario inválido");
                return BadRequest(ModelState);
            }

            BlobStorageFileResult? photo = GetUserPhoto(user);

            var claims = new List<Claim>
            {
                new Claim(GPAClaimTypes.FullName, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(GPAClaimTypes.UserId, user.Id.ToString()),
                new Claim(GPAClaimTypes.Photo, photo?.FileUrl ?? "")
            };

            var profileId = await AssignProfileAsClaimIfUserHasOnlyOneProfile(user.Id, claims);

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = claims.ToArray()
            });

            var permissions = await GetProfilePermissions(profileId);
            _logger.LogInformation("User: '{User}' logged in.", model.UserName);

            return Ok(new { token = token, permissions = permissions });
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpDto model)
        {
            if (model is null)
            {
                ModelState.AddModelError("model", "The model is null");
                _logger.LogError("Sign up failed. Modelo nulo");
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
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var passwordHasher = new PasswordHasher<GPAUser>();
            entity.PasswordHash = passwordHasher.HashPassword(entity, model.Password);
            var result = await _userManager.CreateAsync(entity, model.Password);

            if (result.Succeeded)
            {
                await AddHistory(entity, ActionConstants.SignUp, Guid.Empty);
                return Created();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            _logger.LogError("Error signing up. {@Error}", result.Errors.Select(x => x.Description));

            return BadRequest(ModelState);
        }

        [HttpGet("profile/{profileId}/change")]
        public async Task<IActionResult> ChooseProfile(Guid profileId)
        {
            var user = User;
            var userId = user.Claims.FirstOrDefault(x => x.Type == GPAClaimTypes.UserId).Value;
            if (userId is null)
            {
                _logger.LogWarning("Usuario no authorizado '{User}'", User?.Identity?.Name);
                return Unauthorized();
            }

            var exists = await _gPAProfileService.ProfileExists(profileId, Guid.Parse(userId));
            if (!exists)
            {
                _logger.LogWarning("El usuario no tiene acceso al perfil '{User}'", User?.Identity?.Name);
                return new ForbidResult($"El usuario no tiene acceso al perfil '{User?.Identity?.Name}'");
            }

            var claims = user.Claims.Where(x => x.Type != GPAClaimTypes.ProfileId).ToList();
            claims.Add(new Claim(GPAClaimTypes.ProfileId, profileId.ToString()));

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = claims.ToArray()
            });

            _logger.LogWarning("El usuario '{User}' accedió con el perfil '{ProfileId}'", User?.Identity?.Name, profileId);
            var permissions = await GetProfilePermissions(profileId.ToString());
            return Ok(new { token = token, permissions = permissions });
        }

        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Auth}", permission: Permissions.UpdateUserProfile)]
        [HttpPut("users/{userId}/profile/edit")]
        public async Task<IActionResult> EditProfile([FromRoute] Guid userId, [FromBody] UserProfileDto model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var userByEmail = await _userManager.FindByEmailAsync(model.Email);

            if (userByEmail is not null && userByEmail.Id != userId)
            {
                _logger.LogWarning("El usuario '{User}' intentó cambiar su correo '{Correo}' al uno existenten", User?.Identity?.Name, model.Email);
                ModelState.AddModelError("usuario", "El correo ya está registrado");
                return BadRequest(ModelState);
            }

            if (user is null)
            {
                _logger.LogWarning("Intento de editar perfil de usuario. El usuario '{User}' no existe", User?.Identity?.Name);
                ModelState.AddModelError("usuario", "El usuario no existe");
                return BadRequest(ModelState);
            }

            var userClaim = User.Claims.FirstOrDefault(x => x.Type == GPAClaimTypes.UserId);
            if (user.Id.ToString() != userClaim.Value)
            {
                _logger.LogWarning("Intento de editar perfil de usuario. El usuario '{User}' intentó modificar un perfil de otra persona", User?.Identity?.Name);
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
                _logger.LogWarning("Error editando perfil de usuario. Usuario '{User}', causa '{@Error}'", User?.Identity?.Name, result.Errors.Select(x => x.Description));
                ModelState.AddModelError("usuario", "Error modificando el usuario");
                return BadRequest(ModelState);
            }

            BlobStorageFileResult? photo = GetUserPhoto(user);
            var cliams = User.Claims.Where(x => x.Type != GPAClaimTypes.FullName &&
                                                x.Type != GPAClaimTypes.Photo).ToList();

            cliams.Add(new Claim(GPAClaimTypes.FullName, $"{user.FirstName} {user.LastName}"));
            cliams.Add(new Claim(GPAClaimTypes.Photo, photo?.FileUrl ?? ""));

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = cliams.ToArray()
            });

            await AddHistory(user, ActionConstants.Update, user.Id);

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

            _logger.LogInformation("Intento de cambio de contraseña. Usuario '{User}'", email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                _logger.LogInformation("Intento de cambio de contraseña. El usuario '{User}' no existe", email);
                ModelState.AddModelError("usuario", "No existe usuario registrado con ese correo");
                return BadRequest(ModelState);
            }

            var timeSpan = DateTimeOffset.Now - user.TOTPAccessCodeAttemptsDate;
            if (user.TOTPAccessCodeAttempts == 3 && timeSpan.Minutes < 1)
            {
                _logger.LogInformation("Intento de cambio de contraseña. Máximo de intentos alcanzado para '{User}'", email);
                ModelState.AddModelError("usuario", "Debe esperar un minuto para volver a intentar");
                return BadRequest(ModelState);
            }

            var emailProvider = new EmailTokenProvider<GPAUser>();
            var token = await emailProvider.GenerateAsync("password-reset", _userManager, user);

            var message = new EmailMessage
            {
                Subject = "Código de verificación",
                Body = $"Su código de verificación es: {token}",
                To = new List<string> { email },
            };

            user.LastTOTPCode = _aesHelper.Encrypt(token);
            user.TOTPAccessCodeAttempts = 0;
            user.TOTPAccessCodeAttemptsDate = DateTimeOffset.Now;
            await _userManager.UpdateAsync(user);

            try
            {
                await _emailServiceFactory.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Intento de cambio de contraseña. Error enviando correo a '{User}'", email);
                ModelState.AddModelError("usuario", "Error enviando el correo");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Intento de cambio de contraseña. Código enviado a '{User}'", email);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Intento de cambio de contraseña. Usuario '{User}'", model.userName);

            if (model.Password != model.ConfirmPassword)
            {
                _logger.LogWarning("Intento de cambio de contraseña. Las contraseñas no coinciden usuario '{User}'", model.userName);
                ModelState.AddModelError("usuario", "Las contraseñas deben coincidir");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.userName);
            if (user is null)
            {
                _logger.LogWarning("Intento de cambio de contraseña. el usuario no existe, usuario '{User}'", model.userName);
                ModelState.AddModelError("usuario", "No existe usuario registrado con ese correo");
                return BadRequest(ModelState);
            }

            if (user.TOTPAccessCodeAttempts == 3)
            {
                _logger.LogWarning("Intento de cambio de contraseña. Máximo de intentos alcanzado, '{User}'", model.userName);
                ModelState.AddModelError("usuario", "Máximo de intentos alcanzados");
                ModelState.AddModelError("usuario", "Debe solicitar otro código de verificación");
                return BadRequest(ModelState);
            }

            var emailProvider = new EmailTokenProvider<GPAUser>();
            var isTOTPCodeValid = await emailProvider.ValidateAsync("password-reset", model.Code, _userManager, user);

            if (!isTOTPCodeValid || model.Code != _aesHelper.Decrypt(user.LastTOTPCode))
            {
                _logger.LogWarning("Intento de cambio de contraseña. Código TOTP inválido, '{User}'", model.userName);
                await UpdateTOTPCodeAttempts(user);
                ModelState.AddModelError("usuario", "El código ingresado no es válido.");
                ModelState.AddModelError("usuario", "Debe ingresar el código que recibió vía correo");
                return BadRequest(ModelState);
            }

            var passwordHasher = new PasswordHasher<GPAUser>();
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
            await _userManager.UpdateAsync(user);
            await AddHistory(user, ActionConstants.ResetPassword, user.Id);
            _logger.LogWarning("Intento de cambio de contraseña. Contraseña cambiada '{User}'", model.userName);
            return Ok();
        }

        [HttpPost("{userId}/photo/upload")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Auth}", permission: Permissions.Upload)]
        public async Task<IActionResult> UploadPhoto([FromRoute] Guid userId, IFormFile photo)
        {
            if (photo is null)
            {
                ModelState.AddModelError("model", "Debe proveer la foto");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                ModelState.AddModelError("model", "El usuario no existe");
                return BadRequest(ModelState);
            }

            var uploadResult = await _blobStorageServiceFactory.UploadFile(photo, folder: "users/", isPublic: true);
            user.Photo = uploadResult.AsJson();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("usuario", "Error modificando el usuario");
                return BadRequest(ModelState);
            }

            BlobStorageFileResult? blobStorage = GetUserPhoto(user);
            var cliams = User.Claims.Where(x => x.Type != GPAClaimTypes.FullName &&
                                                x.Type != GPAClaimTypes.Photo).ToList();

            cliams.Add(new Claim(GPAClaimTypes.FullName, $"{user.FirstName} {user.LastName}"));
            cliams.Add(new Claim(GPAClaimTypes.Photo, blobStorage?.FileUrl ?? ""));

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = cliams.ToArray()
            });

            var permissions = await GetProfilePermissions(User.Claims.First(x => x.Type == GPAClaimTypes.ProfileId).Value);

            _logger.LogInformation("El usuario '{User}' cambió su foto de perfil", user.UserName);
            await AddHistory(user, ActionConstants.Update, user.Id);
            return Ok(new { token = token, permissions = permissions });
        }

        private static BlobStorageFileResult? GetUserPhoto(GPAUser? user)
        {
            BlobStorageFileResult? photo = null;
            if (user?.Photo is not null)
            {
                try
                {
                    photo = JsonSerializer.Deserialize<BlobStorageFileResult>(user.Photo, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    });
                }
                catch
                {
                    photo = null;
                }
            }

            return photo;
        }

        private async Task UpdateTOTPCodeAttempts(GPAUser user)
        {
            user.TOTPAccessCodeAttempts++;
            await _userManager.UpdateAsync(user);
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
                var gpaProfile = await _gPAProfileService.GetProfilesByIdAsync(Guid.Parse(profileId!));
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


        private async Task AddHistory(GPAUser model, string action, Guid by)
        {
            var query = @"
                INSERT INTO [Audit].[UserHistory]
                       ([Name]
                       ,[UserName]
                       ,[Email]
                       ,[Photo]
                       ,[Phone]
                       ,[IdentityId]
                       ,[Action]
                       ,[By]
                       ,[At])
                 VALUES
                       (@Name
                       ,@UserName
                       ,@Email
                       ,@Photo
                       ,@Phone
                       ,@IdentityId
                       ,@Action
                       ,@By
                       ,@At)
                    ";

            var parameters = new SqlParameter[]
            {
                new("@Name", model.FirstName + " " + model.LastName)
               ,new("@UserName", model.UserName)
               ,new("@Email", model.Email)
               ,new("@Photo", model.Photo ?? "")
               ,new("@Phone", model.PhoneNumber ?? "")
               ,new("@IdentityId", model.Id)
               ,new("@Action", action)
               ,new("@By", by)
               ,new("@At", DateTimeOffset.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        }
    }
}
