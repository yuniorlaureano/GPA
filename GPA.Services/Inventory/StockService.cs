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
        public Task<StockWithDetailDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<StockDto>> GetAllAsync(SearchDto search, Expression<Func<Stock, bool>>? expression = null);

        public Task<ResponseDto<RawProductCatalogDto>> GetProductCatalogAsync(int page = 1, int pageSize = 10);

        public Task<StockDto?> AddAsync(StockCreationDto dto);

        public Task UpdateAsync(StockCreationDto dto);

        public Task RemoveAsync(Guid id);
    }

    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IStockRepository _repository;
        private readonly IMapper _mapper;

        public StockService(IStockRepository repository, IStockRepository stockRepository, IMapper mapper)
        {
            _repository = repository;
            _stockRepository = stockRepository;
            _mapper = mapper;
        }

        public async Task<StockWithDetailDto?> GetByIdAsync(Guid id)
        {
            var savedStock = await _repository.GetByIdAsync(query =>
            {
                return query
                    .Include(x => x.Provider)
                    .Include(X => X.Reason)
                    .Include(x => x.StockDetails)
                        .ThenInclude(x => x.Product);
            }, x => x.Id == id);

            var stock = _mapper.Map<StockWithDetailDto>(savedStock);

            if (stock is not null)
            {
                var productsId = stock.StockDetails.Select(x => x.ProductId).ToList();
                if (productsId is not null)
                {
                    var stocks = (await _stockRepository.GetProductCatalogAsync(productsId.ToArray()))
                            .ToDictionary(k => k.ProductId, v => v);

                    foreach (var stockDetail in stock.StockDetails)
                    {
                        if (stocks.TryGetValue(stockDetail.ProductId, out var product))
                        {
                            stockDetail.StockProduct = _mapper.Map<RawProductCatalogDto>(product);
                        }
                    }
                }
            }

            return _mapper.Map<StockWithDetailDto>(stock);
        }

        public async Task<ResponseDto<StockDto>> GetAllAsync(SearchDto search, Expression<Func<Stock, bool>>? expression = null)
        {
            var stocks = await _repository.GetAllAsync(query =>
            {
                return query
                     .Include(x => x.Provider)
                     .Include(x => x.Reason)
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

            var savedStock = _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);

            if (savedStock is not null)
            {
                var newStock = _mapper.Map<Stock>(dto);
                var stockDetails = _mapper.Map<List<StockDetails>>(dto.StockDetails);

                foreach (var detail in stockDetails)
                {
                    detail.StockId = newStock.Id;
                }

                await _repository.UpdateAsync(newStock, stockDetails);
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            var newStock = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(newStock);
        }
    }
}
