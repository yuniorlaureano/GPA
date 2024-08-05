using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;

namespace GPA.Business.Services.Inventory
{
    public interface IAddonService
    {
        public Task<AddonDto?> GetAddonsAsync(Guid id);

        public Task<ResponseDto<AddonDto>> GetAddonsAsync(RequestFilterDto search);

        public Task<AddonDto?> AddAsync(AddonDto addonDto);

        public Task UpdateAsync(AddonDto addonDto);

        public Task RemoveAsync(Guid id);
    }

    public class AddonService : IAddonService
    {
        private readonly IAddonRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public AddonService(
            IAddonRepository repository,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<AddonDto?> GetAddonsAsync(Guid id)
        {
            var addon = await _repository.GetAddonsAsync(id);
            return _mapper.Map<AddonDto>(addon);
        }

        public async Task<ResponseDto<AddonDto>> GetAddonsAsync(RequestFilterDto search)
        {
            var categories = await _repository.GetAddonsAsync(search);
            return new ResponseDto<AddonDto>
            {
                Count = await _repository.GetAddonsCountAsync(search),
                Data = _mapper.Map<IEnumerable<AddonDto>>(categories)
            };
        }

        public async Task<AddonDto> AddAsync(AddonDto dto)
        {
            var addon = _mapper.Map<Addon>(dto);
            addon.CreatedBy = _userContextService.GetCurrentUserId();
            addon.CreatedAt = DateTimeOffset.UtcNow;
            var savedAddon = await _repository.AddAsync(addon);
            return _mapper.Map<AddonDto>(savedAddon);
        }

        public async Task UpdateAsync(AddonDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newAddon = _mapper.Map<Addon>(dto);
            newAddon.Id = dto.Id.Value;
            var savedAddon = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            newAddon.UpdatedBy = _userContextService.GetCurrentUserId();
            newAddon.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(savedAddon, newAddon, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedAddon = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedAddon);
        }
    }
}
