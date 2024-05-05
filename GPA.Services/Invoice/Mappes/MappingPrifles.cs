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
                .ForMember(dest => dest.ExpirationDate, opt =>
                {
                    opt.PreCondition(src => src.ExpirationDate is not null);
                    opt.MapFrom(src => new DateTime(src.ExpirationDate.Year, src.ExpirationDate.Month, src.ExpirationDate.Day));
                });
            CreateMap<GPA.Common.Entities.Invoice.Invoice, InvoiceDto>()
                .ForMember(dest => dest.ExpirationDate, opt =>
                {
                    opt.PreCondition(src => src.ExpirationDate is not null);
                    opt.MapFrom(src => new DetailedDate(src.ExpirationDate.Value.Year, src.ExpirationDate.Value.Month, src.ExpirationDate.Value.Day));
                });

            CreateMap<InvoiceDetailDto, InvoiceDetails>();
            CreateMap<InvoiceDetails, InvoiceDetailDto>();
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
