using AutoMapper;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Entities.Unmapped;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Inventory.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>();

            //ToDo: Obtener estas descripciones desde la base de datos mediante un include
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => "Unidad"))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => "Category"))
                .ForMember(dest => dest.ProductLocation, opt => opt.MapFrom(src => "Loc-002"));
            CreateMap<ProductDto, Product>();                
            CreateMap<ProductCreationDto, Product>();
            CreateMap<Product, ProductCreationDto>();

            CreateMap<ProductLocation, ProductLocationDto>();
            CreateMap<ProductLocationDto, ProductLocation>();

            CreateMap<Provider, ProviderDto>();
            CreateMap<ProviderDto, Provider>();

            CreateMap<ProviderAddress, ProviderAddressDto>();
            CreateMap<ProviderAddressDto, ProviderAddress>();

            CreateMap<Stock, StockDto>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.PreCondition(src => src.Date is not null);
                    opt.MapFrom(src => new DetailedDate(src.Date.Value.Year, src.Date.Value.Month, src.Date.Value.Day));
                });

            CreateMap<StockDto, Stock>();

            CreateMap<Stock, StockCreationDto>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.PreCondition(src => src.Date is not null);
                    opt.MapFrom(src => new DetailedDate(src.Date.Value.Year, src.Date.Value.Month, src.Date.Value.Day));
                });

            CreateMap<StockCreationDto, Stock>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.PreCondition(src => src.Date is not null);
                    opt.MapFrom(src => new DateTime(src.Date.Year, src.Date.Month, src.Date.Day));
                });

            CreateMap<Stock, StockWithDetailDto>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.PreCondition(src => src.Date is not null);
                    opt.MapFrom(src => new DetailedDate(src.Date.Value.Year, src.Date.Value.Month, src.Date.Value.Day));
                });

            CreateMap<StockDetails, StockDetailsDto>();
            CreateMap<StockDetailsDto, StockDetails>();

            CreateMap<StockDetails, StockCreationDetailDto>();
            CreateMap<StockCreationDetailDto, StockDetails>();

            CreateMap<Reason, ReasonDto>();
            CreateMap<ReasonDto, Reason>();

            CreateMap<Store, StoreDto>();
            CreateMap<StoreDto, Store>();

            CreateMap<RawProductCatalog, RawProductCatalogDto>();
            CreateMap<RawProductCatalogDto, RawProductCatalog>();
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
