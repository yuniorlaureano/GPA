using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IItemService
    {
        public Task<ItemDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<ItemDto>> GetAllAsync(SearchDto search, Expression<Func<Item, bool>>? expression = null);

        public Task<ItemDto?> AddAsync(ItemDto itemDto);

        public Task UpdateAsync(ItemDto itemDto);

        public Task RemoveAsync(Guid id);
    }

    public class ItemService : IItemService
    {
        private readonly IItemRepository _repository;
        private readonly IMapper _mapper;

        public ItemService(IItemRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ItemDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<ItemDto>(item);
        }

        public async Task<ResponseDto<ItemDto>> GetAllAsync(SearchDto search, Expression<Func<Item, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<ItemDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<ItemDto>>(categories)
            };
        }

        public async Task<ItemDto> AddAsync(ItemDto dto)
        {
            var item = _mapper.Map<Item>(dto);
            var savedItem = await _repository.AddAsync(item);
            return _mapper.Map<ItemDto>(savedItem);
        }

        public async Task UpdateAsync(ItemDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newItem = _mapper.Map<Item>(dto);
            newItem.Id = dto.Id.Value;
            var savedItem = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedItem, newItem, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedItem = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedItem);
        }
    }
}
