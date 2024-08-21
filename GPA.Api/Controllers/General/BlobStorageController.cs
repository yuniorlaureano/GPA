using AutoMapper;
using GPA.Api.Utils.Filters;
using GPA.Common.DTOs;
using GPA.Dtos.General;
using GPA.Services.General;
using GPA.Services.General.BlobStorage;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GPA.General.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("general/[controller]")]
    [ApiController()]
    public class BlobStorageController : ControllerBase
    {
        private readonly IBlobStorageConfigurationService _blobStorageConfigurationService;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly IMapper _mapper;

        public BlobStorageController(
            IBlobStorageConfigurationService blobStorageConfigurationService,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IMapper mapper)
        {
            _blobStorageConfigurationService = blobStorageConfigurationService;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Blob}", permission: Permissions.Read)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _blobStorageConfigurationService.GetByIdAsync(id));
        }

        [HttpGet()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Blob}", permission: Permissions.Read)]
        public async Task<IActionResult> Get([FromQuery] RequestFilterDto filter)
        {
            return Ok(await _blobStorageConfigurationService.GetAllAsync(filter));
        }

        [HttpPost()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Blob}", permission: Permissions.Create)]
        public async Task<IActionResult> Create(BlobStorageConfigurationCreationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _blobStorageConfigurationService.CreateConfigurationAsync(model);
            return Ok();
        }

        [HttpPut()]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Blob}", permission: Permissions.Update)]
        public async Task<IActionResult> Update(BlobStorageConfigurationUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _blobStorageConfigurationService.UpdateConfigurationAsync(model);
            return NoContent();
        }

        [HttpPost("blob-storage/upload")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Blob}", permission: Permissions.Upload)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fileResult = await _blobStorageServiceFactory.UploadFile(file);
            return Ok(fileResult);
        }

        [HttpGet("blob-storage/download")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Blob}", permission: Permissions.Download)]
        public async Task<IActionResult> DownloadFile(string fullFileName, string bucketOrCotainer, bool isPublic = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fileResult = await _blobStorageServiceFactory.DownloadFile(fullFileName, isPublic);
            return File(fileResult, "application/octet-stream", fullFileName);
        }

        [HttpDelete("blob-storage/remove")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Blob}", permission: Permissions.Download)]
        public async Task<IActionResult> DeleteFile(string fullFileName, bool isPublic)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _blobStorageServiceFactory.DeleteFile(fullFileName, isPublic);
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProfileFilter(path: $"{Apps.GPA}.{Modules.General}.{Components.Blob}", permission: Permissions.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _blobStorageConfigurationService.RemoveAsync(id);
            return NoContent();
        }
    }
}
