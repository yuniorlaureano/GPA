using GPA.Common.DTOs;
using GPA.Data.Inventory;
using GPA.Entities.General;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using GPA.Entities.Unmapped.Invoice;
using GPA.Services.Report;
using GPA.Services.Security;
using GPA.Utils;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GPA.Business.Services.Inventory
{
    public interface IStockReportsService
    {
        Task<byte[]> ExportExistenceToExcelAsync(RequestFilterDto filter);
        Task<byte[]> ExportStockCycleDetails(Guid stockCycleId);
        Task<byte[]> ExportTransactions(RequestFilterDto filter);
        Task<byte[]> ExportSales(RequestFilterDto filter);
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

        public InventoryService(
            IUserContextService userContextService,
            IStockReportRepository repository,
            IStockCycleRepository stockCycleRepository,
            IReportExcel reportExcel,
            IReportPdfBase reportPdfBase,
            IReportTemplateRepository reportTemplateRepository,
            ILogger<InventoryService> logger)
        {
            _userContextService = userContextService;
            _repository = repository;
            _stockCycleRepository = stockCycleRepository;
            _reportExcel = reportExcel;
            _reportPdfBase = reportPdfBase;
            _reportTemplateRepository = reportTemplateRepository;
            _logger = logger;
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
            var htmlContent = await GetSaleTemplate(sales);
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
                        {detail[_initial]?.ProductName}
                      </td>
                      <td>
                        <span class=""initial-detail""
                          >{(detail[_initial]?.ProductPrice ?? 0.0M).ToString("C2")}</span
                        >
                        <span class=""final-detail""
                          >{(detail[_final]?.ProductPrice ?? 0.0M).ToString("C2")}</span
                        >
                      </td>
                      <td>
                        <span class=""initial-detail""
                          >{detail[_initial]?.Input}</span
                        >
                        <span class=""final-detail""
                          >{detail[_final]?.Input ?? 0}</span
                        >
                      </td>
                      <td>
                        <span class=""initial-detail""
                          >{detail[_initial]?.Output}</span
                        >
                        <span class=""final-detail""
                          >{detail[_final]?.Output ?? 0}</span
                        >
                      </td>
                      <td>
                        <span class=""initial-detail""
                          >{detail[_initial]?.Stock}</span
                        >
                        <span class=""final-detail""
                          >{detail[_final]?.Stock ?? 0}</span
                        >
                      </td>
                    </tr>");
            }

            var template = await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.STOCK_DETAILS_TEMPLATE);
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
                  </tr>");
            }

            var template = await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.TRANSACTION_TEMPLATE);
            if (template == null || template.Template is null)
            {
                throw new Exception("El template para el reporte no existe");
            }

            return template.Template.Replace("{{Content}}", content.ToString());
        }

        private async Task<string> GetSaleTemplate(IEnumerable<RawAllInvoice> rawInvoices)
        {
            var content = new StringBuilder();
            foreach (var invoice in rawInvoices)
            {
                content.Append($@"
                    <tr class=""content"">
                    <td>{GetSaleStatus(invoice.Status)}</td>
                    <td>{invoice.Code}</td>
                    <td>{GetSaleType(invoice.Type)}</td>
                    <td>{invoice.Date.ToString("MM d yyyy")}</td>
                    <td>{invoice.Note}</td>
                    <td>{invoice.ClientName} {invoice.ClientLastName}</td>
                    <td>{GetPaymentStatus(invoice.PaymentStatus)}</td>
                  </tr>");
            }

            var template = await _reportTemplateRepository.GetTemplateByCode(TemplateConstants.SALE_TEMPLATE);
            if (template == null || template.Template is null)
            {
                throw new Exception("El template para el reporte no existe");
            }

            return template.Template.Replace("{{Content}}", content.ToString());
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
