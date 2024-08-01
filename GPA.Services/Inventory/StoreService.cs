using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IStoreService
    {
        public Task<StoreDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<StoreDto>> GetAllAsync(RequestFilterDto search, Expression<Func<Store, bool>>? expression = null);

        public Task<StoreDto?> AddAsync(StoreDto storeDto);

        public Task UpdateAsync(StoreDto storeDto);

        public Task RemoveAsync(Guid id);
    }

    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public StoreService(
            IStoreRepository repository, 
            IUserContextService userContextService, 
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<StoreDto?> GetByIdAsync(Guid id)
        {
            var store = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<StoreDto>(store);
        }

        public async Task<ResponseDto<StoreDto>> GetAllAsync(RequestFilterDto search, Expression<Func<Store, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.OrderByDescending(x => x.Id).Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<StoreDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<StoreDto>>(categories)
            };
        }

        public async Task<StoreDto> AddAsync(StoreDto dto)
        {
            var store = _mapper.Map<Store>(dto);
            store.CreatedBy = _userContextService.GetCurrentUserId();
            store.CreatedAt = DateTimeOffset.UtcNow;
            var savedStore = await _repository.AddAsync(store);
            return _mapper.Map<StoreDto>(savedStore);
        }

        public async Task UpdateAsync(StoreDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newStore = _mapper.Map<Store>(dto);
            newStore.Id = dto.Id.Value;
            var savedStore = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            newStore.UpdatedBy = _userContextService.GetCurrentUserId();
            newStore.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(savedStore, newStore, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedStore = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedStore);
        }
    }
}
