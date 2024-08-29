using GPA.Data.Invoice;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
using GPA.Utils;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using QRCoder;

namespace GPA.Business.Services.Invoice
{
    public interface IProofOfPaymentPrintService
    {
        Task<Stream> PrintInvoice(Guid invoiceId);
    }

    public class ProofOfPaymentPrintService : IProofOfPaymentPrintService
    {
        private readonly IInvoicePrintRepository _invoicePrintRepository;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly IUserContextService _userContextService;

        public ProofOfPaymentPrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IUserContextService userContextService
            )
        {
            _invoicePrintRepository = invoicePrintRepository;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _userContextService = userContextService;
        }

        public async Task<Stream> PrintInvoice(Guid invoiceId)
        {
            var invoice = await _invoicePrintRepository.GetInvoiceById(invoiceId);
            var invoiceDetails = await _invoicePrintRepository.GetInvoiceDetailByInvoiceId(invoiceId);
            var invoiceDetailsAddon = await _invoicePrintRepository.GetInvoiceDetailAddonByInvoiceId(invoiceId);

            var invoicePrintData = new InvoicePrintData()
            {
                Hour = DateTime.Now.ToString("hh:mm:ss tt"),
                Date = DateTime.Now.ToString("MM/dd/yyyy"),
                InvoiceId = invoiceId,
                User = _userContextService.GetCurrentUserName()
            };

            invoicePrintData.InvoicePrintDetails.AddRange(invoiceDetails.Select(detail => new InvoicePrintDetails
            {
                RawInvoiceDetails = detail,
                RawInvoiceDetailsAddon = invoiceDetailsAddon[detail.Id]
            }));

            var printConfiguration = await _invoicePrintRepository.GetPrintConfiguration();

            if (printConfiguration is null)
            {
                throw new InvalidOperationException("The configuration for printing is not present in db");
            }

            invoicePrintData.SetParams(printConfiguration);

            return await GenerateInvoice(invoicePrintData);
        }

        public async Task<Stream> GenerateInvoice(InvoicePrintData invoicePrintData)
        {
            Dictionary<string, decimal> accumulatedAddons = new();
            var separtor = "----------------------------------------------";
            PdfDocument document = new PdfDocument();
            document.Info.Title = invoicePrintData.CompanyDocument;

            PdfPage page = document.AddPage();
            page.Width = XUnit.FromMillimeter(80);
            XGraphics gfx = XGraphics.FromPdfPage(page);

            var widthWithMargin = XUnit.FromMillimeter(75);
            //set logo
            var logo = await _blobStorageServiceFactory.DownloadFile(invoicePrintData.CompanyLogo);
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
            WriteFileLine(gfx, invoicePrintData.CompanyName, fontBold, XBrushes.Black, new XRect(0, y, widthWithMargin, 20), XStringFormats.TopCenter, ref y);
            WriteFileLine(gfx, invoicePrintData.CompanyDocument, font, XBrushes.Black, new XRect(0, y + 20, widthWithMargin, 20), XStringFormats.TopCenter, ref y);
            WriteFileLine(gfx, invoicePrintData.CompanyPhone, font, XBrushes.Black, new XRect(0, y + 15, widthWithMargin, 20), XStringFormats.TopCenter, ref y);
            WriteFileLine(gfx, invoicePrintData.CompanyEmail, font, XBrushes.Black, new XRect(0, y + 15, widthWithMargin, 20), XStringFormats.TopCenter, ref y);
            WriteFileLine(gfx, invoicePrintData.CompanyAddress, font, XBrushes.Black, new XRect(0, y + 12, widthWithMargin, 20), XStringFormats.TopCenter, ref y);
            WriteFileLine(gfx, separtor, font, XBrushes.Black, new XRect(6, y + 7, widthWithMargin, 20), XStringFormats.Center, ref y);


            WriteFileLine(gfx, invoicePrintData.User, font, XBrushes.Black, new XRect(8, y + 10, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, invoicePrintData.Hour, font, XBrushes.Black, new XRect(0, y + 11, widthWithMargin, 20), XStringFormats.TopRight, ref y);
            WriteFileLine(gfx, invoicePrintData.Date, font, XBrushes.Black, new XRect(0, y + 11, widthWithMargin, 20), XStringFormats.TopRight, ref y);
            WriteFileLine(gfx, "Hora.:", font, XBrushes.Black, new XRect(8, y - 11, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, "Fecha.:", font, XBrushes.Black, new XRect(8, y + 11, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, separtor, font, XBrushes.Black, new XRect(6, y + 7, widthWithMargin, 20), XStringFormats.Center, ref y);


            WriteFileLine(gfx, "PRODUCTOS", font, XBrushes.Black, new XRect(6, y + 15, widthWithMargin, 20), XStringFormats.TopLeft, ref y);


            // Add items
            var totalPrice = 0.0M;
            var itemsCount = invoicePrintData.InvoicePrintDetails.Count;
            foreach (var item in invoicePrintData.InvoicePrintDetails)
            {
                totalPrice += item.RawInvoiceDetails.Price;

                WriteFileLine(gfx, ShortenName(item.RawInvoiceDetails.ProductName), font, XBrushes.Black, new XRect(6, y + 13, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
                WriteFileLine(gfx, item.RawInvoiceDetails.Price.ToString("C"), font, XBrushes.Black, new XRect(6, y, widthWithMargin, 20), XStringFormats.TopRight, ref y);
                WriteFileLine(gfx, "CANT: " + item.RawInvoiceDetails.Quantity.ToString(), font, XBrushes.Black, new XRect(6, y + 13, widthWithMargin, 20), XStringFormats.TopLeft, ref y);

                foreach (var detailsAddon in item.RawInvoiceDetailsAddon)
                {
                    var addonComputedValue = Math.Round(AddonCalculator.CalculateAmountOrPercentage(detailsAddon, item.RawInvoiceDetails.Price), 2);
                    addonComputedValue = detailsAddon.IsDiscount ? -addonComputedValue : addonComputedValue;
                    WriteFileLine(gfx, $"{detailsAddon.Concept}: {addonComputedValue}", font, XBrushes.Black, new XRect(15, y + 13, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
                    AddAccumulatedAddon(accumulatedAddons, detailsAddon.Concept, addonComputedValue);
                }
                y += 10;
            }

            WriteFileLine(gfx, separtor, font, XBrushes.Black, new XRect(6, y + 10, widthWithMargin, 20), XStringFormats.Center, ref y);

            WriteFileLine(gfx, "TOTAL.:", font, XBrushes.Black, new XRect(6, y + 10, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, "Precio.:", font, XBrushes.Black, new XRect(15, y + 10, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
            WriteFileLine(gfx, totalPrice.ToString("C"), font, XBrushes.Black, new XRect(6, y, widthWithMargin, 20), XStringFormats.TopRight, ref y);

            foreach (var item in accumulatedAddons)
            {
                WriteFileLine(gfx, item.Key, font, XBrushes.Black, new XRect(15, y + 15, widthWithMargin, 20), XStringFormats.TopLeft, ref y);
                WriteFileLine(gfx, item.Value.ToString("C"), font, XBrushes.Black, new XRect(6, y, widthWithMargin, 20), XStringFormats.TopRight, ref y);
            }

            var total = totalPrice + accumulatedAddons.Sum(x => x.Value);
            WriteFileLine(gfx, separtor, font, XBrushes.Black, new XRect(6, y + 10, widthWithMargin, 20), XStringFormats.Center, ref y);
            WriteFileLine(gfx, total.ToString("C"), font, XBrushes.Black, new XRect(6, y + 10, widthWithMargin, 20), XStringFormats.TopRight, ref y);

            /// Generate QR Code
            string qrCodeText = "https://example.com/invoice/12345";
            var qrCodeImage = GenerateQRCode(qrCodeText);

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

        private void WriteFileLine(XGraphics gfx, string data, XFont font, XSolidBrush brushes, XRect xRect, XStringFormat position, ref double y)
        {
            y = xRect.Y;
            gfx.DrawString(data, font, brushes, xRect, position);
        }

        private string ShortenName(string name)
        {
            if (name.Length > 15)
            {
                return name.Substring(0, 15);
            }
            return name;
        }

        private XImage LoadImage(byte[] bitmapImage)
        {
            XImage qrCodeXImage;
            using (MemoryStream ms = new MemoryStream(bitmapImage, 0, bitmapImage.Length, writable: false, publiclyVisible: true))
            {
                ms.Position = 0;
                qrCodeXImage = XImage.FromStream(ms);
            }

            return qrCodeXImage;
        }

        private void AddAccumulatedAddon(Dictionary<string, decimal> accumulatedAddons, string concept, decimal value)
        {
            if (accumulatedAddons.ContainsKey(concept))
            {
                accumulatedAddons[concept] += value;
            }
            else
            {
                accumulatedAddons.Add(concept, value);
            }
        }

        private byte[] GenerateQRCode(string text)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new BitmapByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }
    }
}
