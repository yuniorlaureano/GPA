using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Dtos.Audit;
using GPA.Dtos.Inventory;
using GPA.Dtos.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
using GPA.Utils.CodeGenerators;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GPA.Business.Services.Inventory
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductAsync(Guid id);
        Task<ResponseDto<ProductDto>> GetProductsAsync(RequestFilterDto filter);
        Task<ProductDto?> AddAsync(ProductCreationDto ProductDto);
        Task UpdateAsync(ProductCreationDto ProductDto);
        Task RemoveAsync(Guid id);
        Task SavePhoto(ProductUploadPhotoDto dto);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly IAddonRepository _addonRepository;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly ProductCodeGenerator _productCodeGenerator;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository repository,
            IUserContextService userContextService,
            IMapper mapper,
            IAddonRepository addonRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            ProductCodeGenerator productCodeGenerator,
            ILogger<ProductService> logger)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _addonRepository = addonRepository;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _productCodeGenerator = productCodeGenerator;
            _logger = logger;
        }

        public async Task<ProductDto?> GetProductAsync(Guid id)
        {
            var product = await _repository.GetProductAsync(id);
            var productDto = _mapper.Map<ProductDto>(product);
            productDto.Addons = await GetAddons(product);
            productDto.RelatedProducts = await GetRelatedProducts(product);
            return productDto;
        }

        public async Task<ResponseDto<ProductDto>> GetProductsAsync(RequestFilterDto filter)
        {
            filter.Search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            return new ResponseDto<ProductDto>
            {
                Count = await _repository.GetProductsCountAsync(filter),
                Data = _mapper.Map<IEnumerable<ProductDto>>(await _repository.GetProductsAsync(filter))
            };
        }

        public async Task<ProductDto> AddAsync(ProductCreationDto dto)
        {
            var newProduct = _mapper.Map<Product>(dto);

            MapAddons(newProduct, dto.Addons);
            MapRelatedProducts(dto, newProduct);
            newProduct.CreatedBy = _userContextService.GetCurrentUserId();
            newProduct.CreatedAt = DateTimeOffset.UtcNow;
            newProduct.Code = _productCodeGenerator.GenerateCode();

            var savedProduct = await _repository.AddAsync(newProduct);

            var rawProduct = await _repository.GetProductAsync(savedProduct.Id);
            await _repository.AddHistory(rawProduct, ActionConstants.Add, _userContextService.GetCurrentUserId());
            _logger.LogInformation("El usuario '{UserId}' ha agregado el producto '{ProductId}'", _userContextService.GetCurrentUserId(), rawProduct.Id);
            return _mapper.Map<ProductDto>(savedProduct);
        }

        public async Task UpdateAsync(ProductCreationDto dto)
        {
            var savedProduct = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            if (dto.Id is null || savedProduct is null)
            {
                throw new ArgumentNullException();
            }

            var newProduct = _mapper.Map<Product>(dto);
            newProduct.Id = dto.Id.Value;

            await MapAddons(newProduct, dto.Addons, dto.Id.Value);
            await MapRelatedProducts(newProduct, dto.RelatedProducts, dto.Id.Value);

            newProduct.UpdatedBy = _userContextService.GetCurrentUserId();
            newProduct.UpdatedAt = DateTimeOffset.UtcNow;
            newProduct.Code = savedProduct.Code;
            await _repository.UpdateAsync(savedProduct, newProduct, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
                entityState.Property(x => x.Photo).IsModified = false;
            });

            var rawProduct = await _repository.GetProductAsync(savedProduct.Id);
            await _repository.AddHistory(rawProduct, ActionConstants.Update, _userContextService.GetCurrentUserId());
            _logger.LogInformation("El usuario '{UserId}' ha modificado el producto '{ProductId}'", _userContextService.GetCurrentUserId(), rawProduct.Id);
        }

        public async Task SavePhoto(ProductUploadPhotoDto dto)
        {
            var savedProduct = await _repository.GetProductAsync(dto.ProductId);
            if (savedProduct is null)
            {
                throw new InvalidOperationException("El producto no existe");
            }

            var uploadResult = await _blobStorageServiceFactory.UploadFile(dto.Photo, folder: "products/", isPublic: true);
            await _repository.SavePhoto(uploadResult.AsJson(), savedProduct.Id);
            _logger.LogInformation("El usuario '{UserId}' ha cambiado la foto del producto '{ProductId}'", _userContextService.GetCurrentUserId(), dto.ProductId);
        }

        public async Task RemoveAsync(Guid id)
        {
            var rawProduct = await _repository.GetProductAsync(id);
            await _repository.AddHistory(rawProduct, ActionConstants.Remove, _userContextService.GetCurrentUserId());
            await _repository.SoftDelete(id);
            _logger.LogInformation("El usuario '{UserId}' ha eliminado el producto '{ProductId}'", _userContextService.GetCurrentUserId(), id);
        }

        private async Task<AddonDto[]?> GetAddons(RawProduct? product)
        {
            if (product == null)
            {
                return null;
            }
            var addons = await _addonRepository.GetAddonsByProductId(product.Id);
            return _mapper.Map<AddonDto[]>(addons);
        }

        private async Task<IEnumerable<RawRelatedProductRead>> GetRelatedProducts(RawProduct? product)
        {
            if (product == null)
            {
                return null;
            }
            var relatedProducts = await _repository.GetRelatedProductsByProductIdAsync(product.Id);
            return relatedProducts;
        }

        private void MapAddons(Product product, Guid[]? addons)
        {
            if (addons is { Length: > 0 })
            {
                product.ProductAddons = addons.Select(addon => new ProductAddon { AddonId = addon }).ToList();
            }
        }

        private async Task MapAddons(Product product, Guid[]? addons, Guid productId)
        {
            await _addonRepository.DeleteAddonsByProductId(productId);

            if (addons is { Length: > 0 })
            {
                product.ProductAddons = addons.Select(
                    addon => new ProductAddon { AddonId = addon }).ToList();
            }
        }

        private async Task MapRelatedProducts(Product product, ProductCreationRelatedProductDto[]? productCreationRelatedProductDtos, Guid productId)
        {
            await _repository.DeleteRelatedProductsByProductIdAsync(productId);

            if (productCreationRelatedProductDtos is { Length: > 0 })
            {
                product.RelatedProducts = productCreationRelatedProductDtos.Select(x => new Entities.Inventory.RelatedProduct
                {
                    RelatedProductId = x.ProductId,
                    Quantity = x.Quantity,
                }).ToList();
            }
        }

        private static void MapRelatedProducts(ProductCreationDto dto, Product newProduct)
        {
            if (dto.RelatedProducts is { Length: > 0 })
            {
                newProduct.RelatedProducts = dto.RelatedProducts.Select(x => new Entities.Inventory.RelatedProduct
                {
                    RelatedProductId = x.ProductId,
                    Quantity = x.Quantity,
                }).ToList();
            }
        }
    }
}
