using GPA.Api.Utils.Filters;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Api.Controllers.Report
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("report/[controller]")]
    [ApiController()]
    public class InventoryController : ControllerBase
    {
        private readonly IStockReportsService _stockReportsService;

        public InventoryController(IStockReportsService stockReportsService)
        {
            _stockReportsService = stockReportsService;
        }

        [HttpGet("existence/print")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Reporting}.{Components.Report}", permission: Permissions.ExistenceReport)]
        public async Task<IActionResult> PrintExistence([FromQuery] RequestFilterDto filter)
        {
            var report = await _stockReportsService.ExportExistenceToExcelAsync(filter);
            return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "existence-report.xlsx");
        }

        [HttpGet("stockcycles/{stockCycleId}/print")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Reporting}.{Components.Report}", permission: Permissions.StockCycleReport)]
        public async Task<IActionResult> PrintStockCycleDetails(Guid stockCycleId)
        {
            var report = await _stockReportsService.ExportStockCycleDetails(stockCycleId);
            return File(report, "application/pdf", "generated.pdf");
        }

        [HttpGet("transactions/print")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Reporting}.{Components.Report}", permission: Permissions.TransactionReport)]
        public async Task<IActionResult> PrintTransactions([FromQuery] RequestFilterDto filter)
        {
            var report = await _stockReportsService.ExportTransactions(filter);
            return File(report, "application/pdf", "generated.pdf");
        }

        [HttpGet("sales/print")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Reporting}.{Components.Report}", permission: Permissions.TransactionReport)]
        public async Task<IActionResult> PrintSales([FromQuery] RequestFilterDto filter)
        {
            var report = await _stockReportsService.ExportSales(filter);
            return File(report, "application/pdf", "generated.pdf");
        }
    }
}