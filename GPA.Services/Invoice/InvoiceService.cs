using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Invoice;
using GPA.Common.DTOs.Invoices;
using GPA.Common.DTOs.Unmapped;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Data.Inventory;
using GPA.Data.Invoice;
using GPA.Entities.General;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Invoice;
using GPA.Services.Security;
using GPA.Utils;

namespace GPA.Business.Services.Invoice
{
    public interface IInvoiceService
    {
        public Task<InvoiceListDto?> GetInvoiceByIdAsync(Guid id);

        public Task<ResponseDto<InvoiceListDto>> GetInvoicesAsync(RequestFilterDto search);

        public Task<InvoiceDto?> AddAsync(InvoiceDto invoiceDto);

        public Task UpdateAsync(InvoiceUpdateDto invoiceDto);

        public Task RemoveAsync(Guid id);

        Task CancelAsync(Guid id);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IClientService _clientService;
        private readonly IInvoiceRepository _repository;
        private readonly IReceivableAccountRepository _receivableAccountRepository;
        private readonly IAddonRepository _addonRepository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public InvoiceService(
            IClientService clientService,
            IClientRepository clientRepository,
            IInvoiceRepository repository,
            IStockRepository stockRepository,
            IReceivableAccountRepository receivableAccountRepository,
            IAddonRepository addonRepository,
            IUserContextService userContextService,
            IMapper mapper)
        {
            _clientRepository = clientRepository;
            _clientService = clientService;
            _repository = repository;
            _stockRepository = stockRepository;
            _receivableAccountRepository = receivableAccountRepository;
            _addonRepository = addonRepository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<InvoiceListDto?> GetInvoiceByIdAsync(Guid id)
        {
            var invoice = await _repository.GetInvoiceByIdAsync(id);

            if (invoice is null)
            {
                throw new InvalidOperationException("The invoice does not exists.");
            }

            var invoiceDetails = await _repository.GetInvoiceDetailsByInvoiceIdAsync(id);
            var invoiceDto = _mapper.Map<InvoiceListDto>(invoice);
            var invoiceDetailDto = _mapper.Map<List<InvoiceListDetailDto>>(invoiceDetails);

            invoiceDto.Client = await _clientService.GetClientAsync(invoice.ClientId) ?? new ClientDto();
            invoiceDto.InvoiceDetails = invoiceDetailDto;

            if (invoiceDetailDto is { Count: 0 })
            {
                return invoiceDto;
            }

            var productsCatalog = await GetRawProductCatalogsAsDictionary(invoiceDetailDto);
            if (productsCatalog is null)
            {
                return invoiceDto;
            }

            MapProductCatalogToInvoiceDetail(invoiceDetailDto, productsCatalog);
            await MapAddonsToProduct(invoiceDetailDto);

            return invoiceDto;
        }

        public async Task<ResponseDto<InvoiceListDto>> GetInvoicesAsync(RequestFilterDto search)
        {
            var invoices = (await _repository.GetInvoicesAsync(search)).ToList();
            var invoicesDto = _mapper.Map<List<InvoiceListDto>>(invoices);

            if (invoices is { Count: 0 })
            {
                return new ResponseDto<InvoiceListDto>
                {
                    Count = await _repository.GetInvoicesCountAsync(search),
                    Data = invoicesDto
                };
            }

            await MapClientsToInvoice(invoices, invoicesDto);

            return new ResponseDto<InvoiceListDto>
            {
                Count = await _repository.GetInvoicesCountAsync(search),
                Data = invoicesDto
            };
        }

        public async Task<InvoiceDto?> AddAsync(InvoiceDto dto)
        {
            var invoice = _mapper.Map<GPA.Common.Entities.Invoice.Invoice>(dto);
            invoice.InvoiceDetails = _mapper.Map<ICollection<InvoiceDetails>>(dto.InvoiceDetails);

            var addons = await _addonRepository
                   .GetAddonsByProductIdAsDictionary(invoice.InvoiceDetails.Select(x => x.ProductId).ToList());

            var isCheckCredits = invoice.Status != InvoiceStatus.Draft;
            if (isCheckCredits)
            {
                await CheckIfClientHasEnoughtCredit(invoice.ClientId, invoice.Payment, invoice.InvoiceDetails, addons);
            }

            InitializeInvoiceDetailWithAddons(invoice.InvoiceDetails, addons);

            invoice.PaymentStatus = GetPaymentStatus(invoice);
            invoice.CreatedBy = _userContextService.GetCurrentUserId();
            invoice.CreatedAt = DateTimeOffset.UtcNow;
            var savedInvoice = await _repository.AddAsync(invoice);

            await AddStock(savedInvoice);
            await AddReceivableAccount(invoice, addons);
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
                var addons = await _addonRepository
                   .GetAddonsByProductIdAsDictionary(dto.InvoiceDetails.Select(x => x.ProductId).ToList());

                var newInvoice = _mapper.Map<GPA.Common.Entities.Invoice.Invoice>(dto);
                var invoiceDetails = _mapper.Map<List<InvoiceDetails>>(dto.InvoiceDetails);

                var isCheckCredits = newInvoice.Status != InvoiceStatus.Draft;
                if (isCheckCredits)
                {
                    await CheckIfClientHasEnoughtCredit(newInvoice.ClientId, newInvoice.Payment, invoiceDetails, addons);
                }

                foreach (var detail in invoiceDetails)
                {
                    detail.InvoiceId = newInvoice.Id;
                }

                newInvoice.PaymentStatus = GetPaymentStatus(newInvoice, invoiceDetails);

                InitializeInvoiceDetailWithAddons(invoiceDetails, addons);
                newInvoice.UpdatedBy = _userContextService.GetCurrentUserId();
                newInvoice.UpdatedAt = DateTimeOffset.UtcNow;
                await _repository.UpdateAsync(newInvoice, invoiceDetails);
                await AddStock(newInvoice);
                await AddReceivableAccount(newInvoice, addons);
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedInvoice = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedInvoice);
        }

        public async Task CancelAsync(Guid id)
        {
            await _repository.CancelAsync(id, _userContextService.GetCurrentUserId());
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
                invoice.CreatedBy = _userContextService.GetCurrentUserId();
                invoice.CreatedAt = DateTimeOffset.UtcNow;
                await _stockRepository.AddAsync(ToStock(invoice));
            }
        }

        private async Task AddReceivableAccount(GPA.Common.Entities.Invoice.Invoice invoice, Dictionary<Guid, List<RawAddons>> addons)
        {
            if (invoice.Status == InvoiceStatus.Saved &&
                invoice.PaymentStatus == PaymentStatus.Pending)
            {
                var payment = invoice.InvoiceDetails.Sum(x => x.Quantity * GetNetPrice(x, addons));
                var paymentDetail = new ClientPaymentsDetails
                {
                    InvoiceId = invoice.Id,
                    PendingPayment = payment - invoice.Payment,
                    Date = DateTime.Now,
                };
                paymentDetail.CreatedBy = _userContextService.GetCurrentUserId();
                paymentDetail.CreatedAt = DateTimeOffset.UtcNow;
                await _receivableAccountRepository.AddAsync(paymentDetail);
            }
        }

        private async Task UpdatePricesByAddons(List<InvoiceDetailUpdateDto> invoiceDetail, Dictionary<Guid, List<RawAddons>> addons)
        {
            foreach (var detail in invoiceDetail)
            {
                if (addons.ContainsKey(detail.ProductId))
                {
                    var (debit, credit) = AddonCalculator.CalculateAddon(detail.Price, addons[detail.ProductId]);
                    detail.Price = detail.Price - debit + credit;
                }
            }
        }

        private void UpdatePricesByAddons(List<InvoiceDetails> invoiceDetail, Dictionary<Guid, List<RawAddons>> addons)
        {
            foreach (var detail in invoiceDetail)
            {
                if (addons.ContainsKey(detail.ProductId))
                {
                    var (debit, credit) = AddonCalculator.CalculateAddon(detail.Price, addons[detail.ProductId]);
                    detail.Price = detail.Price - debit + credit;
                }
            }
        }

        private decimal GetNetPrice(InvoiceDetails detail, Dictionary<Guid, List<RawAddons>> addons)
        {
            if (addons.ContainsKey(detail.ProductId))
            {
                var (debit, credit) = AddonCalculator.CalculateAddon(detail.Price, addons[detail.ProductId]);
                return detail.Price - debit + credit;
            }
            return detail.Price;
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

        private async Task CheckIfClientHasEnoughtCredit(Guid clientId, decimal payment, ICollection<InvoiceDetails> invoiceDetails, Dictionary<Guid, List<RawAddons>> addons)
        {
            var debits = await _receivableAccountRepository.GetPenddingPaymentByClientId(clientId);
            var credits = await _clientRepository.GetCreditsByClientIdAsync(new List<Guid> { clientId });

            var debit = debits.Sum(x => x.PendingPayment);
            var credit = credits.Sum(x => x.Credit);
            var toPay = invoiceDetails.Sum(x => x.Quantity * GetNetPrice(x, addons));
            var duePayment = toPay - payment;

            if (duePayment > (credit - debit))
            {
                throw new InvalidOperationException("No se puede proceder con la venta, el cliente no tiene suficiente credito");
            }
        }

        private void InitializeInvoiceDetailWithAddons(ICollection<InvoiceDetails> invoiceDetail, Dictionary<Guid, List<RawAddons>> addons)
        {
            foreach (var detail in invoiceDetail)
            {
                if (addons.ContainsKey(detail.ProductId))
                {
                    detail.InvoiceDetailsAddons = addons[detail.ProductId].Select(x => new InvoiceDetailsAddon
                    {
                        Concept = x.Concept,
                        IsDiscount = x.IsDiscount,
                        Type = x.Type,
                        Value = x.Value
                    }).ToList();
                }
            }
        }

        private async Task MapClientsToInvoice(IEnumerable<RawInvoice> invoices, IEnumerable<InvoiceListDto> invoicesDto)
        {
            var clients = await _clientRepository.GetClientsByIdsAsync(invoices.Select(x => x.ClientId));

            var clientDictionary = new Dictionary<Guid, ClientDto?>();
            foreach (var client in clients)
            {
                if (!clientDictionary.ContainsKey(client.Id))
                {
                    clientDictionary.Add(client.Id, null);
                }
                clientDictionary[client.Id] = _mapper.Map<ClientDto>(client);
            }

            foreach (var invoice in invoicesDto)
            {
                if (clientDictionary.ContainsKey(invoice.ClientId))
                {
                    invoice.Client = clientDictionary[invoice.ClientId]!;
                }
            }
        }

        private async Task<Dictionary<Guid, RawProductCatalog>?> GetRawProductCatalogsAsDictionary(List<InvoiceListDetailDto> invoiceDetailDto)
        {
            var productsId = invoiceDetailDto.Select(x => x.ProductId).ToList();

            if (productsId is { Count: 0 })
            {
                return null;
            }

            return (await _stockRepository.GetProductCatalogAsync(productsId.ToArray()))
                        .ToDictionary(k => k.ProductId, v => v);
        }

        private void MapProductCatalogToInvoiceDetail(List<InvoiceListDetailDto> invoiceDetailDto, Dictionary<Guid, RawProductCatalog> productsCatalog)
        {
            foreach (var invoiceDetail in invoiceDetailDto)
            {
                if (productsCatalog.TryGetValue(invoiceDetail.ProductId, out var product))
                {
                    invoiceDetail.StockProduct = _mapper.Map<ProductCatalogDto>(product);
                }
            }
        }
    }
}
