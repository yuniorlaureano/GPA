using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Entities.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IStockService
    {
        public Task<StockWithDetailDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<StockDto>> GetAllAsync(SearchDto search, Expression<Func<Stock, bool>>? expression = null);

        public Task<ResponseDto<ProductCatalogDto>> GetProductCatalogAsync(int page = 1, int pageSize = 10);

        Task<ResponseDto<ExistanceDto>> GetExistanceAsync(int page = 1, int pageSize = 10);

        public Task<StockDto?> AddAsync(StockCreationDto dto);

        public Task UpdateInputAsync(StockCreationDto dto);
        public Task UpdateOutputAsync(StockCreationDto dto);

        public Task RemoveAsync(Guid id);

        Task CancelAsync(Guid id);
    }

    public class StockService : IStockService
    {
        private readonly IAddonRepository _addonRepository;
        private readonly IStockRepository _repository;
        private readonly IMapper _mapper;

        public StockService(IAddonRepository addonRepository, IStockRepository repository, IMapper mapper)
        {
            _addonRepository = addonRepository;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<StockWithDetailDto?> GetByIdAsync(Guid id)
        {
            var stock = await _repository.GetByIdAsync(query =>
            {
                return query
                    .Include(x => x.Provider)
                    .Include(X => X.Reason)
                    .Include(x => x.StockDetails)
                        .ThenInclude(x => x.Product);
            }, x => x.Id == id);

            return _mapper.Map<StockWithDetailDto>(stock);
        }

        public async Task<ResponseDto<StockDto>> GetAllAsync(SearchDto search, Expression<Func<Stock, bool>>? expression = null)
        {
            var stocks = await _repository.GetAllAsync(query =>
            {
                return query
                     .Include(x => x.Provider)
                     .Include(x => x.Reason)
                     .OrderByDescending(x => x.Id)
                     .Skip(search.PageSize * Math.Abs(search.Page - 1))
                     .Take(search.PageSize);
            }, expression);
            return new ResponseDto<StockDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<StockDto>>(stocks)
            };
        }

        public async Task<ResponseDto<ProductCatalogDto>> GetProductCatalogAsync(int page = 1, int pageSize = 10)
        {
            var productCatalog = await _repository.GetProductCatalogAsync(page, pageSize);
            var productCatalogDto =  new ResponseDto<ProductCatalogDto>
            {
                Count = await _repository.GetProductCatalogCountAsync(),
                Data = _mapper.Map<IEnumerable<ProductCatalogDto>>(productCatalog)
            };

            await MapAddonsToProduct(productCatalogDto.Data);

            return productCatalogDto;
        }

        public async Task<ResponseDto<ExistanceDto>> GetExistanceAsync(int page = 1, int pageSize = 10)
        {
            var productCatalog = await _repository.GetExistenceAsync(page, pageSize);
            return new ResponseDto<ExistanceDto>
            {
                Count = await _repository.GetExistenceCountAsync(),
                Data = _mapper.Map<IEnumerable<ExistanceDto>>(productCatalog)
            };
        }

        public async Task<StockDto?> AddAsync(StockCreationDto dto)
        {
            var newStock = _mapper.Map<Stock>(dto);
            newStock.StockDetails = _mapper.Map<ICollection<StockDetails>>(dto.StockDetails);
            var savedStock = await _repository.AddAsync(newStock);
            return _mapper.Map<StockDto>(savedStock);
        }

        public async Task UpdateInputAsync(StockCreationDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var savedStock = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);

            var canEditStock =
                    savedStock is not null &&
                    savedStock.TransactionType == TransactionType.Input &&
                    savedStock.Status == StockStatus.Draft &&
                    savedStock.ReasonId != (int)ReasonTypes.Sale &&
                    savedStock.ReasonId != (int)ReasonTypes.Return;

            if (canEditStock)
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

        public async Task UpdateOutputAsync(StockCreationDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var savedStock = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);

            var canEditStock =
                    savedStock is not null &&
                    savedStock.TransactionType == TransactionType.Output &&
                    savedStock.Status == StockStatus.Draft &&
                    (
                        savedStock.ReasonId == (int)ReasonTypes.DamagedProduct ||
                        savedStock.ReasonId != (int)ReasonTypes.ExpiredProduct ||
                        savedStock.ReasonId != (int)ReasonTypes.RawMaterial
                    );

            if (canEditStock)
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

        public async Task CancelAsync(Guid id)
        {
            await _repository.CancelAsync(id);
        }

        private async Task MapAddonsToProduct(IEnumerable<ProductCatalogDto> products)
        {
            if (products is not null)
            {
                var addons = await _addonRepository.GetAddonsByProductId(products.Select(x => x.ProductId).ToList());
                Dictionary<Guid, List<AddonDto>> mappedAddons = new();
                foreach (var addon in addons)
                {
                    if (!mappedAddons.ContainsKey(addon.ProductId))
                    {
                        mappedAddons.Add(addon.ProductId, new List<AddonDto>());
                    }

                    mappedAddons[addon.ProductId].Add(new AddonDto
                    {
                        Id = addon.Id,
                        Concept = addon.Concept,
                        IsDiscount = addon.IsDiscount,
                        Type = addon.Type,
                        Value = addon.Value
                    });
                }

                foreach (var product in products)
                {
                    if (mappedAddons.ContainsKey(product.ProductId))
                    {
                        product.Addons = mappedAddons[product.ProductId].ToArray();
                    }
                }
            }
        }
    }
}
