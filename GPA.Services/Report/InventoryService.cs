using GPA.Common.DTOs;
using GPA.Common.Entities.Invoice;
using GPA.Data.Inventory;
using GPA.Dtos.Invoice;
using GPA.Entities.General;
using GPA.Entities.Report;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using GPA.Entities.Unmapped.Invoice;
using GPA.Services.Report;
using GPA.Services.Security;
using GPA.Utils;
using GPA.Utils.Caching;
using GPA.Utils.Database;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace GPA.Business.Services.Inventory
{
    public interface IStockReportsService
    {
        Task<byte[]> ExportExistenceToExcelAsync(RequestFilterDto filter);
        Task<byte[]> ExportStockCycleDetails(Guid stockCycleId);
        Task<byte[]> ExportExistence(RequestFilterDto filter);
        Task<byte[]> ExportTransactions(RequestFilterDto filter);
        Task<byte[]> ExportSales(RequestFilterDto filter);
        Task<IEnumerable<RawAllInvoice>> GetSales(RequestFilterDto filter);
    }

    public class InventoryService : IStockReportsService
    {
        private readonly IUserContextService _userContextService;
        private readonly IStockReportRepository _repository;
        private readonly IStockCycleRepository _stockCycleRepository;
        private readonly IReportExcel _reportExcel;
        private readonly IReportPdfBase _reportPdfBase;
        private readonly IReportTemplateRepository _reportTemplateRepository;
        private readonly ILogger<InventoryService> _logger;
        private const string _initial = "initial";
        private const string _final = "final";
        private readonly IGenericCache<ReportTemplate> _cache;

        public InventoryService(
            IUserContextService userContextService,
            IStockReportRepository repository,
            IStockCycleRepository stockCycleRepository,
            IReportExcel reportExcel,
            IReportPdfBase reportPdfBase,
            IReportTemplateRepository reportTemplateRepository,
            ILogger<InventoryService> logger,
            IGenericCache<ReportTemplate> cache)
        {
            _userContextService = userContextService;
            _repository = repository;
            _stockCycleRepository = stockCycleRepository;
            _reportExcel = reportExcel;
            _reportPdfBase = reportPdfBase;
            _reportTemplateRepository = reportTemplateRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<byte[]> ExportTransactions(RequestFilterDto filter)
        {
            _logger.LogInformation("El usuario '{UserId}' está generando el reporte de transacciones", _userContextService.GetCurrentUserId());
            var transactions = await _repository.GetTransactionsAsync(filter);
            var htmlContent = await GetTransactionTemplate(transactions);
            return _reportPdfBase.GeneratePdf(htmlContent);
        }

        public async Task<byte[]> ExportSales(RequestFilterDto filter)
        {
            _logger.LogInformation("El usuario '{UserId}' está generando el reporte de ventas", _userContextService.GetCurrentUserId());
            var sales = await _repository.GetAllInvoicesAsync(filter);
            var salesSummary = await _repository.GetPaymentByPaymentMethodSummary(filter);

            var htmlContent = await GetSaleTemplate(sales, salesSummary, filter);
            return _reportPdfBase.GeneratePdf(htmlContent);
        }

        public async Task<IEnumerable<RawAllInvoice>> GetSales(RequestFilterDto filter)
        {
            return await _repository.GetAllInvoicesAsync(filter);
        }

        public async Task<byte[]> ExportExistence(RequestFilterDto filter)
        {
            _logger.LogInformation("El usuario '{UserId}' está generando el reporte de existencia", _userContextService.GetCurrentUserId());
            var existences = await _repository.GetAllExistenceAsync(filter);
            var content = new StringBuilder();

            foreach (var item in existences)
            {
                var type = item.ProductType == ProductType.RawProduct ? "Materia prima" : "Producto terminado";
                content.Append($@"
                    <tr class=""content"">
                    <td>{item.ProductCode}</td>
                    <td>{item.ProductName}</td>
                    <td>{type}</td>
                    <td>{item.Input}</td>
                    <td>{item.Output}</td>
                    <td>{item.Stock}</td>
                    <td>{(double)Math.Round(item.Price * item.Stock, 2)}</td>
                  </tr>
                ");
            }

            var template = await _cache.GetOrCreate(CacheType.ReportTemplates, TemplateConstants.EXISTENCE_TEMPLATE, async () =>
            {
                return await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.EXISTENCE_TEMPLATE);
            });

            if (template == null || template.Template is null)
            {
                throw new Exception("El template para el reporte no existe");
            }

            var htmlContent = template.Template.Replace("{{Content}}", content.ToString());
            return _reportPdfBase.GeneratePdf(htmlContent);
        }

        public async Task<byte[]> ExportStockCycleDetails(Guid stockCycleId)
        {
            _logger.LogInformation("El usuario '{UserId}' está generando el reporte ciclos de inventario", _userContextService.GetCurrentUserId());

            var stockCycle = await _stockCycleRepository.GetStockCycleAsync(stockCycleId);
            var stockCycleDetails = await _stockCycleRepository.GetStockCycleDetailsAsync(stockCycleId);

            var stockDetailSummary = new Dictionary<Guid, Dictionary<string, RawStockCycleDetails?>>();

            foreach (var item in stockCycleDetails)
            {
                if (!stockDetailSummary.ContainsKey(item.ProductId))
                {
                    stockDetailSummary.Add(item.ProductId, new Dictionary<string, RawStockCycleDetails?>()
                    {
                        { _initial,  null },
                        { _final,  null }
                    });
                }

                if (item.Type == 0) // initial
                {
                    stockDetailSummary[item.ProductId][_initial] = item;
                }

                if (item.Type == 1) // initial
                {
                    stockDetailSummary[item.ProductId][_final] = item;
                }
            }

            var htmlContent = await GetStockCycleDetailsTemplate(stockCycle, stockDetailSummary);
            return _reportPdfBase.GeneratePdf(htmlContent);
        }

        public async Task<byte[]> ExportExistenceToExcelAsync(RequestFilterDto filter)
        {
            _logger.LogInformation("El usuario '{UserId}' está generando el reporte de existencia", _userContextService.GetCurrentUserId());
            var existences = await _repository.GetAllExistenceAsync(filter);
            var report = _reportExcel.CreateWorkBook();
            report.CreateSheet("Existences");

            report.CreateHeader(new[]
            {
                "Código",
                "Producto",
                "Tipo",
                "Entrada",
                "Salida",
                "Existencia",
                "Monto en producto",
            });

            report.CreateRowData<Existence>(existences, (row, item) =>
            {
                row.CreateCell(0).SetCellValue(item.ProductCode);
                row.CreateCell(1).SetCellValue(item.ProductName);
                row.CreateCell(2).SetCellValue(item.ProductType == ProductType.RawProduct ? "Materia prima" : "Producto terminado");
                row.CreateCell(3).SetCellValue(item.Input);
                row.CreateCell(4).SetCellValue(item.Output);
                row.CreateCell(5).SetCellValue(item.Stock);
                row.CreateCell(6).SetCellValue((double)Math.Round(item.Price * item.Stock, 2));
            });

            using var stream = new MemoryStream();
            report.Workbook.Write(stream);
            _logger.LogInformation("existence report generated");
            return stream.ToArray();
        }

        private async Task<string> GetStockCycleDetailsTemplate(RawStockCycle rawStockCycle, Dictionary<Guid, Dictionary<string, RawStockCycleDetails?>> details)
        {
            var content = new StringBuilder();
            foreach (var detail in details.Values)
            {
                content.Append($@"
                    <tr>
                      <td>
                        {detail[_initial]?.ProductName ?? detail[_final]?.ProductName}
                      </td>
                        <td>
                        {detail[_initial]?.ProductCode ?? detail[_final]?.ProductCode}
                      </td>
                      <td>
                        <span class=""initial-detail""
                          >{(detail[_initial]?.ProductPrice ?? 0.0M).ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</span
                        >
                        <span class=""final-detail""
                          >{(detail[_final]?.ProductPrice ?? 0.0M).ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</span
                        >
                      </td>
                      <td>
                        <span class=""initial-detail""
                          >{detail[_initial]?.Input ?? 0}</span
                        >
                        <span class=""final-detail""
                          >{detail[_final]?.Input ?? 0}</span
                        >
                      </td>
                      <td>
                        <span class=""initial-detail""
                          >{detail[_initial]?.Output ?? 0}</span
                        >
                        <span class=""final-detail""
                          >{detail[_final]?.Output ?? 0}</span
                        >
                      </td>
                      <td>
                        <span class=""initial-detail""
                          >{detail[_initial]?.Stock ?? 0}</span
                        >
                        <span class=""final-detail""
                          >{detail[_final]?.Stock ?? 0}</span
                        >
                      </td>
                    </tr>");
            }

            var template = await _cache.GetOrCreate(CacheType.ReportTemplates, TemplateConstants.STOCK_CYCLE_DETAILS_TEMPLATE, async () =>
            {
                return await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.STOCK_CYCLE_DETAILS_TEMPLATE);
            });
            if (template == null || template.Template is null)
            {
                throw new Exception("El template para el reporte no existe");
            }

            return template.Template
                    .Replace("{{StartDate}}", rawStockCycle.StartDate.ToString("MM d yyyy"))
                    .Replace("{{EndDate}}", rawStockCycle.EndDate.ToString("MM d yyyy"))
                    .Replace("{{Note}}", rawStockCycle.Note)
                    .Replace("{{Status}}", rawStockCycle.IsClose ? "Cerrado" : "Abierto")
                    .Replace("{{Content}}", content.ToString());

        }

        private string GetStockStatus(int status)
        {
            return status switch
            {
                0 => "Borrador",
                1 => "Guardado",
                2 => "Cancelado",
                _ => ""
            };
        }

        private string GetTransactionType(int transactionType)
        {
            return transactionType switch
            {
                0 => "Entrada",
                1 => "Salida",
                _ => ""
            };
        }

        private async Task<string> GetTransactionTemplate(IEnumerable<RawStock> rawStocks)
        {
            var content = new StringBuilder();
            foreach (var stock in rawStocks)
            {
                content.Append($@"
                    <tr class=""content"">
                    <td>{GetStockStatus(stock.Status)}</td>
                    <td>{GetTransactionType(stock.TransactionType)}</td>
                    <td>{stock.Date?.ToString("MM d yyyy")}</td>
                    <td>{stock.ProviderName}</td>
                    <td>{stock.ReasonName}</td>
                    <td>{stock.Description}</td>
                    <td>{stock.CreatedByName}</td>
                    <td>{stock.UpdatedByName}</td>
                  </tr>");
            }

            var template = await _cache.GetOrCreate(CacheType.ReportTemplates, TemplateConstants.TRANSACTION_TEMPLATE, async () =>
            {
                return await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.TRANSACTION_TEMPLATE);
            });

            if (template == null || template.Template is null)
            {
                throw new Exception("El template para el reporte no existe");
            }

            return template.Template.Replace("{{Content}}", content.ToString());
        }

        private async Task<string> GetSaleTemplate(IEnumerable<RawAllInvoice> rawInvoices, IEnumerable<RawPaymentByPaymentMethodSummary> summary, RequestFilterDto filter)
        {
            var content = new StringBuilder();
            var summaryContent = new StringBuilder();
            foreach (var invoice in rawInvoices)
            {
                content.Append($@"
                     <tr>
                      <td>{invoice.Code}</td>
                      <td>{invoice.Date.ToString("MM d yyyy")}</td>
                      <td>{invoice.ClientName} {invoice.ClientLastName}</td>
                      <td>{GetPaymentMethod((PaymentMethod)invoice.PaymentMethod)}</td>
                      <td>{invoice.ToPay.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</td>
                    </tr>");
            }

            decimal total = 0.0M;
            foreach (var sum in summary)
            {
                summaryContent.Append($@"
                     <tr>
                      <td>{GetPaymentMethod((PaymentMethod)sum.PaymentMethod)}</td>
                      <td>{sum.Payment.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}</td>
                    </tr>");
                total = total + sum.Payment;
            }

            var template = await _cache.GetOrCreate(CacheType.ReportTemplates, TemplateConstants.SALE_TEMPLATE, async () =>
            {
                return await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.SALE_TEMPLATE);
            });

            if (template == null || template.Template is null)
            {
                throw new Exception("El template para el reporte no existe");
            }

            var search = SearchHelper.ConvertSearchToString(filter);
            var invoiceListFilter = JsonSerializer.Deserialize<InvoiceFilterDto>(search, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return template.Template
                .Replace("{{Content}}", content.ToString())
                .Replace("{{Date}}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt", new CultureInfo("es-ES")))
                .Replace("{{From}}", DetailedDateUtil.GetDetailedDateAsDateTime(invoiceListFilter.From).ToString("MM/dd/yyyy", new CultureInfo("es-ES")))
                .Replace("{{To}}", DetailedDateUtil.GetDetailedDateAsDateTime(invoiceListFilter.To).ToString("MM/dd/yyyy", new CultureInfo("es-ES")))
                .Replace("{{Summary}}", summaryContent.ToString())
                .Replace("{{SummaryTotal}}", total.ToString("C2", CultureInfo.GetCultureInfo("en-US")));
        } 

        private string GetPaymentMethod(PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.Cash => "Efectivo",
                PaymentMethod.BankTransfer => "Transferencia",
                PaymentMethod.CreditCard => "Tarjeta",
                PaymentMethod.Check => "Cheque",
                PaymentMethod.Other => "Otro",
                _ => ""
            };
        }
        private string GetSaleType(int transactionType)
        {
            return transactionType switch
            {
                0 => "A crédito",
                1 => "Al contado",
                _ => ""
            };
        }

        private string GetPaymentStatus(byte transactionType)
        {
            return transactionType switch
            {
                0 => "Pagado",
                1 => "Pendiente",
                _ => ""
            };
        }

        private string GetSaleStatus(byte status)
        {
            return status switch
            {
                0 => "Borrador",
                1 => "Guardado",
                2 => "Cancelado",
                _ => ""
            };
        }
    }
}
