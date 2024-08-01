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
    public interface IEmailProviderService
    {
        Task<EmailConfigurationDto?> GetByIdAsync(Guid id);
        Task<ResponseDto<EmailConfigurationDto>> GetAllAsync(RequestFilterDto search, Expression<Func<EmailConfiguration, bool>>? expression = null);
        Task CreateConfigurationAsync(EmailConfigurationCreationDto dto);
        Task UpdateConfigurationAsync(EmailConfigurationUpdateDto dto);
        Task RemoveAsync(Guid id);
    }

    public class EmailProviderService : IEmailProviderService
    {
        private readonly IEmailConfigurationRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IEmailProviderHelper _emailProviderHelper;
        private readonly IMapper _mapper;

        public EmailProviderService(
            IEmailConfigurationRepository repository,
            IUserContextService userContextService,
            IEmailProviderHelper emailProviderHelper,
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _emailProviderHelper = emailProviderHelper;
            _mapper = mapper;
        }

        public async Task<EmailConfigurationDto?> GetByIdAsync(Guid id)
        {
            var emailConfiguration = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<EmailConfigurationDto>(emailConfiguration);
        }

        public async Task<ResponseDto<EmailConfigurationDto>> GetAllAsync(RequestFilterDto search, Expression<Func<EmailConfiguration, bool>>? expression = null)
        {
            var emailConfigurations = await _repository.GetAllAsync(query =>
            {
                return query.OrderByDescending(x => x.Id)
                    .Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<EmailConfigurationDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<EmailConfigurationDto>>(emailConfigurations)
            };
        }

        public async Task CreateConfigurationAsync(EmailConfigurationCreationDto dto)
        {
            var value = _emailProviderHelper.EncryptCredentialsInOptions(dto.Value, dto.Engine);
            dto.Value = _emailProviderHelper.SerializeOptions(value, dto.Engine);
            var emailConfiguration = _mapper.Map<EmailConfiguration>(dto);
            emailConfiguration.CreatedBy = _userContextService.GetCurrentUserId();
            await _repository.CreateConfigurationAsync(emailConfiguration);
        }

        public async Task UpdateConfigurationAsync(EmailConfigurationUpdateDto dto)
        {
            var savedEmailConfiguration = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id);

            if (savedEmailConfiguration is null)
            {
                throw new DataException("No existe la configuración de correo");
            }

            var credentialChanged = _emailProviderHelper.CredentialChanged(dto.Value, savedEmailConfiguration.Value, dto.Engine);

            var value = _emailProviderHelper.EncryptCredentialsInOptions(dto.Value, dto.Engine, credentialChanged);
            dto.Value = _emailProviderHelper.SerializeOptions(value, dto.Engine);

            var emailConfiguration = _mapper.Map<EmailConfiguration>(dto);
            emailConfiguration.CreatedBy = _userContextService.GetCurrentUserId();
            await _repository.UpdateConfigurationAsync(emailConfiguration);
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedCEmailConfiguration = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedCEmailConfiguration);
        }
    }
}
