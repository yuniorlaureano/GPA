using GPA.Data.General;
using GPA.Data.Invoice;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Invoice;
using GPA.Services.Security;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Globalization;

namespace GPA.Business.Services.Invoice
{
    public interface IReceivableAccountProofOfPaymentPrintService
    {
        Task<Stream> PrintInvoice(Guid invoiceId);
    }

    public class ReceivableAccountProofOfPaymentPrintService : InvoicePrintServiceBase, IReceivableAccountProofOfPaymentPrintService
    {
        private readonly IInvoicePrintRepository _invoicePrintRepository;
        private readonly IUserContextService _userContextService;
        private readonly IClientRepository _clientRepository;
        private readonly IPrintRepository _printRepository;

        private readonly IReceivableAccountRepository _receivableAccountRepository;

        public ReceivableAccountProofOfPaymentPrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IClientRepository clientRepository,
            IUserContextService userContextService,
            IPrintRepository printRepository,

            IReceivableAccountRepository receivableAccountRepository
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _clientRepository = clientRepository;
            _printRepository = printRepository;


            _receivableAccountRepository = receivableAccountRepository;
        }

        public async Task<Stream> PrintInvoice(Guid receivableId)
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

        public async Task<Stream> GenerateInvoice(InvoicePrintData invoicePrintData)
        {
            Dictionary<string, decimal> accumulatedAddons = new();
            var separator = "------------------------------------------------";
            PdfDocument document = new PdfDocument();
            document.Info.Title = invoicePrintData.CompanyDocument;

            PdfPage page = document.AddPage();
            page.Width = XUnit.FromMillimeter(80);
            XGraphics gfx = XGraphics.FromPdfPage(page);

            var widthWithMargin = XUnit.FromMillimeter(80);
            //set logo
            using var logo = await GetLogo(invoicePrintData.CompanyLogo);
            var distanceFormImageHeader = 0;
            if (logo is not null)
            {
                distanceFormImageHeader = 50;
                XImage logoXImage = XImage.FromStream(logo);
                gfx.DrawImage(logoXImage, new XRect(60, 0, 100, 100));
            }

            double y = 50 + distanceFormImageHeader;
            //write title
            XFont fontBold = new("Verdana", 10, XFontStyleEx.Bold);
            XFont font = new("Verdana", 10, XFontStyleEx.Regular);
            WriteFileLine(gfx, invoicePrintData.CompanyName, fontBold, XBrushes.Black, new XRect(0, y, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, invoicePrintData.CompanyDocument, font, XBrushes.Black, new XRect(0, y + 20, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, invoicePrintData.CompanyPhone, font, XBrushes.Black, new XRect(0, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, invoicePrintData.CompanyEmail, font, XBrushes.Black, new XRect(0, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, invoicePrintData.CompanyAddress, font, XBrushes.Black, new XRect(0, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);

            WriteFileLine(gfx, "RECIBO DE CUENTA POR PAGAR", fontBold, XBrushes.Black, new XRect(0, y + 7, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, FormatDate(DateTime.Now), font, XBrushes.Black, new XRect(0, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);

            WriteFileLine(gfx, "Recibí de:", fontBold, XBrushes.Black, new XRect(6, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, $"{invoicePrintData.Client.Name} {invoicePrintData.Client.LastName}", font, XBrushes.Black, new XRect(63, y, widthWithMargin, 20), XStringFormats.TopLeft, ref y);

            WriteFileLine(gfx, "En fecha:", fontBold, XBrushes.Black, new XRect(6, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, invoicePrintData.ReceivableAccounts.Date.ToString("MMM d yyyy"), font, XBrushes.Black, new XRect(120, y, widthWithMargin, 20), XStringFormats.TopLeft, ref y);

            WriteFileLine(gfx, "Monto:", fontBold, XBrushes.Black, new XRect(6, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, invoicePrintData.ReceivableAccounts.Payment.ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(120, y, widthWithMargin, 20), XStringFormats.TopLeft, ref y);

            WriteFileLine(gfx, "Pendiente:", fontBold, XBrushes.Black, new XRect(6, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, invoicePrintData.ReceivableAccounts.PendingPayment.ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(120, y, widthWithMargin, 20), XStringFormats.TopLeft, ref y);

            WriteFileLine(gfx, "Concepto:", fontBold, XBrushes.Black, new XRect(6, y + 50, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, "PAGO CUENTAS POR COBRAR", font, XBrushes.Black, new XRect(15, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);

            WriteFileLine(gfx, "________________________________", font, XBrushes.Black, new XRect(1, y + 50, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, invoicePrintData.Signer, font, XBrushes.Black, new XRect(1, y + 10, widthWithMargin, 20), XStringFormats.Center, ref y);

            MemoryStream stream = new();
            document.Save(stream, false);
            stream.Position = 0;
            return stream;
        }
    }
}
