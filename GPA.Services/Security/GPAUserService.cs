using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Security;
using GPA.Data.Security;
using GPA.Dtos.Security;
using System.Linq.Expressions;

namespace GPA.Business.Services.Security
{
    public interface IGPAUserService
    {
        public Task<GPAUserDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<GPAUserDto>> GetAllAsync(SearchDto search, Expression<Func<GPAUser, bool>>? expression = null);

        public Task<GPAUserDto?> AddAsync(GPAUserUpdateDto dto);

        public Task UpdateAsync(GPAUserUpdateDto dto);

        public Task RemoveAsync(Guid id);
    }

    public class GPAUserService : IGPAUserService
    {
        private readonly IGPAUserRepository _repository;
        private readonly IGPAProfileRepository _profileRepository;
        private readonly IMapper _mapper;

        public GPAUserService(IGPAUserRepository repository, IGPAProfileRepository profileRepository, IMapper mapper)
        {
            _repository = repository;
            _profileRepository = profileRepository;
            _mapper = mapper;
        }

        public async Task<GPAUserDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            var dto = _mapper.Map<GPAUserDto>(entity);
            if (dto is not null)
            {
                var profiles = await _profileRepository.GetProfilesByUserId(entity.Id);
                dto.Profiles = profiles is null ? 
                    dto.Profiles : 
                    _mapper.Map<List<RawProfileDto>>(profiles);
            }
            return dto;
        }

        public async Task<ResponseDto<GPAUserDto>> GetAllAsync(SearchDto search, Expression<Func<GPAUser, bool>>? expression = null)
        {
            var entities = await _repository.GetAllAsync(query =>
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);

            return new ResponseDto<GPAUserDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<GPAUserDto>>(entities)
            };
        }

        public async Task<GPAUserDto> AddAsync(GPAUserUpdateDto dto)
        {
            var entity = _mapper.Map<GPAUser>(dto);
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
