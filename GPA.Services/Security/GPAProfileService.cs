using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.Entities.Security;
using GPA.Data.Security;
using GPA.Dtos.Security;
using GPA.Entities.Unmapped;
using System.Linq.Expressions;

namespace GPA.Business.Services.Security
{
    public interface IGPAProfileService
    {
        public Task<GPAProfileDto?> GetByIdAsync(Guid id);
        public Task<ResponseDto<GPAProfileDto>> GetAllAsync(SearchDto search, Expression<Func<GPAProfile, bool>>? expression = null);
        public Task<GPAProfileDto?> AddAsync(GPAProfileDto dto);
        public Task UpdateAsync(GPAProfileDto dto);
        Task AssignProfileToUser(Guid profileId, Guid userId);
        Task<ResponseDto<RawUser>> GetUsers(Guid profileId, SearchDto search);
        public Task RemoveAsync(Guid id);
        public Task UnAssignProfileFromUser(Guid profileId, Guid userId);
        Task<List<GPAProfileDto>> GetProfilesByUserId(Guid userId);
        Task<bool> ProfileExists(Guid profileId, Guid userId);
        Task<bool> ProfileExists(Guid userId);
    }

    public class GPAProfileService : IGPAProfileService
    {
        private readonly IGPAProfileRepository _repository;
        private readonly IMapper _mapper;

        public GPAProfileService(IGPAProfileRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GPAProfileDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(query => query, x => x.Id == id);
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

        public async Task<ResponseDto<GPAProfileDto>> GetAllAsync(SearchDto search, Expression<Func<GPAProfile, bool>>? expression = null)
        {
            var entities = await _repository.GetAllAsync(query =>
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<GPAProfileDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
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
            var savedEntity = await _repository.AddAsync(entity);
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
            savedEntity.Name = dto.Name;
            savedEntity.Value = dto.Value;
            await _repository.UpdateAsync(savedEntity, savedEntity, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task AssignProfileToUser(Guid profileId, Guid userId)
        {
            var createdBy = Guid.Empty;
            await _repository.AssignProfileToUser(profileId, userId, createdBy);
        }

        public async Task<ResponseDto<RawUser>> GetUsers(Guid profileId, SearchDto search)
        {
            return new ResponseDto<RawUser>
            {
                Count = await _repository.GetUsersCount(),
                Data = await _repository.GetUsers(profileId, search.Page, search.PageSize)
            };
        }

        public async Task UnAssignProfileFromUser(Guid profileId, Guid userId)
        {
            await _repository.UnAssignProfileFromUser(profileId, userId);
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
            await _repository.RemoveAsync(entity);
        }
    }
}
