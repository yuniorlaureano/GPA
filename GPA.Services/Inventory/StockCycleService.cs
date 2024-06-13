using AutoMapper;
using GPA.Common.DTOs;
using GPA.Common.DTOs.Inventory;
using GPA.Common.Entities.Inventory;
using GPA.Data.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GPA.Business.Services.Inventory
{
    public interface IStockCycleService
    {
        public Task<StockCycleDto?> GetByIdAsync(Guid id);

        public Task<ResponseDto<StockCycleDto>> GetAllAsync(SearchDto search, Expression<Func<StockCycle, bool>>? expression = null);

        public Task<Guid> OpenCycleAsync(StockCycleCreationDto dto);

        public Task RemoveAsync(Guid id);
    }

    public class StockCycleService : IStockCycleService
    {
        private readonly IStockCycleRepository _repository;
        private readonly IMapper _mapper;

        public StockCycleService(IStockCycleRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<StockCycleDto?> GetByIdAsync(Guid id)
        {
            var stockCycle = await _repository.GetByIdAsync(query =>
            {
                return query
                    .Include(x => x.StockCycleDetails);
            }, x => x.Id == id);

            return _mapper.Map<StockCycleDto>(stockCycle);
        }

        public async Task<ResponseDto<StockCycleDto>> GetAllAsync(SearchDto search, Expression<Func<StockCycle, bool>>? expression = null)
        {
            var stocks = await _repository.GetAllAsync(query =>
            {
                return query
                     .OrderByDescending(x => x.Id)
                     .Skip(search.PageSize * Math.Abs(search.Page - 1))
                     .Take(search.PageSize);
            }, expression);
            return new ResponseDto<StockCycleDto>
            {
                Count = await _repository.CountAsync(query => query, expression),
                Data = _mapper.Map<IEnumerable<StockCycleDto>>(stocks)
            };
        }

        public async Task<Guid> OpenCycleAsync(StockCycleCreationDto dto)
        {
            var newStockCycle = _mapper.Map<StockCycle>(dto);
            var cycleId = await _repository.OpenCycleAsync(newStockCycle);
            return cycleId;
        }

        public async Task RemoveAsync(Guid id)
        {
            var newStockCycle = await _repository.GetByIdAsync(query => query, x => x.Id == id);
            await _repository.RemoveAsync(newStockCycle);
        }
    }
}
