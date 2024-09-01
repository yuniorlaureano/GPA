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
    public interface IInvoicePrintService
    {
        Task<Stream> PrintInvoice(Guid invoiceId);
    }

    public class InvoicePrintService : InvoicePrintServiceBase, IInvoicePrintService
    {
        private readonly IInvoicePrintRepository _invoicePrintRepository;
        private readonly IUserContextService _userContextService;
        private readonly IPrintRepository _printRepository;

        public InvoicePrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IUserContextService userContextService,
            IPrintRepository printRepository
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _printRepository = printRepository;
        }

        public async Task<Stream> PrintInvoice(Guid invoiceId)
        {
            var invoice = await _invoicePrintRepository.GetInvoiceById(invoiceId);
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
            var separator = "----------------------------------------------";
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
            WriteFileLine(gfx, invoicePrintData.CompanyAddress, font, XBrushes.Black, new XRect(0, y + 12, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 7, widthWithMargin, 20), XStringFormats.Center, ref y);


            WriteFileLine(gfx, invoicePrintData.User, font, XBrushes.Black, new XRect(8, y + 10, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, invoicePrintData.Hour, font, XBrushes.Black, new XRect(-6, y + 11, widthWithMargin, 20), XStringFormats.TopRight, ref y);
            WriteFileLine(gfx, invoicePrintData.Date, font, XBrushes.Black, new XRect(-6, y + 11, widthWithMargin, 20), XStringFormats.TopRight, ref y);
            WriteFileLine(gfx, "Hora.:", font, XBrushes.Black, new XRect(8, y - 11, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, "Fecha.:", font, XBrushes.Black, new XRect(8, y + 11, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 7, widthWithMargin, 20), XStringFormats.Center, ref y);


            WriteFileLine(gfx, "PRODUCTOS", font, XBrushes.Black, new XRect(6, y + 15, widthWithMargin, 20), XStringFormats.TopLeft, ref y);


            // Add items
            var totalPrice = 0.0M;
            var itemsCount = invoicePrintData.InvoicePrintDetails.Count;
            foreach (var item in invoicePrintData.InvoicePrintDetails)
            {
                totalPrice += (item.RawInvoiceDetails.Price * item.RawInvoiceDetails.Quantity);

                WriteFileLine(gfx, ShortenName(item.RawInvoiceDetails.ProductName), font, XBrushes.Black, new XRect(6, y + 13, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
                WriteFileLine(gfx, item.RawInvoiceDetails.Price.ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(-6, y, widthWithMargin, 20), XStringFormats.TopRight, ref y);
                WriteFileLine(gfx, "CANT: " + item.RawInvoiceDetails.Quantity.ToString(), font, XBrushes.Black, new XRect(6, y + 13, widthWithMargin, 20), XStringFormats.TopLeft, ref y);

                foreach (var detailsAddon in item.RawInvoiceDetailsAddon)
                {
                    var addonComputedValue = Math.Round(AddonCalculator.CalculateAmountOrPercentage(detailsAddon, item.RawInvoiceDetails.Price * item.RawInvoiceDetails.Quantity), 2);
                    addonComputedValue = detailsAddon.IsDiscount ? -addonComputedValue : addonComputedValue;
                    WriteFileLine(gfx, $"{detailsAddon.Concept}: {addonComputedValue}", font, XBrushes.Black, new XRect(15, y + 13, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
                    AddAccumulatedAddon(accumulatedAddons, detailsAddon.Concept, addonComputedValue);
                }
                y += 10;
            }

            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 10, widthWithMargin, 20), XStringFormats.Center, ref y);

            WriteFileLine(gfx, "TOTAL.:", font, XBrushes.Black, new XRect(6, y + 10, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, "Precio.:", font, XBrushes.Black, new XRect(15, y + 10, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, totalPrice.ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(-6, y, widthWithMargin, 20), XStringFormats.TopRight, ref y);

            foreach (var item in accumulatedAddons)
            {
                WriteFileLine(gfx, item.Key, font, XBrushes.Black, new XRect(15, y + 15, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
                WriteFileLine(gfx, item.Value.ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(-6, y, widthWithMargin, 20), XStringFormats.TopRight, ref y);
            }

            var total = totalPrice + accumulatedAddons.Sum(x => x.Value);
            WriteFileLine(gfx, separator, font, XBrushes.Black, new XRect(1, y + 10, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, total.ToString("C2", CultureInfo.GetCultureInfo("en-US")), font, XBrushes.Black, new XRect(-6, y + 10, widthWithMargin, 20), XStringFormats.TopRight, ref y);

            /// Generate QR Code
            var qrCodeImage = GenerateQRCode(invoicePrintData.Invoice.Id.ToString());

            // Convert Bitmap to XImage
            XImage qrCodeXImage = LoadImage(qrCodeImage);

            // Draw QR Code
            // Insert QR Code
            gfx.DrawImage(qrCodeXImage, 60, y + 10, 100, 100);

            MemoryStream stream = new();
            document.Save(stream, false);
            stream.Position = 0;
            return stream;
        }
    }
}
