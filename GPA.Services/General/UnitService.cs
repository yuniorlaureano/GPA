using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.General;
using GPA.Data.General;
using GPA.Entities.General;
using GPA.Entities.Unmapped.General;
using GPA.Services.Security;
using GPA.Utils.Database;
using Microsoft.Extensions.Logging;

namespace GPA.Business.Services.General
{
    public interface IUnitService
    {
        Task<RawUnit?> GetByIdAsync(Guid id);
        Task<ResponseDto<RawUnit>> GetAllAsync(RequestFilterDto filter);
        Task<UnitDto?> AddAsync(UnitDto unitDto);
        Task UpdateAsync(UnitDto unitDto);
        Task RemoveAsync(Guid id);
    }

    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<UnitService> _logger;
        private readonly IUserContextService _userContextService;

        public UnitService(
            IUnitRepository repository,
            IMapper mapper,
            ILogger<UnitService> logger,
            IUserContextService userContextService)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _userContextService = userContextService;
            _logger = logger;
        }

        public async Task<RawUnit?> GetByIdAsync(Guid id)
        {
            var unit = await _repository.GetUnitAsync(id);
            return _mapper.Map<RawUnit>(unit);
        }

        public async Task<ResponseDto<RawUnit>> GetAllAsync(RequestFilterDto filter)
        {
            filter.Search = SearchHelper.ConvertSearchToString(filter);
            return new ResponseDto<RawUnit>
            {
                Count = await _repository.GetUnitsCountAsync(filter),
                Data = await _repository.GetUnitsAsync(filter)
            };
        }

        public async Task<UnitDto> AddAsync(UnitDto dto)
        {
            var unit = _mapper.Map<Unit>(dto);
            var savedUnit = await _repository.AddAsync(unit);
            _logger.LogInformation("El usuario '{UserId}' ha agregado la unidad '{UnitId}'", _userContextService.GetCurrentUserId(), savedUnit.Id);
            return _mapper.Map<UnitDto>(savedUnit);
        }

        public async Task UpdateAsync(UnitDto dto)
        {
            if (dto.Id is null)
            {
                throw new ArgumentNullException();
            }

            var newUnit = _mapper.Map<Unit>(dto);
            newUnit.Id = dto.Id.Value;
            var savedUnit = await _repository.GetByIdAsync(query => query, x => x.Id == dto.Id.Value);
            await _repository.UpdateAsync(savedUnit, newUnit, (entityState, _) =>
            {
                entityState.Property(x => x.Id).IsModified = false;
            });
            _logger.LogInformation("El usuario '{UserId}' ha modificado la unidad '{UnitId}'", _userContextService.GetCurrentUserId(), savedUnit.Id);
        }

        public async Task RemoveAsync(Guid id)
        {
            await _repository.SoftDeleteUnitAsync(id);
            _logger.LogInformation("El usuario '{UserId}' ha eliminado la unidad '{UnitId}'", _userContextService.GetCurrentUserId(), id);
        }
    }
}
