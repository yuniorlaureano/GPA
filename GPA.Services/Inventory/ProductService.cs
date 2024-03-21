using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IProductService
    {
        public Task<ProductDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<ProductDto>> GetAllAsync(SearchDto search, Expression<Func<Product, bool>>? expression = null);

        public Task<ProductDto?> AddAsync(ProductCreationDto ProductDto);

        public Task UpdateAsync(ProductCreationDto ProductDto);

        public Task RemoveAsync(Guid id);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var Product = await _repository.GetByIdAsync(query =>
            {
                return query.Include(x => x.ProductLocation).Include(x => x.Category);
            }, x => x.Id == id);
            return _mapper.Map<ProductDto>(Product);
        }

        public async Task<ResponseDto<ProductDto>> GetAllAsync(SearchDto search, Expression<Func<Product, bool>>? expression = null)
        {
            var products = await _repository.GetAllAsync(query => 
            {
                return query.Include(x => x.ProductLocation)
                     .Include(x => x.Category)
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
