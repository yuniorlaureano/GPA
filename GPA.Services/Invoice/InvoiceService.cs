using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Invoices;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Data.Inventory;
using GPA.Data.Invoice;
using GPA.Entities.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Invoice
{
    public interface IInvoiceService
    {
        public Task<InvoiceListDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<InvoiceListDto>> GetAllAsync(SearchDto search, Expression<Func<GPA.Common.Entities.Invoice.Invoice, bool>>? expression = null);

        public Task<InvoiceDto?> AddAsync(InvoiceDto invoiceDto);

        public Task UpdateAsync(InvoiceUpdateDto invoiceDto);

        public Task RemoveAsync(Guid id);

        Task CancelAsync(Guid id);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IInvoiceRepository _repository;
        private readonly IReceivableAccountRepository _receivableAccountRepository;
        private readonly IMapper _mapper;

        public InvoiceService(
            IInvoiceRepository repository,
            IStockRepository stockRepository,
            IReceivableAccountRepository receivableAccountRepository,
            IMapper mapper)
        {
            _repository = repository;
            _stockRepository = stockRepository;
            _receivableAccountRepository = receivableAccountRepository;
            _mapper = mapper;
        }

        public async Task<InvoiceListDto?> GetByIdAsync(Guid id)
        {
            var savedInvoice = await _repository.GetByIdAsync(
                query => query.Include(x => x.Client).Include(x => x.InvoiceDetails).ThenInclude(x => x.Product)
                , x => x.Id == id);

            var invoice = _mapper.Map<InvoiceListDto>(savedInvoice);

            if (savedInvoice is { InvoiceDetails: { Count: > 0 } })
            {
                var productsId = savedInvoice.InvoiceDetails.Select(x => x.Product.Id).ToList();
                if (productsId is not null)
                {
                    var stocks = (await _stockRepository.GetProductCatalogAsync(productsId.ToArray()))
                            .ToDictionary(k => k.ProductId, v => v);

                    foreach (var invoiceDetail in invoice.InvoiceDetails)
                    {
                        if (stocks.TryGetValue(invoiceDetail.ProductId, out var product))
                        {
                            invoiceDetail.StockProduct = _mapper.Map<ProductCatalogDto>(product);
                        }
                    }
                }
            }

            return invoice;
        }

        public async Task<ResponseDto<InvoiceListDto>> GetAllAsync(SearchDto search, Expression<Func<GPA.Common.Entities.Invoice.Invoice, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.OrderByDescending(x => x.Id).Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<InvoiceListDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<InvoiceListDto>>(categories)
            };
        }

        public async Task<InvoiceDto?> AddAsync(InvoiceDto dto)
        {
            var invoice = _mapper.Map<GPA.Common.Entities.Invoice.Invoice>(dto);
            invoice.InvoiceDetails = _mapper.Map<ICollection<InvoiceDetails>>(dto.InvoiceDetails);

            invoice.PaymentStatus = GetPaymentStatus(invoice);
            var savedInvoice = await _repository.AddAsync(invoice);

            await AddStock(invoice);
            await AddReceivableAccount(invoice);
            return _mapper.Map<InvoiceDto>(savedInvoice);
        }

        public async Task UpdateAsync(InvoiceUpdateDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var savedInvoice = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);

            var canEditInvoice =
                    savedInvoice is not null &&
                    savedInvoice.Status == InvoiceStatus.Draft;
            if (canEditInvoice)
            {
                var newInvoice = _mapper.Map<GPA.Common.Entities.Invoice.Invoice>(dto);
                var invoiceDetails = _mapper.Map<List<InvoiceDetails>>(dto.InvoiceDetails);

                foreach (var detail in invoiceDetails)
                {
                    detail.InvoiceId = newInvoice.Id;
                }

                await _repository.UpdateAsync(newInvoice, invoiceDetails);
                await AddStock(newInvoice);
                await AddReceivableAccount(newInvoice);
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedInvoice = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedInvoice);
        }

        public async Task CancelAsync(Guid id)
        {
            await _repository.CancelAsync(id);
        }

        private Stock ToStock(GPA.Common.Entities.Invoice.Invoice invoice)
        {
            return new Stock
            {
                TransactionType = TransactionType.Output,
                Description = $"Venta {(invoice.Type == SaleType.Credit ? "a Crédito" : "al Contado")}",
                Date = DateTime.Now,
                ReasonId = (int)ReasonTypes.Sale,
                CreatedAt = DateTime.Now,
                Status = StockStatus.Saved,
                StockDetails = invoice.InvoiceDetails.Select(x => new StockDetails
                {
                    Quantity = x.Quantity,
                    ProductId = x.ProductId,
                    CreatedAt = DateTime.Now
                }).ToList()
            };
        }

        private PaymentStatus GetPaymentStatus(GPA.Common.Entities.Invoice.Invoice invoice)
        {
            if (invoice.Payment < invoice.InvoiceDetails.Sum(x => x.Quantity * x.Price))
            {
                return PaymentStatus.Pending;
            }

            return PaymentStatus.Payed;
        }

        private async Task AddStock(GPA.Common.Entities.Invoice.Invoice invoice)
        {
            if (invoice.Status == InvoiceStatus.Saved)
            {
                await _stockRepository.AddAsync(ToStock(invoice));
            }
        }

        private async Task AddReceivableAccount(GPA.Common.Entities.Invoice.Invoice invoice)
        {
            if (invoice.Status == InvoiceStatus.Saved &&
                invoice.PaymentStatus == PaymentStatus.Pending)
            {
                var payment = invoice.InvoiceDetails.Sum(x => x.Quantity * x.Price);
                var paymentDetail = new ClientPaymentsDetails
                {
                    InvoiceId = invoice.Id,
                    PendingPayment = payment - invoice.Payment,
                    Date = DateTime.Now,
                };

                await _receivableAccountRepository.AddAsync(paymentDetail);
            }
        }
    }
}
