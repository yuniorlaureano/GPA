using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoice;
using GPA.Common.Entities.Invoice;
using GPA.Data.Invoice;
using System.Linq.Expressions;

namespace GPA.Business.Services.Invoice
{
    public interface IReceivableAccountService
    {
        public Task<ClientPaymentsDetailDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<ClientPaymentsDetailDto>> GetAllAsync(SearchDto search, Expression<Func<ClientPaymentsDetails, bool>>? expression = null);

        public Task<ClientPaymentsDetailDto?> AddAsync(ClientPaymentsDetailCreationDto clientDto);

        public Task UpdateAsync(ClientPaymentsDetailCreationDto clientDto);

        public Task RemoveAsync(Guid id);
    }

    public class ReceivableAccountService : IReceivableAccountService
    {
        private readonly IReceivableAccountRepository _repository;
        private readonly IMapper _mapper;

        public ReceivableAccountService(IReceivableAccountRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ClientPaymentsDetailDto?> GetByIdAsync(Guid id)
        {
            var client = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<ClientPaymentsDetailDto>(client);
        }

        public async Task<ResponseDto<ClientPaymentsDetailDto>> GetAllAsync(SearchDto search, Expression<Func<ClientPaymentsDetails, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.OrderByDescending(x => x.Id).Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<ClientPaymentsDetailDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<ClientPaymentsDetailDto>>(categories)
            };
        }

        public async Task<ClientPaymentsDetailDto> AddAsync(ClientPaymentsDetailCreationDto dto)
        {
            var payment = _mapper.Map<ClientPaymentsDetails>(dto);
            var savedClient = await _repository.AddAsync(payment);
            return _mapper.Map<ClientPaymentsDetailDto>(savedClient);
        }

        public async Task UpdateAsync(ClientPaymentsDetailCreationDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newClient = _mapper.Map<ClientPaymentsDetails>(dto);
            newClient.Id = dto.Id.Value;
            var savedClient = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedClient, newClient, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedClient = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedClient);
        }
    }
}
