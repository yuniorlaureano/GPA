using AutoMapper;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Invoice;
using GPA.Common.DTOs.Invoices;
using GPA.Common.Entities.Invoice;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Invoice.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<Client, ClientDto>();
            CreateMap<ClientDto, Client>();

            CreateMap<InvoiceDto, GPA.Common.Entities.Invoice.Invoice>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.MapFrom(src => new DateTime(src.Date.Year, src.Date.Month, src.Date.Day));
                }).ForMember(x => x.InvoiceDetails, opt => opt.Ignore());

            CreateMap<GPA.Common.Entities.Invoice.Invoice, InvoiceDto>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.MapFrom(src => new DetailedDate(src.Date.Year, src.Date.Month, src.Date.Day));
                });

            CreateMap<InvoiceUpdateDto, GPA.Common.Entities.Invoice.Invoice>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.MapFrom(src => new DateTime(src.Date.Year, src.Date.Month, src.Date.Day));
                }).ForMember(x => x.InvoiceDetails, opt => opt.Ignore());

            CreateMap<InvoiceDetailDto, InvoiceDetails>();
            CreateMap<InvoiceDetails, InvoiceDetailDto>();

            CreateMap<InvoiceDetailUpdateDto, InvoiceDetails>();

            CreateMap<InvoiceListDto, GPA.Common.Entities.Invoice.Invoice>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.MapFrom(src => new DateTime(src.Date.Year, src.Date.Month, src.Date.Day));
                });
            CreateMap<GPA.Common.Entities.Invoice.Invoice, InvoiceListDto>()
                .ForMember(dest => dest.Date, opt =>
                {
                    opt.MapFrom(src => new DetailedDate(src.Date.Year, src.Date.Month, src.Date.Day));
                });

            CreateMap<InvoiceListDetailDto, InvoiceDetails>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<InvoiceDetails, InvoiceListDetailDto>();
        }
    }

    public static class InvoiceMapperExtensions
    {
        public static void AddInvoiceMappers(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
