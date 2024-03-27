using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IProviderService
    {
        public Task<ProviderDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<ProviderDto>> GetAllAsync(SearchDto search, Expression<Func<Provider, bool>>? expression = null);

        public Task<ProviderDto?> AddAsync(ProviderDto providerDto);

        public Task UpdateAsync(ProviderDto providerDto);

        public Task RemoveAsync(Guid id);
    }

    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _repository;
        private readonly IMapper _mapper;

        public ProviderService(IProviderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProviderDto?> GetByIdAsync(Guid id)
        {
            var Provider = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<ProviderDto>(Provider);
        }

        public async Task<ResponseDto<ProviderDto>> GetAllAsync(SearchDto search, Expression<Func<Provider, bool>>? expression = null)
        {
            var providers = await _repository.GetAllAsync(query => 
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<ProviderDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<ProviderDto>>(providers)
            };
        }

        public async Task<ProviderDto> AddAsync(ProviderDto dto)
        {
            var newProvider = _mapper.Map<Provider>(dto);
            var savedProvider = await _repository.AddAsync(newProvider);
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
            var savedProvider = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedProvider, newProvider, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var newProvider = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(newProvider);
        }
    }
}
