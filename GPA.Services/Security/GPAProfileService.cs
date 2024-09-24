using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.Entities.Security;
using GPA.Data.Security;
using GPA.Dtos.Audit;
using GPA.Dtos.Security;
using GPA.Entities.Unmapped;
using GPA.Services.Security;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GPA.Business.Services.Security
{
    public interface IGPAProfileService
    {
        Task<GPAProfileDto?> GetProfilesByIdAsync(Guid id);
        Task<ResponseDto<GPAProfileDto>> GetProfilesAsync(RequestFilterDto search);
        Task<GPAProfileDto?> AddAsync(GPAProfileDto dto);
        Task UpdateAsync(GPAProfileDto dto);
        Task AssignProfileToUser(Guid profileId, Guid userId);
        Task<ResponseDto<RawUser>> GetUsers(Guid profileId, RequestFilterDto search);
        Task RemoveAsync(Guid id);
        Task UnAssignProfileFromUser(Guid profileId, Guid userId);
        Task<List<GPAProfileDto>> GetProfilesByUserId(Guid userId);
        Task<bool> ProfileExists(Guid profileId, Guid userId);
        Task<bool> ProfileExists(Guid userId);
    }

    public class GPAProfileService : IGPAProfileService
    {
        private readonly IGPAProfileRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly ILogger<GPAProfileService> _logger;

        public GPAProfileService(
            IGPAProfileRepository repository,
            IUserContextService userContextService,
            IMapper mapper,
            ILogger<GPAProfileService> logger)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GPAProfileDto?> GetProfilesByIdAsync(Guid id)
        {
            var entity = await _repository.GetProfilesByIdAsync(id);
            return entity is null ? null : new GPAProfileDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Value = entity.Value
            };
        }

        public async Task<List<GPAProfileDto>> GetProfilesByUserId(Guid userId)
        {
            var profilesDto = new List<GPAProfileDto>();
            var profiles = await _repository.GetProfilesByUserId(userId);
            if (profiles is not null)
            {
                profilesDto.AddRange(profiles.Select(entity => new GPAProfileDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Value = entity.Value
                }));
            }
            return profilesDto;
        }

        public async Task<ResponseDto<GPAProfileDto>> GetProfilesAsync(RequestFilterDto filter)
        {
            filter.Search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            var entities = await _repository.GetProfilesAsync(filter);
            return new ResponseDto<GPAProfileDto>
            {
                Count = await _repository.GetProfilesCountAsync(filter),
                Data = new List<GPAProfileDto>(entities.Select(x => new GPAProfileDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Value = x.Value
                }))
            };
        }

        public async Task<GPAProfileDto> AddAsync(GPAProfileDto dto)
        {
            var entity = new GPAProfile
            {
                Name = dto.Name,
            };
            entity.CreatedBy = _userContextService.GetCurrentUserId();
            entity.CreatedAt = DateTimeOffset.UtcNow;
            var savedEntity = await _repository.AddAsync(entity);

            await _repository.AddHistory(savedEntity, ActionConstants.Add, _userContextService.GetCurrentUserId());
            _logger.LogInformation("El usuario '{UserId}' ha creado el perfil '{ProfileId}'", _userContextService.GetCurrentUserId(), savedEntity.Id);
            return new GPAProfileDto
            {
                Id = savedEntity.Id,
                Name = savedEntity.Name,
                Value = savedEntity.Value
            };
        }

        public async Task UpdateAsync(GPAProfileDto dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException();
            }

            var savedEntity = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id);

            if (savedEntity?.Name == "administrador")
            {
                throw new InvalidOperationException("No puede modificar el perfil administrador");
            }

            savedEntity.Name = dto.Name;
            savedEntity.Value = dto.Value;
            savedEntity.UpdatedBy = _userContextService.GetCurrentUserId();
            savedEntity.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(savedEntity, savedEntity, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });

            _logger.LogInformation("El usuario '{UserId}' ha modificado el perfil '{ProfileId}'", _userContextService.GetCurrentUserId(), savedEntity.Id);
            await _repository.AddHistory(savedEntity, ActionConstants.Update, _userContextService.GetCurrentUserId());
        }

        public async Task AssignProfileToUser(Guid profileId, Guid userId)
        {
            var createdBy = _userContextService.GetCurrentUserId();
            await _repository.AssignProfileToUser(profileId, userId, createdBy);
            await _repository.AddUserProfileHistory(userId, profileId, ActionConstants.Assign, _userContextService.GetCurrentUserId());
            _logger.LogInformation("El usuario '{UserId}' ha asignado el perfil '{ProfileId}' al usuario '{AsignToId}'", _userContextService.GetCurrentUserId(), profileId, userId);
        }

        public async Task<ResponseDto<RawUser>> GetUsers(Guid profileId, RequestFilterDto filter)
        {
            filter.Search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            return new ResponseDto<RawUser>
            {
                Count = await _repository.GetUsersCount(filter),
                Data = await _repository.GetUsers(profileId, filter)
            };
        }

        public async Task UnAssignProfileFromUser(Guid profileId, Guid userId)
        {
            await _repository.AddUserProfileHistory(userId, profileId, ActionConstants.UnAssign, _userContextService.GetCurrentUserId());
            await _repository.UnAssignProfileFromUser(profileId, userId);
            _logger.LogInformation("El usuario '{UserId}' ha desasignado el perfil '{ProfileId}' al usuario '{AsignToId}'", _userContextService.GetCurrentUserId(), profileId, userId);
        }

        public Task<bool> ProfileExists(Guid profileId, Guid userId)
        {
            return _repository.ProfileExists(profileId, userId);
        }

        public Task<bool> ProfileExists(Guid userId)
        {
            return _repository.ProfileExists(userId);
        }

        public async Task RemoveAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(query => query, x => x.Id == id);

            if (entity?.Name == "administrador")
            {
                throw new InvalidOperationException("No puede eliminar el perfil administrador");
            }

            await _repository.AddHistory(entity, ActionConstants.Remove, _userContextService.GetCurrentUserId());

            await _repository.RemoveAsync(entity);
            _logger.LogInformation("El usuario '{UserId}' ha eliminado el perfil '{ProfileId}'", _userContextService.GetCurrentUserId(), id);
        }
    }
}
