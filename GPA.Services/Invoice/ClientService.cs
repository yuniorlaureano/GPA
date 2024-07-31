using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoice;
using GPA.Common.Entities.Invoice;
using GPA.Data.Invoice;
using GPA.Services.Security;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Invoice
{
    public interface IClientService
    {
        public Task<ClientDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<ClientDto>> GetAllAsync(SearchDto search, Expression<Func<Client, bool>>? expression = null);

        public Task<ClientDto?> AddAsync(ClientDto clientDto);

        public Task UpdateAsync(ClientDto clientDto);

        public Task RemoveAsync(Guid id);

        Task<List<ClientCreditDto>> GetCredits(Guid clientId);
    }

    public class ClientService : IClientService
    {
        private readonly IClientRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly IReceivableAccountRepository _receivableAccountRepository;

        public ClientService(
            IClientRepository repository,
            IUserContextService userContextService,
            IMapper mapper, 
            IReceivableAccountRepository receivableAccountRepository)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _receivableAccountRepository = receivableAccountRepository;
        }

        public async Task<ClientDto?> GetByIdAsync(Guid id)
        {
            var client = await _repository.GetByIdAsync(query => query.Include(x => x.Credits), x => x.Id == id);
            var penddingPayments = await _receivableAccountRepository.GetPenddingPaymentByClientId(id);

            var clientDto = _mapper.Map<ClientDto>(client);
            if (client is not null)
            {                
                clientDto.Debits = _mapper.Map<ClientDebitDto[]>(penddingPayments);
            }

            return clientDto;
        }

        public async Task<ResponseDto<ClientDto>> GetAllAsync(SearchDto search, Expression<Func<Client, bool>>? expression = null)
        {
            var clients = await _repository.GetAllAsync(query =>
            {
                return query
                    .Include(x => x.Credits)
                    .OrderByDescending(x => x.Id)
                    .Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            var clientsDto = new ResponseDto<ClientDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<ClientDto>>(clients)
            };

            return clientsDto;
        }

        public async Task<ClientDto> AddAsync(ClientDto dto)
        {
            var client = _mapper.Map<Client>(dto);
            client.CreatedBy = _userContextService.GetCurrentUserId();
            client.CreatedAt = DateTimeOffset.UtcNow;
            var savedClient = await _repository.AddAsync(client);
            return _mapper.Map<ClientDto>(savedClient);
        }

        public async Task UpdateAsync(ClientDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newClient = _mapper.Map<Client>(dto);
            newClient.Id = dto.Id.Value;            
            newClient.UpdatedBy = _userContextService.GetCurrentUserId();
            newClient.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(newClient);
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedClient = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedClient);
        }

        public async Task<List<ClientCreditDto>> GetCredits(Guid clientId)
        {
            var credits = await _repository.GetCredits(clientId);
            return _mapper.Map<List<ClientCreditDto>>(credits);
        }
    }
}
