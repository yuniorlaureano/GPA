using GPA.Dtos.General;
using GPA.Services.General.BlobStorage;
using GPA.Utils.Exceptions;
using PdfSharp.Drawing;
using QRCoder;
using System.Globalization;
using System.Text.Json;

namespace GPA.Services.Invoice
{
    public abstract class InvoicePrintServiceBase
    {
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;

        protected InvoicePrintServiceBase(IBlobStorageServiceFactory blobStorageServiceFactory)
        {
            _blobStorageServiceFactory = blobStorageServiceFactory;
        }

        protected async Task<Stream?> GetLogo(string logo)
        {
            BlobStorageFileResult? fileResult = null;

            try
            {
                fileResult = JsonSerializer.Deserialize<BlobStorageFileResult>(logo, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                    ?? throw new AttachmentDeserializingException("Error deserializing exception");
            }
            catch (Exception e)
            {
                throw new AttachmentDeserializingException("Error deserializing exception");
            }

            return await _blobStorageServiceFactory.DownloadFile(fileResult.UniqueFileName, isPublic: true);
        }

        protected void WriteFileLine(XGraphics gfx, string data, XFont font, XSolidBrush brushes, XRect xRect, XStringFormat position, ref double y)
        {
            y = xRect.Y;
            gfx.DrawString(data, font, brushes, xRect, position);
        }

        protected string ShortenName(string name)
        {
            if (name.Length > 15)
            {
                return name.Substring(0, 15);
            }
            return name;
        }

        protected XImage LoadImage(byte[] bitmapImage)
        {
            XImage qrCodeXImage;
            using (MemoryStream ms = new MemoryStream(bitmapImage, 0, bitmapImage.Length, writable: false, publiclyVisible: true))
            {
                ms.Position = 0;
                qrCodeXImage = XImage.FromStream(ms);
            }

            return qrCodeXImage;
        }

        protected void AddAccumulatedAddon(Dictionary<string, decimal> accumulatedAddons, string concept, decimal value)
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

        protected byte[] GenerateQRCode(string text)
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

        protected string FormatDate(DateTime date)
        {
            // Define the custom format string
            string format = "d 'de' MMMM 'de' yyyy";

            // Set the culture to Spanish (Spain)
            CultureInfo culture = new CultureInfo("es-ES");

            // Format the date
            return date.ToString(format, culture);
        }
    }
}
