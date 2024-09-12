using GPA.Common.DTOs;
using GPA.Data.Inventory;
using GPA.Entities.General;
using GPA.Entities.Unmapped;
using GPA.Services.Report;
using GPA.Services.Security;
using Microsoft.Extensions.Logging;

namespace GPA.Business.Services.Inventory
{
    public interface IStockReportsService
    {
        Task<byte[]> ExportExistenceToExcelAsync(RequestFilterDto filter);
    }

    public class StockReportsService : IStockReportsService
    {
        private readonly IUserContextService _userContextService;
        private readonly IStockReportRepository _repository;
        private readonly IReportExcel _reportExcel;
        private readonly ILogger<StockReportsService> _logger;

        public StockReportsService(
            IProductRepository productRepository,
            IAddonRepository addonRepository,
            IUserContextService userContextService,
            IStockReportRepository repository,
            IReportExcel reportExcel,
            ILogger<StockReportsService> logger)
        {
            _userContextService = userContextService;
            _repository = repository;
            _reportExcel = reportExcel;
            _logger = logger;
        }

        public async Task<byte[]> ExportExistenceToExcelAsync(RequestFilterDto filter)
        {
            _logger.LogInformation("User '{User}' is generating existence report", _userContextService.GetCurrentUserId());
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
    }
}
