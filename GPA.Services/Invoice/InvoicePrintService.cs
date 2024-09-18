using DinkToPdf;
using GPA.Data.General;
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

        public InvoicePrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IUserContextService userContextService,
            IPrintRepository printRepository,
            IReportPdfBase reportPdfBase
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _printRepository = printRepository;
            _reportPdfBase = reportPdfBase;
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
            //using var logo = await GetLogo(invoicePrintData.CompanyLogo);

            //var qrCodeImage = GenerateQRCode(invoicePrintData.Invoice.Id.ToString());

            // Convert Bitmap to XImage
            //XImage qrCodeXImage = LoadImage(qrCodeImage);

            //_logger.LogInformation("El usuario '{UserId}' está generando el reporte ciclos de inventario", _userContextService.GetCurrentUserId());
            Dictionary<string, decimal> accumulatedAddons = new();

            var htmlContent = GetTemplate();

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
                .Replace("{TotalPrice}", total.ToString("C2", CultureInfo.GetCultureInfo("en-US")));

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = new PechkinPaperSize("65mm", "297mm"),
                Margins = new MarginSettings(0, 0, 0, 0)
            };

            return _reportPdfBase.GeneratePdf(htmlContent, settings: globalSettings);
        }

        private string GetTemplate()
        {
            var template = """
                <!DOCTYPE html>
                <html lang="en">
                  <head>
                    <meta charset="UTF-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                    <title>Invoice</title>
                    <style>
                      html {
                  font-family: Verdana, Geneva, Tahoma, sans-serif;
                  margin: 0;
                  padding: 0;
                  font-size: 12px;
                  display: flex;
                  justify-content: center;
                  flex-direction: column;
                }

                          body {
                  margin: 5px;
                  padding: 0;
                }

                          table {
                  width: 100%;
                }

                table td {
                  padding: 5px;
                  vertical-align: top;
                }
                table tr td:nth-child(2) {
                  text-align: right;
                }
                .qr-code {
                  text-align: center;
                  margin-top: 20px;
                }

                /* invoice header */
                .invoce-header {
                  text-align: center;
                }
                .mb-9 {
                  margin-bottom: 9px;
                }

                .receit-title {
                  font-size: 20px;
                  font-weight: bold;
                  align-items: center;
                }

                .bold {
                  font-weight: bold;
                }

                .m0 {
                  margin: 0;
                }
                .p0 {
                  padding: 0;
                }

                /* details */
                table.details th,
                td {
                  margin: 0 !important;
                  padding: 0 !important;
                }
                    </style>
                  </head>
                  <body>
                      <table>
                        <tr>
                          <td class="title">
                            <center>
                              <img
                                src="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSz7xOJSrO_80yAlVx3xLYSExnII1jTTPdTOA&s"
                                style="width: 200px"
                              />
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td class="invoce-header">
                            <div class="mb-9 receit-title">{Company}</div>
                            <div class="mb-9">{Document}</div>
                            <div class="mb-9">{Tel}</div>
                            <div class="mb-9">{Mail}</div>
                            <div>
                                {Address}
                            </div>
                          </td>
                        </tr>
                      </table>
                      <table class="details">
                        <tr>
                          <td colspan="2">
                            <center>
                              ------------------------------------------------------
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td>Usuario: {User}</td>
                          <td></td>
                        </tr>
                        <tr>
                          <td>Hora.:</td>
                          <td>{Hour}</td>
                        </tr>
                        <tr>
                          <td>Fecha.:</td>
                          <td>{Date}</td>
                        </tr>
                        <tr>
                          <td colspan="2">
                            <center>
                              ------------------------------------------------------
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td colspan="2">PRODUCTOS</td>
                        </tr>
                        {Products}
                        <tr>
                          <td colspan="2">
                            <center>
                              ------------------------------------------------------
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td>Total</td>
                          <td></td>
                        </tr>
                        {Totals}
                        <tr>
                          <td colspan="2">
                            <center>
                              ------------------------------------------------------
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td></td>
                          <td>{TotalPrice}</td>
                        </tr>
                      </table>
                      <div class="qr-code">
                        <img
                          style="width: 100px"
                          src="https://upload.wikimedia.org/wikipedia/commons/d/d0/QR_code_for_mobile_English_Wikipedia.svg"
                          alt="QR Code"
                        />
                      </div>
                  </body>
                </html>
                """;
            return template;
        }
    }
}
