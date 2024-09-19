using AutoMapper;
using GPA.Api.Utils.Filters;
using GPA.Common.DTOs;
using GPA.Data.Inventory;
using GPA.Dtos.General;
using GPA.Entities.Report;
using GPA.Services.General;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.Api.Controllers.General
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("general/[controller]")]
    [ApiController()]
    public class PrintsController : ControllerBase
    {
        private readonly IPrintService _printService;
        private readonly IReportTemplateRepository _reportTemplateRepository;
        private readonly IMapper _mapper;

        public PrintsController(
            IPrintService printService,
            IReportTemplateRepository reportTemplateRepository,
            IMapper mapper)
        {
            _printService = printService;
            _reportTemplateRepository = reportTemplateRepository;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _printService.GetPrintInformationByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _printService.GetPrintInformationAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(CreatePrintInformationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _printService.AddAsync(model);
            return Created(Url.Action(nameof(Get)), new { id = entity.Id });
        }

        [HttpPut("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, UpdatePrintInformationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _printService.UpdateAsync(id, model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _printService.RemoveAsync(id);
            return NoContent();
        }

        [HttpPost("photo/upload")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Upload)]
        public async Task<IActionResult> UploadPhoto(PrintInformationUploadPhotoDto printInformationUploadPhotoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _printService.SavePhoto(printInformationUploadPhotoDto);
            return NoContent();
        }

        [HttpGet("templates")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Read)]
        public async Task<IActionResult> GetReportTemplates()
        {
            return Ok(await _reportTemplateRepository.GetTemplates());
        }

        [HttpGet("templates/{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Read)]
        public async Task<IActionResult> GetReportTemplateById(Guid id)
        {
            return Ok(await _reportTemplateRepository.GetTemplateById(id));
        }

        [HttpPut("templates/{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.PrintInformation}", permission: Permissions.Update)]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] ReportTemplate model)
        {
            await _reportTemplateRepository.UpdateTemplate(id, model);
            return NoContent();
        }
    }
}
