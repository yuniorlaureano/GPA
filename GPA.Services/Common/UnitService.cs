using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Common;
using GPA.Data.Common;
using GPA.Entities.Common;
using System.Linq.Expressions;

namespace GPA.Business.Services.Common
{
    public interface IUnitService
    {
        public Task<UnitDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<UnitDto>> GetAllAsync(SearchDto search, Expression<Func<Unit, bool>>? expression = null);

        public Task<UnitDto?> AddAsync(UnitDto unitDto);

        public Task UpdateAsync(UnitDto unitDto);

        public Task RemoveAsync(Guid id);
    }

    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _repository;
        private readonly IMapper _mapper;

        public UnitService(IUnitRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UnitDto?> GetByIdAsync(Guid id)
        {
            var unit = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            return _mapper.Map<UnitDto>(unit);
        }

        public async Task<ResponseDto<UnitDto>> GetAllAsync(SearchDto search, Expression<Func<Unit, bool>>? expression = null)
        {
            var categories = await _repository.GetAllAsync(query =>
            {
                return query.Skip(search.PageSize * Math.Abs(search.Page - 1)).Take(search.PageSize);
            }, expression);
            return new ResponseDto<UnitDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<UnitDto>>(categories)
            };
        }

        public async Task<UnitDto> AddAsync(UnitDto dto)
        {
            var unit = _mapper.Map<Unit>(dto);
            var savedUnit = await _repository.AddAsync(unit);
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
        }

        public async Task RemoveAsync(Guid id)
        {
            var savedUnit = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(savedUnit);
        }
    }
}
