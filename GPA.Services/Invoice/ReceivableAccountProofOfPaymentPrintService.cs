using DinkToPdf;
using GPA.Data.General;
using GPA.Data.Inventory;
using GPA.Data.Invoice;
using GPA.Entities.Report;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Invoice;
using GPA.Services.Report;
using GPA.Services.Security;
using GPA.Utils;
using System.Globalization;

namespace GPA.Business.Services.Invoice
{
    public interface IReceivableAccountProofOfPaymentPrintService
    {
        Task<byte[]> PrintInvoice(Guid invoiceId);
    }

    public class ReceivableAccountProofOfPaymentPrintService : InvoicePrintServiceBase, IReceivableAccountProofOfPaymentPrintService
    {
        private readonly IInvoicePrintRepository _invoicePrintRepository;
        private readonly IUserContextService _userContextService;
        private readonly IClientRepository _clientRepository;
        private readonly IPrintRepository _printRepository;
        private readonly IReportPdfBase _reportPdfBase;
        private readonly IReceivableAccountRepository _receivableAccountRepository;
        private readonly IReportTemplateRepository _reportTemplateRepository;


        public ReceivableAccountProofOfPaymentPrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IClientRepository clientRepository,
            IUserContextService userContextService,
            IPrintRepository printRepository,
            IReportPdfBase reportPdfBase,
            IReceivableAccountRepository receivableAccountRepository,
            IReportTemplateRepository reportTemplateRepository
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _clientRepository = clientRepository;
            _printRepository = printRepository;
            _reportPdfBase = reportPdfBase;
            _receivableAccountRepository = receivableAccountRepository;
            _reportTemplateRepository = reportTemplateRepository;

        }

        public async Task<byte[]> PrintInvoice(Guid receivableId)
        {

            var receivableAccount = await _receivableAccountRepository.GetByIdAsync(query => query, x => x.Id == receivableId);
            var invoice = await _invoicePrintRepository.GetInvoiceById(receivableAccount.InvoiceId);
            var client = await _clientRepository.GetClientAsync(invoice.ClientId);

            if (invoice is null)
            {
                throw new InvalidOperationException("La factura no existe");
            }

            var invoicePrintData = new InvoicePrintData()
            {
                Hour = DateTime.Now.ToString("hh:mm:ss tt", new CultureInfo("es-ES")),
                Date = DateTime.Now.ToString("MM/dd/yyyy", new CultureInfo("es-ES")),
                Invoice = invoice,
                Client = client,
                ReceivableAccounts = receivableAccount,
                User = _userContextService.GetCurrentUserName()
            };

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
            var template = await GetTemplate();
            var htmlContent = template.Template;
            var logo = await GetLogoAsDataUri(invoicePrintData.CompanyLogo);
            htmlContent = htmlContent
                .Replace("{Company}", invoicePrintData.CompanyName)
                .Replace("{Document}", $"{invoicePrintData.CompanyDocumentPrefix} {invoicePrintData.CompanyDocument}")
                .Replace("{Tel}", $"{invoicePrintData.CompanyPhonePrefix} {invoicePrintData.CompanyPhone}")
                .Replace("{Mail}", $"{invoicePrintData.CompanyEmail}")
                .Replace("{Address}", $"{invoicePrintData.CompanyAddress}")
                .Replace("{Date}", $"{FormatDate(DateTime.Now)}")
                .Replace("{Client}", $"{invoicePrintData.Client.Name} {invoicePrintData.Client.LastName}")
                .Replace("{PaymentDate}", invoicePrintData.ReceivableAccounts.Date.ToString("MMM d yyyy"))
                .Replace("{Paid}", invoicePrintData.ReceivableAccounts.Payment.ToString("C2", CultureInfo.GetCultureInfo("en-US")))
                .Replace("{Pending}", (invoicePrintData.ReceivableAccounts.PendingPayment - invoicePrintData.ReceivableAccounts.Payment).ToString("C2", CultureInfo.GetCultureInfo("en-US")))
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
            var template = await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.RECEIVABLE_PROOF_OF_PAYMENT_TEMPLATE);
            if (template == null || template.Template is null)
            {
                throw new Exception("El template de impresión no exíste");
            }
            return template;
        }
    }
}
