using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IProductService
    {
        public Task<ProductDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<ProductDto>> GetAllAsync(RequestFilterDto search);

        public Task<ProductDto?> AddAsync(ProductCreationDto ProductDto);

        public Task UpdateAsync(ProductCreationDto ProductDto);

        public Task RemoveAsync(Guid id);
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

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(query =>
            {
                return query.Include(x => x.ProductLocation).Include(x => x.Category);
            }, x => x.Id == id);

            var productDto = _mapper.Map<ProductDto>(product);
            if (product is not null)
            {
                var addons = await _addonRepository.GetAddonsByProductId(product.Id);
                productDto.Addons = _mapper.Map<AddonDto[]>(addons);
            }

            return productDto;
        }

        public async Task<ResponseDto<ProductDto>> GetAllAsync(RequestFilterDto search)
        {
            Expression<Func<Product, bool>>? expression = null;
            if (search.Search is { Length: > 0 })
            {
                expression = x =>
                    x.Code.Contains(search.Search) ||
                    x.Name.Contains(search.Search) ||
                    x.Description.Contains(search.Search);
            }

            var products = await _repository.GetAllAsync(query => 
            {
                return query.Include(x => x.ProductLocation)
                     .Include(x => x.Category)
                     .OrderByDescending(x => x.Id)
                     .Skip(search.PageSize * Math.Abs(search.Page - 1))
                     .Take(search.PageSize);
            }, expression);

            return new ResponseDto<ProductDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<ProductDto>>(products)
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
