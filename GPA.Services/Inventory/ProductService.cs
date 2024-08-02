using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
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
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly IAddonRepository _addonRepository;

        public ProductService(
            IProductRepository repository,
            IUserContextService userContextService,
            IMapper mapper,
            IAddonRepository addonRepository)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _addonRepository = addonRepository;
        }

        public async Task<ProductDto?> GetProductAsync(Guid id)
        {
            var product = await _repository.GetProductAsync(id);
            var productDto = _mapper.Map<ProductDto>(await _repository.GetProductAsync(id));

            if (product is not null)
            {
                var addons = await _addonRepository.GetAddonsByProductId(product.Id);
                productDto.Addons = _mapper.Map<AddonDto[]>(addons);
            }

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
            if (dto.Addons is { Length: > 0 })
            {
                newProduct.ProductAddons = dto.Addons.Select(addon => new ProductAddon { AddonId = addon }).ToList();
            }
            newProduct.CreatedBy = _userContextService.GetCurrentUserId();
            newProduct.CreatedAt = DateTimeOffset.UtcNow;
            var savedProduct = await _repository.AddAsync(newProduct);
            return _mapper.Map<ProductDto>(savedProduct);
        }

        public async Task UpdateAsync(ProductCreationDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newProduct = _mapper.Map<Product>(dto);
            newProduct.Id = dto.Id.Value;
            var savedProduct = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);

            await _addonRepository.DeleteAddonsByProductId(dto.Id.Value);

            if (dto.Addons is { Length: > 0 })
            {
                newProduct.ProductAddons = dto.Addons.Select(
                    addon => new ProductAddon { AddonId = addon }).ToList();
            }
            newProduct.UpdatedBy = _userContextService.GetCurrentUserId();
            newProduct.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(savedProduct, newProduct, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        public async Task RemoveAsync(Guid id)
        {
            var newProduct = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(newProduct);
        }
    }
}
