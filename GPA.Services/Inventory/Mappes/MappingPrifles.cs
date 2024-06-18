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
                .ForMember(dest => dest.ProductLocation, opt => opt.MapFrom(src => "Loc-002"))
                .ForMember(dest => dest.ExpirationDate, opt =>
                {
                    opt.PreCondition(src => src.ExpirationDate is not null);
                    opt.MapFrom(src => new DetailedDate(src.ExpirationDate.Value.Year, src.ExpirationDate.Value.Month, src.ExpirationDate.Value.Day));
                });

            CreateMap<ProductDto, Product>();                
            CreateMap<ProductCreationDto, Product>()
                .ForMember(dest => dest.ExpirationDate, opt =>
                {
                    opt.PreCondition(src => src.ExpirationDate is not null);
                    opt.MapFrom(src => new DateTime(src.ExpirationDate.Year, src.ExpirationDate.Month, src.ExpirationDate.Day));
                });
            CreateMap<Product, ProductCreationDto>()
                .ForMember(dest => dest.ExpirationDate, opt =>
                {
                    opt.PreCondition(src => src.ExpirationDate is not null);
                    opt.MapFrom(src => new DetailedDate(src.ExpirationDate.Value.Year, src.ExpirationDate.Value.Month, src.ExpirationDate.Value.Day));
                });

            CreateMap<ProductLocation, ProductLocationDto>();
            CreateMap<ProductLocationDto, ProductLocation>();

            CreateMap<Provider, ProviderDto>();
            CreateMap<ProviderDto, Provider>();

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
                }).ForMember(dest => dest.StockDetails, opt => opt.Ignore());

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

            CreateMap<RawProductCatalog, ProductCatalogDto>();
            CreateMap<ProductCatalogDto, RawProductCatalog>();

            CreateMap<Existence, ExistanceDto>();
            CreateMap<ExistanceDto, Existence>();

            CreateMap<StockCycle, StockCycleDto>()
                .ForMember(dest => dest.StartDate, opt =>
                {
                    opt.MapFrom(src => new DetailedDate(src.StartDate.Year, src.StartDate.Month, src.StartDate.Day));
                })
                .ForMember(dest => dest.EndDate, opt =>
                {
                    opt.MapFrom(src => new DetailedDate(src.EndDate.Year, src.EndDate.Month, src.EndDate.Day));
                });

            CreateMap<StockCycle, StockCycleCreationDto>()
                .ForMember(dest => dest.StartDate, opt =>
                {
                    opt.MapFrom(src => new DetailedDate(src.StartDate.Year, src.StartDate.Month, src.StartDate.Day));
                })
                .ForMember(dest => dest.EndDate, opt =>
                {
                    opt.MapFrom(src => new DetailedDate(src.EndDate.Year, src.EndDate.Month, src.EndDate.Day));
                });

            CreateMap<StockCycleCreationDto, StockCycle>()
                .ForMember(dest => dest.StartDate, opt =>
                {
                    opt.MapFrom(src => new DateOnly(src.StartDate.Year, src.StartDate.Month, src.StartDate.Day));
                })
                .ForMember(dest => dest.EndDate, opt =>
                {
                    opt.MapFrom(src => new DateOnly(src.EndDate.Year, src.EndDate.Month, src.EndDate.Day));
                }).ForMember(dest => dest.StockCycleDetails, opt => opt.Ignore());

            CreateMap<StockCycleDetail, StockCycleDetailDto>();
            CreateMap<StockCycleDetailDto, StockCycleDetail>();

            CreateMap<StockCycleDetail, StockCycleCreationDetailDto>();
            CreateMap<StockCycleCreationDetailDto, StockCycleDetail>();
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
