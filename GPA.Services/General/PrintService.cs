using AutoMapper;
using GPA.Common.DTOs;
using GPA.Data.General;
using GPA.Dtos.General;
using GPA.Entities.General;
using GPA.Entities.Unmapped.General;
using GPA.Services.General.BlobStorage;
using GPA.Services.Security;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GPA.Services.General
{
    public interface IPrintService
    {
        Task<RawPrintInformation?> GetPrintInformationByIdAsync(Guid id);
        Task<ResponseDto<RawPrintInformation>> GetPrintInformationAsync(RequestFilterDto filter);
        Task<RawPrintInformation?> AddAsync(CreatePrintInformationDto model);
        Task UpdateAsync(Guid id, UpdatePrintInformationDto model);
        Task SavePhoto(PrintInformationUploadPhotoDto dto);
        Task RemoveAsync(Guid id);
    }

    public class PrintService : IPrintService
    {
        private readonly IPrintRepository _repository;
        private readonly IBlobStorageServiceFactory _blobStorageServiceFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<PrintService> _logger;
        private readonly IUserContextService _userContextService;

        public PrintService(
            IPrintRepository printRepository,
            IBlobStorageServiceFactory blobStorageServiceFactory,
            IMapper mapper,
            ILogger<PrintService> logger,
            IUserContextService userContextService)
        {
            _repository = printRepository;
            _blobStorageServiceFactory = blobStorageServiceFactory;
            _mapper = mapper;
            _logger = logger;
            _userContextService = userContextService;
        }

        public async Task<RawPrintInformation?> GetPrintInformationByIdAsync(Guid id)
        {
            var printInformation = await _repository.GetPrintInformationByIdAsync(id);
            return printInformation;
        }

        public async Task<ResponseDto<RawPrintInformation>> GetPrintInformationAsync(RequestFilterDto filter)
        {
            filter.Search = Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
            return new ResponseDto<RawPrintInformation>
            {
                Count = await _repository.GetPrintInformationCountAsync(filter),
                Data = _mapper.Map<IEnumerable<RawPrintInformation>>(await _repository.GetPrintInformationAsync(filter))
            };
        }

        public async Task<RawPrintInformation> AddAsync(CreatePrintInformationDto model)
        {
            var printInformation = _mapper.Map<PrintInformation>(model);
            var savedUnit = await _repository.AddAsync(printInformation);
            _logger.LogInformation("El usuario '{User}' ha agregado la informacioón de impresión '{Print}'", _userContextService.GetCurrentUserId(), savedUnit.Id);
            return _mapper.Map<RawPrintInformation>(savedUnit);
        }

        public async Task UpdateAsync(Guid Id, UpdatePrintInformationDto model)
        {
            var printInformation = _mapper.Map<PrintInformation>(model);
            var savedPrintInformation = await _repository.GetByIdAsync(query => query, x => x.Id == Id);
            printInformation.Id = Id;
            await _repository.UpdateAsync(savedPrintInformation, printInformation, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
            _logger.LogInformation("El usuario '{User}' ha modificado la informacioón de impresión '{Print}'", _userContextService.GetCurrentUserId(), Id);
        }

        public async Task SavePhoto(PrintInformationUploadPhotoDto dto)
        {
            var model = await _repository.GetPrintInformationByIdAsync(dto.PrintInformationId);
            if (model is null)
            {
                throw new InvalidOperationException("El información de impresión no existe");
            }

            var uploadResult = await _blobStorageServiceFactory.UploadFile(dto.Photo, folder: "", isPublic: true);
            await _repository.SavePhoto(uploadResult.AsJson(), model.Id);
        }

        public async Task RemoveAsync(Guid id)
        {
            var model = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(model);
            _logger.LogInformation("El usuario '{User}' ha eliminado la informacioón de impresión '{Print}'", _userContextService.GetCurrentUserId(), id);
        }
    }
}
