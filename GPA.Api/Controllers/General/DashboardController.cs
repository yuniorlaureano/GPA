using GPA.Api.Utils.Filters;
using GPA.Entities.General;
using GPA.Services.General.BlobStorage;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Api.Controllers.General
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("general/[controller]")]
    [ApiController()]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("clients/count")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Dashboard}", permission: Permissions.Read)]
        public async Task<IActionResult> GetClientsCount()
        {
            return Ok(await _dashboardService.GetClientsCount());
        }

        [HttpGet("sales/months/{month}/revenue")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Dashboard}", permission: Permissions.Read)]
        public async Task<IActionResult> GetSelesRevenue(int month = 0)
        {
            return Ok(await _dashboardService.GetSelesRevenue(month));
        }

        [HttpGet("input-vs-output-vs-existence")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Dashboard}", permission: Permissions.Read)]
        public async Task<IActionResult> GetInputVsOutputVsExistence()
        {
            return Ok(await _dashboardService.GetInputVsOutputVsExistence());
        }

        [HttpGet("reasons/{reason}/transactions")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Dashboard}", permission: Permissions.Read)]
        public async Task<IActionResult> GetTransactionsPerMonthByReason(ReasonTypes reason)
        {            
            return Ok(await _dashboardService.GetTransactionsPerMonthByReason(reason));
        }
    }
}
