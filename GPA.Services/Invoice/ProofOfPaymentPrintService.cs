using GPA.Data.General;
using GPA.Data.Invoice;
using GPA.Entities.General;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Invoice;
using GPA.Services.Security;
using GPA.Utils;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Globalization;

namespace GPA.Business.Services.Invoice
{
    public interface IProofOfPaymentPrintService
    {
        Task<Stream> PrintInvoice(Guid invoiceId);
    }

    public class ProofOfPaymentPrintService : InvoicePrintServiceBase, IProofOfPaymentPrintService
    {
        private readonly IInvoicePrintRepository _invoicePrintRepository;
        private readonly IUserContextService _userContextService;
        private readonly IClientRepository _clientRepository;
        private readonly IPrintRepository _printRepository;

        public ProofOfPaymentPrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IClientRepository clientRepository,
            IUserContextService userContextService,
            IPrintRepository printRepository
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _clientRepository = clientRepository;
            _printRepository = printRepository;
        }

        public async Task<Stream> PrintInvoice(Guid invoiceId)
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
                RawInvoiceDetailsAddon = invoiceDetailsAddon[detail.Id]
            }));

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

            WriteFileLine(gfx, "RECIBO DE PAGO", fontBold, XBrushes.Black, new XRect(0, y + 7, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, FormatDate(DateTime.Now), font, XBrushes.Black, new XRect(0, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, "No: 899", font, XBrushes.Black, new XRect(0, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 15, widthWithMargin, 20), XStringFormats.Center, ref y);

            WriteFileLine(gfx, "Recibí de:", fontBold, XBrushes.Black, new XRect(6, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, $"{invoicePrintData.Client.Name} {invoicePrintData.Client.LastName}", font, XBrushes.Black, new XRect(63, y, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 10, widthWithMargin, 20), XStringFormats.Center, ref y);


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

            WriteFileLine(gfx, "Monto:", fontBold, XBrushes.Black, new XRect(6, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, totalPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(120, y, widthWithMargin, 20), XStringFormats.TopLeft, ref y);


            foreach (var item in accumulatedAddons)
            {
                WriteFileLine(gfx, item.Key, fontBold, XBrushes.Black, new XRect(6, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
                WriteFileLine(gfx, item.Value.ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(120, y, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            }

            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 10, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, "Total", fontBold, XBrushes.Black, new XRect(6, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, (totalPrice + totalAddon).ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(120, y, widthWithMargin, 20), XStringFormats.TopLeft, ref y);


            WriteFileLine(gfx, "Concepto:", fontBold, XBrushes.Black, new XRect(6, y + 50, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, "CAD - PAGO RECARGO 2018-19", font, XBrushes.Black, new XRect(15, y + 12, widthWithMargin, 20), XStringFormats.TopLeft, ref y);

            WriteFileLine(gfx, "________________________________", font, XBrushes.Black, new XRect(1, y + 50, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, invoicePrintData.Signer, font, XBrushes.Black, new XRect(1, y + 10, widthWithMargin, 20), XStringFormats.Center, ref y);

            MemoryStream stream = new();
            document.Save(stream, false);
            stream.Position = 0;
            return stream;
        }
    }
}
