using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IProviderAddressService
    {
        public Task<ProviderAddressDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<ProviderAddressDto>> GetAllAsync(SearchDto search, Expression<Func<ProviderAddress, bool>>? expression = null);

        public Task<ProviderAddressDto?> AddAsync(ProviderAddressDto providerDto);

        public Task UpdateAsync(ProviderAddressDto providerDto);

        public Task RemoveAsync(Guid id);
    }

    public class ProviderAddressService : IProviderAddressService
    {
        private readonly IProviderAddressRepository _repository;
        private readonly IMapper _mapper;

        public ProviderAddressService(IProviderAddressRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProviderAddressDto?> GetByIdAsync(Guid id)
        {
            var providerAddress = await _repository.GetByIdAsync(query =>
            {
                return query;
            }, x => x.Id == id);
            return _mapper.Map<ProviderAddressDto>(providerAddress);
        }

        public async Task<ResponseDto<ProviderAddressDto>> GetAllAsync(SearchDto search, Expression<Func<ProviderAddress, bool>>? expression = null)
        {
            var providerAddresss = await _repository.GetAllAsync(query => 
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1))
                     .Take(search.PageSize);
            }, expression);
            return new ResponseDto<ProviderAddressDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<ProviderAddressDto>>(providerAddresss)
            };
        }

        public async Task<ProviderAddressDto> AddAsync(ProviderAddressDto dto)
        {
            var newProviderAddress = _mapper.Map<ProviderAddress>(dto);
            var savedProviderAddress = await _repository.AddAsync(newProviderAddress);
            return _mapper.Map<ProviderAddressDto>(savedProviderAddress);
        }

        public async Task UpdateAsync(ProviderAddressDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newProviderAddress = _mapper.Map<ProviderAddress>(dto);
            newProviderAddress.Id = dto.Id.Value;
            var savedProviderAddress = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedProviderAddress, newProviderAddress, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var newProviderAddress = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(newProviderAddress);
        }
    }
}
