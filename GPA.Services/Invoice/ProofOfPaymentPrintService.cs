using DinkToPdf;
using GPA.Data.General;
using GPA.Data.Inventory;
using GPA.Data.Invoice;
using GPA.Entities.General;
using GPA.Entities.Report;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Invoice;
using GPA.Services.Report;
using GPA.Services.Security;
using GPA.Utils;
using GPA.Utils.Caching;
using System.Globalization;
using System.Text;

namespace GPA.Business.Services.Invoice
{
    public interface IProofOfPaymentPrintService
    {
        Task<byte[]> PrintInvoice(Guid invoiceId);
    }

    public class ProofOfPaymentPrintService : InvoicePrintServiceBase, IProofOfPaymentPrintService
    {
        private readonly IInvoicePrintRepository _invoicePrintRepository;
        private readonly IUserContextService _userContextService;
        private readonly IClientRepository _clientRepository;
        private readonly IPrintRepository _printRepository;
        private readonly IReportPdfBase _reportPdfBase;
        private readonly IReportTemplateRepository _reportTemplateRepository;
        private readonly IGenericCache<ReportTemplate> _cache;
        private readonly IGenericCache<string> _logoCache;

        public ProofOfPaymentPrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IClientRepository clientRepository,
            IUserContextService userContextService,
            IPrintRepository printRepository,
            IReportPdfBase reportPdfBase,
            IReportTemplateRepository reportTemplateRepository,
            IGenericCache<ReportTemplate> cache,
            IGenericCache<string> logoCache
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _clientRepository = clientRepository;
            _printRepository = printRepository;
            _reportPdfBase = reportPdfBase;
            _reportTemplateRepository = reportTemplateRepository;
            _cache = cache;
            _logoCache = logoCache;
        }

        public async Task<byte[]> PrintInvoice(Guid invoiceId)
        {
            var invoice = await _invoicePrintRepository.GetInvoiceById(invoiceId);
            var client = await _clientRepository.GetClientAsync(invoice.ClientId);
            var invoiceDetails = await _invoicePrintRepository.GetInvoiceDetailByInvoiceId(invoiceId);
            var invoiceDetailsAddon = await _invoicePrintRepository.GetInvoiceDetailAddonByInvoiceId(invoiceId);

            if (invoice is null)
            {
                throw new InvalidOperationException("La factura no existe");
            }

            if (invoice.PaymentStatus == (byte)PaymentStatus.Pending)
            {
                throw new InvalidOperationException("La factura no se ha pagado, no puede imprimir el comprobante de pago");
            }

            var invoicePrintData = new InvoicePrintData()
            {
                Hour = DateTime.Now.ToString("hh:mm:ss tt", new CultureInfo("es-ES")),
                Date = DateTime.Now.ToString("MM/dd/yyyy", new CultureInfo("es-ES")),
                Invoice = invoice,
                Client = client,
                User = _userContextService.GetCurrentUserName()
            };

            invoicePrintData.InvoicePrintDetails.AddRange(invoiceDetails.Select(detail => new InvoicePrintDetails
            {
                RawInvoiceDetails = detail,
                RawInvoiceDetailsAddon = invoiceDetailsAddon.ContainsKey(detail.Id) ? invoiceDetailsAddon[detail.Id] : []
            }));

            var printConfiguration = await _printRepository.GetCurrentPrintInformationAsync();

            if (printConfiguration is null)
            {
                throw new InvalidOperationException("La configuración de impresión no existe");
            }

            invoicePrintData.SetParams(printConfiguration);

            return await GenerateInvoice(invoicePrintData);
        }

        public async Task<byte[]> GenerateInvoice(InvoicePrintData invoicePrintData)
        {
            Dictionary<string, decimal> accumulatedAddons = new();

            var template = await GetTemplate();
            var htmlContent = template.Template;

            string? logo = string.Empty;
            if (invoicePrintData.CompanyLogo is not null)
            {
                logo = await _logoCache.GetOrCreate(CacheType.CompanyLogo, invoicePrintData.CompanyLogo, async () =>
                {
                    return await GetLogoAsDataUri(invoicePrintData.CompanyLogo);
                });
            }

            var totalPrice = 0.0M;
            var totalAddon = 0.0M;
            foreach (var item in invoicePrintData.InvoicePrintDetails)
            {
                totalPrice += (item.RawInvoiceDetails.Price * item.RawInvoiceDetails.Quantity);
                foreach (var detailsAddon in item.RawInvoiceDetailsAddon)
                {
                    var addonComputedValue = Math.Round(AddonCalculator.CalculateAmountOrPercentage(detailsAddon, item.RawInvoiceDetails.Price * item.RawInvoiceDetails.Quantity), 2);
                    addonComputedValue = detailsAddon.IsDiscount ? -addonComputedValue : addonComputedValue;
                    totalAddon += addonComputedValue;
                    AddAccumulatedAddon(accumulatedAddons, detailsAddon.Concept, addonComputedValue);
                }
            }

            var amounts = new StringBuilder();
            amounts.Append($"""
                <tr>
                    <th>Monto:</th>
                    <td>{totalPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</td>
                </tr>
                """);

            foreach (var item in accumulatedAddons)
            {
                amounts.Append($"""
                      <tr>
                        <th>{item.Key}</th>
                        <td>{item.Value.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</td>
                      </tr>
                """);
            }

            htmlContent = htmlContent
                .Replace("{Company}", invoicePrintData.CompanyName)
                .Replace("{Document}", $"{invoicePrintData.CompanyDocumentPrefix} {invoicePrintData.CompanyDocument}")
                .Replace("{Tel}", $"{invoicePrintData.CompanyPhonePrefix} {invoicePrintData.CompanyPhone}")
                .Replace("{Mail}", $"{invoicePrintData.CompanyEmail}")
                .Replace("{Address}", $"{invoicePrintData.CompanyAddress}")
                .Replace("{Date}", $"{FormatDate(DateTime.Now)}")
                .Replace("{Client}", $"{invoicePrintData.Client.Name} {invoicePrintData.Client.LastName}")
                .Replace("{Amounts}", amounts.ToString())
                .Replace("{Total}", (totalPrice + totalAddon).ToString("C2", CultureInfo.GetCultureInfo("en-US")))
                .Replace("{Concept}", $"Factura {invoicePrintData.Invoice.Code}")
                .Replace("{Signer}", invoicePrintData.Signer)
                .Replace("{Logo}", logo);

            var width = $"{template.Width}mm" ?? "65mm";
            var height = $"{template.Height}mm" ?? "297mm";
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = new PechkinPaperSize(width, height),
                Margins = new MarginSettings(0, 0, 0, 0)
            };

            return _reportPdfBase.GeneratePdf(htmlContent, settings: globalSettings);
        }

        private async Task<ReportTemplate> GetTemplate()
        {
            var template = await _cache.GetOrCreate(CacheType.ReportTemplates, TemplateConstants.PROOF_OF_PAYMENT_TEMPLATE, async () =>
            {
                return await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.PROOF_OF_PAYMENT_TEMPLATE);
            });

            if (template == null || template.Template is null)
            {
                throw new Exception("El template de impresión no exíste");
            }
            return template;
        }
    }
}
