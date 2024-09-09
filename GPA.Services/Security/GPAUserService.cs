using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Unmapped;
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
    }
}
