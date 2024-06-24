using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Invoices;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Data.Inventory;
using GPA.Data.Invoice;
using GPA.Entities.Common;
using GPA.Utils;
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
        private readonly IClientService _clientService;
        private readonly IInvoiceRepository _repository;
        private readonly IReceivableAccountRepository _receivableAccountRepository;
        private readonly IAddonRepository _addonRepository;
        private readonly IMapper _mapper;

        public InvoiceService(
            IClientService clientService,
            IInvoiceRepository repository,
            IStockRepository stockRepository,
            IReceivableAccountRepository receivableAccountRepository,
            IAddonRepository addonRepository,
            IMapper mapper)
        {
            _clientService = clientService;
            _repository = repository;
            _stockRepository = stockRepository;
            _receivableAccountRepository = receivableAccountRepository;
            _addonRepository = addonRepository;
            _mapper = mapper;
        }

        public async Task<InvoiceListDto?> GetByIdAsync(Guid id)
        {
            var savedInvoice = await _repository.GetByIdAsync(
                query => query.Include(x => x.InvoiceDetails).ThenInclude(x => x.Product)
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
                    invoice.Client = await _clientService.GetByIdAsync(savedInvoice.ClientId);
                    await MapAddonsToProduct(invoice.InvoiceDetails);
                }
            }

            return invoice;
        }

        public async Task<ResponseDto<InvoiceListDto>> GetAllAsync(SearchDto search, Expression<Func<GPA.Common.Entities.Invoice.Invoice, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.Include(x => x.Client).OrderByDescending(x => x.Id).Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
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

            await UpdatePricesByAddons(dto.InvoiceDetails.ToList());

            //ToDo: calcular el credito y debito, y comparar con el pago, para verificar si puede proceder

            invoice.PaymentStatus = GetPaymentStatus(invoice);
            var savedInvoice = await _repository.AddAsync(invoice);

            await AddStock(savedInvoice);
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
                await UpdatePricesByAddons(dto.InvoiceDetails.ToList());

                //ToDo: calcular el credito y debito, y comparar con el pago, para verificar si puede proceder

                var newInvoice = _mapper.Map<GPA.Common.Entities.Invoice.Invoice>(dto);
                var invoiceDetails = _mapper.Map<List<InvoiceDetails>>(dto.InvoiceDetails);

                foreach (var detail in invoiceDetails)
                {
                    detail.InvoiceId = newInvoice.Id;
                }

                newInvoice.PaymentStatus = GetPaymentStatus(newInvoice, invoiceDetails);

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
                InvoiceId = invoice.Id,
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

        private PaymentStatus GetPaymentStatus(
            GPA.Common.Entities.Invoice.Invoice invoice,
            List<InvoiceDetails> invoiceDetails)
        {
            if (invoice.Payment < invoiceDetails.Sum(x => x.Quantity * x.Price))
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

        private async Task UpdatePricesByAddons(List<InvoiceDetailUpdateDto> invoiceDetail)
        {
            var addons = await _addonRepository
                   .GetAddonsByProductIdAsDictionary(invoiceDetail.Select(x => x.ProductId).ToList());

            foreach (var detail in invoiceDetail)
            {
                if (addons.ContainsKey(detail.ProductId))
                {
                    var (debit, credit) = AddonCalculator.CalculateAddon(detail.Price, addons[detail.ProductId]);
                    detail.Price = detail.Price - debit + credit;
                }
            }
        }

        private async Task UpdatePricesByAddons(List<InvoiceDetailDto> invoiceDetail)
        {
            var addons = await _addonRepository
                   .GetAddonsByProductIdAsDictionary(invoiceDetail.Select(x => x.ProductId).ToList());

            foreach (var detail in invoiceDetail)
            {
                if (addons.ContainsKey(detail.ProductId))
                {
                    var (debit, credit) = AddonCalculator.CalculateAddon(detail.Price, addons[detail.ProductId]);
                    detail.Price = detail.Price - debit + credit;
                }
            }
        }

        private async Task MapAddonsToProduct(IEnumerable<InvoiceListDetailDto> products)
        {
            if (products is not null)
            {
                var mappedAddons = await _addonRepository
                    .GetAddonsByProductIdAsDictionary(products.Select(x => x.ProductId).ToList());

                foreach (var product in products)
                {
                    if (mappedAddons.ContainsKey(product.ProductId))
                    {
                        product.StockProduct.Addons = _mapper.Map<AddonDto[]>(mappedAddons[product.ProductId]);
                        var (debit, credit) = AddonCalculator.CalculateAddon(product.StockProduct.Price, product.StockProduct.Addons);
                        product.StockProduct.Debit = debit;
                        product.StockProduct.Credit = credit;
                    }
                }
            }
        }
    }
}
