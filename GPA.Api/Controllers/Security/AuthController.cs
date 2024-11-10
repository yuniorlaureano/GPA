using FluentValidation;
using GPA.Api.Extensions;
using GPA.Api.Utils.Filters;
using GPA.Business.Security;
using GPA.Business.Services.Security;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Dtos.Audit;
using GPA.Dtos.General;
using GPA.Dtos.Security;
using GPA.Services.General;
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
        private readonly IGPAProfileService _gPAProfileService;
        private readonly IValidator<SignUpDto> _signUpValidator;
        private readonly IEmailServiceFactory _emailServiceFactory;
        private readonly IAesHelper _aesHelper;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly IPasswordResetTemplate _passwordResetTemplate;
        private readonly IGPAUserService _gPAUserService;
        private readonly IInvitationRedemptionTemplate _invitationRedemptionTemplate;
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
            IPasswordResetTemplate passwordResetTemplate,
            IGPAUserService gPAUserService,
            IInvitationRedemptionTemplate invitationRedemptionTemplate,
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
            _passwordResetTemplate = passwordResetTemplate;
            _gPAUserService = gPAUserService;
            _invitationRedemptionTemplate = invitationRedemptionTemplate;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LogInDto model)
        {
            _logger.LogInformation("Usuario: '{UserName}' intento de login.", model.UserName);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorMessage());
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user is null)
            {
                _logger.LogWarning("Usuario: '{UserName}' no existe", model.UserName);
                return BadRequest(new[] { "El usuario no existe" });
            }

            if (user.Deleted)
            {
                _logger.LogWarning("Usuario: '{UserId}' desabilitado", user.Id);
                return Unauthorized(new[] { "El usuario está desabilitado" });
            }

            if (!user.Invited)
            {
                _logger.LogWarning("Usuario: '{UserId}' no ha sido invitado", user.Id);
                return Unauthorized(new[] { "El usuario no ha sido invitado", "Comuniquese con el administrador para que le envíe otra invitación" });
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Usuario: '{UserId}' no ha confirmado su invitación", user.Id);
                return Unauthorized(new[] { "El usuario no ha confirmado su invitación", "Comuniquese con el administrador para que le envíe otra invitación con su nuevo código de invitación" });
            }

            var result = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!result)
            {
                _logger.LogWarning("Usuario: '{UserId}' error autenticando", user.Id);
                return BadRequest(new[] { "Usuario inválido" });
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

            var profileId = await AssignProfileAsClaim(user.Id, claims);

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = claims.ToArray()
            });

            var permissions = await GetProfilePermissions(profileId);
            _logger.LogWarning("Usuario: '{UserId}' authenticado", user.Id);

            return Ok(new { token = token, permissions = permissions });
        }

        [HttpGet("profile/{profileId}/change")]
        public async Task<IActionResult> ChooseProfile(Guid profileId)
        {
            var user = User;
            var userId = user.Claims.FirstOrDefault(x => x.Type == GPAClaimTypes.UserId).Value;
            if (userId is null)
            {
                _logger.LogWarning("Usuario no authorizado '{UserName}'", User?.Identity?.Name);
                return Unauthorized();
            }

            var exists = await _gPAProfileService.ProfileExists(profileId, Guid.Parse(userId));
            if (!exists)
            {
                _logger.LogWarning("El usuario no tiene acceso al perfil '{UserId}'", userId);
                return new ForbidResult($"El usuario no tiene acceso al perfil '{User?.Identity?.Name}'");
            }

            var claims = user.Claims.Where(x => x.Type != GPAClaimTypes.ProfileId).ToList();
            claims.Add(new Claim(GPAClaimTypes.ProfileId, profileId.ToString()));

            var token = _jwtService.GenerateToken(new TokenDescriptorDto
            {
                Algorithm = SecurityAlgorithms.HmacSha256Signature,
                Claims = claims.ToArray()
            });

            _logger.LogWarning("El usuario '{UserId}' accedió con el perfil '{ProfileId}'", userId, profileId);
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
                _logger.LogWarning("El usuario '{UserId}' intentó cambiar su correo '{Correo}' al uno existenten", userId, model.Email);
                return BadRequest(new[] { "El correo ya está registrado" });
            }

            if (user is null)
            {
                _logger.LogWarning("Intento de editar perfil de usuario. El usuario '{UserId}' no existe", userId);
                return BadRequest(new[] { "El usuario no existe" });
            }

            if (user.Deleted)
            {
                return Unauthorized(new[] { "El usuario está desabilitado" });
            }

            var userClaim = User.Claims.FirstOrDefault(x => x.Type == GPAClaimTypes.UserId);
            if (user.Id.ToString() != userClaim.Value)
            {
                _logger.LogWarning("Intento de editar perfil de usuario. El usuario '{UserId}' intentó modificar un perfil de otra persona", userId);
                return BadRequest(new[] { "Solo debe modificar su propio usuario" });
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.SecurityStamp = Guid.NewGuid().ToString();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Error editando perfil de usuario. Usuario '{UserId}', causa '{@Error}'", userId, result.Errors.Select(x => x.Description));
                return BadRequest(new[] { "Error modificando el usuario" });
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
            _logger.LogWarning("Perfil de usuario '{UserId}' editado.", userId);

            return Ok(new { token = token, permissions = permissions });
        }

        [AllowAnonymous]
        [HttpGet("totp/send/{email}")]
        public async Task<IActionResult> SendTOTPCode([FromRoute] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorMessage());
            }

            _logger.LogInformation("Intento de cambio de contraseña. Usuario '{UserEmail}'", email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                _logger.LogInformation("Intento de cambio de contraseña. El usuario '{UserId}' no existe", user.Id);
                return BadRequest(new[] { "No existe usuario registrado con ese correo" });
            }

            if (user.Deleted)
            {
                return Unauthorized(new[] { "El usuario está desabilitado" });
            }

            if (!user.Invited)
            {
                return Unauthorized(new[] { "El usuario no ha sido invitado", "Comuniquese con el administrador para que le envíe otra invitación" });
            }

            var timeSpan = DateTimeOffset.Now - user.TOTPAccessCodeAttemptsDate;
            if (user.TOTPAccessCodeAttempts == 3 && timeSpan.Minutes < 1)
            {
                _logger.LogInformation("Intento de cambio de contraseña. Máximo de intentos alcanzado para '{UserId}'", user.Id);
                return BadRequest(new[] { "Debe esperar un minuto para volver a intentar" });
            }

            var emailProvider = new EmailTokenProvider<GPAUser>();
            var token = await emailProvider.GenerateAsync("password-reset", _userManager, user);

            var template = await _passwordResetTemplate.GetPasswordResetTemplate();
            var message = new EmailMessage
            {
                Subject = "Código de verificación",
                Body = template.Replace("{Code}", token),
                IsBodyHtml = true,
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
                _logger.LogInformation("Intento de cambio de contraseña. Error enviando correo a '{UserId}'", user.Id);
                return BadRequest(new[] { "Error enviando el correo" });
            }

            _logger.LogInformation("Intento de cambio de contraseña. Código enviado a '{UserId}'", user.Id);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorMessage());
            }

            _logger.LogInformation("Intento de cambio de contraseña. Usuario '{UserName}'", model.userName);

            if (model.Password != model.ConfirmPassword)
            {
                _logger.LogWarning("Intento de cambio de contraseña. Las contraseñas no coinciden usuario '{UserName}'", model.userName);
                return BadRequest(new[] { "Las contraseñas deben coincidir" });
            }

            var user = await _userManager.FindByNameAsync(model.userName);
            if (user is null)
            {
                _logger.LogWarning("Intento de cambio de contraseña. el usuario no existe, usuario '{UserName}'", model.userName);
                return BadRequest(new[] { "No existe usuario registrado con ese correo" });
            }

            if (user.Deleted)
            {
                _logger.LogWarning("El usuario '{UserId}' está desabilitado", user.Id);
                return Unauthorized(new[] { "El usuario está desabilitado" });
            }

            if (!user.Invited)
            {
                _logger.LogWarning("El usuario '{UserId}' no ha sido invitado", user.Id);
                return Unauthorized(new[] { "El usuario no ha sido invitado" });
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("El usuario '{UserId}' no ha confirmado su invitación", user.Id);
                return Unauthorized(new[] { "El usuario no ha confirmado su invitación", "Comuniquese con el administrador para que le envíe otra invitación con su nuevo código de invitación" });
            }

            if (user.TOTPAccessCodeAttempts == 3)
            {
                _logger.LogWarning("Intento de cambio de contraseña. Máximo de intentos alcanzado, '{UserId}'", user.Id);
                return BadRequest(new[] { "Máximo de intentos alcanzados", "Debe solicitar otro código de verificación" });
            }

            var emailProvider = new EmailTokenProvider<GPAUser>();
            var isTOTPCodeValid = await emailProvider.ValidateAsync("password-reset", model.Code, _userManager, user);

            if (!isTOTPCodeValid || model.Code != _aesHelper.Decrypt(user.LastTOTPCode))
            {
                _logger.LogWarning("Intento de cambio de contraseña. Código TOTP inválido, '{UserId}'", user.Id);
                await UpdateTOTPCodeAttempts(user);
                return BadRequest(new[] { "El código ingresado no es válido.", "Debe ingresar el código que recibió vía correo" });
            }

            var passwordHasher = new PasswordHasher<GPAUser>();
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            await AddHistory(user, ActionConstants.ResetPassword, user.Id);
            _logger.LogWarning("Intento de cambio de contraseña. Contraseña cambiada '{UserId}'", user.Id);
            return Ok();
        }

        [HttpPost("{userId}/photo/upload")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Auth}", permission: Permissions.Upload)]
        public async Task<IActionResult> UploadPhoto([FromRoute] Guid userId, IFormFile photo)
        {
            if (photo is null)
            {
                return BadRequest(new[] { "Debe proveer la foto" });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new[] { "Solo admite imágenes .jpg, .jpeg, .png.", $"{fileExtension} no es válida" });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return BadRequest(new[] { "El usuario no existe" });
            }

            if (user.Deleted)
            {
                return Unauthorized(new[] { "El usuario está desabilitado" });
            }

            var uploadResult = await _blobStorageServiceFactory.UploadFile(photo, folder: "users/", isPublic: true);
            user.Photo = uploadResult.AsJson();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new[] { "Error modificando el usuario" });
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

            _logger.LogInformation("El usuario '{UserId}' cambió su foto de perfil", user.Id);
            await AddHistory(user, ActionConstants.Update, user.Id);
            return Ok(new { token = token, permissions = permissions });
        }

        [AllowAnonymous]
        [HttpGet("totp-invitation/send")]
        public async Task<IActionResult> SendTOTPInvitationCode([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(ModelState.ErrorMessage());
            }

            var serializedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var decryptedToken = _aesHelper.Decrypt(serializedToken);
            var tokenData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedToken);

            var tokenId = tokenData["id"];
            var userId = tokenData["userId"];
            var profileId = tokenData["profileId"];

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return BadRequest(new[] { "No existe el usuario" });
            }

            if (user.Deleted)
            {
                return Unauthorized(new[] { "El usuario está desabilitado" });
            }

            if (!user.Invited)
            {
                return Unauthorized(new[] { "El usuario no ha sido invitado", "Comuniquese con el administrador para que le envíe otra invitación" });
            }

            var invitationToken = await _gPAUserService.GetInvitationTokenAsync(Guid.Parse(tokenId));
            if (invitationToken is null)
            {
                return BadRequest(new[] { "El token de invitación no existe" });
            }

            var isExpired = (invitationToken.Expiration - DateTime.UtcNow).TotalDays;
            if (isExpired <= 0)
            {
                return BadRequest(new[] { "El token de invitación está expirado" });
            }

            if (invitationToken.Revoked)
            {
                return BadRequest(new[] { "El token de invitación ha sido revocado" });
            }

            if (invitationToken.Redeemed)
            {
                return BadRequest(new[] { "El token de invitación ha sido utilizado", "Si olvidó su contreña restablezcala" });
            }

            var emailProvider = new EmailTokenProvider<GPAUser>();
            var totpcode = await emailProvider.GenerateAsync("invitation-redemption", _userManager, user);

            var template = await _invitationRedemptionTemplate.GetInvitationRedemptionTemplate();
            var message = new EmailMessage
            {
                Subject = "Código de verificación",
                Body = template.Replace("{Code}", totpcode).Replace("{User}", user.UserName),
                IsBodyHtml = true,
                To = new List<string> { user.Email },
            };

            user.LastTOTPCode = _aesHelper.Encrypt(totpcode);
            user.TOTPAccessCodeAttempts = 0;
            user.TOTPAccessCodeAttemptsDate = DateTimeOffset.Now;
            await _userManager.UpdateAsync(user);

            try
            {
                await _emailServiceFactory.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Redención de invitación. Error enviando correo a '{UserId}'", user.Id);
                return BadRequest(new[] { "Error enviando el correo" });
            }

            await _gPAProfileService.AssignProfileToUser(Guid.Parse(profileId), user.Id);

            _logger.LogInformation("Redención de invitación. Código enviado a '{UserId}'", user.Id);
            return Ok(new { id = user.Id, firstName = user.FirstName, lastName = user.LastName, email = user.Email, userName = user.UserName });
        }

        [AllowAnonymous]
        [HttpPost("reset-password-for-invitation")]
        public async Task<IActionResult> SetPasswordAfterInvitationRedemption([FromBody] PasswordResetInvitationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorMessage());
            }

            if (model.Password != model.ConfirmPassword)
            {
                _logger.LogWarning("Aceptando invitación. Las contraseñas no coinciden usuario");
                return BadRequest(new[] { "Las contraseñas deben coincidir" });
            }

            var user = await _userManager.FindByIdAsync(model.userId);
            if (user is null)
            {
                _logger.LogWarning("Intento de cambio de contraseña. El usuario no existe, usuario '{User}'", user.Email);
                return BadRequest(new[] { "El usaurio no existe." });
            }

            if (user.Deleted)
            {
                return Unauthorized(new[] { "El usuario está desabilitado" });
            }

            if (!user.Invited)
            {
                return Unauthorized(new[] { "El usuario no ha sido invitado" });
            }

            if (user.TOTPAccessCodeAttempts == 3)
            {
                _logger.LogWarning("Intento de cambio de contraseña. Máximo de intentos alcanzado, '{UserId}'", user.Id);
                return BadRequest(new[] { "Máximo de intentos alcanzados", "Debe solicitar otro código de verificación" });
            }

            var emailProvider = new EmailTokenProvider<GPAUser>();
            var isTOTPCodeValid = await emailProvider.ValidateAsync("invitation-redemption", model.Code, _userManager, user);

            if (!isTOTPCodeValid || model.Code != _aesHelper.Decrypt(user.LastTOTPCode))
            {
                _logger.LogWarning("Intento de cambio de contraseña. Código TOTP inválido, '{UserId}'", user.Id);
                await UpdateTOTPCodeAttempts(user);
                return BadRequest(new[] { "El código ingresado no es válido.", "Debe ingresar el código que recibió vía correo" });
            }

            var passwordHasher = new PasswordHasher<GPAUser>();
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            await _gPAUserService.RedeemInvitationAsync(user.Id);

            await AddHistory(user, ActionConstants.InvitationAccepted, user.Id);

            _logger.LogWarning("Intento de cambio de contraseña. Contraseña cambiada '{UserId}'", user.Id);
            return Ok();
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

        private async Task<string> AssignProfileAsClaim(Guid userId, List<Claim> claims)
        {
            var profiles = await _gPAProfileService.GetProfilesByUserId(userId);

            if (profiles is { Count: > 0 })
            {
                var currentProfile = "";
                var profileId = "";
                foreach (var profile in profiles)
                {
                    if (string.IsNullOrEmpty(currentProfile) || profile.Value?.Length > currentProfile.Length)
                    {
                        currentProfile = profile.Value;
                        profileId = profile.Id.ToString();
                    }
                }
                claims.Add(new Claim(GPAClaimTypes.ProfileId, profileId));
                return profileId;
            }
            return "";
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
