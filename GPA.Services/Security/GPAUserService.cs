using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Unmapped;
using GPA.Data.Security;
using GPA.Dtos.Security;
using GPA.Dtos.Unmapped;
using GPA.Entities.Security;
using GPA.Entities.Unmapped.Security;
using GPA.Services.Security;
using System.Text;

namespace GPA.Business.Services.Security
{
    public interface IGPAUserService
    {
        Task<GPAUserDto?> GetUserByIdAsync(Guid id);
        Task<ResponseDto<GPAUserDto>> GetUsersAsync(RequestFilterDto filter);
        Task<RawInvitationToken?> GetInvitationTokenAsync(Guid userId, string token);
        Task AddInvitationTokenAsync(InvitationToken invitationToken);
        Task RedeemInvitationAsync(Guid userId);
        Task RevokeInvitationAsync(Guid userId);
        Task<IEnumerable<RawInvitationTokenDto>> GetInvitationTokensAsync(Guid userId);
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
            filter.Search = filter.Search;
            var entities = await _repository.GetUsersAsync(filter);
            return new ResponseDto<GPAUserDto>
            {
                Count = await _repository.GetUsersCountAsync(filter),
                Data = _mapper.Map<IEnumerable<GPAUserDto>>(entities)
            };
        }

        public async Task<RawInvitationToken?> GetInvitationTokenAsync(Guid userId, string token)
        {
            return await _repository.GetInvitationTokenAsync(userId, token);
        }

        public async Task AddInvitationTokenAsync(InvitationToken invitationToken)
        {
            await _repository.AddInvitationTokenAsync(invitationToken);
        }

        public async Task RedeemInvitationAsync(Guid userId)
        {
            await _repository.RedeemInvitationAsync(userId);
        }

        public async Task RevokeInvitationAsync(Guid userId)
        {
            await _repository.RevokeInvitationAsync(userId);
        }

        public async Task<IEnumerable<RawInvitationTokenDto>> GetInvitationTokensAsync(Guid userId)
        {
            var invitations =  await _repository.GetInvitationTokensAsync(userId);
            var users = await _repository.GetUsersAsync(invitations.Select(i => i.CreatedBy).ToList());

            var invitationsDto = new List<RawInvitationTokenDto>();
            foreach (var item in invitations)
            {
                var invitation = _mapper.Map<RawInvitationTokenDto>(item);
                invitation.CreatedByName = users.FirstOrDefault(x => x.Id == item.CreatedBy)?.UserName;
                invitationsDto.Add(invitation);
            }
            return invitationsDto;
        }
    }
}
