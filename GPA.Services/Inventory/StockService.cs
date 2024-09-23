using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Dtos.Audit;
using GPA.Dtos.General;
using GPA.Dtos.Inventory;
using GPA.Entities.General;
using GPA.Entities.Inventory;
using GPA.Entities.Unmapped.Inventory;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
using GPA.Utils;
using GPA.Utils.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace GPA.Business.Services.Inventory
{
    public interface IStockService
    {
        Task<StockWithDetailDto?> GetByIdAsync(Guid id);
        Task<ResponseDto<StockDto>> GetStocksAsync(RequestFilterDto search);
        Task<ResponseDto<ProductCatalogDto>> GetProductCatalogAsync(RequestFilterDto search);
        Task<ResponseDto<ExistanceDto>> GetExistenceAsync(RequestFilterDto filter);
        Task<StockDto?> AddAsync(StockCreationDto dto);
        Task UpdateInputAsync(StockCreationDto dto);
        Task UpdateOutputAsync(StockCreationDto dto);
        Task CancelAsync(Guid id);
        Task SaveAttachment(Guid stockId, IFormFile file);
        Task<IEnumerable<StockAttachmentDto>> GetAttachmentByStockIdAsync(Guid stockId);
        Task<(Stream? file, string fileName)> DownloadAttachmentAsync(Guid id);
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
        private readonly ILogger<StockService> _logger;

        public StockService(
            IProductRepository productRepository,
            IAddonRepository addonRepository,
            IUserContextService userContextService,
            IStockRepository repository,
            IStockAttachmentRepository stockAttachmentRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IMapper mapper,
            ILogger<StockService> logger)
        {
            _addonRepository = addonRepository;
            _productRepository = productRepository;
            _userContextService = userContextService;
            _repository = repository;
            _stockAttachmentRepository = stockAttachmentRepository;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _mapper = mapper;
            _logger = logger;
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

        public async Task<ResponseDto<StockDto>> GetStocksAsync(RequestFilterDto search)
        {
            var stocks = await _repository.GetStocksAsync(search);
            return new ResponseDto<StockDto>
            {
                Count = await _repository.GetStocksCountAsync(search),
                Data = _mapper.Map<IEnumerable<StockDto>>(stocks)
            };
        }

        public async Task<ResponseDto<ProductCatalogDto>> GetProductCatalogAsync(RequestFilterDto filter)
        {
            filter.Search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            var productCatalog = await _repository.GetProductCatalogAsync(filter);
            var productCatalogDto = new ResponseDto<ProductCatalogDto>
            {
                Count = await _repository.GetProductCatalogCountAsync(filter),
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
            newStock.Date = DateTime.UtcNow;
            var savedStock = await _repository.AddAsync(newStock);
            var action = dto.TransactionType == (int)TransactionType.Input ? "Entrada" : "Salida";
            _logger.LogInformation("El usuario '{User}' ha realidado una '{Transaction}' de inventario de inventario '{Stock}'", _userContextService.GetCurrentUserId(), action, savedStock.Id);
            await _repository.AddHistory(newStock, newStock.StockDetails, ActionConstants.Add, _userContextService.GetCurrentUserId());
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
                newStock.StockDetails = Enumerable.Empty<StockDetails>().ToList();

                foreach (var detail in stockDetails)
                {
                    detail.StockId = newStock.Id;
                }
                newStock.UpdatedBy = _userContextService.GetCurrentUserId();
                newStock.UpdatedAt = DateTimeOffset.UtcNow;
                newStock.Date = savedStock.Date;
                await _repository.UpdateAsync(newStock, stockDetails);
                await _repository.AddHistory(newStock, newStock.StockDetails, ActionConstants.Update, _userContextService.GetCurrentUserId());
                var action = savedStock.TransactionType == TransactionType.Input ? "Entrada" : "Salida";
                _logger.LogInformation("El usuario '{User}' ha actualizado una '{Transaction}' de inventario de inventario '{Stock}'", _userContextService.GetCurrentUserId(), action, savedStock.Id);
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
                newStock.StockDetails = Enumerable.Empty<StockDetails>().ToList();

                foreach (var detail in stockDetails)
                {
                    detail.StockId = newStock.Id;
                }
                newStock.UpdatedBy = _userContextService.GetCurrentUserId();
                newStock.UpdatedAt = DateTimeOffset.UtcNow;
                newStock.Date = savedStock.Date;
                await _repository.UpdateAsync(newStock, stockDetails);
                await _repository.AddHistory(newStock, newStock.StockDetails, ActionConstants.Update, _userContextService.GetCurrentUserId());
                var action = savedStock.TransactionType == TransactionType.Input ? "Entrada" : "Salida";
                _logger.LogInformation("El usuario '{User}' ha actualizado una '{Transaction}' de inventario de inventario '{Stock}'", _userContextService.GetCurrentUserId(), action, savedStock.Id);
            }
        }

        public async Task CancelAsync(Guid id)
        {
            var stock = await _repository.GetByIdAsync(entity => entity.Include(x => x.StockDetails));
            await _repository.CancelAsync(id, _userContextService.GetCurrentUserId());
            var action = stock.TransactionType == TransactionType.Input ? "Entrada" : "Salida";
            _logger.LogInformation("El usuario '{User}' ha cancelado una '{Transaction}' de inventario de inventario '{Stock}'", _userContextService.GetCurrentUserId(), action, stock.Id);
            if (stock is not null)
            {
                await _repository.AddHistory(stock, stock.StockDetails, ActionConstants.Canceled, _userContextService.GetCurrentUserId());
            }
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

        public async Task<(Stream? file, string fileName)> DownloadAttachmentAsync(Guid id)
        {
            var attachment = await _stockAttachmentRepository.GetAttachmentByIdAsync(id);
            if (attachment is null)
            {
                throw new AttachmentNotFoundException("Attachment not found");
            }

            BlobStorageFileResult? fileResult = null;

            try
            {
                fileResult = JsonSerializer.Deserialize<BlobStorageFileResult>(attachment.File, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                    ?? throw new AttachmentDeserializingException("Error deserializing exception");
            }
            catch (Exception e)
            {
                throw new AttachmentDeserializingException("Error deserializing exception");
            }

            var file = await _blobStorageServiceFactory.DownloadFile(fileResult.UniqueFileName);
            return (file, fileResult.UniqueFileName);
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
