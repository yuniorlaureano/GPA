using DinkToPdf;
using GPA.Data.General;
using GPA.Data.Inventory;
using GPA.Data.Invoice;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Invoice;
using GPA.Services.Report;
using GPA.Services.Security;
using GPA.Utils;
using System.Globalization;
using System.Text;

namespace GPA.Business.Services.Invoice
{
    public interface IInvoicePrintService
    {
        Task<byte[]> PrintInvoice(Guid invoiceId);
    }

    public class InvoicePrintService : InvoicePrintServiceBase, IInvoicePrintService
    {
        private readonly IInvoicePrintRepository _invoicePrintRepository;
        private readonly IUserContextService _userContextService;
        private readonly IPrintRepository _printRepository;
        private readonly IReportPdfBase _reportPdfBase;
        private readonly IReportTemplateRepository _reportTemplateRepository;

        public InvoicePrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IUserContextService userContextService,
            IPrintRepository printRepository,
            IReportPdfBase reportPdfBase,
            IReportTemplateRepository reportTemplateRepository
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _printRepository = printRepository;
            _reportPdfBase = reportPdfBase;
            _reportTemplateRepository = reportTemplateRepository;
        }

        public async Task<byte[]> PrintInvoice(Guid invoiceId)
        {
            var invoice = await _invoicePrintRepository.GetInvoiceById(invoiceId);
            var invoiceDetails = await _invoicePrintRepository.GetInvoiceDetailByInvoiceId(invoiceId);
            var invoiceDetailsAddon = await _invoicePrintRepository.GetInvoiceDetailAddonByInvoiceId(invoiceId);

            if (invoice is null)
            {
                throw new InvalidOperationException("La factura no existe");
            }

            var invoicePrintData = new InvoicePrintData()
            {
                Hour = DateTime.Now.ToString("hh:mm:ss tt", new CultureInfo("es-ES")),
                Date = DateTime.Now.ToString("MM/dd/yyyy", new CultureInfo("es-ES")),
                Invoice = invoice,
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
            var logo = await GetLogoAsDataUri(invoicePrintData.CompanyLogo);
            var qrCodeImage = ConvertQrCodeToDataUri(GenerateQRCode(invoicePrintData.Invoice.Id.ToString()));

            //_logger.LogInformation("El usuario '{UserId}' está generando el reporte ciclos de inventario", _userContextService.GetCurrentUserId());
            Dictionary<string, decimal> accumulatedAddons = new();

            var htmlContent = await GetTemplate();

            var products = new StringBuilder();
            var totalPrice = 0.0M;
            var itemsCount = invoicePrintData.InvoicePrintDetails.Count;
            foreach (var item in invoicePrintData.InvoicePrintDetails)
            {
                totalPrice += (item.RawInvoiceDetails.Price * item.RawInvoiceDetails.Quantity);

                products.Append($"""
                    <tr>
                      <td>{ShortenName(item.RawInvoiceDetails.ProductName)}</td>
                      <td>{item.RawInvoiceDetails.Price.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</td>
                    </tr>
                    <tr style="display: block; padding-left: 15px">
                      <td>Cant:</td>
                      <td>x {item.RawInvoiceDetails.Quantity}</td>
                    </tr>
                    """);
                foreach (var detailsAddon in item.RawInvoiceDetailsAddon)
                {
                    var addonComputedValue = Math.Round(AddonCalculator.CalculateAmountOrPercentage(detailsAddon, item.RawInvoiceDetails.Price * item.RawInvoiceDetails.Quantity), 2);
                    addonComputedValue = detailsAddon.IsDiscount ? -addonComputedValue : addonComputedValue;

                    products.Append($"""
                        <tr style="display: block; padding-left: 15px">
                          <td>{detailsAddon.Concept}</td>
                          <td>{addonComputedValue}</td>
                        </tr>
                    """);

                    AddAccumulatedAddon(accumulatedAddons, detailsAddon.Concept, addonComputedValue);
                }
            }

            var totals = new StringBuilder();
            totals.Append($"""
                <tr>
                  <td>Precio.:</td>
                  <td>{totalPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</td>
                </tr>
            """);

            foreach (var item in accumulatedAddons)
            {
                totals.Append($"""
                <tr>
                  <td>{item.Key}</td>
                  <td>{item.Value.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</td>
                </tr>
                """);
            }

            var total = totalPrice + accumulatedAddons.Sum(x => x.Value);

            htmlContent = htmlContent
                .Replace("{Company}", invoicePrintData.CompanyName)
                .Replace("{Document}", $"{invoicePrintData.CompanyDocumentPrefix} {invoicePrintData.CompanyDocument}")
                .Replace("{Tel}", $"{invoicePrintData.CompanyPhonePrefix} {invoicePrintData.CompanyPhone}")
                .Replace("{Mail}", $"{invoicePrintData.CompanyEmail}")
                .Replace("{Address}", $"{invoicePrintData.CompanyAddress}")
                .Replace("{User}", $"{invoicePrintData.User}")
                .Replace("{Hour}", $"{invoicePrintData.Hour}")
                .Replace("{Date}", $"{invoicePrintData.Date}")
                .Replace("{Products}", products.ToString())
                .Replace("{Totals}", totals.ToString())
                .Replace("{TotalPrice}", total.ToString("C2", CultureInfo.GetCultureInfo("en-US")))
                .Replace("{Logo}", logo)
                .Replace("{QrCode}", qrCodeImage);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = new PechkinPaperSize("65mm", "297mm"),
                Margins = new MarginSettings(0, 0, 0, 0)
            };

            return _reportPdfBase.GeneratePdf(htmlContent, settings: globalSettings);
        }

        private async Task<string> GetTemplate()
        {
            var template = await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.INVOICE_TEMPLATE);
            if (template == null || template.Template is null)
            {
                throw new Exception("El template de impresión no exíste");
            }
            return template.Template;
        }
    }
}
