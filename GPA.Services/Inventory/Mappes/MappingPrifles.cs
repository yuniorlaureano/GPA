using AutoMapper;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Dtos.Inventory;
using GPA.Dtos.Invoice;
using GPA.Entities.Inventory;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Inventory.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<RawCategory, CategoryDto>();
            CreateMap<CategoryDto, Category>();

            CreateMap<Addon, AddonDto>();
            CreateMap<AddonDto, Addon>();
            CreateMap<RawAddons, AddonDto>();
            CreateMap<RawAddonsList, AddonDto>();
            CreateMap<Addon, RawAddonsList>();           
            CreateMap<RawAddonsList, Addon>();

            CreateMap<ProductAddon, ProductAddonDto>();
            CreateMap<ProductAddonDto, ProductAddon>();

            CreateMap<InvoiceAttachmentDto, InvoiceAttachment>();
            CreateMap<InvoiceAttachment, InvoiceAttachmentDto>();

            //ToDo: Obtener estas descripciones desde la base de datos mediante un include
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => "Unidad"))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => "Category"))
                .ForMember(dest => dest.ProductLocation, opt => opt.MapFrom(src => "Loc-002"));

            CreateMap<RawProduct, ProductDto>();
            CreateMap<ProductDto, Product>();
            CreateMap<ProductCreationDto, Product>();
            CreateMap<Product, ProductCreationDto>();

            CreateMap<ProductLocation, ProductLocationDto>();
            CreateMap<ProductLocationDto, ProductLocation>();

            CreateMap<Provider, ProviderDto>();
            CreateMap<RawProviders, ProviderDto>();
            CreateMap<ProviderDto, Provider>();

            CreateMap<Stock, StockDto>();

            CreateMap<RawStock, StockDto>();

            CreateMap<StockDto, Stock>();

            CreateMap<Stock, StockCreationDto>();

            CreateMap<StockCreationDto, Stock>();

            CreateMap<Stock, StockWithDetailDto>();

            CreateMap<RawStock, StockWithDetailDto>();

            CreateMap<StockDetails, StockDetailsDto>();
            CreateMap<RawStockDetails, StockDetailsDto>();
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
                    opt.MapFrom(src => new DetailedDate(src.EndDate.Value.Year, src.EndDate.Value.Month, src.EndDate.Value.Day));
                });

            CreateMap<RawStockCycle, StockCycleDto>()
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
                    opt.MapFrom(src => new DetailedDate(src.EndDate.Value.Year, src.EndDate.Value.Month, src.EndDate.Value.Day));
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
            CreateMap<RawStockCycleDetails, StockCycleDetailDto>();

            CreateMap<StockCycleDetail, StockCycleCreationDetailDto>();
            CreateMap<StockCycleCreationDetailDto, StockCycleDetail>();

            CreateMap<Stock, OutputCreationDto>();
            CreateMap<OutputCreationDto, Stock>();

            CreateMap<StockAttachment, StockAttachmentDto>();
            CreateMap<StockAttachmentDto, StockAttachment>();
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
