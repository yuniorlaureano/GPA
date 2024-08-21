using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Dtos.Inventory;
using GPA.Entities.General;
using GPA.Entities.Inventory;
using GPA.Entities.Unmapped.Inventory;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
using GPA.Utils;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Text.Json;

namespace GPA.Business.Services.Inventory
{
    public interface IStockService
    {
        public Task<StockWithDetailDto?> GetByIdAsync(Guid id);
        public Task<ResponseDto<StockDto>> GetStocksAsync(RequestFilterDto search, Expression<Func<Stock, bool>>? expression = null);
        public Task<ResponseDto<ProductCatalogDto>> GetProductCatalogAsync(int page = 1, int pageSize = 10);
        Task<ResponseDto<ExistanceDto>> GetExistenceAsync(RequestFilterDto filter);
        public Task<StockDto?> AddAsync(StockCreationDto dto);
        public Task UpdateInputAsync(StockCreationDto dto);
        public Task UpdateOutputAsync(StockCreationDto dto);
        public Task RemoveAsync(Guid id);
        Task CancelAsync(Guid id);
        Task SaveAttachment(Guid stockId, IFormFile file);
        Task<IEnumerable<StockAttachmentDto>> GetAttachmentByStockIdAsync(Guid stockId);
    }

    public class StockService : IStockService
    {
        private readonly IAddonRepository _addonRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserContextService _userContextService;
        private readonly IStockRepository _repository;
        private readonly IStockAttachmentRepository _stockAttachmentRepository;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly IMapper _mapper;

        public StockService(
            IProductRepository productRepository,
            IAddonRepository addonRepository,
            IUserContextService userContextService,
            IStockRepository repository,
            IStockAttachmentRepository stockAttachmentRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IMapper mapper)
        {
            _addonRepository = addonRepository;
            _productRepository = productRepository;
            _userContextService = userContextService;
            _repository = repository;
            _stockAttachmentRepository = stockAttachmentRepository;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _mapper = mapper;
        }

        public async Task<StockWithDetailDto?> GetByIdAsync(Guid id)
        {
            var stock = await _repository.GetStockByIdAsync(id);
            if (stock == null)
            {
                return null;
            }

            var stockDto = _mapper.Map<StockWithDetailDto>(stock);
            var stockDetails = await _repository.GetStockDetailsByStockIdAsync(id);
            stockDto.StockDetails = _mapper.Map<ICollection<StockDetailsDto>>(stockDetails);
            await MapProductsToStockDetails(stockDto, stockDetails);

            return stockDto;
        }

        public async Task<ResponseDto<StockDto>> GetStocksAsync(RequestFilterDto search, Expression<Func<Stock, bool>>? expression = null)
        {
            var stocks = await _repository.GetStocksAsync(search);
            return new ResponseDto<StockDto>
            {
                Count = await _repository.GetStocksCountAsync(search),
                Data = _mapper.Map<IEnumerable<StockDto>>(stocks)
            };
        }

        public async Task<ResponseDto<ProductCatalogDto>> GetProductCatalogAsync(int page = 1, int pageSize = 10)
        {
            var productCatalog = await _repository.GetProductCatalogAsync(page, pageSize);
            var productCatalogDto = new ResponseDto<ProductCatalogDto>
            {
                Count = await _repository.GetProductCatalogCountAsync(),
                Data = _mapper.Map<IEnumerable<ProductCatalogDto>>(productCatalog)
            };

            await MapAddonsToProduct(productCatalogDto.Data);

            return productCatalogDto;
        }

        public async Task<ResponseDto<ExistanceDto>> GetExistenceAsync(RequestFilterDto filter)
        {
            var productCatalog = await _repository.GetExistenceAsync(filter);
            var productCatalogDto = new ResponseDto<ExistanceDto>
            {
                Count = await _repository.GetExistenceCountAsync(filter),
                Data = _mapper.Map<IEnumerable<ExistanceDto>>(productCatalog)
            };

            await MapAddonsToProduct(productCatalogDto.Data);
            return productCatalogDto;
        }

        public async Task<StockDto?> AddAsync(StockCreationDto dto)
        {
            var newStock = _mapper.Map<Stock>(dto);
            newStock.StockDetails = _mapper.Map<ICollection<StockDetails>>(dto.StockDetails);
            newStock.CreatedBy = _userContextService.GetCurrentUserId();
            newStock.CreatedAt = DateTimeOffset.UtcNow;
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
                newStock.UpdatedBy = _userContextService.GetCurrentUserId();
                newStock.UpdatedAt = DateTimeOffset.UtcNow;
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
                newStock.UpdatedBy = _userContextService.GetCurrentUserId();
                newStock.UpdatedAt = DateTimeOffset.UtcNow;
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
            await _repository.CancelAsync(id, _userContextService.GetCurrentUserId());
        }

        public async Task SaveAttachment(Guid stockId, IFormFile file)
        {
            var fileResult = await _blobStorageServiceFactory.UploadFile(file, "stock/", isPublic: false);
            var jsonFileResult = JsonSerializer.Serialize(fileResult, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var attachment = new StockAttachment
            {
                StockId = stockId,
                File = jsonFileResult,
                UploadedAt = DateTimeOffset.UtcNow,
                UploadedBy = _userContextService.GetCurrentUserId()
            };
            await _stockAttachmentRepository.SaveAttachmentAsync(attachment);
        }

        public async Task<IEnumerable<StockAttachmentDto>> GetAttachmentByStockIdAsync(Guid stockId)
        {
            var attachments = await _stockAttachmentRepository.GetAttachmentByStockIdAsync(stockId);
            return _mapper.Map<IEnumerable<StockAttachmentDto>>(attachments);
        }

        private async Task MapAddonsToProduct(IEnumerable<ProductCatalogDto> products)
        {
            if (products is not null && products.Any())
            {
                var mappedAddons = await _addonRepository
                    .GetAddonsByProductIdAsDictionary(products.Select(x => x.ProductId).ToList());

                foreach (var product in products)
                {
                    if (mappedAddons.ContainsKey(product.ProductId))
                    {
                        product.Addons = _mapper.Map<AddonDto[]>(mappedAddons[product.ProductId]);
                        var (debit, credit) = AddonCalculator.CalculateAddon(product.Price, product.Addons);
                        product.Debit = debit;
                        product.Credit = credit;
                    }
                }
            }
        }

        private async Task MapAddonsToProduct(IEnumerable<ExistanceDto> products)
        {
            if (products is not null)
            {
                var mappedAddons = await _addonRepository
                    .GetAddonsByProductIdAsDictionary(products.Select(x => x.ProductId).ToList());

                foreach (var product in products)
                {
                    if (mappedAddons.ContainsKey(product.ProductId))
                    {
                        product.Addons = _mapper.Map<AddonDto[]>(mappedAddons[product.ProductId]);
                        var (debit, credit) = AddonCalculator.CalculateAddon(product.Price, product.Addons);
                        product.Debit = debit;
                        product.Credit = credit;
                    }
                }
            }
        }

        private async Task MapProductsToStockDetails(StockWithDetailDto stockDto, IEnumerable<RawStockDetails> stockDetails)
        {
            var productIds = stockDetails.Select(x => x.ProductId).ToList();
            if (productIds is { Count: > 0 })
            {
                var products = await _productRepository.GetProductsAsync(productIds);
                var productDict = products.ToDictionary(x => x.Id, x => x);

                foreach (var detail in stockDto.StockDetails)
                {
                    if (productDict.ContainsKey(detail.ProductId))
                    {
                        detail.Product = _mapper.Map<ProductDto>(productDict[detail.ProductId]);
                    }
                }
            }
        }
    }
}
