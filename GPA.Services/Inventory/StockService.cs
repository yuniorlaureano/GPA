using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IStockService
    {
        public Task<StockDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<StockDto>> GetAllAsync(SearchDto search, Expression<Func<Stock, bool>>? expression = null);

        public Task<ResponseDto<RawProductCatalogDto>> GetProductCatalogAsync(int page = 1, int pageSize = 10);

        public Task<StockDto?> AddAsync(StockCreationDto dto);

        Task<IEnumerable<StockDto>> AddManyAsync(IEnumerable<StockCreationDto> dtos);

        public Task UpdateAsync(StockCreationDto dto);

        public Task RemoveAsync(Guid id);
    }

    public class StockService : IStockService
    {
        private readonly IStockRepository _repository;
        private readonly IMapper _mapper;

        public StockService(IStockRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<StockDto?> GetByIdAsync(Guid id)
        {
            var stock = await _repository.GetByIdAsync(query =>
            {
                return query.Include(x => x.Product).Include(x => x.Provider);
            }, x => x.Id == id);
            return _mapper.Map<StockDto>(stock);
        }

        public async Task<ResponseDto<StockDto>> GetAllAsync(SearchDto search, Expression<Func<Stock, bool>>? expression = null)
        {
            var stocks = await _repository.GetAllAsync(query =>
            {
                return query.Include(x => x.Product)
                     .Include(x => x.Provider)
                     .Skip(search.PageSize * Math.Abs(search.Page - 1))
                     .Take(search.PageSize);
            }, expression);
            return new ResponseDto<StockDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<StockDto>>(stocks)
            };
        }

        public async Task<ResponseDto<RawProductCatalogDto>> GetProductCatalogAsync(int page = 1, int pageSize = 10)
        {
            var productCatalog = await _repository.GetProductCatalogAsync(page, pageSize);
            return new ResponseDto<RawProductCatalogDto>
            {
                Count = await _repository.GetProductCatalogCountAsync(),
                Data = _mapper.Map<IEnumerable<RawProductCatalogDto>>(productCatalog)
            };
        }

        public async Task<IEnumerable<StockDto>> AddManyAsync(IEnumerable<StockCreationDto> dtos)
        {
            var newStocks = _mapper.Map<IEnumerable<Stock>>(dtos);
            var savedStocks = await _repository.AddManyAsync(newStocks);
            return _mapper.Map<IEnumerable<StockDto>>(savedStocks);
        }

        public async Task<StockDto> AddAsync(StockCreationDto dto)
        {
            var newStock = _mapper.Map<Stock>(dto);
            var savedStock = await _repository.AddAsync(newStock);
            return _mapper.Map<StockDto>(savedStock);
        }

        public async Task UpdateAsync(StockCreationDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newStock = _mapper.Map<Stock>(dto);
            newStock.Id = dto.Id.Value;
            var savedStock = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedStock, newStock, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var newStock = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(newStock);
        }
    }
}
