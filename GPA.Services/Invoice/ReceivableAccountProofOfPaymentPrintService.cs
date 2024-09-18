using DinkToPdf;
using GPA.Data.General;
using GPA.Data.Invoice;
using GPA.Entities.Unmapped;
using GPA.Services.General.BlobStorage;
using GPA.Services.Invoice;
using GPA.Services.Report;
using GPA.Services.Security;
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

        public ReceivableAccountProofOfPaymentPrintService(
            IInvoicePrintRepository invoicePrintRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IClientRepository clientRepository,
            IUserContextService userContextService,
            IPrintRepository printRepository,
            IReportPdfBase reportPdfBase,
            IReceivableAccountRepository receivableAccountRepository
            ) : base(blobStorageServiceFactory)
        {
            _invoicePrintRepository = invoicePrintRepository;
            _userContextService = userContextService;
            _clientRepository = clientRepository;
            _printRepository = printRepository;
            _reportPdfBase = reportPdfBase;


            _receivableAccountRepository = receivableAccountRepository;
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
            var htmlContent = GetTemplate();

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
                .Replace("{Pending}", invoicePrintData.ReceivableAccounts.PendingPayment.ToString("C2", CultureInfo.GetCultureInfo("en-US")))
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
                          <b class="bold">RECIBO DE CUENTA POR COBRAR</b>
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
                      <tr>
                      <th>Fecha</th>
                      <td>{PaymentDate}</td>
                    </tr>
                    <tr>
                      <th>Monto</th>
                      <td>{Paid}</td>
                    </tr>
                    <tr>
                      <th>Pendiente:</th>
                      <td>{Pending}</td>
                    </tr>
                    </table>
                    <div>
                      <br />
                      <br />
                      <div class="bold">Concento:</div>
                      <div style="margin-left: 20px">PAGO DE CUENTAS POR COBRAR</div>

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
