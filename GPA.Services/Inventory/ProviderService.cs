using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GPA.Business.Services.Inventory
{
    public interface IProviderService
    {
        public Task<ProviderDto?> GetProviderByIdAsync(Guid id);

        public Task<ResponseDto<ProviderDto>> GetProvidersAsync(RequestFilterDto search);

        public Task<ProviderDto?> AddAsync(ProviderDto providerDto);

        public Task UpdateAsync(ProviderDto providerDto);

        public Task RemoveAsync(Guid id);
    }

    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProviderService> _logger;

        public ProviderService(
            IProviderRepository repository,
            IUserContextService userContextService,
            IMapper mapper,
            ILogger<ProviderService> logger)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProviderDto?> GetProviderByIdAsync(Guid id)
        {
            var provider = await _repository.GetProviderByIdAsync(id);
            return _mapper.Map<ProviderDto>(provider);
        }

        public async Task<ResponseDto<ProviderDto>> GetProvidersAsync(RequestFilterDto filter)
        {
            filter.Search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            var providers = await _repository.GetProvidersAsync(filter);
            return new ResponseDto<ProviderDto>
            {
                Count = await _repository.GetProvidersCountAsync(filter),
                Data = _mapper.Map<IEnumerable<ProviderDto>>(providers)
            };
        }

        public async Task<ProviderDto> AddAsync(ProviderDto dto)
        {
            var newProvider = _mapper.Map<Provider>(dto);
            newProvider.CreatedBy = _userContextService.GetCurrentUserId();
            newProvider.CreatedAt = DateTimeOffset.UtcNow;
            var savedProvider = await _repository.AddAsync(newProvider);
            _logger.LogInformation("El usuario '{UserId}' ha agregado el proveedor '{ProviderId}'", _userContextService.GetCurrentUserId(), savedProvider?.Id);
            return _mapper.Map<ProviderDto>(savedProvider);
        }

        public async Task UpdateAsync(ProviderDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newProvider = _mapper.Map<Provider>(dto);
            newProvider.Id = dto.Id.Value;
            newProvider.UpdatedBy = _userContextService.GetCurrentUserId();
            newProvider.UpdatedAt = DateTimeOffset.UtcNow;
            var savedProvider = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedProvider, newProvider, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
            _logger.LogInformation("El usuario '{UserId}' ha modificado el proveedor '{ProviderId}'", _userContextService.GetCurrentUserId(), savedProvider.Id);
        }

        public async Task RemoveAsync(Guid id)
        {
            var newProvider = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(newProvider);
            _logger.LogInformation("El usuario '{User}' ha borrado el proveedor '{ProviderId}'", _userContextService.GetCurrentUserId(), id);
        }
    }
}
