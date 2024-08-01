using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IProductLocationService
    {
        public Task<ProductLocationDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<ProductLocationDto>> GetAllAsync(RequestFilterDto search, Expression<Func<ProductLocation, bool>>? expression = null);

        public Task<ProductLocationDto?> AddAsync(ProductLocationDto ProductLocationDto);

        public Task UpdateAsync(ProductLocationDto ProductLocationDto);

        public Task RemoveAsync(Guid id);
    }

    public class ProductLocationService : IProductLocationService
    {
        private readonly IProductLocationRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public ProductLocationService(
            IProductLocationRepository repository,
            IUserContextService userContextService, 
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<ProductLocationDto?> GetByIdAsync(Guid id)
        {
            var productLocation = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<ProductLocationDto>(productLocation);
        }

        public async Task<ResponseDto<ProductLocationDto>> GetAllAsync(RequestFilterDto search, Expression<Func<ProductLocation, bool>>? expression = null)
        {
            var productLocations = await _repository.GetAllAsync(query =>
            {
                return query.OrderByDescending(x => x.Id).Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<ProductLocationDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<ProductLocationDto>>(productLocations)
            };
        }

        public async Task<ProductLocationDto> AddAsync(ProductLocationDto dto)
        {
            var newProductLocation = _mapper.Map<ProductLocation>(dto);
            newProductLocation.CreatedBy = _userContextService.GetCurrentUserId();
            newProductLocation.CreatedAt = DateTimeOffset.UtcNow;
            var savedProductLocation = await _repository.AddAsync(newProductLocation);
            return _mapper.Map<ProductLocationDto>(savedProductLocation);
        }

        public async Task UpdateAsync(ProductLocationDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newProductLocation = _mapper.Map<ProductLocation>(dto);
            newProductLocation.Id = dto.Id.Value;
            newProductLocation.UpdatedBy = _userContextService.GetCurrentUserId();
            newProductLocation.UpdatedAt = DateTimeOffset.UtcNow;
            var savedProductLocation = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedProductLocation, newProductLocation, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedProductLocation = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedProductLocation);
        }
    }
}
