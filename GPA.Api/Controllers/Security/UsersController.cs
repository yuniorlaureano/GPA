using AutoMapper;
using FluentValidation;
using GPA.Api.Utils.Filters;
using GPA.Business.Services.Security;
using GPA.Common.DTOs;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Dtos.Audit;
using GPA.Dtos.Security;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
        private readonly IUserContextService _userContextService;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly GPADbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly IValidator<GPAUserCreationDto> _creationValidator;
        private readonly IValidator<GPAUserUpdateDto> _updateValidator;

        public UsersController(
            UserManager<GPAUser> userManager,
            IMapper mapper,
            IGPAUserService gPAUserService,
            IUserContextService userContextService,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            GPADbContext context,
            IValidator<GPAUserCreationDto> creationValidator,
            IValidator<GPAUserUpdateDto> updateValidator,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _gPAUserService = gPAUserService;
            _userContextService = userContextService;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _context = context;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _gPAUserService.GetUserByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _gPAUserService.GetUsersAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Create)]
        public async Task<IActionResult> Post(GPAUserCreationDto model)
        {
            var validationResult = await _creationValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            if (model is null)
            {
                ModelState.AddModelError("model", "The model is null");
                return BadRequest(ModelState);
            }

            var entity = _mapper.Map<GPAUser>(model);
            entity.Id = Guid.Empty;
            entity.Deleted = false;
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.CreatedBy = _userContextService.GetCurrentUserId();
            var result = await _userManager.CreateAsync(entity, $"Dummy-Password-{Guid.NewGuid().ToString()}*-$#$%");

            if (!result.Succeeded)
            {
                var errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                return BadRequest(errors);
            }
            _logger.LogInformation("'{User}' ha creado un usuario", _userContextService.GetCurrentUserName());
            await AddHistory(entity, ActionConstants.Add, _userContextService.GetCurrentUserId());
            return Created($"/{entity.Id}", new { Id = entity.Id, Email = entity.Email, UserName = entity.UserName });
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Update)]
        public async Task<IActionResult> Put(GPAUserUpdateDto model)
        {
            var validationResult = await _updateValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            if (model is null)
            {
                ModelState.AddModelError("model", "The model is null");
                return BadRequest(ModelState);
            }

            var savedEntity = await _userManager.FindByIdAsync(model.Id.ToString());

            if (savedEntity is null)
            {
                return BadRequest(new[] { "El usuario no existe" });
            }

            var byEmail = await _userManager.FindByEmailAsync(model.Email);
            if (byEmail is not null && savedEntity.Id != byEmail.Id)
            {
                return BadRequest(new[] { $"El email '{model.Email}' pertenece a otro usuario" });
            }

            var byName = await _userManager.FindByNameAsync(model.UserName);
            if (byName is not null && savedEntity.Id != byName.Id)
            {
                return BadRequest(new[] { $"El nombre de usuario '{model.UserName}' pertenece a otro usuario" });
            }

            var originalUserName = savedEntity.UserName;
            savedEntity.FirstName = model.FirstName;
            savedEntity.LastName = model.LastName;
            savedEntity.Email = model.Email;
            savedEntity.UserName = model.UserName;
            savedEntity.UpdatedAt = DateTimeOffset.UtcNow;
            savedEntity.UpdatedBy = _userContextService.GetCurrentUserId();
            await _userManager.UpdateAsync(savedEntity);

            _logger.LogInformation("'{User}' ha modificado al usuario '{ModifiedUser}'", _userContextService.GetCurrentUserName(), originalUserName);
            await AddHistory(savedEntity, ActionConstants.Update, _userContextService.GetCurrentUserId());
            return NoContent();
        }

        [HttpPost("{userId}/photo/upload")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Security}.{Components.User}", permission: Permissions.Upload)]
        public async Task<IActionResult> UploadPhoto([FromRoute] Guid userId, IFormFile photo)
        {
            if (photo is null)
            {
                return BadRequest(new[] { "Debe proveer la foto" });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return BadRequest(new[] { "El usuario no existe" });
            }

            var uploadResult = await _blobStorageServiceFactory.UploadFile(photo, folder: "users/", isPublic: true);
            user.Photo = uploadResult.AsJson();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new[] { "Error modificando el usuario" });
            }

            _logger.LogInformation("'{User}' ha cambiado la foto de perfil del usuario '{ModifiedUser}'", _userContextService.GetCurrentUserName(), user.UserName);
            return Ok();
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
            entity.DeletedAt = DateTimeOffset.UtcNow;
            entity.DeletedBy = _userContextService.GetCurrentUserId();
            var result = await _userManager.UpdateAsync(entity);

            if (!result.Succeeded)
            {
                var errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                return BadRequest(errors);
            }

            _logger.LogInformation("'{User}' ha eliminado al usuario '{ModifiedUser}'", _userContextService.GetCurrentUserName(), entity.UserName);
            await AddHistory(entity, ActionConstants.Remove, _userContextService.GetCurrentUserId());
            return NoContent();
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
