using AutoMapper;
using GPA.Common.DTOs;
using GPA.Data.Network;
using GPA.Dtos.Network;
using GPA.Entities.Network;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IEmailProviderService
    {
        public Task<EmailConfigurationDto?> GetByIdAsync(Guid id);
        public Task<ResponseDto<EmailConfigurationDto>> GetAllAsync(SearchDto search, Expression<Func<EmailConfiguration, bool>>? expression = null);
        public Task<EmailConfigurationDto?> AddAsync(EmailConfigurationCreationDto emailConfig);
        public Task UpdateAsync(EmailConfigurationUpdateDto emailConfig);
        public Task RemoveAsync(Guid id);
    }

    public class EmailProviderService : IEmailProviderService
    {
        private readonly IEmailConfigurationRepository _repository;
        private readonly IMapper _mapper;

        public EmailProviderService(IEmailConfigurationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<EmailConfigurationDto?> GetByIdAsync(Guid id)
        {
            var emailConfiguration = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<EmailConfigurationDto>(emailConfiguration);
        }

        public async Task<ResponseDto<EmailConfigurationDto>> GetAllAsync(SearchDto search, Expression<Func<EmailConfiguration, bool>>? expression = null)
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

        public async Task<EmailConfigurationDto> AddAsync(EmailConfigurationCreationDto dto)
        {
            var emailConfiguration = _mapper.Map<EmailConfiguration>(dto);
            var savedemailConfiguration = await _repository.AddAsync(emailConfiguration);
            return _mapper.Map<EmailConfigurationDto>(savedemailConfiguration);
        }

        public async Task UpdateAsync(EmailConfigurationUpdateDto dto)
        {
            if (dto.Id == Guid.Empty)
            {
                throw new ArgumentNullException();
            }

            var newEmailConfiguration = _mapper.Map<EmailConfiguration>(dto);
            newEmailConfiguration.Id = dto.Id;
            var savedEmailConfiguration = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id);
            await _repository.UpdateAsync(savedEmailConfiguration, newEmailConfiguration, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedCEmailConfiguration = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedCEmailConfiguration);
        }
    }
}
