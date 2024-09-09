using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoice;
using GPA.Common.Entities.Invoice;
using GPA.Data.Invoice;
using GPA.Dtos.Audit;
using GPA.Entities.Unmapped.Invoice;
using GPA.Services.Security;
using Microsoft.Extensions.Logging;

namespace GPA.Business.Services.Invoice
{
    public interface IClientService
    {
        Task<ClientDto?> GetClientAsync(Guid id);
        Task<ResponseDto<ClientDto>> GetClientsAsync(RequestFilterDto search);
        Task<ClientDto?> AddAsync(ClientDto clientDto);
        Task UpdateAsync(ClientDto clientDto);
        Task RemoveAsync(Guid id);
        Task<List<ClientCreditDto>> GetCredits(Guid clientId);
    }

    public class ClientService : IClientService
    {
        private readonly IClientRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly IReceivableAccountRepository _receivableAccountRepository;
        private readonly ILogger<ClientService> _logger;

        public ClientService(
            IClientRepository repository,
            IUserContextService userContextService,
            IMapper mapper,
            IReceivableAccountRepository receivableAccountRepository,
            ILogger<ClientService> logger)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _receivableAccountRepository = receivableAccountRepository;
            _logger = logger;
        }

        public async Task<ClientDto?> GetClientAsync(Guid id)
        {
            var client = await _repository.GetClientAsync(id);

            var clientDto = _mapper.Map<ClientDto>(client);
            if (client is not null)
            {
                clientDto.Debits = await GetClientDebit(id, client);
                clientDto.Credits = await GetClientCredits(id);
            }

            return clientDto;
        }

        public async Task<ResponseDto<ClientDto>> GetClientsAsync(RequestFilterDto search)
        {
            var clients = await _repository.GetClientsAsync(search);
            var clientsDto = new ResponseDto<ClientDto>
            {
                Count = await _repository.GetClientsCountAsync(search),
                Data = _mapper.Map<List<ClientDto>>(clients)
            };
            await MapCreditsToClients(clients, clientsDto);

            return clientsDto;
        }

        public async Task<ClientDto> AddAsync(ClientDto dto)
        {
            var client = _mapper.Map<Client>(dto);
            client.CreatedBy = _userContextService.GetCurrentUserId();
            client.CreatedAt = DateTimeOffset.UtcNow;
            var savedClient = await _repository.AddAsync(client);

            var credits = dto.Credits?.Select(x => new ClientCredit { Concept = x.Concept, Credit = x.Credit }).ToList();
            await _repository.AddHistory(savedClient, credits, ActionConstants.Update, _userContextService.GetCurrentUserId());

            _logger.LogInformation("El usuario '{User}', ha agregado el cliente: '{ClientId}'", _userContextService.GetCurrentUserName(), savedClient.Name + " " + savedClient.LastName);
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

            _logger.LogInformation("El usuario '{User}', ha modificado el cliente: '{ClientId}'", _userContextService.GetCurrentUserName(), newClient.Name + " " + newClient.LastName);
            var credits = dto.Credits?.Select(x => new ClientCredit { Concept = x.Concept, Credit = x.Credit }).ToList();
            await _repository.AddHistory(newClient, credits, ActionConstants.Update, _userContextService.GetCurrentUserId());
        }

        public async Task RemoveAsync(Guid id)
        {
            var client = await _repository.GetClientAsync(id);
            var credit = await _repository.GetCreditsByClientIdAsync(new List<Guid> { id });

            await _repository.SoftDeleteClientAsync(id, _userContextService.GetCurrentUserId());

            _logger.LogInformation("El usuario '{User}', ha borrado el cliente: '{ClientId}'", _userContextService.GetCurrentUserName(), client.Name + " " + client.LastName);
            var credits = credit?.Select(x => new ClientCredit { Concept = x.Concept, Credit = x.Credit }).ToList();
            await _repository.AddHistory(_mapper.Map<Client>(client), credits, ActionConstants.Remove, _userContextService.GetCurrentUserId());
        }

        public async Task<List<ClientCreditDto>> GetCredits(Guid clientId)
        {
            var credits = await _repository.GetCreditsByClientIdAsync(new List<Guid> { clientId });
            return _mapper.Map<List<ClientCreditDto>>(credits);
        }

        private async Task MapCreditsToClients(IEnumerable<RawClient> clients, ResponseDto<ClientDto> clientsDto)
        {
            if (clientsDto?.Data?.Any() == true)
            {
                var credits = await _repository.GetCreditsByClientIdAsync(clients.Select(x => x.Id).ToList());
                var creditsDictionary = new Dictionary<Guid, List<ClientCreditDto>>();
                foreach (var credit in credits)
                {
                    if (!creditsDictionary.ContainsKey(credit.ClientId))
                    {
                        creditsDictionary.Add(credit.ClientId, new List<ClientCreditDto>());
                    }
                    creditsDictionary[credit.ClientId].Add(_mapper.Map<ClientCreditDto>(credit));
                }

                foreach (var client in clientsDto.Data)
                {
                    client.Credits = creditsDictionary.ContainsKey(client.Id.Value) ? creditsDictionary[client.Id.Value].ToArray() : [];
                }
            }
        }

        private async Task<ClientCreditDto[]> GetClientCredits(Guid clientId)
        {
            var credits = await _repository.GetCreditsByClientIdAsync(new List<Guid> { clientId });
            return _mapper.Map<ClientCreditDto[]>(credits) ?? [];
        }

        private async Task<ClientDebitDto[]> GetClientDebit(Guid id, RawClient? client)
        {
            if (client is not null)
            {
                var pendingPayments = await _receivableAccountRepository.GetPenddingPaymentByClientId(client.Id);
                return _mapper.Map<ClientDebitDto[]>(pendingPayments);
            }
            return [];
        }
    }
}
