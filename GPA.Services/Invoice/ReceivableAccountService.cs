using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoice;
using GPA.Common.Entities.Invoice;
using GPA.Data.Invoice;
using GPA.Entities.General;
using GPA.Services.Security;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Invoice
{
    public interface IReceivableAccountService
    {
        Task<ClientPaymentsDetailDto?> GetByIdAsync(Guid id);

        Task<InvoiceWithReceivableAccountsDto?> GetByInvoiceIdAsync(Guid id);

        //Task<ResponseDto<ClientPaymentsDetailDto>> GetAllAsync(RequestFilterDto search, Expression<Func<ClientPaymentsDetails, bool>>? expression = null);

        Task<ResponseDto<ClientPaymentsDetailSummaryDto>> GetReceivableSummaryAsync(RequestFilterDto search);

        Task<ClientPaymentsDetailDto?> AddAsync(ClientPaymentsDetailCreationDto clientDto);

        Task UpdateAsync(ClientPaymentsDetailCreationDto clientDto);

        Task RemoveAsync(Guid id);
    }

    public class ReceivableAccountService : IReceivableAccountService
    {
        private readonly IReceivableAccountRepository _repository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public ReceivableAccountService(
            IReceivableAccountRepository repository,
            IUserContextService userContextService,
            IInvoiceRepository invoiceRepository,
            IMapper mapper)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<ClientPaymentsDetailDto?> GetByIdAsync(Guid id)
        {
            var client = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<ClientPaymentsDetailDto>(client);
        }

        public async Task<InvoiceWithReceivableAccountsDto?> GetByInvoiceIdAsync(Guid id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(
                    query => query.Include(x => x.ClientPaymentsDetails)
                                  .Include(x => x.Client),
                    x => x.Id == id
                );

            var invoiceWithReceivableAccounts = _mapper.Map<InvoiceWithReceivableAccountsDto>(invoice);

            if (invoice?.ClientPaymentsDetails is { Count: > 0 })
            {
                invoiceWithReceivableAccounts.PendingPayment = _mapper.Map<ClientPaymentsDetailDto>(
                    invoice.ClientPaymentsDetails.FirstOrDefault(
                        x => x.PendingPayment != x.Payment && x.Payment == 0)
                );
            }

            invoiceWithReceivableAccounts.ReceivableAccounts =
                _mapper.Map<IEnumerable<ClientPaymentsDetailDto>>(invoice.ClientPaymentsDetails);
            return invoiceWithReceivableAccounts;
        }

        //public async Task<ResponseDto<ClientPaymentsDetailDto>> GetAllAsync(RequestFilterDto search, Expression<Func<ClientPaymentsDetails, bool>>? expression = null)
        //{
        //    var categories = await _repository.GetAllAsync(query =>
        //    {
        //        return query
        //            .OrderByDescending(x => x.Id)
        //            .Skip(search.PageSize * Math.Abs(search.Page - 1))
        //            .Take(search.PageSize);
        //    }, expression);
        //    return new ResponseDto<ClientPaymentsDetailDto>
        //    {
        //        Count = await _repository.CountAsync(query => query, expression),
        //        Data = _mapper.Map<IEnumerable<ClientPaymentsDetailDto>>(categories)
        //    };
        //}

        public async Task<ResponseDto<ClientPaymentsDetailSummaryDto>> GetReceivableSummaryAsync(RequestFilterDto search)
        {
            var receivables = await _repository.GetReceivableSummaryAsync(search);

            return new ResponseDto<ClientPaymentsDetailSummaryDto>
            {
                Count = await _repository.GetReceivableSummaryCountAsync(search),
                Data = _mapper.Map<IEnumerable<ClientPaymentsDetailSummaryDto>>(receivables)
            };
        }

        public async Task<ClientPaymentsDetailDto?> AddAsync(ClientPaymentsDetailCreationDto dto)
        {
            var payment = _mapper.Map<ClientPaymentsDetails>(dto);
            payment.CreatedBy = _userContextService.GetCurrentUserId();
            payment.CreatedAt = DateTimeOffset.UtcNow;
            payment.Date = DateTime.UtcNow;
            var savedClient = await _repository.AddAsync(payment);
            return _mapper.Map<ClientPaymentsDetailDto>(savedClient);
        }

        public async Task UpdateAsync(ClientPaymentsDetailCreationDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            if (dto.Payment <= 0)
            {
                throw new ArgumentNullException();
            }

            var invoice = await _invoiceRepository.GetByIdAsync(query => query, x => x.Id == dto.InvoiceId);
            var paymentDetail = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);

            if (paymentDetail is not null && invoice is not null)
            {
                var (pendingPayment, hasMorePayments) = await MakePayment(paymentDetail, dto);

                if (hasMorePayments)
                {
                    await CreateNextPayment(dto, pendingPayment);
                }
                else
                {
                    await MarkInvoiceAsPayed(invoice);
                }
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedClient = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedClient);
        }

        private async Task CreateNextPayment(ClientPaymentsDetailCreationDto dto, decimal pendingPayment)
        {
            var nextPayment = new ClientPaymentsDetailCreationDto
            {
                PendingPayment = pendingPayment,
                InvoiceId = dto.InvoiceId,
                Payment = 0.0M
            };
            await AddAsync(nextPayment);
        }

        private async Task MarkInvoiceAsPayed(GPA.Common.Entities.Invoice.Invoice invoice)
        {
            invoice.PaymentStatus = PaymentStatus.Paid;
            invoice.UpdatedBy = _userContextService.GetCurrentUserId();
            invoice.UpdatedAt = DateTimeOffset.UtcNow;
            await _invoiceRepository.UpdateAsync(invoice, invoice, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
        }

        private async Task<(decimal pendingPayment, bool hasMorePayments)> MakePayment(ClientPaymentsDetails paymentDetail, ClientPaymentsDetailCreationDto dto)
        {
            var pendingPayment = paymentDetail.PendingPayment - dto.Payment;
            paymentDetail.Payment = dto.Payment;
            paymentDetail.Date = DateTime.UtcNow;
            paymentDetail.UpdatedBy = _userContextService.GetCurrentUserId();
            paymentDetail.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(paymentDetail, paymentDetail, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });

            return (pendingPayment, pendingPayment > 0);
        }
    }
}
