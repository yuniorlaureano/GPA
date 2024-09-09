using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Dtos.Audit;
using GPA.Dtos.Inventory;
using GPA.Entities.Unmapped.Inventory;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
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

        public ProductService(
            IProductRepository repository,
            IUserContextService userContextService,
            IMapper mapper,
            IAddonRepository addonRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _addonRepository = addonRepository;
            _blobStorageServiceFactory = blobStorageServiceFactory;
        }

        public async Task<ProductDto?> GetProductAsync(Guid id)
        {
            var product = await _repository.GetProductAsync(id);
            var productDto = _mapper.Map<ProductDto>(product);
            productDto.Addons = await GetAddons(product);
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
            newProduct.CreatedBy = _userContextService.GetCurrentUserId();
            newProduct.CreatedAt = DateTimeOffset.UtcNow;
            var savedProduct = await _repository.AddAsync(newProduct);

            var rawProduct = await _repository.GetProductAsync(savedProduct.Id);
            await _repository.AddHistory(rawProduct, ActionConstants.Add, _userContextService.GetCurrentUserId());

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
            newProduct.UpdatedBy = _userContextService.GetCurrentUserId();
            newProduct.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.UpdateAsync(savedProduct, newProduct, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
                entityState.Property(x => x.Photo).IsModified = false;
            });

            var rawProduct = await _repository.GetProductAsync(savedProduct.Id);
            await _repository.AddHistory(rawProduct, ActionConstants.Update, _userContextService.GetCurrentUserId());
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
        }

        public async Task RemoveAsync(Guid id)
        {
            var rawProduct = await _repository.GetProductAsync(id);
            await _repository.AddHistory(rawProduct, ActionConstants.Remove, _userContextService.GetCurrentUserId());
            await _repository.SoftDelete(id);
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
    }
}
