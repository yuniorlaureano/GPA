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

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Read)]
        public async Task<IActionResult> GetClientsCount()
        {
            return Ok(await _dashboardService.GetClientsCount());
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Read)]
        public async Task<IActionResult> GetSelesRevenue()
        {
            return Ok(await _dashboardService.GetSelesRevenue());
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Read)]
        public async Task<IActionResult> GetInputVsOutputVsExistence()
        {
            return Ok(await _dashboardService.GetInputVsOutputVsExistence());
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Read)]
        public async Task<IActionResult> GetTransactionsPerMonthByReason(ReasonTypes reason, TransactionType transactionType)
        {            
            return Ok(await _dashboardService.GetTransactionsPerMonthByReason(reason, transactionType));
        }
    }
}
