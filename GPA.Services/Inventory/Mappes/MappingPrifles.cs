using AutoMapper;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Inventory.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>();

            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();
            CreateMap<ProductCreationDto, Product>();
            CreateMap<Product, ProductCreationDto>();

            CreateMap<ProductLocation, ProductLocationDto>();
            CreateMap<ProductLocationDto, ProductLocation>();

            CreateMap<Provider, ProviderDto>();
            CreateMap<ProviderDto, Provider>();

            CreateMap<ProviderAddress, ProviderAddressDto>();
            CreateMap<ProviderAddressDto, ProviderAddress>();

            CreateMap<Stock, StockDto>();
            CreateMap<StockDto, Stock>();

            CreateMap<Item, ItemDto>();
            CreateMap<ItemDto, Item>();
        }
    }

    public static class InventoryMapperExtensions
    {
        public static void AddInventoryMappers(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
