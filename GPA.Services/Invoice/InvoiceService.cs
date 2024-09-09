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
using GPA.Dtos.Audit;
using GPA.Dtos.General;
using GPA.Dtos.Invoice;
using GPA.Entities.General;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Invoice;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
using GPA.Utils;
using GPA.Utils.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace GPA.Business.Services.Invoice
{
    public interface IInvoiceService
    {
        public Task<InvoiceListDto?> GetInvoiceByIdAsync(Guid id);
        public Task<ResponseDto<InvoiceListDto>> GetInvoicesAsync(RequestFilterDto search);
        public Task<InvoiceDto?> AddAsync(InvoiceDto invoiceDto);
        public Task UpdateAsync(InvoiceUpdateDto invoiceDto);
        Task SaveAttachment(Guid invoiceId, IFormFile file);
        Task<IEnumerable<InvoiceAttachmentDto>> GetAttachmentByInvoiceIdAsync(Guid invoiceId);
        Task<(Stream? file, string fileName)> DownloadAttachmentAsync(Guid id);
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
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly IInvoiceAttachmentRepository _invoiceAttachmentRepository;
        private readonly IMapper _mapper;

        public InvoiceService(
            IClientService clientService,
            IClientRepository clientRepository,
            IInvoiceRepository repository,
            IStockRepository stockRepository,
            IReceivableAccountRepository receivableAccountRepository,
            IAddonRepository addonRepository,
            IUserContextService userContextService,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IInvoiceAttachmentRepository invoiceAttachmentRepository,
            IMapper mapper)
        {
            _clientRepository = clientRepository;
            _clientService = clientService;
            _repository = repository;
            _stockRepository = stockRepository;
            _receivableAccountRepository = receivableAccountRepository;
            _addonRepository = addonRepository;
            _userContextService = userContextService;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _invoiceAttachmentRepository = invoiceAttachmentRepository;
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
                var debits = await _receivableAccountRepository.GetPenddingPaymentByClientId(invoice.ClientId);
                var credits = await _clientRepository.GetCreditsByClientIdAsync(new List<Guid> { invoice.ClientId });
                PaymentCalculator.CheckIfClientHasEnoughCredit(debits, credits, invoice.Payment, invoice.InvoiceDetails, addons);
            }

            InitializeInvoiceDetailWithAddons(invoice.InvoiceDetails, addons);

            invoice.PaymentStatus = PaymentCalculator.GetPaymentStatus(invoice, addons);
            invoice.CreatedBy = _userContextService.GetCurrentUserId();
            invoice.CreatedAt = DateTimeOffset.UtcNow;
            invoice.Date = DateTime.UtcNow;
            var savedInvoice = await _repository.AddAsync(invoice);

            await AddStock(savedInvoice);
            await AddReceivableAccount(invoice, addons);
            await _repository.AddHistory(savedInvoice, savedInvoice.InvoiceDetails, ActionConstants.Add, _userContextService.GetCurrentUserId());
            
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

                var newInvoice = _mapper.Map<Common.Entities.Invoice.Invoice>(dto);
                var invoiceDetails = _mapper.Map<List<InvoiceDetails>>(dto.InvoiceDetails);

                var isCheckCredits = newInvoice.Status != InvoiceStatus.Draft;
                if (isCheckCredits)
                {
                    var debits = await _receivableAccountRepository.GetPenddingPaymentByClientId(newInvoice.ClientId);
                    var credits = await _clientRepository.GetCreditsByClientIdAsync(new List<Guid> { newInvoice.ClientId });
                    PaymentCalculator.CheckIfClientHasEnoughCredit(debits, credits, newInvoice.Payment, invoiceDetails, addons);
                }

                foreach (var detail in invoiceDetails)
                {
                    detail.InvoiceId = newInvoice.Id;
                    detail.CreatedBy = _userContextService.GetCurrentUserId();
                    detail.CreatedAt = DateTimeOffset.UtcNow;
                }

                newInvoice.PaymentStatus = PaymentCalculator.GetPaymentStatus(newInvoice, invoiceDetails, addons);

                InitializeInvoiceDetailWithAddons(invoiceDetails, addons);
                newInvoice.UpdatedBy = _userContextService.GetCurrentUserId();
                newInvoice.UpdatedAt = DateTimeOffset.UtcNow;
                newInvoice.Date = savedInvoice.Date;
                await _repository.UpdateAsync(newInvoice, invoiceDetails);
                await AddStock(newInvoice);
                await AddReceivableAccount(newInvoice, addons);
                await _repository.AddHistory(newInvoice, invoiceDetails, ActionConstants.Add, _userContextService.GetCurrentUserId());
            }
        }

        public async Task CancelAsync(Guid id)
        {
            await _repository.CancelAsync(id, _userContextService.GetCurrentUserId());
        }

        public async Task SaveAttachment(Guid invoiceId, IFormFile file)
        {
            var fileResult = await _blobStorageServiceFactory.UploadFile(file, "invoice/", isPublic: false);
            var jsonFileResult = JsonSerializer.Serialize(fileResult, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var attachment = new InvoiceAttachment
            {
                InvoiceId = invoiceId,
                File = jsonFileResult,
                UploadedAt = DateTimeOffset.UtcNow,
                UploadedBy = _userContextService.GetCurrentUserId()
            };
            await _invoiceAttachmentRepository.SaveAttachmentAsync(attachment);
        }

        public async Task<IEnumerable<InvoiceAttachmentDto>> GetAttachmentByInvoiceIdAsync(Guid invoiceId)
        {
            var attachments = await _invoiceAttachmentRepository.GetAttachmentByInvoiceIdAsync(invoiceId);
            return _mapper.Map<IEnumerable<InvoiceAttachmentDto>>(attachments);
        }

        public async Task<(Stream? file, string fileName)> DownloadAttachmentAsync(Guid id)
        {
            var attachment = await _invoiceAttachmentRepository.GetAttachmentByIdAsync(id);
            if (attachment is null)
            {
                throw new AttachmentNotFoundException("Attachment not found");
            }

            BlobStorageFileResult? fileResult = null;

            try
            {
                fileResult = JsonSerializer.Deserialize<BlobStorageFileResult>(attachment.File, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                    ?? throw new AttachmentDeserializingException("Error deserializing exception");
            }
            catch (Exception e)
            {
                throw new AttachmentDeserializingException("Error deserializing exception");
            }

            var file = await _blobStorageServiceFactory.DownloadFile(fileResult.UniqueFileName);
            return (file, fileResult.UniqueFileName);
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
                var toPay = invoice.InvoiceDetails.Sum(x => x.Quantity * PaymentCalculator.GetNetPrice(x, addons));
                var paymentDetail = new ClientPaymentsDetails
                {
                    InvoiceId = invoice.Id,
                    PendingPayment = toPay - invoice.Payment,
                    Date = DateTime.Now,
                };
                paymentDetail.CreatedBy = _userContextService.GetCurrentUserId();
                paymentDetail.CreatedAt = DateTimeOffset.UtcNow;
                await _receivableAccountRepository.AddAsync(paymentDetail);
            }
        }

        private void UpdatePricesByAddons(List<InvoiceDetailUpdateDto> invoiceDetail, Dictionary<Guid, List<RawAddons>> addons)
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
