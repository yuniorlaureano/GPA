using DinkToPdf;
using GPA.Data.General;
using GPA.Data.Invoice;
using GPA.Entities.General;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Invoice;
using GPA.Services.Report;
using GPA.Services.Security;
using GPA.Utils;
using PdfSharp.Drawing;
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

        public ProofOfPaymentPrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IClientRepository clientRepository,
            IUserContextService userContextService,
            IPrintRepository printRepository,
            IReportPdfBase reportPdfBase
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _clientRepository = clientRepository;
            _printRepository = printRepository;
            _reportPdfBase = reportPdfBase;
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

        public async Task<byte[]> GenerateInvoice(InvoicePrintData invoicePrintData)
        {
            //using var logo = await GetLogo(invoicePrintData.CompanyLogo);

            //var qrCodeImage = GenerateQRCode(invoicePrintData.Invoice.Id.ToString());

            // Convert Bitmap to XImage
            //XImage qrCodeXImage = LoadImage(qrCodeImage);

            //_logger.LogInformation("El usuario '{UserId}' está generando el reporte ciclos de inventario", _userContextService.GetCurrentUserId());
            Dictionary<string, decimal> accumulatedAddons = new();

            var htmlContent = GetTemplate();

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
                .Replace("{Signer}", invoicePrintData.Signer);

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

                      table {
                        width: 100%;
                      }
                      body {
                        margin: 5px;
                        padding: 0;
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
                          <div>{Address}</div>
                          <div>------------------------------------------------------</div>
                          <b class="bold">RECIBO DE PAGO</b>
                          <div>{Date}</div>
                          <div>------------------------------------------------------</div>
                        </td>
                      </tr>
                    </table>
                    <table class="details">
                      <tr>
                        <th>Recibí de:</th>
                        <td>{Client}</td>
                      </tr>
                      <tr>
                        <td colspan="2">
                          <center>
                            ------------------------------------------------------
                          </center>
                        </td>
                      </tr>
                      {Amounts}
                      <tr>
                        <td colspan="2">
                          <center>
                            ------------------------------------------------------
                          </center>
                        </td>
                      </tr>
                      <tr>
                         <th>Total</th>
                         <td>{Total}</td>                        
                      </tr>

                    </table>
                    <div>
                      <br />
                      <br />
                      <div class="bold">Concento:</div>
                      <div style="margin-left: 20px">{Concept}</div>

                      <br />
                      <br />
                      <br />
                      <br />
                      <div style="border-top: solid 1px black; text-align: center">
                        {Signer}
                      </div>
                    </div>
                  </body>
                </html>
                
                """;
            return template;
        }
    }
}
