using GPA.Data.Invoice;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
using GPA.Utils;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using QRCoder;
using System;

namespace GPA.Business.Services.Invoice
{
    public interface IInvoicePrintService
    {
        Task<Stream> PrintInvoice(Guid invoiceId);
    }

    public class InvoicePrintService : IInvoicePrintService
    {
        private readonly IInvoicePrintRepository _invoicePrintRepository;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly IUserContextService _userContextService;

        public InvoicePrintService(
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
                distanceFormImageHeader = 70;
                XImage logoXImage = XImage.FromStream(logo);
                gfx.DrawImage(logoXImage, new XRect(60, 0, 100, 100));
            }

            //write title
            XFont font = new("Verdana", 10, XFontStyleEx.Regular);
            gfx.DrawString(invoicePrintData.CompanyName, font, XBrushes.Black, new XRect(0, 50 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.TopCenter);
            gfx.DrawString(invoicePrintData.CompanyDocument, font, XBrushes.Black, new XRect(0, 60 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.Center);
            gfx.DrawString(invoicePrintData.CompanyPhone, font, XBrushes.Black, new XRect(0, 75 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.Center);
            gfx.DrawString(invoicePrintData.CompanyAddress, font, XBrushes.Black, new XRect(0, 90 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.Center);
            gfx.DrawString("----------------------------------------------", font, XBrushes.Black, new XRect(6, 100 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.Center);


            gfx.DrawString(invoicePrintData.User, font, XBrushes.Black, new XRect(8, 130 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.TopLeft);
            gfx.DrawString(invoicePrintData.Hour, font, XBrushes.Black, new XRect(0, 145 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.TopRight);
            gfx.DrawString(invoicePrintData.Date, font, XBrushes.Black, new XRect(0, 160 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.TopRight);
            gfx.DrawString("Hora.:", font, XBrushes.Black, new XRect(8, 145 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.TopLeft);
            gfx.DrawString("Fecha.:", font, XBrushes.Black, new XRect(8, 160 + distanceFormImageHeader, widthWithMargin, 50), XStringFormats.TopLeft);
            gfx.DrawString("----------------------------------------------", font, XBrushes.Black, new XRect(6, 170 + distanceFormImageHeader, widthWithMargin, 10), XStringFormats.Center);

            gfx.DrawString("PRODUCTOS", font, XBrushes.Black, new XRect(6, 180 + distanceFormImageHeader, widthWithMargin, 10), XStringFormats.TopLeft);
            // Add items
            var lastY = 180 + distanceFormImageHeader;
            var itemY = 0;
            var totalPrice = 0.0M;
            var itemsCount = invoicePrintData.InvoicePrintDetails.Count;
            foreach (var item in invoicePrintData.InvoicePrintDetails)
            {
                totalPrice += item.RawInvoiceDetails.Price;
                gfx.DrawString(item.RawInvoiceDetails.ProductName.Substring(0, 15), font, XBrushes.Black, new XRect(6, lastY + itemY + 13, widthWithMargin, 50), XStringFormats.TopLeft);
                gfx.DrawString(item.RawInvoiceDetails.Price.ToString("C"), font, XBrushes.Black, new XRect(6, lastY + itemY + 13, widthWithMargin, 50), XStringFormats.TopRight);
                gfx.DrawString("CANT: " + item.RawInvoiceDetails.Quantity.ToString(), font, XBrushes.Black, new XRect(15, lastY + itemY + 25, widthWithMargin, 30), XStringFormats.TopLeft);
                foreach (var detailsAddon in item.RawInvoiceDetailsAddon)
                {
                    var addonComputedValue = Math.Round(AddonCalculator.CalculateAmountOrPercentage(detailsAddon, item.RawInvoiceDetails.Price), 2);
                    addonComputedValue = detailsAddon.IsDiscount ? -addonComputedValue : addonComputedValue;
                    gfx.DrawString($"{detailsAddon.Concept}: {addonComputedValue}", font, XBrushes.Black, new XRect(15, lastY + itemY + 37, widthWithMargin, 30), XStringFormats.TopLeft);
                    itemY += 12;
                    AddAccumulatedAddon(accumulatedAddons, detailsAddon.Concept, addonComputedValue);
                }
                itemY += 33;
            }

            gfx.DrawString("----------------------------------------------", font, XBrushes.Black, new XRect(6, lastY + itemY + 10, widthWithMargin, 10), XStringFormats.Center);

            gfx.DrawString("TOTAL.:", font, XBrushes.Black, new XRect(6, lastY + itemY + 20, widthWithMargin, 10), XStringFormats.TopLeft);
            gfx.DrawString("Precio.:", font, XBrushes.Black, new XRect(15, lastY + itemY + 30, widthWithMargin, 10), XStringFormats.TopLeft);
            gfx.DrawString(totalPrice.ToString("C"), font, XBrushes.Black, new XRect(6, lastY + itemY + 30, widthWithMargin, 10), XStringFormats.TopRight);

            foreach (var item in accumulatedAddons)
            {
                gfx.DrawString(item.Key, font, XBrushes.Black, new XRect(15, lastY + itemY + 40, widthWithMargin, 10), XStringFormats.TopLeft);
                gfx.DrawString(item.Value.ToString("C"), font, XBrushes.Black, new XRect(6, lastY + itemY + 40, widthWithMargin, 10), XStringFormats.TopRight);
                itemY += 12;
            }

            var total = totalPrice + accumulatedAddons.Sum(x => x.Value);
            gfx.DrawString("----------------------------------------------", font, XBrushes.Black, new XRect(6, lastY + itemY + 50, widthWithMargin, 10), XStringFormats.Center);
            gfx.DrawString(total.ToString("C"), font, XBrushes.Black, new XRect(6, lastY + itemY + 60, widthWithMargin, 10), XStringFormats.TopRight);



            /// Generate QR Code
            string qrCodeText = "https://example.com/invoice/12345";
            var qrCodeImage = GenerateQRCode(qrCodeText);

            // Convert Bitmap to XImage
            XImage qrCodeXImage = LoadImage(qrCodeImage);

            // Draw QR Code
            // Insert QR Code
            gfx.DrawImage(qrCodeXImage, 60, lastY + itemY + 90, 100, 100);

            MemoryStream stream = new();
            document.Save(stream, false);
            stream.Position = 0;
            return stream;
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
