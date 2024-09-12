using GPA.Api.Utils.Filters;
using GPA.Business.Services.Inventory;
using GPA.Common.DTOs;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Api.Controllers.Report
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("report")]
    [ApiController()]
    public class StockReportsController : ControllerBase
    {
        private readonly IStockReportsService _stockReportsService;

        public StockReportsController(IStockReportsService stockReportsService)
        {
            _stockReportsService = stockReportsService;
        }

        [HttpGet("existence")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.Reporting}.{Components.Report}", permission: Permissions.ExistenceReport)]
        public async Task<IActionResult> Get(RequestFilterDto filter)
        {
            var report = await _stockReportsService.ExportExistenceToExcelAsync(filter);
            return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "existence-report.xlsx");
        }
    }
}
