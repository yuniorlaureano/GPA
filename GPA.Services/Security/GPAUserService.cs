using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Security;
using GPA.Data.Security;
using GPA.Dtos.Security;
using GPA.Services.Security;
using System.Text;

namespace GPA.Business.Services.Security
{
    public interface IGPAUserService
    {
        public Task<GPAUserDto?> GetUserByIdAsync(Guid id);
        public Task<ResponseDto<GPAUserDto>> GetUsersAsync(RequestFilterDto filter);
        public Task<GPAUserDto?> AddAsync(GPAUserUpdateDto dto);
        public Task UpdateAsync(GPAUserUpdateDto dto);
        public Task RemoveAsync(Guid id);
    }

    public class GPAUserService : IGPAUserService
    {
        private readonly IGPAUserRepository _repository;
        private readonly IGPAProfileRepository _profileRepository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public GPAUserService(
            IGPAUserRepository repository,
            IGPAProfileRepository profileRepository,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _repository = repository;
            _profileRepository = profileRepository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<GPAUserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _repository.GetUserByIdAsync(id);
            var dto = _mapper.Map<GPAUserDto>(user);
            if (dto is not null)
            {
                var profiles = await _profileRepository.GetProfilesByUserId(user.Id);
                dto.Profiles = profiles is null ?
                    dto.Profiles :
                    _mapper.Map<List<RawProfileDto>>(profiles);
            }
            return dto;
        }

        public async Task<ResponseDto<GPAUserDto>> GetUsersAsync(RequestFilterDto filter)
        {
            filter.Search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            var entities = await _repository.GetUsersAsync(filter);
            return new ResponseDto<GPAUserDto>
            {
                Count = await _repository.GetUsersCountAsync(filter),
                Data = _mapper.Map<IEnumerable<GPAUserDto>>(entities)
            };
        }

        public async Task<GPAUserDto> AddAsync(GPAUserUpdateDto dto)
        {
            var entity = _mapper.Map<GPAUser>(dto);
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.CreatedBy = _userContextService.GetCurrentUserId();
            var savedEntity = await _repository.AddAsync(entity);
            return _mapper.Map<GPAUserDto>(savedEntity);
        }

        public async Task UpdateAsync(GPAUserUpdateDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newEntity = _mapper.Map<GPAUser>(dto);
            newEntity.Id = dto.Id.Value;
            newEntity.UpdatedAt = DateTimeOffset.UtcNow;
            newEntity.UpdatedBy = _userContextService.GetCurrentUserId();
            var savedEntity = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedEntity, newEntity, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
                entityState.Property(x => x.Profiles).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(entity);
        }
    }
}
