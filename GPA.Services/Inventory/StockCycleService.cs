using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using GPA.Services.Security;
using Microsoft.Extensions.Logging;

namespace GPA.Business.Services.Inventory
{
    public interface IStockCycleService
    {
        public Task<StockCycleDto?> GetStockCycleAsync(Guid id);
        Task<Guid> OpenCycleAsync(StockCycleCreationDto dto);
        public Task<ResponseDto<StockCycleDto>> GetStockCyclesAsync(RequestFilterDto search);
        public Task CloseCycleAsync(Guid id);
        public Task RemoveAsync(Guid id);
    }

    public class StockCycleService : IStockCycleService
    {
        private readonly IStockCycleRepository _repository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly ILogger<StockCycleService> _logger;

        public StockCycleService(
            IStockCycleRepository repository,
            IUserContextService userContextService,
            IMapper mapper,
            ILogger<StockCycleService> logger)
        {
            _repository = repository;
            _userContextService = userContextService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<StockCycleDto?> GetStockCycleAsync(Guid id)
        {
            var stockCycle = await _repository.GetStockCycleAsync(id);
            var dto = _mapper.Map<StockCycleDto>(stockCycle);
            await MapStockCycleDetails(dto);
            return dto;
        }

        public async Task<ResponseDto<StockCycleDto>> GetStockCyclesAsync(RequestFilterDto search)
        {
            var stocks = await _repository.GetStockCyclesAsync(search);
            return new ResponseDto<StockCycleDto>
            {
                Count = await _repository.GetStockCycleCountAsync(search),
                Data = _mapper.Map<IEnumerable<StockCycleDto>>(stocks)
            };
        }

        public async Task<Guid> OpenCycleAsync(StockCycleCreationDto dto)
        {
            var newStockCycle = _mapper.Map<StockCycle>(dto);
            newStockCycle.CreatedBy = _userContextService.GetCurrentUserId();
            newStockCycle.CreatedAt = DateTimeOffset.UtcNow;
            var cycleId = await _repository.OpenCycleAsync(newStockCycle);
            _logger.LogInformation("El usuario '{User}' ha abierto el ciclo de inventario '{Cicle}'", _userContextService.GetCurrentUserId(), cycleId);
            return cycleId;
        }

        public async Task CloseCycleAsync(Guid id)
        {
            await _repository.CloseCycleAsync(id, _userContextService.GetCurrentUserId());
            _logger.LogInformation("El usuario '{User}' ha cerrado el ciclo de inventario '{Cicle}'", _userContextService.GetCurrentUserId(), id);
        }

        public async Task RemoveAsync(Guid id)
        {
            await _repository.SoftDeleteStockCycleAsync(id, _userContextService.GetCurrentUserId());
            _logger.LogInformation("El usuario '{User}' ha eliminado el ciclo de inventario '{Cicle}'", _userContextService.GetCurrentUserId(), id);
        }

        private async Task MapStockCycleDetails(StockCycleDto dto)
        {
            if (dto is not null)
            {
                var stockCycleDetails = await _repository.GetStockCycleDetailsAsync(dto.Id.Value);
                dto.StockCycleDetails = _mapper.Map<ICollection<StockCycleDetailDto>>(stockCycleDetails);
            }
        }
    }
}
