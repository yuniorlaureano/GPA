using AutoMapper;
using GPA.Common.DTOs.Invoice;
using GPA.Common.DTOs.Invoices;
using GPA.Common.Entities.Invoice;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Invoice;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Invoice.Mappers
{
    public class MappingPrifles : Profile
    {
        public MappingPrifles()
        {
            CreateMap<Client, ClientDto>();
            CreateMap<RawClient, ClientDto>();
            CreateMap<ClientDto, Client>();
            CreateMap<RawClient, Client>();
            CreateMap<Client, RawClient>();

            CreateMap<ClientCreditDto, ClientCredit>();
            CreateMap<ClientCredit, ClientCreditDto>();
            CreateMap<RawCredit, ClientCreditDto>();

            CreateMap<RawPenddingPayment, ClientDebitDto>();

            CreateMap<InvoiceDto, GPA.Common.Entities.Invoice.Invoice>();

            CreateMap<GPA.Common.Entities.Invoice.Invoice, InvoiceDto>();

            CreateMap<InvoiceUpdateDto, GPA.Common.Entities.Invoice.Invoice>()
                .ForMember(dest => dest.InvoiceDetails, opt => opt.Ignore());

            CreateMap<InvoiceDetailDto, InvoiceDetails>();
            CreateMap<InvoiceDetails, InvoiceDetailDto>();

            CreateMap<InvoiceDetailUpdateDto, InvoiceDetails>();

            CreateMap<InvoiceListDto, GPA.Common.Entities.Invoice.Invoice>();

            CreateMap<GPA.Common.Entities.Invoice.Invoice, InvoiceListDto>();

            CreateMap<RawInvoice, InvoiceListDto>();

            CreateMap<InvoiceListDetailDto, InvoiceDetails>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<InvoiceDetails, InvoiceListDetailDto>();

            CreateMap<RawInvoiceDetails, InvoiceListDetailDto>();

            CreateMap<ClientPaymentsDetails, ClientPaymentsDetailDto>();

            CreateMap<ClientPaymentsDetailCreationDto, ClientPaymentsDetails>();

            CreateMap<ClientPaymentsDetailDto, ClientPaymentsDetailCreationDto>();

            CreateMap<GPA.Common.Entities.Invoice.Invoice, InvoiceWithReceivableAccountsDto>()
                .ForMember(dest => dest.ClientId, src => src.MapFrom(x => x.Client.Id))
                .ForMember(dest => dest.ClientName, src => src.MapFrom(x => x.Client.Name + " " + x.Client.LastName))
                .ForMember(dest => dest.ClientIdentification, src => src.MapFrom(x => x.Client.Identification))
                .ForMember(dest => dest.ClientEmail, src => src.MapFrom(x => x.Client.Email))
                .ForMember(dest => dest.ClientPhone, src => src.MapFrom(x => x.Client.Phone))
                .ForMember(dest => dest.SaleType, src => src.MapFrom(x => x.Type))
                .ForMember(dest => dest.InvoiceCode, src => src.MapFrom(x => x.Code))
                .ForMember(dest => dest.InvoiceStatus, src => src.MapFrom(x => x.Status))
                .ForMember(dest => dest.InvoiceNote, src => src.MapFrom(x => x.Note))
                .ForMember(dest => dest.InvoiceId, src => src.MapFrom(x => x.Id))
                .ForMember(dest => dest.PaymentStatus, src => src.MapFrom(x => (int)x.PaymentStatus));

            CreateMap<ClientPaymentsDetailSummary, ClientPaymentsDetailSummaryDto>();
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
