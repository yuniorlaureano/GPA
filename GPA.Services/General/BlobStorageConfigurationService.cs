using AutoMapper;
using GPA.Common.DTOs;
using GPA.Data.General;
using GPA.Dtos.General;
using GPA.Entities.General;
using GPA.Services.Security;
using GPA.Utils;
using System.Data;
using System.Linq.Expressions;

namespace GPA.Services.General
{
    public interface IBlobStorageConfigurationService
    {
        Task<BlobStorageConfigurationDto?> GetByIdAsync(Guid id);
        Task<ResponseDto<BlobStorageConfigurationDto>> GetAllAsync(RequestFilterDto search, Expression<Func<BlobStorageConfiguration, bool>>? expression = null);
        Task CreateConfigurationAsync(BlobStorageConfigurationCreationDto dto);
        Task UpdateConfigurationAsync(BlobStorageConfigurationUpdateDto dto);
        Task RemoveAsync(Guid id);
    }

    public class BlobStorageConfigurationService : IBlobStorageConfigurationService
    {
        private readonly IBlobStorageConfigurationRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IBlobStorageHelper _blobStorageProviderHelper;
        private readonly IMapper _mapper;

        public BlobStorageConfigurationService(
            IBlobStorageConfigurationRepository repository,
            IUserContextService userContextService,
            IBlobStorageHelper blobStorageProviderHelper,
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _blobStorageProviderHelper = blobStorageProviderHelper;
            _mapper = mapper;
        }

        public async Task<BlobStorageConfigurationDto?> GetByIdAsync(Guid id)
        {
            var emailConfiguration = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<BlobStorageConfigurationDto>(emailConfiguration);
        }

        public async Task<ResponseDto<BlobStorageConfigurationDto>> GetAllAsync(RequestFilterDto search, Expression<Func<BlobStorageConfiguration, bool>>? expression = null)
        {
            var emailConfigurations = await _repository.GetAllAsync(query =>
            {
                return query.OrderByDescending(x => x.Id)
                    .Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<BlobStorageConfigurationDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<BlobStorageConfigurationDto>>(emailConfigurations)
            };
        }

        public async Task CreateConfigurationAsync(BlobStorageConfigurationCreationDto dto)
        {
            var value = _blobStorageProviderHelper.EncryptCredentialsInOptions(dto.Value, dto.Provider);
            dto.Value = _blobStorageProviderHelper.SerializeOptions(value, dto.Provider);
            var blobStorageConfiguration = _mapper.Map<BlobStorageConfiguration>(dto);
            blobStorageConfiguration.CreatedBy = _userContextService.GetCurrentUserId();
            blobStorageConfiguration.CreatedAt = DateTimeOffset.UtcNow;
            await _repository.CreateConfigurationAsync(blobStorageConfiguration);
        }

        public async Task UpdateConfigurationAsync(BlobStorageConfigurationUpdateDto dto)
        {
            var savedEmailConfiguration = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id);

            if (savedEmailConfiguration is null)
            {
                throw new DataException("No existe la configuración de storage");
            }

            var credentialChanged = _blobStorageProviderHelper.CredentialChanged(dto.Value, savedEmailConfiguration.Value, dto.Provider);

            var value = _blobStorageProviderHelper.EncryptCredentialsInOptions(dto.Value, dto.Provider, credentialChanged);
            dto.Value = _blobStorageProviderHelper.SerializeOptions(value, dto.Provider);

            var blobStorageConfiguration = _mapper.Map<BlobStorageConfiguration>(dto);
            blobStorageConfiguration.UpdatedBy = _userContextService.GetCurrentUserId();
            blobStorageConfiguration.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateConfigurationAsync(blobStorageConfiguration);
        }

        public async Task RemoveAsync(Guid id)
        {
            var configuration = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(configuration);
        }
    }
}
